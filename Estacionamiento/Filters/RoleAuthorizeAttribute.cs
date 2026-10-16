using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Estacionamiento.Filters
{
    // Autoriza por rol usando Session["Rol"] (int) y mapeo en Web.config (appSettings):
    //  RoleMap:Administrador=3
    //  RoleMap:Solicitante=2
    //  RoleMap:Seguridad=4
    // Usa nombres de rol en el atributo: [RoleAuthorize("Administrador", "Seguridad")]
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _roleNames;

        public RoleAuthorizeAttribute(params string[] roleNames)
        {
            _roleNames = roleNames ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) return false;
            if (!httpContext.User.Identity.IsAuthenticated) return false;

            var sess = httpContext.Session;
            if (sess == null) return false;

            // Obtener roles del usuario desde sesión: soporta int (Rol), int[] (Roles) o string "1,2,3"
            var userRoles = GetUserRoles(sess);
            if (userRoles == null || userRoles.Length == 0) return false;

            if (_roleNames.Length == 0) return true; // sin restricción específica

            // Obtener IDs permitidos desde appSettings (soporta múltiples IDs por nombre: "1,2,5")
            var allowedIds = _roleNames
                .Select(name => ConfigurationManager.AppSettings["RoleMap:" + name])
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .SelectMany(v => v.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .Select(s => { int x; return int.TryParse(s, out x) ? (int?)x : null; })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToArray();

            if (allowedIds.Length == 0) return false; // sin mapeos -> negar

            // Autorizado si cualquier rol del usuario coincide con allowedIds
            return userRoles.Intersect(allowedIds).Any();
        }

        private static int[] GetUserRoles(HttpSessionStateBase sess)
        {
            if (sess == null) return new int[0];

            // 1) Array de ints en Session["Roles"]
            var arr = sess["Roles"] as int[];
            if (arr != null && arr.Length > 0) return arr;

            // 2) String separado por comas en Session["Roles"]
            var sRoles = sess["Roles"] as string;
            if (!string.IsNullOrWhiteSpace(sRoles))
            {
                var parsed = sRoles
                    .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Select(x => { int n; return int.TryParse(x, out n) ? (int?)n : null; })
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct()
                    .ToArray();
                if (parsed.Length > 0) return parsed;
            }

            // 3) Único rol en Session["Rol"]
            var one = sess["Rol"];
            int uno;
            if (one != null && int.TryParse(Convert.ToString(one), out uno))
            {
                return new[] { uno };
            }

            return new int[0];
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Para una mejor UX (y para recuperar sesión/rol), siempre delegamos al comportamiento base.
            // Con Forms Authentication, esto devuelve 401 que provoca redirección a LoginUrl.
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}

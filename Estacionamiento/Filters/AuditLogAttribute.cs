using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Estacionamiento.Filters
{
    // Filtro global para auditar movimientos de los usuarios en la plataforma.
    // Registra: fecha, usuario, rol, controlador, acción, método, URL, querystring, IP, user-agent, éxito y mensaje.
    public class AuditLogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                var ctx = filterContext.HttpContext;
                var req = ctx?.Request;
                if (req == null) return;

                // Evitar log de Login POST para no capturar credenciales
                var controller = (string)filterContext.RouteData.Values["controller"] ?? "";
                var action = (string)filterContext.RouteData.Values["action"] ?? "";
                var method = req.HttpMethod ?? "";
                if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Datos de usuario/rol desde sesión
                int? idUsuario = null;
                string nombreUsuario = null;
                int? rolId = null;
                string rolNombre = null;
                try
                {
                    if (ctx.Session != null)
                    {
                        var idObj = ctx.Session["IdUsuario"]; if (idObj != null) { int v; if (int.TryParse(Convert.ToString(idObj), out v)) idUsuario = v; }
                        var rolObj = ctx.Session["Rol"]; if (rolObj != null) { int v; if (int.TryParse(Convert.ToString(rolObj), out v)) rolId = v; }
                        nombreUsuario = Convert.ToString(ctx.Session["Nombre"]);
                    }
                }
                catch { }

                // Resolver nombre del rol desde appSettings RoleMap:*
                rolNombre = GetRoleNameById(rolId);

                // Construir querystring "k1=v1&k2=v2"
                string qs = null;
                try
                {
                    if (req.QueryString != null && req.QueryString.HasKeys())
                    {
                        var keys = req.QueryString.AllKeys.Where(k => k != null);
                        qs = string.Join("&", keys.Select(k => k + "=" + req.QueryString[k]));
                    }
                }
                catch { }

                // IP y UserAgent
                string ip = req.Headers["X-Forwarded-For"] ?? req.UserHostAddress;
                string ua = req.UserAgent;

                // Estado de ejecución
                bool exito = filterContext.Exception == null || filterContext.ExceptionHandled;
                string mensaje = filterContext.Exception != null ? SafeTruncate(filterContext.Exception.Message, 1000) : null;

                // URL completa (sin body)
                string url = null; try { url = req.Url != null ? SafeTruncate(req.Url.ToString(), 500) : null; } catch { }

                InsertLog(
                    DateTime.Now,
                    idUsuario,
                    SafeTruncate(nombreUsuario, 200),
                    rolId,
                    SafeTruncate(rolNombre, 100),
                    SafeTruncate(controller, 100),
                    SafeTruncate(action, 100),
                    SafeTruncate(method, 10),
                    url,
                    qs,
                    SafeTruncate(ip, 50),
                    SafeTruncate(ua, 400),
                    exito,
                    mensaje
                );
            }
            catch
            {
                // Evitar romper el flujo por errores de auditoría
            }
            base.OnActionExecuted(filterContext);
        }

        private static string GetRoleNameById(int? rolId)
        {
            if (!rolId.HasValue) return null;
            try
            {
                // Invertir mapeos RoleMap:* en appSettings
                var keys = ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("RoleMap:", StringComparison.OrdinalIgnoreCase));
                foreach (var k in keys)
                {
                    var value = ConfigurationManager.AppSettings[k];
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    var ids = value.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .Select(s => { int x; return int.TryParse(s, out x) ? (int?)x : null; })
                                    .Where(x => x.HasValue)
                                    .Select(x => x.Value);
                    if (ids.Contains(rolId.Value))
                        return k.Substring("RoleMap:".Length);
                }
            }
            catch { }
            return null;
        }

        private static string SafeTruncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private static void InsertLog(DateTime fecha, int? idUsuario, string nombreUsuario, int? rolId, string rolNombre,
            string controller, string action, string metodo, string url, string query, string ip, string userAgent, bool exito, string mensaje)
        {
            var cs = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"INSERT INTO dbo.logMovimientos
                (dtFecha, IdUsuario, sUsuario, iRol, sRolNombre, sController, sAction, sMetodo, sUrl, sQuery, sIp, sUserAgent, bExito, sMensaje)
                VALUES (@dtFecha, @IdUsuario, @sUsuario, @iRol, @sRolNombre, @sController, @sAction, @sMetodo, @sUrl, @sQuery, @sIp, @sUserAgent, @bExito, @sMensaje)", cn))
            {
                cmd.Parameters.Add("@dtFecha", SqlDbType.DateTime).Value = fecha;
                cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = (object)idUsuario ?? DBNull.Value;
                cmd.Parameters.Add("@sUsuario", SqlDbType.NVarChar, 200).Value = (object)nombreUsuario ?? DBNull.Value;
                cmd.Parameters.Add("@iRol", SqlDbType.Int).Value = (object)rolId ?? DBNull.Value;
                cmd.Parameters.Add("@sRolNombre", SqlDbType.NVarChar, 100).Value = (object)rolNombre ?? DBNull.Value;
                cmd.Parameters.Add("@sController", SqlDbType.NVarChar, 100).Value = (object)controller ?? DBNull.Value;
                cmd.Parameters.Add("@sAction", SqlDbType.NVarChar, 100).Value = (object)action ?? DBNull.Value;
                cmd.Parameters.Add("@sMetodo", SqlDbType.NVarChar, 10).Value = (object)metodo ?? DBNull.Value;
                cmd.Parameters.Add("@sUrl", SqlDbType.NVarChar, 500).Value = (object)url ?? DBNull.Value;
                cmd.Parameters.Add("@sQuery", SqlDbType.NVarChar).Value = (object)query ?? DBNull.Value;
                cmd.Parameters.Add("@sIp", SqlDbType.NVarChar, 50).Value = (object)ip ?? DBNull.Value;
                cmd.Parameters.Add("@sUserAgent", SqlDbType.NVarChar, 400).Value = (object)userAgent ?? DBNull.Value;
                cmd.Parameters.Add("@bExito", SqlDbType.Bit).Value = exito;
                cmd.Parameters.Add("@sMensaje", SqlDbType.NVarChar).Value = (object)mensaje ?? DBNull.Value;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

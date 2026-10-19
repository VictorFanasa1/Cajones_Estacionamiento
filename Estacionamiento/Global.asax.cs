using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Estacionamiento.Filters;
using Fanasa.SSO.AspNet;
using Fanasa.SSO.Core;

namespace Estacionamiento
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            // Registrar filtro global de auditoría
            GlobalFilters.Filters.Add(new AuditLogAttribute());
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Configurar SSO Fanasa (Microsoft Entra ID)
            FanasaSSOConfig.Initialize(new SsoOptions
            {
                SsoUrl = ConfigurationManager.AppSettings["FanasaSSO:SsoUrl"],
                ClientId = ConfigurationManager.AppSettings["FanasaSSO:ClientId"],
                TenantId = ConfigurationManager.AppSettings["FanasaSSO:TenantId"],
                ClientName = ConfigurationManager.AppSettings["FanasaSSO:ClientName"]
            });
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            // Evitar que páginas autenticadas queden en caché del navegador.
            // Así, después de cerrar sesión, el botón atrás no mostrará contenido privado.
            var ctx = HttpContext.Current;
            if (ctx == null) return;

            var response = ctx.Response;
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        }
    }
}

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using Estacionamiento.Models;

namespace Estacionamiento.Helpers
{
    public static class LogEstacionamientoHelper
    {
        // Catálogo de acciones (catLog). Ajusta los IDs según tus datos reales.
        public static class Accion
        {
            public const int GeneracionSolicitud = 1;     // Generación de Solicitud
            public const int AsignacionSolicitud = 2;     // Asignación de Solicitud
            public const int RechazoSolicitud = 3;        // Rechazo de Solicitud
            public const int AsignacionEmpleado = 4;      // Asignación Empleado (Formerly EdicionLugar)
            public const int LiberacionCajon = 5;         // Liberación de Cajón
            public const int EdicionLugar = 6;     // Liberación de Solicitud
            public const int EdicionHoraSalida = 7;       // Edición Hora de Salida
        }

        public static void Log(HttpContextBase ctx, int iLog, int? iDireccion = null, int? iCajon = null, DateTime? fecha = null)
        {
            if (ctx == null) return;

            int? uiUser = null;
            int? iRol = null;
            try
            {
                var user = ctx.Session != null ? ctx.Session["log"] as MUsuarios : null;
                if (user != null && user.uiNumeroEmpleado > 0) uiUser = user.uiNumeroEmpleado;

                // Priorizar Session["Rol"], de lo contrario usar user.iRol si viene poblado
                object rolObj = ctx.Session != null ? ctx.Session["Rol"] : null;
                int rolTmp;
                if (rolObj != null && int.TryParse(Convert.ToString(rolObj), out rolTmp)) iRol = rolTmp;
                else if (user != null && user.iRol > 0) iRol = user.iRol;
            }
            catch { }

            Insert(uiUser, iRol, iDireccion, iCajon, iLog, fecha ?? DateTime.Now);
        }

        private static void Insert(int? uiUser, int? iRol, int? iDireccion, int? iCajon, int iLog, DateTime dtFechaAccion)
        {
            var cs = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"INSERT INTO dbo.logEstacionamiento
                (uiUser, iRol, iDireccion, iCajon, iLog, dtFechaAccion)
                VALUES (@uiUser, @iRol, @iDireccion, @iCajon, @iLog, @dtFechaAccion)", cn))
            {
                cmd.Parameters.Add("@uiUser", SqlDbType.Int).Value = (object)uiUser ?? DBNull.Value;
                cmd.Parameters.Add("@iRol", SqlDbType.Int).Value = (object)iRol ?? DBNull.Value;
                cmd.Parameters.Add("@iDireccion", SqlDbType.Int).Value = (object)iDireccion ?? DBNull.Value;
                cmd.Parameters.Add("@iCajon", SqlDbType.Int).Value = (object)iCajon ?? DBNull.Value;
                cmd.Parameters.Add("@iLog", SqlDbType.Int).Value = iLog;
                cmd.Parameters.Add("@dtFechaAccion", SqlDbType.DateTime).Value = dtFechaAccion;

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

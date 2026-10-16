using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Estacionamiento.Filters;

namespace Estacionamiento.Controllers
{
    [Authorize]
    [RoleAuthorize("Administrador")]
    public class ReportesController : Controller
    {
        [HttpGet]
        public ActionResult DescargarAuditoria(string fechaDesde = null, string fechaHasta = null, int? idUsuario = null, int? rolId = null, string controller = null, string action = null, bool? exito = null)
        {
            // Si no se proporcionan fechas, por defecto últimos 30 días
            DateTime desde, hasta;
            var now = DateTime.Now;
            if (!DateTime.TryParse(fechaDesde, out desde))
                desde = now.AddDays(-30).Date;
            if (!DateTime.TryParse(fechaHasta, out hasta))
                hasta = now.Date.AddDays(1).AddTicks(-1);

            var cs = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.sp_ReporteLogsMovimientos", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fechaDesde", (object)desde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaHasta", (object)hasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idUsuario", (object)idUsuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@rolId", (object)rolId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@controller", (object)controller ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@action", (object)action ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@exito", (object)exito ?? DBNull.Value);

                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    // Construir CSV en memoria (UTF-8 con BOM para Excel)
                    var sb = new StringBuilder();
                    // Encabezados
                    sb.AppendLine(string.Join(",",
                        new[] {
                            "IdLog","Fecha","IdUsuario","Usuario","RolId","Rol","Controller","Action","Metodo","Url","Query","IP","UserAgent","Exito","Mensaje"
                        }));

                    while (rdr.Read())
                    {
                        string[] cols = new string[]
                        {
                            ToCsv(rdr["IdLog"]),
                            ToCsv(Convert.ToDateTime(rdr["dtFecha"]).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                            ToCsv(rdr["IdUsuario"]),
                            ToCsv(rdr["sUsuario"]),
                            ToCsv(rdr["iRol"]),
                            ToCsv(rdr["sRolNombre"]),
                            ToCsv(rdr["sController"]),
                            ToCsv(rdr["sAction"]),
                            ToCsv(rdr["sMetodo"]),
                            ToCsv(rdr["sUrl"]),
                            ToCsv(rdr["sQuery"]),
                            ToCsv(rdr["sIp"]),
                            ToCsv(rdr["sUserAgent"]),
                            ToCsv(Convert.ToBoolean(rdr["bExito"]) ? "1" : "0"),
                            ToCsv(rdr["sMensaje"])
                        };
                        sb.AppendLine(string.Join(",", cols));
                    }

                    var csv = sb.ToString();
                    // Agregar BOM para que Excel detecte UTF-8 correctamente
                    var bom = new byte[] { 0xEF, 0xBB, 0xBF };
                    var bytes = Encoding.UTF8.GetBytes(csv);
                    var all = new byte[bom.Length + bytes.Length];
                    Buffer.BlockCopy(bom, 0, all, 0, bom.Length);
                    Buffer.BlockCopy(bytes, 0, all, bom.Length, bytes.Length);

                    var fileName = string.Format("Auditoria_{0:yyyyMMdd_HHmm}.csv", DateTime.Now);
                    return File(all, "text/csv", fileName);
                }
            }
        }

        private static string ToCsv(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            var s = Convert.ToString(value);
            if (s == null) return "";
            // Normalizar saltos de línea y escapar comillas
            s = s.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            s = s.Replace("\"", "\"\"");
            // Envolver en comillas si contiene separador o comillas
            if (s.Contains(",") || s.Contains("\""))
                s = "\"" + s + "\"";
            return s;
        }
    }
}

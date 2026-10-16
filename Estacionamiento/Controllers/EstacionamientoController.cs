using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Estacionamiento.Models;
using System.Configuration;
using System.DirectoryServices;
using Estacionamiento.Helpers;
using Estacionamiento.Filters;

namespace Estacionamiento.Controllers
{
    [Authorize]
    public class EstacionamientoController : Controller
    {
        // Verifica si un Stored Procedure contiene un parámetro específico
        [NonAction]
        private bool StoredProcHasParameter(SqlConnection connection, string procName, string paramName)
        {
            using (var checkCmd = new SqlCommand(@"SELECT 1 FROM sys.parameters WHERE object_id = OBJECT_ID(@proc) AND name = @param", connection))
            {
                checkCmd.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.NVarChar, ParameterName = "@proc", Value = procName });
                checkCmd.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.NVarChar, ParameterName = "@param", Value = paramName });
                var exists = checkCmd.ExecuteScalar();
                return exists != null;
            }
        }

        // Verifica si existe un Stored Procedure por nombre
        [NonAction]
        private bool StoredProcExists(SqlConnection connection, string procName)
        {
            using (var checkCmd = new SqlCommand(@"SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(@proc) AND type IN ('P','PC')", connection))
            {
                checkCmd.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.NVarChar, ParameterName = "@proc", Value = procName });
                var exists = checkCmd.ExecuteScalar();
                return exists != null;
            }
        }

        // Obtiene (o reconstruye) el usuario de sesión enriquecido (Puesto, UEN, Jefe Directo).
        // Si Session["log"] está vacío o sin datos clave, intenta cargarlo desde BD usando Session["IdUsuario"].
        [NonAction]
        private MUsuarios ObtenerUsuarioSesionEnriquecido()
        {
            var sesionUser = Session["log"] as MUsuarios;
            int idSesion = 0;
            try { idSesion = Session["IdUsuario"] != null ? Convert.ToInt32(Session["IdUsuario"]) : 0; } catch { idSesion = 0; }

            bool faltanDatos = sesionUser == null ||
                               (string.IsNullOrWhiteSpace(sesionUser.Puesto) || string.IsNullOrWhiteSpace(sesionUser.UEN));

            if (faltanDatos && idSesion > 0)
            {
                string cs = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand(@"SELECT TOP 1 IdUsuarios, uiNumeroEmpleado, sNombre, sCorreo, Puesto, JefeDirecto, UEN
                                                  FROM usuarios WHERE IdUsuarios = @id", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = idSesion });
                    try
                    {
                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                sesionUser = new MUsuarios
                                {
                                    IdUsuarios = rd.GetInt32(0),
                                    uiNumeroEmpleado = rd[1] != DBNull.Value ? Convert.ToInt32(rd[1]) : 0,
                                    sNombre = rd[2] != DBNull.Value ? Convert.ToString(rd[2]) : "",
                                    sCorreo = rd[3] != DBNull.Value ? Convert.ToString(rd[3]) : "",
                                    Puesto = rd[4] != DBNull.Value ? Convert.ToString(rd[4]) : null,
                                    JefeDirecto = rd[5] != DBNull.Value ? Convert.ToString(rd[5]) : null,
                                    UEN = rd[6] != DBNull.Value ? Convert.ToString(rd[6]) : null
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("[ObtenerUsuarioSesionEnriquecido] Error al leer BD: " + ex.Message);
                    }
                }

                // Actualizar la sesión si logramos obtener datos.
                if (sesionUser != null) Session["log"] = sesionUser;
            }

            return sesionUser;
        }
        // GET: Estacionamiento
        //[Authorize]
        //[RoleAuthorize("Administrador")]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult AdmEstacionamientos()
        {
            MUsuarios user = (MUsuarios)Session["log"];
            user.iRol = 1;
            return View();
        }

        [NonAction]
        private List<MLugares> ObtenerCatalogosBD()
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand("Select * from catLugares", conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<MLugares> listaLugares = new List<MLugares>();

            foreach (DataRow r in dt.Rows)
            {
                MLugares lugares = new MLugares(r);
                listaLugares.Add(lugares);
            }

            return listaLugares;
        }

        public JsonResult MostrarCatalogoLugares_AJAX()
        {
            try
            {
                List<MLugares> listaCalagos = ObtenerCatalogosBD();

                return Json(new { error = false, listaCalagos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private List<MLugares> ObtenerCatalogosVisitantesBD()
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand("Select * from catLugares where bVisita = 1", conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<MLugares> listaLugares = new List<MLugares>();

            foreach (DataRow r in dt.Rows)
            {
                MLugares lugares = new MLugares(r);
                listaLugares.Add(lugares);
            }

            return listaLugares;
        }

        public JsonResult MostrarCatalogoLugaresVisitante_AJAX()
        {
            try
            {
                List<MLugares> listaCalagos = ObtenerCatalogosVisitantesBD();

                return Json(new { error = false, listaCalagos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [NonAction]
        private List<MLugarDireccion> ObtenerCatalogosLugaresBD(int direccion)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from catNumeroLugares where iLugar = {0} and bActivo = 1 and bVisitante = 0 and bPresidencia = 0", direccion), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<MLugarDireccion> listaLugares = new List<MLugarDireccion>();

            foreach (DataRow r in dt.Rows)
            {
                MLugarDireccion lugares = new MLugarDireccion(r);
                listaLugares.Add(lugares);
            }

            return listaLugares;
        }

        public JsonResult MostrarCatalogoLugaresPorDireccion_AJAX(int direccion)
        {
            try
            {
                List<MLugarDireccion> listaCalagos = ObtenerCatalogosLugaresBD(direccion);

                return Json(new { error = false, listaCalagos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private List<MLugarDireccion> ObtenerCatalogosLugaresVisitantesBD(int direccion)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from catNumeroLugares where iLugar = {0} and bActivo = 1 and bVisitante = 1", direccion), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<MLugarDireccion> listaLugares = new List<MLugarDireccion>();

            foreach (DataRow r in dt.Rows)
            {
                MLugarDireccion lugares = new MLugarDireccion(r);
                listaLugares.Add(lugares);
            }

            return listaLugares;
        }

        public JsonResult MostrarCatalogoLugaresPorDireccionVisitantes_AJAX(int direccion)
        {
            try
            {
                List<MLugarDireccion> listaCalagos = ObtenerCatalogosLugaresVisitantesBD(direccion);

                return Json(new { error = false, listaCalagos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarUserAD_AJAX(string parametroBusqueda, string errorMensaje)
        {
            try
            {
                MUsuarios Usuario = new MUsuarios();

                // Normalize the incoming search term so values like DOMAIN\user or user@domain
                // are reduced to the account name used in the DB/AD.
                string parametro = (parametroBusqueda ?? string.Empty).Trim();
                // If DOMAIN\user format, take the part after backslash
                var bs = parametro.IndexOf('\\');
                if (bs >= 0 && bs + 1 < parametro.Length) parametro = parametro.Substring(bs + 1);
                // If user@domain format, take the left side
                var at = parametro.IndexOf('@');
                if (at > 0) parametro = parametro.Substring(0, at);

                // First, attempt a fast DB lookup in the local 'usuarios' table to
                // return cached/stored user data quickly (avoids waiting on AD/FAST).
                int posibleNumero;
                if (int.TryParse(parametro, out posibleNumero))
                {
                    // lookup by employee number
                    string cadenaConexionQuick = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
                    using (SqlConnection connQuick = new SqlConnection(cadenaConexionQuick))
                    {
                        connQuick.Open();
                        using (SqlCommand cmd = new SqlCommand("Select top 1 uiNumeroEmpleado, sNombre, sCorreo, Puesto, JefeDirecto, UEN from usuarios where uiNumeroEmpleado = @num and bActivo = 1", connQuick))
                        {
                            cmd.Parameters.Add(new SqlParameter("@num", SqlDbType.Int) { Value = posibleNumero });
                            using (SqlDataReader rdr = cmd.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    Usuario.uiNumeroEmpleado = rdr["uiNumeroEmpleado"] != DBNull.Value ? Convert.ToInt32(rdr["uiNumeroEmpleado"]) : 0;
                                    Usuario.sNombre = rdr["sNombre"] as string ?? "";
                                    Usuario.sCorreo = rdr["sCorreo"] as string ?? "";
                                    // Try to read optional columns if present
                                    try { Usuario.Puesto = rdr["Puesto"] as string; } catch { }
                                    try { Usuario.JefeDirecto = rdr["JefeDirecto"] as string; } catch { }
                                    try { Usuario.UEN = rdr["UEN"] as string; } catch { }
                                    return Json(new { Usuario, error = false }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Non-numeric search: try matching sAMAccountName or email in DB for a fast result
                    string q = parametro; // already normalized above
                    if (!string.IsNullOrEmpty(q))
                    {
                        string cadenaConexionQuick = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
                        using (SqlConnection connQuick = new SqlConnection(cadenaConexionQuick))
                        {
                            connQuick.Open();
                            using (SqlCommand cmd = new SqlCommand("Select top 1 uiNumeroEmpleado, sNombre, sCorreo, Puesto, JefeDirecto, UEN from usuarios where (sCorreo = @q OR sNombre = @q) and bActivo = 1", connQuick))
                            {
                                cmd.Parameters.Add(new SqlParameter("@q", SqlDbType.NVarChar) { Value = q });
                                using (SqlDataReader rdr = cmd.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        Usuario.uiNumeroEmpleado = rdr["uiNumeroEmpleado"] != DBNull.Value ? Convert.ToInt32(rdr["uiNumeroEmpleado"]) : 0;
                                        Usuario.sNombre = rdr["sNombre"] as string ?? "";
                                        Usuario.sCorreo = rdr["sCorreo"] as string ?? "";
                                        try { Usuario.Puesto = rdr["Puesto"] as string; } catch { }
                                        try { Usuario.JefeDirecto = rdr["JefeDirecto"] as string; } catch { }
                                        try { Usuario.UEN = rdr["UEN"] as string; } catch { }
                                        return Json(new { Usuario, error = false }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                    }
                }

                string Cuenta = "";
                string noEmpleado = "";
                string nombreEmpleado = "";
                string correoUsuario = "";
                string areaUsuario = "";
                string cargoUsuario = "";
                string telefonoUsuario = "";
                string ubicacionUsuario = "";
                string Company = "";

                string UAN = "LDAP://gf.grupofarmacos.net/DC=gf,DC=grupofarmacos,DC=net";

                DirectoryEntry oroot = new DirectoryEntry(UAN, "GFLSAEC1", "Sa3cgf19*", AuthenticationTypes.Secure);
                DirectorySearcher nombre = new DirectorySearcher(oroot);
                int NoBuscarEmpleado;

                if (int.TryParse(parametroBusqueda, out NoBuscarEmpleado))
                {
                    nombre.Filter = $"employeeNumber= {NoBuscarEmpleado}";
                }

                else
                {
                    nombre.Filter = $"sAMAccountName= {parametroBusqueda}";
                }

                nombre.PropertiesToLoad.Add("sAMAccountName"); /* Cuenta de Empleado*/
                nombre.PropertiesToLoad.Add("employeeNumber"); /* Número de Empleado*/
                nombre.PropertiesToLoad.Add("cn"); /* Nombre de Empleado*/
                nombre.PropertiesToLoad.Add("mail"); /* Correo de Empleado*/
                nombre.PropertiesToLoad.Add("description"); /* Área de Empleado*/
                nombre.PropertiesToLoad.Add("title"); /* Cargo de Empleado*/
                nombre.PropertiesToLoad.Add("telephoneNumber"); /* Teléfono de Empleado*/
                nombre.PropertiesToLoad.Add("physicalDeliveryOfficeName"); /* Ubicación de Empleado*/
                nombre.PropertiesToLoad.Add("Company"); /* UEN de Empleado*/

                if (nombre.FindAll().Count > 0)
                {
                    foreach (SearchResult resultNombre in nombre.FindAll())
                    {
                        try
                        {
                            noEmpleado = (string)resultNombre.Properties["employeeNumber"][0];
                            Usuario.uiNumeroEmpleado = Convert.ToInt32(noEmpleado);
                        }
                        catch
                        {
                            noEmpleado = "";
                        }

                        try
                        {
                            nombreEmpleado = (string)resultNombre.Properties["cn"][0];
                            Usuario.sNombre = nombreEmpleado;
                        }

                        catch
                        {

                        }

                        try
                        {
                            correoUsuario = (string)resultNombre.Properties["mail"][0];
                            Usuario.sCorreo = correoUsuario;
                        }

                        catch
                        {

                        }

                    }

                    // Enriquecer datos del usuario desde el servicio FAST (Nombre, Puesto, Jefe Directo, UEN)
                    try
                    {
                        if (Usuario.uiNumeroEmpleado > 0)
                        {
                            // Llamar al método de instancia para poblar datos si el proxy FAST está presente
                            try { Usuario.PoblarDatosDesdeServicioFAST(Usuario.uiNumeroEmpleado); } catch { }
                        }
                    }
                    catch (Exception)
                    {
                        // Si falla el servicio FAST, continuamos con los datos de AD sin interrumpir el flujo
                    }
                }

                else
                {
                    errorMensaje = "Error: El usuario no existe.";
                    return Json(new { error = true, mensaje = errorMensaje }, JsonRequestBehavior.AllowGet);
                }

                // Persist enriched user to local DB for faster future lookups (UPSERT)
                try
                {
                    string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(cadenaConexion))
                    {
                        conn.Open();
                        // Prefer matching by employee number, otherwise by email
                        if (Usuario != null && Usuario.uiNumeroEmpleado > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM usuarios WHERE uiNumeroEmpleado = @uiNumeroEmpleado)
    UPDATE usuarios SET sNombre = @sNombre, sCorreo = @sCorreo, Puesto = @Puesto, JefeDirecto = @JefeDirecto, UEN = @UEN, bActivo = 1 WHERE uiNumeroEmpleado = @uiNumeroEmpleado
ELSE
    INSERT INTO usuarios (uiNumeroEmpleado, sNombre, sCorreo, Puesto, JefeDirecto, UEN, bActivo) VALUES (@uiNumeroEmpleado, @sNombre, @sCorreo, @Puesto, @JefeDirecto, @UEN, 1)
", conn))
                            {
                                cmd.Parameters.Add(new SqlParameter("@uiNumeroEmpleado", SqlDbType.Int) { Value = Usuario.uiNumeroEmpleado });
                                cmd.Parameters.Add(new SqlParameter("@sNombre", SqlDbType.VarChar, 200) { Value = (object)Usuario.sNombre ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@sCorreo", SqlDbType.VarChar, 100) { Value = (object)Usuario.sCorreo ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@Puesto", SqlDbType.VarChar, 200) { Value = (object)Usuario.Puesto ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@JefeDirecto", SqlDbType.VarChar, 200) { Value = (object)Usuario.JefeDirecto ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@UEN", SqlDbType.VarChar, 200) { Value = (object)Usuario.UEN ?? DBNull.Value });
                                try { cmd.ExecuteNonQuery(); } catch { /* ignore persistence errors to not break the lookup */ }
                            }
                        }
                        else if (Usuario != null && !string.IsNullOrEmpty(Usuario.sCorreo))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM usuarios WHERE sCorreo = @sCorreo)
    UPDATE usuarios SET sNombre = @sNombre, Puesto = @Puesto, JefeDirecto = @JefeDirecto, UEN = @UEN, bActivo = 1 WHERE sCorreo = @sCorreo
ELSE
    INSERT INTO usuarios (sNombre, sCorreo, Puesto, JefeDirecto, UEN, bActivo) VALUES (@sNombre, @sCorreo, @Puesto, @JefeDirecto, @UEN, 1)
", conn))
                            {
                                cmd.Parameters.Add(new SqlParameter("@sNombre", SqlDbType.VarChar, 200) { Value = (object)Usuario.sNombre ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@sCorreo", SqlDbType.VarChar, 100) { Value = (object)Usuario.sCorreo ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@Puesto", SqlDbType.VarChar, 200) { Value = (object)Usuario.Puesto ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@JefeDirecto", SqlDbType.VarChar, 200) { Value = (object)Usuario.JefeDirecto ?? DBNull.Value });
                                cmd.Parameters.Add(new SqlParameter("@UEN", SqlDbType.VarChar, 200) { Value = (object)Usuario.UEN ?? DBNull.Value });
                                try { cmd.ExecuteNonQuery(); } catch { }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore persistence errors — we don't want to break the lookup flow
                }

                return Json(new { Usuario, error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [NonAction]

        private void ValidarUsuarioEmpleadoBD(int numeroEmpleado, string nombreUser)
        {
            bool filas;
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_ValidarUsuarioEmpelado", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiNumeroEmpleado", Value = numeroEmpleado });

                        filas = Convert.ToBoolean(comando.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }

                if (filas == true)
                {
                    throw new ApplicationException(string.Format("El usuario: [{0}] ya cuenta con cajón de Estacionamiento", nombreUser));
                }
            }

        }

        [NonAction]
        private MUsuarios ValidarExistenciaUsuarioEmpleadoBD(int numeroEmpleado)
        {
            MUsuarios usuario = new MUsuarios();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand(string.Format("Select * from usuarios where uiNumeroEmpleado = {0} and iRol = 5 and bActivo = 1", numeroEmpleado), conexionSQL))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            usuario = new MUsuarios();
                        }
                        else
                        {
                            DataRow r = dt.Rows[0];
                            usuario.IdUsuarios = Convert.ToInt32(r["IdUsuarios"]);
                            usuario.uiNumeroEmpleado = Convert.ToInt32(r["uiNumeroEmpleado"]);
                            usuario.sNombre = Convert.ToString(r["sNombre"]);
                        }

                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }

            }

            return usuario;
        }

        [NonAction]
        private MEstacionamiento ObtenerCajonEmpleadoBD(int registroEmpleado)
        {
            MEstacionamiento estacionamiento = new MEstacionamiento();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    //using (SqlCommand comando = new SqlCommand(string.Format("Select * from cajonesEstacionamientos where iUsuario = {0}", registroEmpleado), conexionSQL))
                    using (SqlCommand comando = new SqlCommand(string.Format("Select * from cajonesEstacionamientos e inner join usuarios u on e.iUsuario = u.IdUsuarios where e.iUsuario = {0} and u.iRol = 5", registroEmpleado), conexionSQL))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow r = dt.Rows[0];
                            estacionamiento.uiRegistroCajones = Convert.ToInt32(r["uiRegistroCajones"]);
                            estacionamiento.iCajon = Convert.ToInt32(r["iCajon"]);
                            estacionamiento.iUsuario = Convert.ToInt32(r["iUsuario"]);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }

            }

            return estacionamiento;
        }

        [NonAction]
        private int AgregarUsuariosSolicitanteBD(int numeroEmpleado, string nombreUser, string correo, string puesto, string jefe, string uen)
        {
            int registroUsuario = 0;
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertUsuariosSolicitante", conexionSQL))
                    {
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiNumeroEmpleado", Value = numeroEmpleado });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sNombre", Size = 200, Value = nombreUser });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sCorreo", Size = 100, Value = correo });

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sPuesto", Size = 200, Value = puesto });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sJefeDirecto", Size = 200, Value = jefe });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sUEN", Size = 200, Value = uen });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Direction = ParameterDirection.Output });

                        int filas = comando.ExecuteNonQuery();
                        registroUsuario = Convert.ToInt32(comando.Parameters["@IdUsuarios"].Value);

                        if (filas != 1 || registroUsuario == 0)
                        {
                            throw new ApplicationException(string.Format("No se agregó el usuario solicitante: {0}", nombreUser));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }

            return registroUsuario;
        }


        [NonAction]
        private int AgregarUsuariosEmpleadosBD(int numeroEmpleado, string nombreUser, string correo, string puesto, string jefe, string uen)
        {
            int registroUsuario = 0;
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertUsuariosEmpleados", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiNumeroEmpleado", Value = numeroEmpleado });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sNombre", Size = 200, Value = nombreUser });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sCorreo", Size = 100, Value = correo });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sPuesto", Size = 200, Value = puesto });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sJefeDirecto", Size = 200, Value = jefe });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sUEN", Size = 200, Value = uen });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Direction = ParameterDirection.Output });

                        int filas = 0;

                        filas = comando.ExecuteNonQuery();
                        registroUsuario = Convert.ToInt32(comando.Parameters["@IdUsuarios"].Value);

                        if (filas != 1)
                        {
                            throw new ApplicationException(string.Format("No se agregó el usuario:{0}", nombreUser));
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();

                }
            }

            return registroUsuario;
        }

        [NonAction]
        private void AgregarAsignacionEstacionamientoEmpleadosBD(int cajon, int lugar, int numUsuario, string observaciones)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertEstacionamiento", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iCajon", Value = cajon });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iLugar", Value = lugar });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iUsuario", Value = numUsuario });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sObservaciones", Value = observaciones });

                        int filas = 0;

                        filas = comando.ExecuteNonQuery();

                        if (filas != 1)
                        {
                            throw new ApplicationException(string.Format("No se asignó el lugar de estacionamient al usuario: {0}", numUsuario));
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }

        }

        [NonAction]

        private MLugarDireccion ObtenerLugarDireccionBD(int numeroLugar, string lugar)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from catNumeroLugares where iLugar = {0} and sLugar = '{1}'", numeroLugar, lugar), conexionSQL);
            SqlDataAdapter da = new SqlDataAdapter(comando);

            DataTable dt = new DataTable();
            da.Fill(dt);

            MLugarDireccion lugarDireccion = new MLugarDireccion();

            if (dt.Rows.Count == 0)
            {
                throw new ApplicationException("Este Cajón de estacionamiento ya no cuenta con Lugares disponibles");
            }
            else
            {
                DataRow r = dt.Rows[0];
                lugarDireccion = new MLugarDireccion(r);
            }

            return lugarDireccion;
        }

        [NonAction]

        private void CambiarEstatusLugaresBD(int lugar, string nombreLugar)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_CambiarStatusLugar", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iLugar", Value = lugar });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sLugar", Size = 50, Value = nombreLugar });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se editó el estatus del Lugar");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }

        }

        public JsonResult AsignarEstacionamientoEmpleado_AJAX(MEstacionamiento estacionamiento)
        {
            try
            {
                MUsuarios userA = (MUsuarios)Session["log"];
                //ValidarUsuarioEmpleadoBD(estacionamiento.iUsuario, estacionamiento.nombreUsuario);
                bool validacion = false;
                MUsuarios user = ValidarExistenciaUsuarioEmpleadoBD(estacionamiento.iUsuario);

                if (user.IdUsuarios != 0)
                {
                    MEstacionamiento estaciona = ObtenerCajonEmpleadoBD(user.IdUsuarios);

                    if (estaciona.iCajon == estacionamiento.iCajon)
                    {
                        validacion = true;
                    }
                    else
                    {
                        int registroUsuario = AgregarUsuariosEmpleadosBD(estacionamiento.iUsuario, estacionamiento.nombreUsuario, estacionamiento.correoUsuario, userA.Puesto, userA.JefeDirecto, userA.UEN);

                        MLugarDireccion lugarDireccion = ObtenerLugarDireccionBD(estacionamiento.iCajon, estacionamiento.sLugar);
                        CambiarEstatusLugaresBD(lugarDireccion.iLugar, lugarDireccion.sLugar);
                        AgregarAsignacionEstacionamientoEmpleadosBD(estacionamiento.iCajon, lugarDireccion.uiRegistroLugar, registroUsuario, estacionamiento.sObservaciones);
                        // Log de asignación (empleado)
                        LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.AsignacionEmpleado, lugarDireccion.iLugar, lugarDireccion.uiRegistroLugar);
                    }
                }
                else
                {
                    int registroUsuario = AgregarUsuariosEmpleadosBD(estacionamiento.iUsuario, estacionamiento.nombreUsuario, estacionamiento.correoUsuario, userA.Puesto, userA.JefeDirecto, userA.UEN);

                    MLugarDireccion lugarDireccion2 = ObtenerLugarDireccionBD(estacionamiento.iCajon, estacionamiento.sLugar);
                    CambiarEstatusLugaresBD(lugarDireccion2.iLugar, lugarDireccion2.sLugar);
                    AgregarAsignacionEstacionamientoEmpleadosBD(estacionamiento.iCajon, lugarDireccion2.uiRegistroLugar, registroUsuario, estacionamiento.sObservaciones);
                    // Log de asignación (empleado)
            LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.AsignacionEmpleado, lugarDireccion2.iLugar, lugarDireccion2.uiRegistroLugar);
                }

                return Json(new { error = false, validacion }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private void ActualizarUsuariosEmpleadosBD(int idUsuario, int numeroEmpleado, string nombreUser, string correo, string puesto, string jefe, string uen)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();

                    if (StoredProcExists(conexionSQL, "sp_UpdateUsuariosEmpleados"))
                    {
                        using (SqlCommand comando = new SqlCommand("sp_UpdateUsuariosEmpleados", conexionSQL))
                        {
                            comando.CommandType = CommandType.StoredProcedure;
                            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Value = idUsuario });
                            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sNombre", Size = 200, Value = nombreUser });
                            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sCorreo", Size = 100, Value = correo });
                            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sPuesto", Size = 200, Value = puesto });
                            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sJefeDirecto", Size = 200, Value = jefe });
                            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sUEN", Size = 200, Value = uen });

                            int filas = comando.ExecuteNonQuery();
                            if (filas == 0)
                            {
                                throw new ApplicationException("No se actualizó el usuario");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [NonAction]
        private int AgregarUsuariosEmpleadosResponsablesBD(int numeroEmpleado, string nombreUser, string correo)
        {
            int registroUsuario = 0;
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertUsuariosEmpleadosResponsables", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiNumeroEmpleado", Value = numeroEmpleado });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sNombre", Size = 200, Value = nombreUser });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sCorreo", Size = 100, Value = correo });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Direction = ParameterDirection.Output });

                        int filas = 0;

                        filas = comando.ExecuteNonQuery();
                        registroUsuario = Convert.ToInt32(comando.Parameters["@IdUsuarios"].Value);

                        if (filas != 1)
                        {
                            throw new ApplicationException(string.Format("No se agregó el usuario:{0}", nombreUser));
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();

                }
            }

            return registroUsuario;
        }

        [NonAction]
        private int AgregarUsuariosVisitantesBD(string nombreUser, int registroUserResponsable)
        {
            int registroUsuario = 0;
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertUsuariosVisitantes", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sNombre", Size = 200, Value = nombreUser });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iRegistroUserResponsable", Value = registroUserResponsable });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Direction = ParameterDirection.Output });

                        int filas = 0;

                        filas = comando.ExecuteNonQuery();
                        registroUsuario = Convert.ToInt32(comando.Parameters["@IdUsuarios"].Value);

                        if (filas != 1)
                        {
                            throw new ApplicationException(string.Format("No se agregó el usuario:{0}", nombreUser));
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();

                }
            }

            return registroUsuario;
        }

        [NonAction]
        private void EstablecerIdUserResponsableBD(int idUsuario, int idUserResponsable, string puesto = null, string jefeDirecto = null, string uen = null)
        {
            if (idUsuario <= 0 || idUserResponsable <= 0) return;

            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();

                    // Construir UPDATE dinámico según los campos disponibles
                    var updates = new List<string> { "idUserResponsables = @idUserResponsables" };
                    if (!string.IsNullOrWhiteSpace(puesto)) updates.Add("Puesto = @sPuesto");
                    if (!string.IsNullOrWhiteSpace(jefeDirecto)) updates.Add("JefeDirecto = @sJefeDirecto");
                    if (!string.IsNullOrWhiteSpace(uen)) updates.Add("UEN = @sUEN");

                    string sql = "UPDATE usuarios SET " + string.Join(", ", updates) + " WHERE IdUsuarios = @IdUsuarios";

                    using (SqlCommand comando = new SqlCommand(sql, conexionSQL))
                    {
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@idUserResponsables", Value = idUserResponsable });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Value = idUsuario });
                        if (!string.IsNullOrWhiteSpace(puesto)) comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sPuesto", Size = 200, Value = puesto });
                        if (!string.IsNullOrWhiteSpace(jefeDirecto)) comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sJefeDirecto", Size = 200, Value = jefeDirecto });
                        if (!string.IsNullOrWhiteSpace(uen)) comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sUEN", Size = 200, Value = uen });
                        comando.ExecuteNonQuery();
                    }
                }
                catch
                {
                    // No propagar error para no romper el flujo; opcionalmente se puede loggear
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [NonAction]
        private void EstablecerRolVisitanteSiSolicitanteBD(int idUsuario)
        {
            if (idUsuario <= 0) return;

            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT iRol FROM usuarios WHERE IdUsuarios = @IdUsuarios", conexionSQL))
                    {
                        cmd.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Value = idUsuario });
                        var obj = cmd.ExecuteScalar();
                        if (obj == null || obj == DBNull.Value) return;
                        int rol = Convert.ToInt32(obj);

                        if (rol == 2)
                        {
                            using (SqlCommand update = new SqlCommand("UPDATE usuarios SET iRol = 4 WHERE IdUsuarios = @IdUsuarios", conexionSQL))
                            {
                                update.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Value = idUsuario });
                                try { update.ExecuteNonQuery(); } catch { /* no propagar */ }
                            }
                        }
                    }
                }
                catch
                {
                    // No propagar errores para no romper el flujo principal
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [NonAction]
        private int AgregarAsignacionEstacionamientoVisitantesBD(int cajon, int lugar, int numUsuario, string observaciones, DateTime fechaVisita, string horaVisita, string horaSalida, string placa, string modelo, string color, bool bInterno = false, bool bExterno = false)
        {
            int registroCajon = 0;
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertEstacionamientoVisitantes", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iCajon", Value = cajon });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iLugar", Value = lugar });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iUsuario", Value = numUsuario });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sObservaciones", Value = observaciones });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.DateTime, ParameterName = "@dtFechaVisita", Value = fechaVisita });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@horaVisita", Size = 50, Value = horaVisita });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@horaSalida", Size = 50, Value = horaSalida });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sPlaca", Size = 50, Value = placa });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sModelo", Size = 50, Value = modelo });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sColor", Size = 50, Value = color });

                        // Parmetros opcionales si el SP ya fue actualizado
                        try
                        {
                            if (StoredProcHasParameter(conexionSQL, "sp_InsertEstacionamientoVisitantes", "@bInterno"))
                                comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Bit, ParameterName = "@bInterno", Value = bInterno });
                            if (StoredProcHasParameter(conexionSQL, "sp_InsertEstacionamientoVisitantes", "@bExterno"))
                                comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Bit, ParameterName = "@bExterno", Value = bExterno });
                        }
                        catch { /* si no existe el SP o los parametros, continuamos sin bloquear */ }
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroCajones", Direction = ParameterDirection.Output });

                        int filas = 0;

                        filas = comando.ExecuteNonQuery();
                        registroCajon = Convert.ToInt32(comando.Parameters["@uiRegistroCajones"].Value);
                        return registroCajon;

                        if (filas != 1)
                        {
                            throw new ApplicationException(string.Format("No se asignó el lugar de estacionamient al usuario: {0}", numUsuario));
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }

        }

        public JsonResult AsignarEstacionamientoVisitante_AJAX(MEstacionamiento estacionamiento)
        {
            try
            {
                MLugarDireccion lugarDireccion = ObtenerLugarDireccionBD(estacionamiento.iCajon, estacionamiento.sLugar);
                CambiarEstatusLugaresBD(lugarDireccion.iLugar, lugarDireccion.sLugar);

                int userResponsable = AgregarUsuariosEmpleadosResponsablesBD(estacionamiento.iUsuario, estacionamiento.nombreUsuario, estacionamiento.correoUsuario);

                int registroUsuario = AgregarUsuariosVisitantesBD(estacionamiento.nombreVisitante, userResponsable);
                AgregarAsignacionEstacionamientoVisitantesBD(estacionamiento.iCajon, lugarDireccion.uiRegistroLugar, registroUsuario, estacionamiento.sObservaciones, estacionamiento.dtFechaVisita, estacionamiento.horaVisita, estacionamiento.horaSalida, estacionamiento.sPlaca, estacionamiento.sModelo, estacionamiento.sColor);
                // Log de asignación (visitante)
                LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.AsignacionSolicitud, lugarDireccion.iLugar, lugarDireccion.uiRegistroLugar);

                return Json(new { error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private List<MEstacionamiento> ListaEstacionamientosAsignados()
        {
            List<MEstacionamiento> listaEstacionamientos = new List<MEstacionamiento>();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand("sp_MostrarAsignacionesEstacionamiento", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            MEstacionamiento estacionamiento = new MEstacionamiento();
                            estacionamiento.uiRegistroCajones = Convert.ToInt32(r["uiRegistroCajones"]);
                            estacionamiento.dtFechaAsignacion = Convert.ToDateTime(r["dtFechaAsignacion"]);
                            estacionamiento.sObservaciones = Convert.ToString(r["sObservaciones"]);
                            estacionamiento.Lugares = new MLugares();
                            estacionamiento.Lugares.sLugar = Convert.ToString(r["sLugar"]);
                            estacionamiento.Usuarios = new MUsuarios();
                            estacionamiento.Usuarios.sNombre = Convert.ToString(r["sNombre"]);
                            estacionamiento.Usuarios.uiNumeroEmpleado = Convert.ToInt32(r["uiNumeroEmpleado"]);
                            estacionamiento.Usuarios.sRoleUsuarios = Convert.ToString(r["sRol"]);
                            estacionamiento.Usuarios.sCorreo = Convert.ToString(r["sCorreo"]);
                            estacionamiento.LugarDireccion = new MLugarDireccion();
                            estacionamiento.LugarDireccion.sLugar = Convert.ToString(r["LugarDireccion"]);
                            listaEstacionamientos.Add(estacionamiento);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {

                }
            }

            return listaEstacionamientos;
        }

        public JsonResult MostrarAsignacionesEstacionamientos_AJAX()
        {
            try
            {
                List<MEstacionamiento> listaEstacionamientos = ListaEstacionamientosAsignados();

                return Json(new { error = false, listaEstacionamientos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Listado de asignaciones internas con filtros opcionales por texto y rango de fechas
        public JsonResult MostrarAsignacionesEstacionamientosFiltrado_AJAX(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            try
            {
                var lista = ListaEstacionamientosAsignados();

                // Normalizar texto
                string q = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim().ToLowerInvariant();

                DateTime? fDesde = null, fHasta = null;
                DateTime tmp;
                if (!string.IsNullOrWhiteSpace(fechaDesde) && DateTime.TryParse(fechaDesde, out tmp)) fDesde = tmp.Date;
                if (!string.IsNullOrWhiteSpace(fechaHasta) && DateTime.TryParse(fechaHasta, out tmp)) fHasta = tmp.Date;

                if (fDesde.HasValue)
                    lista = lista.Where(x => x.dtFechaAsignacion.Date >= fDesde.Value).ToList();
                if (fHasta.HasValue)
                    lista = lista.Where(x => x.dtFechaAsignacion.Date <= fHasta.Value).ToList();

                if (!string.IsNullOrEmpty(q))
                {
                    lista = lista.Where(x =>
                        (x.Usuarios != null && (
                            (!string.IsNullOrEmpty(x.Usuarios.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(q)) ||
                            (!string.IsNullOrEmpty(x.Usuarios.sCorreo) && x.Usuarios.sCorreo.ToLowerInvariant().Contains(q)) ||
                            x.Usuarios.uiNumeroEmpleado.ToString().Contains(q)
                        )) ||
                        (x.LugarDireccion != null && !string.IsNullOrEmpty(x.LugarDireccion.sLugar) && x.LugarDireccion.sLugar.ToLowerInvariant().Contains(q)) ||
                        (x.Lugares != null && !string.IsNullOrEmpty(x.Lugares.sLugar) && x.Lugares.sLugar.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sObservaciones) && x.sObservaciones.ToLowerInvariant().Contains(q))
                    ).ToList();
                }

                return Json(new { error = false, listaEstacionamientos = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private List<MEstacionamiento> ListaEstacionamientosAsignadosVisitantes()
        {
            List<MEstacionamiento> listaEstacionamientos = new List<MEstacionamiento>();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand("sp_MostrarAsignacionesEstacionamientoVisitantes", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            MEstacionamiento estacionamiento = new MEstacionamiento();
                            estacionamiento.uiRegistroCajones = Convert.ToInt32(r["uiRegistroCajones"]);
                            estacionamiento.horaVisita = Convert.ToString(r["horaVisita"]);
                            estacionamiento.horaSalida = Convert.ToString(r["horaSalida"]);
                            estacionamiento.dtFechaAsignacion = Convert.ToDateTime(r["dtFechaAsignacion"]);
                            estacionamiento.dtFechaVisita = Convert.ToDateTime(r["dtFechaVisita"]);
                            estacionamiento.sObservaciones = Convert.ToString(r["sObservaciones"]);
                            estacionamiento.Lugares = new MLugares();
                            estacionamiento.Lugares.sLugar = Convert.ToString(r["sLugar"]);
                            estacionamiento.Usuarios = new MUsuarios();
                            estacionamiento.Usuarios.sNombre = Convert.ToString(r["sNombre"]);
                            estacionamiento.Usuarios.sRoleUsuarios = Convert.ToString(r["sRol"]);
                            //estacionamiento.Usuarios.idUserResponsables = Convert.ToInt32(r["idUserResponsables"]);
                            estacionamiento.Usuarios.nombreResponsable = Convert.ToString(r["NombreResponsable"]);
                            estacionamiento.LugarDireccion = new MLugarDireccion();
                            estacionamiento.LugarDireccion.sLugar = Convert.ToString(r["LugarDireccion"]);
                            estacionamiento.sPlaca = Convert.ToString(r["sPlaca"]);
                            estacionamiento.sModelo = Convert.ToString(r["sModelo"]);
                            estacionamiento.sColor = Convert.ToString(r["sColor"]);
                            listaEstacionamientos.Add(estacionamiento);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {

                }
            }

            return listaEstacionamientos;
        }

        public JsonResult MostrarAsignacionesEstacionamientosVisitantes_AJAX()
        {
            try
            {
                List<MEstacionamiento> listaEstacionamientos = ListaEstacionamientosAsignadosVisitantes();

                return Json(new { error = false, listaEstacionamientos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Listado de asignaciones de visitantes con filtros por texto y rango de fechas (fecha de visita)
        public JsonResult MostrarAsignacionesEstacionamientosVisitantesFiltrado_AJAX(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            try
            {
                var lista = ListaEstacionamientosAsignadosVisitantes();

                // Normalizar texto
                string q = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim().ToLowerInvariant();

                DateTime? fDesde = null, fHasta = null;
                DateTime tmp;
                if (!string.IsNullOrWhiteSpace(fechaDesde) && DateTime.TryParse(fechaDesde, out tmp)) fDesde = tmp.Date;
                if (!string.IsNullOrWhiteSpace(fechaHasta) && DateTime.TryParse(fechaHasta, out tmp)) fHasta = tmp.Date;

                if (fDesde.HasValue)
                    lista = lista.Where(x => x.dtFechaVisita.Date >= fDesde.Value).ToList();
                if (fHasta.HasValue)
                    lista = lista.Where(x => x.dtFechaVisita.Date <= fHasta.Value).ToList();

                if (!string.IsNullOrEmpty(q))
                {
                    lista = lista.Where(x =>
                        (x.Usuarios != null && (
                            (!string.IsNullOrEmpty(x.Usuarios.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(q)) ||
                            (x.Usuarios.idUserResponsables.ToString().ToLowerInvariant().Contains(q))
                        )) ||
                        (x.LugarDireccion != null && !string.IsNullOrEmpty(x.LugarDireccion.sLugar) && x.LugarDireccion.sLugar.ToLowerInvariant().Contains(q)) ||
                        (x.Lugares != null && !string.IsNullOrEmpty(x.Lugares.sLugar) && x.Lugares.sLugar.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sPlaca) && x.sPlaca.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sModelo) && x.sModelo.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sColor) && x.sColor.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sObservaciones) && x.sObservaciones.ToLowerInvariant().Contains(q))
                    ).ToList();
                }

                return Json(new { error = false, listaEstacionamientos = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Filtros avanzados para pantalla de visitantes (Administrador y Seguridad)
        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador", "Seguridad")]
        public JsonResult MostrarAsignacionesVisitantesConFiltros_AJAX(
            string fechaVisitaDesde = null,
            string fechaVisitaHasta = null,
            string fechaAsignacionDesde = null,
            string fechaAsignacionHasta = null,
            string nombreResponsable = null,
            string nombreVisitante = null,
            string horaVisitaDesde = null,
            string horaVisitaHasta = null,
            string horaSalidaDesde = null,
            string horaSalidaHasta = null,
            string direccion = null,
            string lugar = null,
            string placa = null)
        {
            try
            {
                var lista = ListaEstacionamientosAsignadosVisitantes();
                var filtros = new Estacionamiento.Helpers.VisitanteFiltros
                {
                    FechaVisitaDesde = fechaVisitaDesde,
                    FechaVisitaHasta = fechaVisitaHasta,
                    FechaAsignacionDesde = fechaAsignacionDesde,
                    FechaAsignacionHasta = fechaAsignacionHasta,
                    NombreResponsable = nombreResponsable,
                    NombreVisitante = nombreVisitante,
                    HoraVisitaDesde = horaVisitaDesde,
                    HoraVisitaHasta = horaVisitaHasta,
                    HoraSalidaDesde = horaSalidaDesde,
                    HoraSalidaHasta = horaSalidaHasta,
                    Direccion = direccion,
                    Lugar = lugar,
                    Placa = placa
                };

                var filtrada = Estacionamiento.Helpers.VisitantesFiltroHelper.Aplicar(lista, filtros);
                return Json(new { error = false, listaEstacionamientos = filtrada }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]

        private MEstacionamiento ObtenerEstacionamientoLiberaBD(int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from cajonesEstacionamientos where uiRegistroCajones = {0}", registroCajon), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            DataRow r = dt.Rows[0];
            MEstacionamiento estacionamiento = new MEstacionamiento(r);

            return estacionamiento;
        }

        [NonAction]
        private void LiberarCajonEstacionamientoBD(int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_UpdateEstacionamiento", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroCajones", Value = registroCajon });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se liberó cajón de estacionamiento");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [NonAction]
        private void LiberarCajonLugarEstacionamientoBD(int registroLugar)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_UpdateLugaresEstacionamiento", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroLugar", Value = registroLugar });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se liberó lugar de estacionamiento");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [NonAction]
        private void LiberarUsuarioEstacionamiento(int idUser)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_UpdateUsuarios", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@IdUsuarios", Value = idUser });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se desactivo el Usuario");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }


        public JsonResult LiberarCajon_AJAX(int registroCajon)
        {
            try
            {
                MEstacionamiento estacionamiento = ObtenerEstacionamientoLiberaBD(registroCajon);

                LiberarCajonEstacionamientoBD(estacionamiento.uiRegistroCajones);
                LiberarCajonLugarEstacionamientoBD(estacionamiento.iLugar);
                LiberarUsuarioEstacionamiento(estacionamiento.iUsuario);
                // Log de liberación, resolviendo dirección y cajón desde iLugar (uiRegistroLugar)
        // Log de liberación/Rechazo
        try { var lugar = ObtenerCajonActualBD(estacionamiento.iLugar); LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.LiberacionCajon, lugar.iLugar, lugar.uiRegistroLugar); } catch { }

                return Json(new { error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]

        private void EditarHoraSalidaBD(string horaSalida, int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_UpdateHoraSalida", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@horaSalida", Size = 50, Value = horaSalida });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroCajones", Value = registroCajon });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se editó la hora de Salida");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        public JsonResult EditarHoraSalida_AJAX(MEstacionamiento estacionamiento)
        {
            try
            {
                EditarHoraSalidaBD(estacionamiento.horaSalida, estacionamiento.uiRegistroCajones);
                // Log de edición de hora de salida
                try
                {
                    var datos = ObtenerEstacionamientoLiberaBD(estacionamiento.uiRegistroCajones);
                    var lugar = ObtenerCajonActualBD(datos.iLugar);
                    LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.EdicionHoraSalida, lugar.iLugar, lugar.uiRegistroLugar);
                }
                catch { }

                return Json(new { error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private void LiberarCajonesDiariosBD()
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_LiberarCajonesDiarios", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.CommandTimeout = 120;
                        comando.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [HttpPost]
        [Authorize]
        [RoleAuthorize("Administrador")]
        public JsonResult LiberarCajonesDiarios_AJAX()
        {
            try
            {
                LiberarCajonesDiariosBD();
                return Json(new { error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region AltaCajonesVisitantes
        //[Authorize]
        //[RoleAuthorize("Solicitante")]
        public ActionResult AltaCajonVisitantes()
        {
            MUsuarios us = new MUsuarios();
            MUsuarios user = (MUsuarios)Session["log"];
            us = ValidarExistenciaUsuarioSolicitanteBD(user.uiNumeroEmpleado);

            // Completar jefe desde HCM (método existente)
            string jefe = CompletaHCM(user.uiNumeroEmpleado);
            user.JefeDirecto = jefe;

            if (us.IdUsuarios != 0)
            {
                try
                {
                    ActualizarUsuariosEmpleadosBD(us.IdUsuarios, user.uiNumeroEmpleado, user.sNombre, user.sCorreo, user.Puesto, user.JefeDirecto, user.UEN);
                }
                catch
                {
                    // No bloquear la vista si falla la actualización
                }
            }
            else
            {
                // Usuario NO EXISTE: crear nuevo con rol 2 y todos los datos (número de empleado, nombre, correo, Puesto, Jefe, UEN)
                try
                {
                    AgregarUsuariosSolicitanteBD(user.uiNumeroEmpleado, user.sNombre, user.sCorreo, user.Puesto, user.JefeDirecto, user.UEN);
                    us = ValidarExistenciaUsuarioSolicitanteBD(user.uiNumeroEmpleado);
                }
                catch (Exception)
                {
                    // No detener la vista si la inserción falla; se puede loggear si es necesario
                }
            }
            user.IdUsuarios = us.IdUsuarios;

            return View();
        }

        [NonAction]

        private string CompletaHCM(int numEmp)
        {
            string numeEmpleadoFAST = "";
            string evaluarNumeEmpleado = "";
            string numJefe = "";
            string nomJefe = "";


            if (numEmp <= 19880)
            {
                evaluarNumeEmpleado = Convert.ToString(numEmp);
                numeEmpleadoFAST = evaluarNumeEmpleado.PadLeft(6, '0');

            }
            else
            {
                evaluarNumeEmpleado = Convert.ToString(numEmp);
                numeEmpleadoFAST = evaluarNumeEmpleado;
            }

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls12;

            HCM.HeaderMessage headerMess = new HCM.HeaderMessage();
            // Request
            HCM.ObtenerFastInputType fastInput = new HCM.ObtenerFastInputType();
            // Response
            HCM.ObtenerFastOutputType fastOut = new HCM.ObtenerFastOutputType();
            // Cliente
            HCM.EmpleadosPortTypeClient cliente = new HCM.EmpleadosPortTypeClient();
            // Encabezado
            HCM.HeaderMessage header = new HCM.HeaderMessage();

            try
            {
                #region Información empleado

                fastInput.personNumber = numeEmpleadoFAST;
                cliente.obtenerFast(header, fastInput, out fastOut);

                //numEmpleado = fastOut.empleado.informacionBasica.idEmpleado;
                //nombreEmpleado = fastOut.empleado.informacionBasica.nombre + " " + fastOut.empleado.informacionBasica.apellidoPaterno + " " + fastOut.empleado.informacionBasica.apellidoMaterno;

                //MUsuarios user = ObtenerCorreosEmpleado(numEmpleado);
                //correoEmpleado = user.sCorreo;
                #endregion

                numJefe = fastOut.empleado.aprobador1.idEmpleado;
                nomJefe = fastOut.empleado.aprobador1.nombre + " " + fastOut.empleado.aprobador1.apellidoPaterno + " " + fastOut.empleado.aprobador1.apellidoMaterno;

                return nomJefe;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [NonAction]
        private MUsuarios ValidarExistenciaUsuarioSolicitanteBD(int numeroEmpleado)
        {
            MUsuarios user = new MUsuarios();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand(string.Format("Select * from usuarios where uiNumeroEmpleado = {0} and (iRol = 2 or iRol = 1) ORDER BY IdUsuarios", numeroEmpleado), conexionSQL))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            DataRow r = dt.Rows[0];
                            user = new MUsuarios(r);
                        }
                        else
                        {
                            user = new MUsuarios();
                        }
                    }
                }
                catch (Exception ex)
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }

            return user;
        }


        // Valida que las solicitudes solo se realicen entre 08:00 y 17:30 (hora del servidor)
        [NonAction]
        private void ValidarHorarioSolicitudes()
        {
            TimeSpan ahora = DateTime.Now.TimeOfDay;
            TimeSpan inicio = new TimeSpan(8, 0, 0);   // 08:00
            TimeSpan fin = new TimeSpan(17, 30, 0);    // 17:30

            if (ahora < inicio || ahora > fin)
            {
                throw new ApplicationException("Las solicitudes solo pueden hacerse entre 08:00 y 17:30 horas.");
            }
        }


        [NonAction]

        private bool ValidarUsuarioVisitanteEstacionamiento(string nombre)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from usuarios where sNombre = '{0}'", nombre), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            bool validaUser = false;

            if (dt.Rows.Count == 0)
            {
                validaUser = false;
            }
            else
            {
                validaUser = true;
            }

            return validaUser;
        }

        [NonAction]

        private bool ValidaEstacionamientoVisitaBD(string fechaVisita, string horaEntrada, string horaSalida)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from cajonesEstacionamientos where dtFechaVisita = '{0}' and horaVisita = '{1}' and horaSalida = '{2}'", fechaVisita, horaEntrada, horaSalida), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            bool validaEstaciona = false;

            if (dt.Rows.Count == 0)
            {
                validaEstaciona = false;
            }
            else
            {
                validaEstaciona = true;
            }

            return validaEstaciona;
        }

        [NonAction]

        private void ValidacionSolicitudesUusariosBD(int numEmpleado)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand("sp_ValidarSolicitudesActivasUser", conexionSQL);
            comando.CommandType = CommandType.StoredProcedure;

            comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiNumeroEmpleado", Value = numEmpleado });

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count >= 3)
            {
                throw new ApplicationException("Ha alcanzado el máximo de solicitudes (3), debe esperar a que se libere al menos una.");
            }
            else
            {

            }
        }

        // Valida que el usuario responsable solo pueda generar una solicitud por día
        [NonAction]
        private void ValidarSolicitudDiariaResponsable(int numEmpleado, DateTime fecha)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            using (SqlCommand comando = new SqlCommand(@"SELECT COUNT(*)
                                                        FROM cajonesEstacionamientos CE
                                                        INNER JOIN usuarios UVIS ON CE.iUsuario = UVIS.IdUsuarios
                                                        INNER JOIN usuarios URESP ON UVIS.idUserResponsables = URESP.IdUsuarios
                                                        WHERE URESP.iRol = 2 -- Visitante
                                                        AND URESP.uiNumeroEmpleado = @uiNumeroEmpleado
                                                        AND CE.bActivo = 1", conexionSQL))
            {
                comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiNumeroEmpleado", Value = numEmpleado });

                conexionSQL.Open();
                int conteo = Convert.ToInt32(comando.ExecuteScalar());
                if (conteo > 0)
                {
                    throw new ApplicationException("Ya existe una solicitud registrada para ese responsable en la fecha indicada. Solo se permite una por día.");
                }
                else
                {
                    bool esDiaDeSemana = (fecha.DayOfWeek == DayOfWeek.Saturday) ||
                         (fecha.DayOfWeek == DayOfWeek.Sunday);

                    if (esDiaDeSemana)
                    {
                        throw new ApplicationException("No se pueden generar solicitudes los días Sábados ni Domingos");
                    }
                    else
                    {
                        string fechaC = fecha.ToString("yyyy-MM-dd");
                        ValidacionDiasFestivos(fechaC);
                    }

                }
            }
        }

        [NonAction]
        private void ValidacionDiasFestivos(string dia)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from diasFestivos where dtDia = '{0}'", dia), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                throw new ApplicationException("No se pueden generar solicitudes en días festivos");
            }
            else
            {

            }
        }

        public JsonResult AsignarEstacionamientoVisitanteUsuario_AJAX(MEstacionamiento estacionamiento)
        {
            try
            {
                MUsuarios user = (MUsuarios)Session["log"];

                if (user.iRol != 1)
                {
                    // Restringir horario de solicitudes (08:00 - 17:30)
                    ValidarHorarioSolicitudes();

                    //ValidacionSolicitudesUusariosBD(estacionamiento.iUsuario);

                    // Regla: solo una solicitud por día por responsable
                    DateTime fechaHoy = DateTime.Now;
                    ValidarSolicitudDiariaResponsable(user.uiNumeroEmpleado, fechaHoy);
                }

                bool validaUser = ValidarUsuarioVisitanteEstacionamiento(estacionamiento.nombreVisitante);
                int registroCajon = 0;

                if (validaUser == true)
                {
                    string fechaVisita = estacionamiento.dtFechaVisita.ToString("yyyy-MM-dd");

                    bool validaEstacionamiento = ValidaEstacionamientoVisitaBD(fechaVisita, estacionamiento.horaVisita, estacionamiento.horaSalida);

                    if (validaEstacionamiento == true)
                    {
                        throw new ApplicationException("Ya existe una solicitud con estos datos.");
                    }
                    else
                    {
                        //int userResponsable = AgregarUsuariosEmpleadosResponsablesBD(estacionamiento.iUsuario, estacionamiento.nombreUsuario, estacionamiento.correoUsuario, out passResp);

                        int registroUsuario = AgregarUsuariosVisitantesBD(estacionamiento.nombreVisitante, user.IdUsuarios);
                        registroCajon = AgregarAsignacionEstacionamientoVisitantesBD(
                            estacionamiento.iCajon,
                            estacionamiento.iLugar,
                            registroUsuario,
                            estacionamiento.sObservaciones,
                            estacionamiento.dtFechaVisita,
                            estacionamiento.horaVisita,
                            estacionamiento.horaSalida,
                            estacionamiento.sPlaca,
                            estacionamiento.sModelo,
                            estacionamiento.sColor,
                            estacionamiento.bInterno,
                            estacionamiento.bExterno);
                        // Log de generación de solicitud
                        try
                        {
                            int? iLugar = null; int? uiRegistroLugar = null;
                            if (estacionamiento.iLugar > 0) { var lugar = ObtenerCajonActualBD(estacionamiento.iLugar); iLugar = lugar.iLugar; uiRegistroLugar = lugar.uiRegistroLugar; }
                            LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.GeneracionSolicitud, iLugar, uiRegistroLugar);
                        }
                        catch { }
                    }
                }
                else
                {
                    //int userResponsable = AgregarUsuariosEmpleadosResponsablesBD(estacionamiento.iUsuario, estacionamiento.nombreUsuario, estacionamiento.correoUsuario, out passResp2);
                    int registroUsuario = AgregarUsuariosVisitantesBD(estacionamiento.nombreVisitante, user.IdUsuarios);
                    registroCajon = AgregarAsignacionEstacionamientoVisitantesBD(
                        estacionamiento.iCajon,
                        estacionamiento.iLugar,
                        registroUsuario,
                        estacionamiento.sObservaciones,
                        estacionamiento.dtFechaVisita,
                        estacionamiento.horaVisita,
                        estacionamiento.horaSalida,
                        estacionamiento.sPlaca,
                        estacionamiento.sModelo,
                        estacionamiento.sColor,
                        estacionamiento.bInterno,
                        estacionamiento.bExterno);
                    // Log de generación de solicitud
                    try
                    {
                        int? iLugar = null; int? uiRegistroLugar = null;
                        if (estacionamiento.iLugar > 0) { var lugar = ObtenerCajonActualBD(estacionamiento.iLugar); iLugar = lugar.iLugar; uiRegistroLugar = lugar.uiRegistroLugar; }
                        LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.GeneracionSolicitud, iLugar, uiRegistroLugar);
                    }
                    catch { }
                }

                return Json(new { error = false, registroCajon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        [NonAction]
        private List<MEstacionamiento> ListaEstacionamientosAsignadosVisitantesSinLugar()
        {
            List<MEstacionamiento> listaEstacionamientos = new List<MEstacionamiento>();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand("sp_MostrarAsignacionesEstacionamientoVisitantesSinLugar", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            MEstacionamiento estacionamiento = new MEstacionamiento();
                            estacionamiento.uiRegistroCajones = Convert.ToInt32(r["uiRegistroCajones"]);
                            estacionamiento.horaVisita = Convert.ToString(r["horaVisita"]);
                            estacionamiento.horaSalida = Convert.ToString(r["horaSalida"]);
                            estacionamiento.dtFechaAsignacion = Convert.ToDateTime(r["dtFechaAsignacion"]);
                            estacionamiento.dtFechaVisita = Convert.ToDateTime(r["dtFechaVisita"]);
                            estacionamiento.sObservaciones = Convert.ToString(r["sObservaciones"]);
                            estacionamiento.Lugares = new MLugares();
                            estacionamiento.Lugares.sLugar = Convert.ToString(r["sLugar"]);

                            if (estacionamiento.Lugares.sLugar == "")
                            {
                                estacionamiento.Lugares.sLugar = "S/A";
                            }
                            else
                            {
                                estacionamiento.Lugares.sLugar = Convert.ToString(r["sLugar"]);
                            }

                            estacionamiento.Usuarios = new MUsuarios();
                            estacionamiento.Usuarios.sNombre = Convert.ToString(r["sNombre"]);
                            estacionamiento.Usuarios.sRoleUsuarios = Convert.ToString(r["sRol"]);
                            estacionamiento.Usuarios.nombreResponsable = Convert.ToString(r["NombreResponsable"]);
                            estacionamiento.LugarDireccion = new MLugarDireccion();
                            estacionamiento.LugarDireccion.sLugar = Convert.ToString(r["LugarDireccion"]);

                            if (estacionamiento.LugarDireccion.sLugar == "")
                            {
                                estacionamiento.LugarDireccion.sLugar = "S/A";
                            }
                            else
                            {
                                estacionamiento.LugarDireccion.sLugar = Convert.ToString(r["LugarDireccion"]);
                            }

                            estacionamiento.sPlaca = Convert.ToString(r["sPlaca"]);
                            estacionamiento.sModelo = Convert.ToString(r["sModelo"]);
                            estacionamiento.sColor = Convert.ToString(r["sColor"]);
                            listaEstacionamientos.Add(estacionamiento);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {

                }
            }

            return listaEstacionamientos;
        }

        public JsonResult MostrarAsignacionesEstacionamientosVisitantesSinLugar_AJAX()
        {
            try
            {
                List<MEstacionamiento> listaEstacionamientos = ListaEstacionamientosAsignadosVisitantesSinLugar();

                return Json(new { error = false, listaEstacionamientos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Listado de solicitudes de visitantes por asignar con filtros por texto y rango de fechas (fecha de visita)
        public JsonResult MostrarAsignacionesEstacionamientosVisitantesSinLugarFiltrado_AJAX(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            try
            {
                var lista = ListaEstacionamientosAsignadosVisitantesSinLugar();

                // Normalizar texto
                string q = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim().ToLowerInvariant();

                DateTime? fDesde = null, fHasta = null;
                DateTime tmp;
                if (!string.IsNullOrWhiteSpace(fechaDesde) && DateTime.TryParse(fechaDesde, out tmp)) fDesde = tmp.Date;
                if (!string.IsNullOrWhiteSpace(fechaHasta) && DateTime.TryParse(fechaHasta, out tmp)) fHasta = tmp.Date;

                if (fDesde.HasValue)
                    lista = lista.Where(x => x.dtFechaVisita.Date >= fDesde.Value).ToList();
                if (fHasta.HasValue)
                    lista = lista.Where(x => x.dtFechaVisita.Date <= fHasta.Value).ToList();

                if (!string.IsNullOrEmpty(q))
                {
                    lista = lista.Where(x =>
                        (x.Usuarios != null && (
                            (!string.IsNullOrEmpty(x.Usuarios.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(q)) ||
                            (!string.IsNullOrEmpty(x.Usuarios.nombreResponsable) && x.Usuarios.nombreResponsable.ToLowerInvariant().Contains(q))
                        )) ||
                        (x.LugarDireccion != null && !string.IsNullOrEmpty(x.LugarDireccion.sLugar) && x.LugarDireccion.sLugar.ToLowerInvariant().Contains(q)) ||
                        (x.Lugares != null && !string.IsNullOrEmpty(x.Lugares.sLugar) && x.Lugares.sLugar.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sPlaca) && x.sPlaca.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sModelo) && x.sModelo.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sColor) && x.sColor.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sObservaciones) && x.sObservaciones.ToLowerInvariant().Contains(q))
                    ).ToList();
                }

                return Json(new { error = false, listaEstacionamientos = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Filtros avanzados empleados
        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador", "Seguridad")]
        public JsonResult MostrarAsignacionesEmpleadosConFiltros_AJAX(
            string nombreEmpleado = null,
            string direccion = null,
            string lugar = null,
            string fechaAsignacionDesde = null,
            string fechaAsignacionHasta = null)
        {
            try
            {
                var lista = ListaEstacionamientosAsignados();
                var filtros = new Estacionamiento.Helpers.EmpleadoFiltros
                {
                    NombreEmpleado = nombreEmpleado,
                    Direccion = direccion,
                    Lugar = lugar,
                    FechaAsignacionDesde = fechaAsignacionDesde,
                    FechaAsignacionHasta = fechaAsignacionHasta
                };
                var filtrada = Estacionamiento.Helpers.EmpleadosFiltroHelper.Aplicar(lista, filtros);
                return Json(new { error = false, listaEstacionamientos = filtrada }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Filtros avanzados asignaciones visitantes sin lugar (solicitudes)
        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador", "Seguridad")]
        public JsonResult MostrarAsignacionesVisitantesSinLugarConFiltros_AJAX(
            string usuarioResponsable = null,
            string nombreVisitante = null,
            string horaVisitaDesde = null,
            string horaVisitaHasta = null,
            string horaSalidaDesde = null,
            string horaSalidaHasta = null,
            string placa = null,
            string fechaVisitaDesde = null,
            string fechaVisitaHasta = null)
        {
            try
            {
                var lista = ListaEstacionamientosAsignadosVisitantesSinLugar();
                var filtros = new Estacionamiento.Helpers.AsignacionesVisitantesFiltros
                {
                    UsuarioResponsable = usuarioResponsable,
                    NombreVisitante = nombreVisitante,
                    HoraVisitaDesde = horaVisitaDesde,
                    HoraVisitaHasta = horaVisitaHasta,
                    HoraSalidaDesde = horaSalidaDesde,
                    HoraSalidaHasta = horaSalidaHasta,
                    Placa = placa,
                    FechaVisitaDesde = fechaVisitaDesde,
                    FechaVisitaHasta = fechaVisitaHasta
                };
                var filtrada = Estacionamiento.Helpers.AsignacionesVisitantesFiltroHelper.Aplicar(lista, filtros);
                return Json(new { error = false, listaEstacionamientos = filtrada }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        private void AsignarCajonVisitanteBD(int cajon, int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_UpdateAsignarCajon", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iCajon", Value = cajon });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroCajones", Value = registroCajon });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se asignó el Cajón al Visitante");
                        }

                        // Actualizar la fecha de asignación al momento de asignar el cajón
                        using (SqlCommand cmdUpdate = new SqlCommand("UPDATE cajonesEstacionamientos SET dtFechaAsignacion = GETDATE() WHERE uiRegistroCajones = @uiRegistroCajones", conexionSQL))
                        {
                            cmdUpdate.Parameters.Add(new SqlParameter("@uiRegistroCajones", SqlDbType.Int) { Value = registroCajon });
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        [NonAction]
        private void AsignarCajonLugarVisitanteBD(int lugar, int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_UpdateAsignarCajonLugar", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@iLugar", Value = lugar });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroCajones", Value = registroCajon });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No se asignó el Lugar al Visitante");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        public JsonResult AsignarCajonVisitante_AJAX(MEstacionamiento estacionamiento)
        {
            try
            {
                MLugarDireccion lugarDireccion = ObtenerLugarDireccionBD(estacionamiento.iCajon, estacionamiento.sLugar);
                CambiarEstatusLugaresBD(lugarDireccion.iLugar, lugarDireccion.sLugar);

                AsignarCajonVisitanteBD(estacionamiento.iCajon, estacionamiento.uiRegistroCajones);
                AsignarCajonLugarVisitanteBD(lugarDireccion.uiRegistroLugar, estacionamiento.uiRegistroCajones);
                // Log de cambio de cajón
        LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.AsignacionSolicitud, lugarDireccion.iLugar, lugarDireccion.uiRegistroLugar);
                return Json(new { error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ValidarFechaHoy_AJAX(string fecha)
        {
            try
            {
                bool validarFecha = false;

                string fechaActual = DateTime.Now.ToString("dd/MM/yyyy");
                DateTime fechaTimer = Convert.ToDateTime(fecha);
                string fechaVisita = fechaTimer.ToString("dd/MM/yyyy");

                if (fechaActual == fechaVisita)
                {
                    validarFecha = true;
                }
                else
                {
                    validarFecha = false;
                }

                return Json(new { error = false, validarFecha }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Estatus Solicitud


        [NonAction]

        private MEstacionamiento ObtenerSolicitudEstacionamientoBD(int solicitud)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from cajonesEstacionamientos where uiRegistroCajones = {0}", solicitud), conexionSQL);
            SqlDataAdapter da = new SqlDataAdapter(comando);

            DataTable dt = new DataTable();
            da.Fill(dt);
            MEstacionamiento estacionamiento = new MEstacionamiento();

            if (dt.Rows.Count == 0)
            {
                throw new ApplicationException(string.Format("La solicitud: {0} ya no se encuentra disponible.", solicitud));
            }
            else
            {
                DataRow r = dt.Rows[0];
                estacionamiento = new MEstacionamiento(r);
                estacionamiento.bActivo = Convert.ToBoolean(r["bActivo"]);
            }

            return estacionamiento;
        }

        [NonAction]
        private List<MEstacionamiento> ListaEstacionamientosAsignadosVisitantesUnico(int registroCajon)
        {
            List<MEstacionamiento> listaEstacionamientos = new List<MEstacionamiento>();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand("sp_MostrarAsignacionesEstacionamientoVisitantesUnico", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroCajones", Value = registroCajon });

                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow r in dt.Rows)
                        {
                            MEstacionamiento estacionamiento = new MEstacionamiento();
                            estacionamiento.uiRegistroCajones = Convert.ToInt32(r["uiRegistroCajones"]);
                            estacionamiento.horaVisita = Convert.ToString(r["horaVisita"]);
                            estacionamiento.horaSalida = Convert.ToString(r["horaSalida"]);
                            estacionamiento.dtFechaAsignacion = Convert.ToDateTime(r["dtFechaAsignacion"]);
                            estacionamiento.dtFechaVisita = Convert.ToDateTime(r["dtFechaVisita"]);
                            estacionamiento.sObservaciones = Convert.ToString(r["sObservaciones"]);
                            estacionamiento.Lugares = new MLugares();
                            estacionamiento.Lugares.sLugar = Convert.ToString(r["sLugar"]);
                            estacionamiento.Usuarios = new MUsuarios();
                            estacionamiento.Usuarios.sNombre = Convert.ToString(r["sNombre"]);
                            estacionamiento.Usuarios.sRoleUsuarios = Convert.ToString(r["sRol"]);
                            estacionamiento.Usuarios.nombreResponsable = Convert.ToString(r["NombreResponsable"]);
                            estacionamiento.LugarDireccion = new MLugarDireccion();
                            estacionamiento.LugarDireccion.sLugar = Convert.ToString(r["LugarDireccion"]);
                            estacionamiento.sPlaca = Convert.ToString(r["sPlaca"]);
                            estacionamiento.sModelo = Convert.ToString(r["sModelo"]);
                            estacionamiento.sColor = Convert.ToString(r["sColor"]);
                            listaEstacionamientos.Add(estacionamiento);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                finally
                {

                }
            }

            return listaEstacionamientos;
        }

        public JsonResult ComprobarSolicitud_AJAX(int solicitud)
        {
            try
            {
                string estatus = "";
                List<MEstacionamiento> listaEstacionamiento = new List<MEstacionamiento>();

                MEstacionamiento estacionamiento = ObtenerSolicitudEstacionamientoBD(solicitud);

                if (estacionamiento.iCajon == 0 && estacionamiento.iLugar == 0)
                {
                    estatus = "Tu solicitud está en proceso";
                }

                else if (estacionamiento.iCajon != 0 && estacionamiento.iLugar != 0 && estacionamiento.bActivo == true)
                {
                    estatus = "Solicitud Asignada, verifique su correo para más detalles";

                    listaEstacionamiento = ListaEstacionamientosAsignadosVisitantesUnico(estacionamiento.uiRegistroCajones);
                }
                else
                {
                    estatus = string.Format("La solicitud: {0} ya no se encuentra activa", solicitud);
                }


                return Json(new { error = false, estatus, listaEstacionamiento }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { mensaje = ex.Message, error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Eliminar Solicitud

        [NonAction]

        private MUsuarios ObtenerUsuarioEstacionamientoBD(int registroUser)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from usuarios where IdUsuarios = {0}", registroUser), conexionSQL);
            SqlDataAdapter da = new SqlDataAdapter(comando);

            DataTable dt = new DataTable();
            da.Fill(dt);

            DataRow r = dt.Rows[0];
            MUsuarios user = new MUsuarios(r);

            if (r["idUserResponsables"] != DBNull.Value)
            {
                user.idUserResponsables = Convert.ToInt32(r["idUserResponsables"]);
            }
            return user;
        }

        [NonAction]

        private void EliminarUsuariosEstacionamientoBD(int idUser)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                conexionSQL.Open();
                using (SqlCommand comando = new SqlCommand(string.Format("delete from usuarios where IdUsuarios = {0}", idUser), conexionSQL))
                {
                    try
                    {
                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException(string.Format("No se eliminó el usuario con Id: {0}", idUser));
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                    finally
                    {

                        conexionSQL.Close();
                        conexionSQL.Dispose();
                    }
                }
            }
        }

        [NonAction]

        private void EliminarSolicitudCajonEstacionamientoBD(int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                conexionSQL.Open();

                using (SqlCommand comando = new SqlCommand(string.Format("delete from cajonesEstacionamientos where uiRegistroCajones = {0}", registroCajon), conexionSQL))
                {
                    try
                    {
                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException(string.Format("No se eliminó el registro de estacionamiento con el Id: ", registroCajon));
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                    finally
                    {
                        conexionSQL.Close();
                        conexionSQL.Dispose();
                    }
                }
            }

        }

        [NonAction]
        private void InsertLogEliminaBD(string sUser, string sPlaca, DateTime dtFechaSolicita, DateTime dtFechaVisita)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();
                    using (SqlCommand comando = new SqlCommand("sp_InsertLogElimina", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sUser", Value = sUser });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.VarChar, ParameterName = "@sPlaca", Value = sPlaca });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Date, ParameterName = "@dtFechaSolicita", Value = dtFechaSolicita });
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Date, ParameterName = "@dtFechaVisita", Value = dtFechaVisita });

                        int filas = 0;
                        filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            throw new ApplicationException("No insertó el log para elimiar");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }


        public JsonResult EliminarSolicitud_AJAX(int solicitud)
        {
            try
            {
                MEstacionamiento estacionamiento = ObtenerSolicitudEstacionamientoBD(solicitud);

                MUsuarios usuario = ObtenerUsuarioEstacionamientoBD(estacionamiento.iUsuario);
                EliminarUsuariosEstacionamientoBD(usuario.IdUsuarios);

                MUsuarios usuarioResponsable = ObtenerUsuarioEstacionamientoBD(usuario.idUserResponsables);
                EliminarUsuariosEstacionamientoBD(usuarioResponsable.IdUsuarios);

                InsertLogEliminaBD(usuario.sNombre, estacionamiento.sPlaca, estacionamiento.dtFechaAsignacion, estacionamiento.dtFechaVisita);

                // Log de rechazo (resolver dirección/cajón desde registro de lugar actual si existe)
                try
                {
                    int? iDir = null; int? uiRegLugar = null;
                    if (estacionamiento.iLugar > 0)
                    {
                        var lugar = ObtenerCajonActualBD(estacionamiento.iLugar);
                        iDir = lugar.iLugar;
                        uiRegLugar = lugar.uiRegistroLugar;
                    }
                    LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.RechazoSolicitud, iDir, uiRegLugar);
                }
                catch { }

                EliminarSolicitudCajonEstacionamientoBD(estacionamiento.uiRegistroCajones);

                return Json(new { error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { mensaje = ex.Message, error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Vigilates

        [Authorize]
        [RoleAuthorize("Seguridad", "Administrador")]
        public ActionResult CajonesVisitantes()
        {
            List<MEstacionamiento> listaEstacionamientos = ListaEstacionamientosAsignadosVisitantes();

            return View(listaEstacionamientos);
        }

        #endregion

        public ActionResult ReportesEstacionamientos()
        {
            return View();
        }

        // Export: Asignaciones internas (Excel .xls)
        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador")]
        public FileResult ExportAsignacionesExcel(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            var lista = ListaEstacionamientosAsignados();

            // Aplicar los mismos filtros que el endpoint AJAX
            string q = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim().ToLowerInvariant();
            DateTime? fDesde = null, fHasta = null; DateTime tmp;
            if (!string.IsNullOrWhiteSpace(fechaDesde) && DateTime.TryParse(fechaDesde, out tmp)) fDesde = tmp.Date;
            if (!string.IsNullOrWhiteSpace(fechaHasta) && DateTime.TryParse(fechaHasta, out tmp)) fHasta = tmp.Date;
            if (fDesde.HasValue) lista = lista.Where(x => x.dtFechaAsignacion.Date >= fDesde.Value).ToList();
            if (fHasta.HasValue) lista = lista.Where(x => x.dtFechaAsignacion.Date <= fHasta.Value).ToList();
            if (!string.IsNullOrEmpty(q))
            {
                lista = lista.Where(x =>
                    (x.Usuarios != null && (
                        (!string.IsNullOrEmpty(x.Usuarios.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.Usuarios.sCorreo) && x.Usuarios.sCorreo.ToLowerInvariant().Contains(q)) ||
                        x.Usuarios.uiNumeroEmpleado.ToString().Contains(q)
                    ))
                ).ToList();
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
            sb.Append("<table border='1'>");
            sb.Append("<tr style='background-color:#f2f2f2; font-weight:bold;'>");
            sb.Append("<td>Fecha Asignación</td>");
            sb.Append("<td>Dirección</td>");
            sb.Append("<td>Lugar</td>");
            sb.Append("<td>Nombre</td>");
            sb.Append("<td>Número Empleado</td>");
            sb.Append("<td>Rol</td>");
            sb.Append("<td>Correo</td>");
            sb.Append("<td>Observaciones</td>");
            sb.Append("</tr>");

            foreach (var x in lista)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", x.dtFechaAsignacion.ToString("yyyy-MM-dd HH:mm"));
                sb.AppendFormat("<td>{0}</td>", x.LugarDireccion != null ? x.LugarDireccion.sLugar : "");
                sb.AppendFormat("<td>{0}</td>", x.Lugares != null ? x.Lugares.sLugar : "");
                sb.AppendFormat("<td>{0}</td>", x.Usuarios != null ? x.Usuarios.sNombre : "");
                sb.AppendFormat("<td>{0}</td>", x.Usuarios != null ? x.Usuarios.uiNumeroEmpleado.ToString() : "");
                sb.AppendFormat("<td>{0}</td>", x.Usuarios != null ? x.Usuarios.sRoleUsuarios : "");
                sb.AppendFormat("<td>{0}</td>", x.Usuarios != null ? x.Usuarios.sCorreo : "");
                sb.AppendFormat("<td>{0}</td>", x.sObservaciones ?? "");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "application/vnd.ms-excel", "Asignaciones_Internas.xls");
        }

        // Export: Asignaciones visitantes (Excel .xls)
        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador")]
        public FileResult ExportVisitantesExcel(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            var lista = ListaEstacionamientosAsignadosVisitantes();

            // Usar helper para consistencia con la pantalla de Admin
            var filtros = new Estacionamiento.Helpers.VisitanteFiltros
            {
                Texto = texto,
                FechaVisitaDesde = fechaDesde,
                FechaVisitaHasta = fechaHasta,
            };

            // Nota: VisitantesFiltroHelper.Aplicar ya maneja la logica de fechas y texto
            var filtrada = Estacionamiento.Helpers.VisitantesFiltroHelper.Aplicar(lista, filtros);

            var sb = new System.Text.StringBuilder();
            sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
            sb.Append("<table border='1'>");
            sb.Append("<tr style='background-color:#dff0d8; font-weight:bold;'>"); // Verde claro para visitantes
            sb.Append("<td>Fecha Asignación</td>");
            sb.Append("<td>Fecha Visita</td>");
            sb.Append("<td>Hora Visita</td>");
            sb.Append("<td>Hora Salida</td>");
            sb.Append("<td>Dirección</td>");
            sb.Append("<td>Lugar</td>");
            sb.Append("<td>Nombre Visitante</td>");
            sb.Append("<td>Nombre Responsable</td>");
            sb.Append("<td>Placa</td>");
            sb.Append("<td>Modelo</td>");
            sb.Append("<td>Color</td>");
            sb.Append("<td>Observaciones</td>");
            sb.Append("</tr>");

            foreach (var x in filtrada)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", x.dtFechaAsignacion.ToString("yyyy-MM-dd HH:mm"));
                sb.AppendFormat("<td>{0}</td>", x.dtFechaVisita.ToString("yyyy-MM-dd"));
                sb.AppendFormat("<td>{0}</td>", x.horaVisita ?? "");
                sb.AppendFormat("<td>{0}</td>", x.horaSalida ?? "");
                sb.AppendFormat("<td>{0}</td>", x.LugarDireccion != null ? x.LugarDireccion.sLugar : "");
                sb.AppendFormat("<td>{0}</td>", x.Lugares != null ? x.Lugares.sLugar : "");
                sb.AppendFormat("<td>{0}</td>", x.Usuarios != null && x.Usuarios.sNombre != null ? x.Usuarios.sNombre : "");
                sb.AppendFormat("<td>{0}</td>", x.Usuarios != null && x.Usuarios.nombreResponsable != null ? x.Usuarios.nombreResponsable : "");
                sb.AppendFormat("<td>{0}</td>", x.sPlaca ?? "");
                sb.AppendFormat("<td>{0}</td>", x.sModelo ?? "");
                sb.AppendFormat("<td>{0}</td>", x.sColor ?? "");
                sb.AppendFormat("<td>{0}</td>", x.sObservaciones ?? "");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "application/vnd.ms-excel", "Asignaciones_Visitantes.xls");
        }

        // Utilidad simple para CSV
        private static string EscapeCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            bool mustQuote = s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r");
            if (mustQuote) return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        #region Cambio de cajón

        [NonAction]

        private MLugarDireccion ObtenerCajonActualBD(int registroCajon)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from catNumeroLugares where uiRegistroLugar = {0}", registroCajon), conexionSQL);
            SqlDataAdapter da = new SqlDataAdapter(comando);

            DataTable dt = new DataTable();
            da.Fill(dt);

            DataRow r = dt.Rows[0];
            MLugarDireccion lugar = new MLugarDireccion(r);

            return lugar;
        }

        [NonAction]

        private void InactivarLugarBD(int registroLugar)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();

                    using (SqlCommand comando = new SqlCommand("sp_LiberarLugarActivo", conexionSQL))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.Add(new SqlParameter() { SqlDbType = SqlDbType.Int, ParameterName = "@uiRegistroLugar", Value = registroLugar });

                        int filas = 0;

                        filas = comando.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    conexionSQL.Close();
                    throw;
                }

                finally
                {
                    conexionSQL.Close();
                    conexionSQL.Dispose();
                }
            }
        }

        public JsonResult CambiarCajonVisitante_AJAX(MEstacionamiento estacionamiento)
        {
            try
            {

                #region Liberar Cajón

                MEstacionamiento cajonActual = ObtenerEstacionamientoLiberaBD(estacionamiento.uiRegistroCajones);

                MLugarDireccion lugarActual = ObtenerCajonActualBD(cajonActual.iLugar);

                InactivarLugarBD(lugarActual.uiRegistroLugar);

                #endregion

                #region Asignar cajón

                MLugarDireccion lugarDireccion = ObtenerLugarDireccionBD(estacionamiento.iCajon, estacionamiento.sLugar);

                CambiarEstatusLugaresBD(lugarDireccion.iLugar, lugarDireccion.sLugar);

                AsignarCajonVisitanteBD(estacionamiento.iCajon, estacionamiento.uiRegistroCajones);
                AsignarCajonLugarVisitanteBD(lugarDireccion.uiRegistroLugar, estacionamiento.uiRegistroCajones);

                // Log de edición de dirección
        LogEstacionamientoHelper.Log(HttpContext, LogEstacionamientoHelper.Accion.EdicionLugar, lugarDireccion.iLugar, lugarDireccion.uiRegistroLugar);

                #endregion

                return Json(new { error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        public ActionResult AsignacionLugares()
        {
            return View();
        }

        [NonAction]
        private List<MLogReport> ListaLogsAsignados()
        {
            List<MLogReport> listaLogs = new List<MLogReport>();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                // Consulta con JOINS para obtener nombres de usuario, dirección, lugar y descripción de la acción.
                string query = @"
                    SELECT
                        L.uiUser as uiNumeroEmpleado,
                        ISNULL(U.sNombre, 'Sistema/Desc') as sNombre,
                        CASE
                            WHEN L.iRol = 1 THEN 'Administrador'
                            WHEN L.iRol = 2 THEN 'Solicitante'
                            WHEN L.iRol = 3 THEN 'Seguridad'
                            ELSE 'N/A'
                        END as sRol,
                        ISNULL(CAT.sLugar, 'N/A') as Direccion,
                        ISNULL(NL.sLugar, 'N/A') as Lugar,
                        L.dtFechaAccion,
                        CASE L.iLog
                            WHEN 1 THEN 'Generación de Solicitud'
                            WHEN 2 THEN 'Asignación de Solicitud'
                            WHEN 3 THEN 'Rechazo de Solicitud'
                            WHEN 4 THEN 'Asignación Empleado'
                            WHEN 5 THEN 'Liberación de Cajón'
                            WHEN 6 THEN 'Edición de Lugar'
                            WHEN 7 THEN 'Edición Hora de Salida'
                            ELSE 'Acción ' + CAST(L.iLog as varchar)
                        END as sAccion
                    FROM logEstacionamiento L
                    LEFT JOIN (SELECT DISTINCT uiNumeroEmpleado, sNombre FROM usuarios ) U ON L.uiUser = U.uiNumeroEmpleado
                    LEFT JOIN catLugares CAT ON L.iDireccion = CAT.uiRegistroCatalogo
                    LEFT JOIN catNumeroLugares NL ON L.iCajon = NL.uiRegistroLugar
                    ORDER BY L.dtFechaAccion DESC";

                using (SqlCommand comando = new SqlCommand(query, conexionSQL))
                {
                    conexionSQL.Open();
                    SqlDataAdapter da = new SqlDataAdapter(comando);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow r in dt.Rows)
                    {
                        MLogReport log = new MLogReport();
                        log.uiNumeroEmpleado = r["uiNumeroEmpleado"] == DBNull.Value ? 0 : Convert.ToInt32(r["uiNumeroEmpleado"]);
                        log.sNombre = Convert.ToString(r["sNombre"]);
                        log.sRol = Convert.ToString(r["sRol"]);
                        log.Direccion = Convert.ToString(r["Direccion"]);
                        log.Lugar = Convert.ToString(r["Lugar"]);
                        log.dtFechaAccion = Convert.ToDateTime(r["dtFechaAccion"]);
                        log.sAccion = Convert.ToString(r["sAccion"]);
                        listaLogs.Add(log);
                    }
                }
            }
            return listaLogs;
        }

        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador")]
        public JsonResult MostrarLogsFiltrado_AJAX(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            try
            {
                var lista = ListaLogsAsignados();

                if (!string.IsNullOrWhiteSpace(texto))
                {
                    string q = texto.Trim().ToLowerInvariant();
                    lista = lista.Where(x => x.sAccion != null && x.sAccion.ToLowerInvariant().Contains(q)).ToList();
                }

                DateTime tmp;
                if (!string.IsNullOrWhiteSpace(fechaDesde) && DateTime.TryParse(fechaDesde, out tmp))
                    lista = lista.Where(x => x.dtFechaAccion.Date >= tmp.Date).ToList();
                if (!string.IsNullOrWhiteSpace(fechaHasta) && DateTime.TryParse(fechaHasta, out tmp))
                    lista = lista.Where(x => x.dtFechaAccion.Date <= tmp.Date).ToList();

                return Json(new { error = false, listaLogs = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        [RoleAuthorize("Administrador")]
        public FileResult ExportLogExcel(string texto = null, string fechaDesde = null, string fechaHasta = null)
        {
            var res = MostrarLogsFiltrado_AJAX(texto, fechaDesde, fechaHasta);
            var dataResult = (dynamic)res.Data;
            List<MLogReport> lista = dataResult.listaLogs;

            var sb = new System.Text.StringBuilder();
            sb.Append("<meta http-equiv='Content-Type' content='text/html; charset=utf-8'>");
            sb.Append("<table border='1'>");
            sb.Append("<tr style='background-color:#eee; font-weight:bold;'>");
            sb.Append("<td># Empleado</td>");
            sb.Append("<td>Nombre</td>");
            sb.Append("<td>Rol</td>");
            sb.Append("<td>Dirección</td>");
            sb.Append("<td>Lugar</td>");
            sb.Append("<td>Acción</td>");
            sb.Append("<td>Fecha Acción</td>");
            sb.Append("</tr>");

            foreach (var x in lista)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", x.uiNumeroEmpleado);
                sb.AppendFormat("<td>{0}</td>", x.sNombre ?? "");
                sb.AppendFormat("<td>{0}</td>", x.sRol ?? "");
                sb.AppendFormat("<td>{0}</td>", x.Direccion ?? "");
                sb.AppendFormat("<td>{0}</td>", x.Lugar ?? "");
                sb.AppendFormat("<td>{0}</td>", x.sAccion ?? "");
                sb.AppendFormat("<td>{0}</td>", x.dtFechaAccion.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "application/vnd.ms-excel", "Reporte_Log_Estacionamiento.xls");
        }

    }
}

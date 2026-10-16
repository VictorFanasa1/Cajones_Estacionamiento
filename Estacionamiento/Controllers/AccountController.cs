using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using Estacionamiento.Models.ViewModels;
using System.DirectoryServices;
using Estacionamiento.Models;
using System.Data;

namespace Estacionamiento.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.HideAuthHeader = true; // ocultar saludo/botón en Login
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            MUsuarios info = new MUsuarios();

            string usuarioRed = model.Usuario;
            string Contra = model.Password;
            string numEmpleado = "";
            string nombreEmpleado = "";
            string correoUsuario = "";
            string cargoUsuario = "";
            string uen = "";

            string UAN = "LDAP://gf.grupofarmacos.net/DC=gf,DC=grupofarmacos,DC=net";

            DirectoryEntry oroot = new DirectoryEntry(UAN, "GFLSAEC1", "Sa3cgf19*", AuthenticationTypes.Secure);
            //DirectoryEntry oroot = new DirectoryEntry(UAN, usuario, password, AuthenticationTypes.Secure);
            DirectorySearcher nombre = new DirectorySearcher(oroot);

            nombre.Filter = $"sAMAccountName={model.Usuario}";
            nombre.PropertiesToLoad.Add("employeeNumber");
            nombre.PropertiesToLoad.Add("cn"); /* Nombre de Empleado*/
            nombre.PropertiesToLoad.Add("mail"); /* Correo de Empleado */
            nombre.PropertiesToLoad.Add("title"); /* Cargo de Empleado*/
            nombre.PropertiesToLoad.Add("Company"); /* UEN de Empleado*/

            try
            {
                if (nombre.FindAll().Count > 0)
                {
                    foreach (SearchResult resultadoAD in nombre.FindAll())
                    {

                        try
                        {
                            numEmpleado = (string)resultadoAD.Properties["employeeNumber"][0];
                            info.uiNumeroEmpleado = Convert.ToInt32(numEmpleado);
                        }
                        catch
                        {
                            numEmpleado = "";
                        }

                        try
                        {
                            nombreEmpleado = (string)resultadoAD.Properties["cn"][0];
                            info.sNombre = nombreEmpleado;
                        }

                        catch
                        {

                        }

                        try
                        {
                            correoUsuario = (string)resultadoAD.Properties["mail"][0];
                            info.sCorreo = correoUsuario;
                        }

                        catch
                        {

                        }

                        try
                        {
                            cargoUsuario = (string)resultadoAD.Properties["title"][0];
                            info.Puesto = cargoUsuario;
                        }

                        catch
                        {

                        }

                        try
                        {
                            uen = (string)resultadoAD.Properties["Company"][0];
                            info.UEN = uen;
                        }

                        catch
                        {

                        }
                    }
                }
                else
                {
                    MUsuarios u = ValidarUsuariosSeguridad(model.Usuario);
                    Session["log"] = u;

                    if(u.iRol != 0)
                    {
                        Session["IdUsuario"] = u.IdUsuarios;
                        Session["Nombre"] = u.sNombre;
                        Session["Rol"] = u.iRol;

                        FormsAuthentication.SetAuthCookie(u.sNombre, false);
                        return RedirectToAction("CajonesVisitantes", "Estacionamiento");
                    }

                    else
                    {
                        ModelState.AddModelError("", "Favor de utilizar username y contraseña institucional para acceder");
                        return View(model);
                    }
                   
                }

                Session["log"] = info;
                MUsuarios user = new MUsuarios();


                //if (info.uiNumeroEmpleado == 0)
                //{
                #region Validar usuario Seguridad
                //MUsuarios u = ValidarUsuariosSeguridad(model.Usuario);
                //Session["log"] = u;

                //Session["IdUsuario"] = u.IdUsuarios;
                //Session["Nombre"] = u.sNombre;
                //Session["Rol"] = u.iRol;

                //FormsAuthentication.SetAuthCookie(u.sNombre, false);
                //return RedirectToAction("CajonesVisitantes", "Estacionamiento");
                #endregion

                //}
                //else
                //{

                user = EnsureUsuarioLocal(info.uiNumeroEmpleado);
                    // Preferir el nombre obtenido de AD (info.sNombre) si el registro local aún no tiene nombre
                    string displayName = !string.IsNullOrWhiteSpace(user.sNombre) ? user.sNombre : info.sNombre;

                    FormsAuthentication.SetAuthCookie(displayName, false);
                    Session["IdUsuario"] = user.IdUsuarios;
                    Session["Nombre"] = displayName;
                    Session["Rol"] = user.iRol;
                    var redirect = GetDefaultUrlForRole(user.iRol);
                    // Redirigir según el rol del usuario
                    return Redirect(redirect);
                //}



            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al iniciar sesión: " + ex.Message);
                return View(model);
            }
        }

        private MUsuarios ValidarUsuariosSeguridad(string user)
        {
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            SqlConnection conexionSQL = new SqlConnection(cadenaConexion);

            SqlCommand comando = new SqlCommand(string.Format("Select * from usuarios where sNombre = '{0}'", user), conexionSQL);

            SqlDataAdapter da = new SqlDataAdapter(comando);
            DataTable dt = new DataTable();
            da.Fill(dt);
            MUsuarios userS = new MUsuarios();

            if (dt.Rows.Count == 1)
            {
                DataRow r = dt.Rows[0];
                userS = new MUsuarios(r);
            }

            return userS;
        }

        //public ActionResult Login(LoginViewModel model)
        //{
        //    ViewBag.HideAuthHeader = true; // mantener oculto si hay error de validación
        //    // Validación manual del token anti-forgery para prevenir excepción 500
        //    try
        //    {
        //        System.Web.Helpers.AntiForgery.Validate();
        //    }
        //    catch (System.Web.Mvc.HttpAntiForgeryException)
        //    {
        //        // Token inválido o emitido para otro usuario. Limpiar estado y redirigir a Login.
        //        try { FormsAuthentication.SignOut(); } catch { }
        //        try { Session.Clear(); Session.Abandon(); } catch { }
        //        var authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.Now.AddYears(-1) };
        //        Response.Cookies.Add(authCookie);
        //        var sessionCookie = new System.Web.HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddYears(-1) };
        //        Response.Cookies.Add(sessionCookie);
        //        return RedirectToAction("Login");
        //    }
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    try
        //    {
        //        // Leer modo de autenticación desde configuración
        //        var authMode = (ConfigurationManager.AppSettings["AuthMode"] ?? "Hybrid").ToLowerInvariant();
        //        UsuarioDto user = null;

        //        if (authMode == "localonly")
        //        {
        //            user = ValidarUsuarioLocal(model.Usuario, model.Password);
        //        }
        //        else if (authMode == "adonly")
        //        {
        //            ADUser adUser;
        //            if (TryAuthenticateAD(model.Usuario, model.Password, out adUser))
        //                user = EnsureUsuarioLocal(adUser);
        //        }
        //        else // Hybrid por defecto
        //        {
        //            // 1) Intentar autenticación contra Active Directory
        //            ADUser adUser;
        //            var adOk = TryAuthenticateAD(model.Usuario, model.Password, out adUser);
        //            if (adOk)
        //            {
        //                // Asegura que exista en la BD local para integrarse con el flujo de estacionamiento
        //                user = EnsureUsuarioLocal(adUser);
        //            }
        //            else
        //            {
        //                // 2) Fallback: validar contra BD local con sPassword (texto plano)
        //                user = ValidarUsuarioLocal(model.Usuario, model.Password);
        //            }
        //        }

        //        if (user == null)
        //        {
        //            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
        //            return View(model);
        //        }

        //        FormsAuthentication.SetAuthCookie(user.Nombre, false);
        //        Session["IdUsuario"] = user.Id;
        //        Session["Nombre"] = user.Nombre;
        //        Session["Rol"] = user.Rol;

        //        // Redirigir según el rol del usuario
        //        var redirect = GetDefaultUrlForRole(user.Rol);
        //        return Redirect(redirect);
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", "Error al iniciar sesión: " + ex.Message);
        //        return View(model);
        //    }
        //}

        // Obtiene la URL por defecto para un rol, basada en appSettings RoleMap:* y RoleDefault:*
        //private string GetDefaultUrlForRole(int roleId)
        //{
        //    try
        //    {
        //        var adminMap = ConfigurationManager.AppSettings["RoleMap:Administrador"];
        //        var soliMap = ConfigurationManager.AppSettings["RoleMap:Solicitante"];
        //        var segMap = ConfigurationManager.AppSettings["RoleMap:Seguridad"];

        //        int id;
        //        if (int.TryParse(adminMap, out id) && roleId == id)
        //        {
        //            var custom = ConfigurationManager.AppSettings["RoleDefault:Administrador"]; // opcional
        //            return !string.IsNullOrWhiteSpace(custom) ? custom : Url.Action("AdmEstacionamientos", "Estacionamiento");
        //        }
        //        if (int.TryParse(soliMap, out id) && roleId == id)
        //        {
        //            var custom = ConfigurationManager.AppSettings["RoleDefault:Solicitante"];
        //            return !string.IsNullOrWhiteSpace(custom) ? custom : Url.Action("AltaCajonVisitantes", "Estacionamiento");
        //        }
        //        if (int.TryParse(segMap, out id) && roleId == id)
        //        {
        //            var custom = ConfigurationManager.AppSettings["RoleDefault:Seguridad"];
        //            return !string.IsNullOrWhiteSpace(custom) ? custom : Url.Action("CajonesVisitantes", "Estacionamiento");
        //        }
        //    }
        //    catch { }

        //    // Fallback seguro
        //    return Url.Action("Login", "Account");
        //}

        private string GetDefaultUrlForRole(int roleId)
        {
            try
            {
                var adminMap = ConfigurationManager.AppSettings["RoleMap:Administrador"];
                var soliMap = ConfigurationManager.AppSettings["RoleMap:Solicitante"];
                var segMap = ConfigurationManager.AppSettings["RoleMap:Seguridad"];

                int id;
                if (int.TryParse(adminMap, out id) && roleId == id)
                {
                    var custom = ConfigurationManager.AppSettings["RoleDefault:Administrador"]; // opcional
                    return !string.IsNullOrWhiteSpace(custom) ? custom : Url.Action("AdmEstacionamientos", "Estacionamiento");
                }
                if (int.TryParse(soliMap, out id) && roleId == id)
                {
                    var custom = ConfigurationManager.AppSettings["RoleDefault:Solicitante"];
                    return !string.IsNullOrWhiteSpace(custom) ? custom : Url.Action("AltaCajonVisitantes", "Estacionamiento");
                }
                if (int.TryParse(segMap, out id) && roleId == id)
                {
                    var custom = ConfigurationManager.AppSettings["RoleDefault:Seguridad"];
                    return !string.IsNullOrWhiteSpace(custom) ? custom : Url.Action("CajonesVisitantes", "Estacionamiento");
                }
            }
            catch { }

            // Fallback seguro
            return Url.Action("Login", "Account");
        }

        [Authorize]
        public ActionResult Logout()
        {
            // Cerrar autenticación y sesión
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            // Expirar cookies de autenticación y sesión
            var authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, "");
            authCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(authCookie);

            var sessionCookie = new System.Web.HttpCookie("ASP.NET_SessionId", "");
            sessionCookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(sessionCookie);

            return RedirectToAction("Login");
        }

        // Valida por correo o número de empleado + sPassword (texto plano)
        private UsuarioDto ValidarUsuarioLocal(string usuario, string password)
        {
            string cs = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 IdUsuarios, sNombre, iRol
FROM dbo.usuarios
WHERE bActivo = 1
  AND sPassword = @pwd
  AND (
        sCorreo = @usuario
        OR CAST(uiNumeroEmpleado AS VARCHAR(50)) = @usuario
      );", cn))
            {
                cmd.Parameters.AddWithValue("@usuario", usuario ?? "");
                cmd.Parameters.AddWithValue("@pwd", password ?? "");
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return new UsuarioDto
                    {
                        Id = rd.GetInt32(0),
                        Nombre = rd.GetString(1),
                        Rol = rd.GetInt32(2)
                    };
                }
            }
        }

        private class ADUser
        {
            public int NumeroEmpleado { get; set; }
            public string Nombre { get; set; }
            public string Correo { get; set; }
            public string Cargo { get; set; }
            public string SamAccountName { get; set; }
            public string UEN { get; set; }
            public string JefeDirecto { get; set; }
        }

        // DTO local usado por los métodos de validación/autenticación dentro de este controller
        private class UsuarioDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public int Rol { get; set; }
        }

        // Autentica contra AD con las credenciales proporcionadas
        //private bool TryAuthenticateAD(string usuario, string password, out ADUser info)
        //{
        //    info = null;
        //    if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        //        return false;

        //    var ldapPath = ConfigurationManager.AppSettings["LdapPath"] ?? "LDAP://gf.grupofarmacos.net/DC=gf,DC=grupofarmacos,DC=net";
        //    var domain = ConfigurationManager.AppSettings["LdapDomain"] ?? "gf";
        //    int timeoutMs = 0;
        //    int.TryParse(ConfigurationManager.AppSettings["LdapClientTimeoutMs"], out timeoutMs);
        //    if (timeoutMs <= 0) timeoutMs = 4000; // 4s por defecto

        //    // Preparar usuario para bind: dominio\\usuario o usuario@dominio
        //    string bindUser = usuario.Contains("@") ? usuario : domain + "\\" + usuario;

        //    try
        //    {
        //        using (var entry = new DirectoryEntry(ldapPath, bindUser, password, AuthenticationTypes.Secure))
        //        {
        //            // Forzar el bind
        //            var native = entry.NativeObject;

        //            using (var searcher = new DirectorySearcher(entry))
        //            {
        //                searcher.ClientTimeout = TimeSpan.FromMilliseconds(timeoutMs);
        //                string escaped = EscapeLdapFilter(usuario);
        //                searcher.Filter = string.Format("(|(sAMAccountName={0})(mail={0})(employeeNumber={0}))", escaped);
        //                searcher.PropertiesToLoad.Add("employeeNumber");
        //                searcher.PropertiesToLoad.Add("cn");
        //                searcher.PropertiesToLoad.Add("mail");
        //                searcher.PropertiesToLoad.Add("title");
        //                searcher.PropertiesToLoad.Add("sAMAccountName");
        //                searcher.PropertiesToLoad.Add("Company");

        //                var result = searcher.FindOne();
        //                if (result == null)
        //                    return false;

        //                Func<string, string> GetProp = key =>
        //                {
        //                    return result.Properties.Contains(key) && result.Properties[key].Count > 0 ? (string)result.Properties[key][0] : null;
        //                };

        //                var numEmpStr = GetProp("employeeNumber");
        //                int? numEmp = null;
        //                int n;
        //                if (int.TryParse(numEmpStr, out n)) numEmp = n;

        //                info = new ADUser
        //                {
        //                    NumeroEmpleado = numEmp,
        //                    Nombre = GetProp("cn"),
        //                    Correo = GetProp("mail"),
        //                    Cargo = GetProp("title"),
        //                    SamAccountName = GetProp("sAMAccountName"),
        //                    UEN = GetProp("Company"),
        //                    JefeDirecto = GetProp("JefeDirecto")
        //                };
        //                Session["log"] = info;
        //                return true;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        private string EscapeLdapFilter(string value)
        {
            if (value == null) return null;
            return value
                .Replace("\\", "\\5c")
                .Replace("*", "\\2a")
                .Replace("(", "\\28")
                .Replace(")", "\\29")
                .Replace("\0", "\\00");
        }

        // Asegura que el usuario AD exista en la tabla 'usuarios'; si no, lo inserta

        [NonAction]
        private MUsuarios EnsureUsuarioLocal(int numeroEmpleado)
        {
            MUsuarios usuario = new MUsuarios();
            string cadenaConexion = ConfigurationManager.ConnectionStrings["SQLESTACIONA"].ConnectionString;

            using (SqlConnection conexionSQL = new SqlConnection(cadenaConexion))
            {
                try
                {
                    conexionSQL.Open();

                    // Buscar si el usuario ya existe
                    using (SqlCommand comando = new SqlCommand("SELECT * FROM usuarios WHERE uiNumeroEmpleado = @numEmpleado", conexionSQL))
                    {
                        comando.Parameters.AddWithValue("@numEmpleado", numeroEmpleado);

                        SqlDataAdapter da = new SqlDataAdapter(comando);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Recuperar datos del AD almacenados en sesión
                        MUsuarios userAD = Session["log"] as MUsuarios;

                        if (dt.Rows.Count == 0)
                        {
                            // 🔹 No existe → Insertar como solicitante
                            using (SqlCommand insertCmd = new SqlCommand(@"
                        INSERT INTO usuarios (uiNumeroEmpleado, sNombre, sCorreo, Puesto, UEN, iRol, bActivo)
                        VALUES (@uiNumeroEmpleado, @sNombre, @sCorreo, @sPuesto, @sUEN, 2, 1);
                        SELECT SCOPE_IDENTITY();", conexionSQL))
                            {
                                insertCmd.Parameters.AddWithValue("@uiNumeroEmpleado", numeroEmpleado);
                                insertCmd.Parameters.AddWithValue("@sNombre", userAD?.sNombre ?? "");
                                insertCmd.Parameters.AddWithValue("@sCorreo", userAD?.sCorreo ?? "");
                                insertCmd.Parameters.AddWithValue("@sPuesto", userAD?.Puesto ?? "");
                                insertCmd.Parameters.AddWithValue("@sUEN", userAD?.UEN ?? "");

                                var newIdObj = insertCmd.ExecuteScalar();
                                int newId; // Declarado antes para compatibilidad con versiones de C# anteriores a 7
                                if (newIdObj != null && int.TryParse(newIdObj.ToString(), out newId))
                                {
                                    using (SqlCommand cmd2 = new SqlCommand("SELECT * FROM usuarios WHERE IdUsuarios = @IdUsuarios", conexionSQL))
                                    {
                                        cmd2.Parameters.AddWithValue("@IdUsuarios", newId);
                                        SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                        DataTable dt2 = new DataTable();
                                        da2.Fill(dt2);
                                        if (dt2.Rows.Count > 0)
                                            usuario = new MUsuarios(dt2.Rows[0]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 🔹 Ya existe → actualizar información si cambió
                            DataRow r = dt.Rows[0];
                            usuario = new MUsuarios(r);

                            using (SqlCommand updateCmd = new SqlCommand(@"
                        UPDATE usuarios
                        SET 
                            sNombre = @sNombre,
                            sCorreo = @sCorreo,
                            Puesto = @sPuesto,
                            UEN = @sUEN,
                            bActivo = 1,
                            iRol = CASE WHEN iRol IN (1, 3) THEN iRol ELSE 2 END
                        WHERE IdUsuarios = @IdUsuarios", conexionSQL))
                            {
                                updateCmd.Parameters.AddWithValue("@sNombre", userAD?.sNombre ?? usuario.sNombre);
                                updateCmd.Parameters.AddWithValue("@sCorreo", userAD?.sCorreo ?? usuario.sCorreo);
                                updateCmd.Parameters.AddWithValue("@sPuesto", userAD?.Puesto ?? usuario.Puesto);
                                updateCmd.Parameters.AddWithValue("@sUEN", userAD?.UEN ?? usuario.UEN);
                                updateCmd.Parameters.AddWithValue("@IdUsuarios", usuario.IdUsuarios);
                                updateCmd.ExecuteNonQuery();
                            }

                            // Recargar los datos actualizados
                            using (SqlCommand cmd2 = new SqlCommand("SELECT * FROM usuarios WHERE IdUsuarios = @IdUsuarios", conexionSQL))
                            {
                                cmd2.Parameters.AddWithValue("@IdUsuarios", usuario.IdUsuarios);
                                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                                DataTable dt2 = new DataTable();
                                da2.Fill(dt2);
                                if (dt2.Rows.Count > 0)
                                    usuario = new MUsuarios(dt2.Rows[0]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al sincronizar el usuario local: " + ex.Message, ex);
                }
                finally
                {
                    if (conexionSQL.State != ConnectionState.Closed)
                        conexionSQL.Close();
                }
            }

            return usuario;
        }


    }
}

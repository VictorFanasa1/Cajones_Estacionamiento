using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Net;
using System.Reflection;

namespace Estacionamiento.Models
{
    public class MUsuarios
    {
        public MUsuarios()
        {

        }

        public MUsuarios(DataRow r)
        {

            if (r["IdUsuarios"] != DBNull.Value)
            {
                IdUsuarios = Convert.ToInt32(r["IdUsuarios"]);
            }

            if (r["uiNumeroEmpleado"] != DBNull.Value)
            {
                uiNumeroEmpleado = Convert.ToInt32(r["uiNumeroEmpleado"]);
            }

            sNombre = Convert.ToString(r["sNombre"]);

            if (r["sCorreo"] != DBNull.Value)
            {
                sCorreo = Convert.ToString(r["sCorreo"]);
            }

            if (r["iRol"] != DBNull.Value)
            {
                iRol = Convert.ToInt32(r["iRol"]);
            }

            // Campos adicionales si existen en la consulta
            try
            {
                if (r.Table.Columns.Contains("Puesto") && r["Puesto"] != DBNull.Value)
                {
                    Puesto = Convert.ToString(r["Puesto"]);
                }
                if (r.Table.Columns.Contains("JefeDirecto") && r["JefeDirecto"] != DBNull.Value)
                {
                    JefeDirecto = Convert.ToString(r["JefeDirecto"]);
                }
                if (r.Table.Columns.Contains("UEN") && r["UEN"] != DBNull.Value)
                {
                    UEN = Convert.ToString(r["UEN"]);
                }
            }
            catch { /* columnas opcionales */ }

        }


    public int IdUsuarios { get; set; }

    public int uiNumeroEmpleado { get; set; }

    public string sNombre { get; set; }

    public string sCorreo { get; set; }

    public int iRol { get; set; }

    public bool bActivo { get; set; }

    public string sRoleUsuarios { get; set; }

    public int idUserResponsables { get; set; }

    // Datos adicionales obtenidos del servicio
    public string Puesto { get; set; }
    public string JefeDirecto { get; set; }
    public string UEN { get; set; }

    // Contraseña temporal en texto plano (solo para entrega al momento de creación)
    public string sPassword { get; set; }

    // PROPIEDADES FALTANTES PARA CORREGIR ERRORES DE COMPILACIÓN
    public string Correo { get; set; }
    public string snombre { get; set; }
    public string nombreResponsable { get; set; }

        /// <summary>
        /// Consume el servicio FAST y asigna los datos al usuario, usando reflexión para evitar
        /// dependencias de compilación al proxy de servicio (Fast.*).
        /// Si el ensamblado/tipos no están presentes en tiempo de ejecución, la llamada se omite silenciosamente.
        /// </summary>
        public void PoblarDatosDesdeServicioFAST(int uiNumEmpleado)
        {
            // Formatear número de empleado para FAST
            string numeEmpleadoFAST;
            string evaluarNumeEmpleado = Convert.ToString(uiNumEmpleado);
            if (uiNumEmpleado <= 19880)
                numeEmpleadoFAST = evaluarNumeEmpleado.PadLeft(6, '0');
            else
                numeEmpleadoFAST = evaluarNumeEmpleado;

            // Preferir TLS 1.2 al consumir el servicio
            ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls12;

            // Función auxiliar para buscar tipos por nombre completo en los ensamblados cargados
            Func<string, Type> FindType = fullName =>
            {
                var t = Type.GetType(fullName, false);
                if (t != null) return t;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        t = asm.GetType(fullName, false);
                        if (t != null) return t;
                    }
                    catch { }
                }
                return null;
            };

            // Resolver tipos del proxy (si están disponibles)
            var headerType = FindType("Fast.HeaderMessage");
            var inputType = FindType("Fast.ObtenerFastInputType");
            var outputType = FindType("Fast.ObtenerFastOutputType");
            var clientType = FindType("Fast.EmpleadosPortTypeClient");

            if (headerType == null || inputType == null || outputType == null || clientType == null)
            {
                // No está disponible el proxy; salir sin afectar el flujo
                return;
            }

            try
            {
                // Instanciar objetos vía reflexión
                object header = Activator.CreateInstance(headerType);
                object input = Activator.CreateInstance(inputType);
                object client = Activator.CreateInstance(clientType);

                // Asignar personNumber
                var personNumberProp = inputType.GetProperty("personNumber", BindingFlags.Public | BindingFlags.Instance);
                if (personNumberProp != null && personNumberProp.CanWrite)
                {
                    personNumberProp.SetValue(input, numeEmpleadoFAST, null);
                }

                // Invocar método obtenerFast(HeaderMessage, ObtenerFastInputType, out ObtenerFastOutputType)
                var method = clientType.GetMethod("obtenerFast", BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    return; // No existe el método esperado
                }

                object output = null; // será llenado por el parámetro out
                var args = new object[] { header, input, output };
                method.Invoke(client, args);
                output = args[2];

                if (output == null) return;

                // Navegar: output.empleado
                var empleadoProp = outputType.GetProperty("empleado", BindingFlags.Public | BindingFlags.Instance);
                var empleado = empleadoProp != null ? empleadoProp.GetValue(output, null) : null;
                if (empleado == null) return;

                var empleadoType = empleado.GetType();
                var infoBasicaProp = empleadoType.GetProperty("informacionBasica", BindingFlags.Public | BindingFlags.Instance);
                var aprobador1Prop = empleadoType.GetProperty("aprobador1", BindingFlags.Public | BindingFlags.Instance);

                var basica = infoBasicaProp != null ? infoBasicaProp.GetValue(empleado, null) : null;
                var jefe = aprobador1Prop != null ? aprobador1Prop.GetValue(empleado, null) : null;

                if (basica != null)
                {
                    var tBasica = basica.GetType();
                    string nombre = Convert.ToString(tBasica.GetProperty("nombre")?.GetValue(basica, null) ?? string.Empty);
                    string apP = Convert.ToString(tBasica.GetProperty("apellidoPaterno")?.GetValue(basica, null) ?? string.Empty);
                    string apM = Convert.ToString(tBasica.GetProperty("apellidoMaterno")?.GetValue(basica, null) ?? string.Empty);
                    var nombreCompleto = string.Join(" ", new[] { nombre, apP, apM }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    if (!string.IsNullOrWhiteSpace(nombreCompleto))
                        sNombre = nombreCompleto;

                    // Campos opcionales: puesto, UEN (los nombres pueden variar)
                    try { Puesto = Convert.ToString(tBasica.GetProperty("puesto")?.GetValue(basica, null)); } catch { }
                    try { UEN = Convert.ToString(tBasica.GetProperty("UEN")?.GetValue(basica, null)); } catch { }
                }

                if (jefe != null)
                {
                    var tJefe = jefe.GetType();
                    string jn = Convert.ToString(tJefe.GetProperty("nombre")?.GetValue(jefe, null) ?? string.Empty);
                    string jap = Convert.ToString(tJefe.GetProperty("apellidoPaterno")?.GetValue(jefe, null) ?? string.Empty);
                    string jam = Convert.ToString(tJefe.GetProperty("apellidoMaterno")?.GetValue(jefe, null) ?? string.Empty);
                    var jefeNombreCompleto = string.Join(" ", new[] { jn, jap, jam }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    if (!string.IsNullOrWhiteSpace(jefeNombreCompleto))
                        JefeDirecto = jefeNombreCompleto;
                }
            }
            catch
            {
                // Evitar romper el flujo si el servicio falla; opcionalmente registrar en log
            }
        }

    }
}
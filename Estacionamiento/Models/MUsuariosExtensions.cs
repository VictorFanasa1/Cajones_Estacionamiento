using System;

namespace Estacionamiento.Models
{
    /// <summary>
    /// Extensiones para MUsuarios. Proporciona un método de respaldo (no-op)
    /// para PoblarDatosDesdeServicioFAST cuando la definición de instancia
    /// aún no está disponible en tiempo de compilación.
    /// </summary>
    public static class MUsuariosExtensions
    {
        /// <summary>
        /// Fallback sin efectos: si existe el método de instancia con la misma firma
        /// será preferido por el compilador y este no se utilizará. Si no existe, este
        /// método evita errores de compilación y permite continuar el flujo.
        /// </summary>
        public static void PoblarDatosDesdeServicioFAST(this MUsuarios usuario, int uiNumEmpleado)
        {
            // Intencionalmente vacío. Puedes registrar logs aquí si lo deseas.
        }
    }
}

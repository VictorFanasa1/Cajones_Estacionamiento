using System;

namespace Estacionamiento.Models
{
    public class MLogReport
    {
        public MLogReport()
        {

        }

        public int uiNumeroEmpleado { get; set; }
        public string sNombre { get; set; }
        public string sRol { get; set; }
        public string Direccion { get; set; }
        public string Lugar { get; set; }
        public DateTime dtFechaAccion { get; set; }
        public string sAccion { get; set; }

        public string dtFechaAccionJSON
        {
            get { return dtFechaAccion.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
    }
}

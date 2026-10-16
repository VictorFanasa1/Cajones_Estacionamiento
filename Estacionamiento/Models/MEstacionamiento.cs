using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Estacionamiento.Models
{
    public class MEstacionamiento
    {
        public MEstacionamiento()
        {

        }

        public MEstacionamiento(DataRow r)
        {
            uiRegistroCajones = Convert.ToInt32(r["uiRegistroCajones"]);
            iCajon = Convert.ToInt32(r["iCajon"]);
            iLugar = Convert.ToInt32(r["iLugar"]);
            iUsuario = Convert.ToInt32(r["iUsuario"]);

            if (r["sObservaciones"] != DBNull.Value)
            {
                sObservaciones = Convert.ToString(r["sObservaciones"]);
            }

            dtFechaAsignacion = Convert.ToDateTime(r["dtFechaAsignacion"]);

            if (r["dtFechaVisita"] != DBNull.Value)
            {
                dtFechaVisita = Convert.ToDateTime(r["dtFechaVisita"]);
            }

            if (r["horaVisita"] != DBNull.Value)
            {
                horaVisita = Convert.ToString(r["horaVisita"]);
            }

            if (r["horaSalida"] != DBNull.Value)
            {
                horaSalida = Convert.ToString(r["horaSalida"]);
            }

            sPlaca = Convert.ToString(r["sPlaca"]);

            sModelo = Convert.ToString(r["sModelo"]);

            sColor = Convert.ToString(r["sColor"]);
        }

        public int uiRegistroCajones { get; set; }

        public int iCajon { get; set; }

        public int iLugar { get; set; }

        public int iUsuario { get; set; }

        public string sObservaciones { get; set; }

        public DateTime dtFechaAsignacion { get; set; }

        public string nombreUsuario { get; set; }

        public string correoUsuario { get; set; }

        public MLugares Lugares { get; set; }

        public MUsuarios Usuarios { get; set; }

        public string dtFechaAsignacionJSON
        {
            get { return dtFechaAsignacion.ToShortDateString(); }
        }
        public DateTime dtFechaVisita { get; set; }

        public string dtFechaVisitaJSON
        {
            get { return dtFechaVisita.ToShortDateString(); }
        }

        public string horaVisita { get; set; }

        public string horaSalida { get; set; }

        public bool bActivo { get; set; }

        public string nombreVisitante { get; set; }

        public string sLugar { get; set; }

        public string sPlaca { get; set; }

        public string sModelo { get; set; }

        public string sColor { get; set; }

        public MLugarDireccion LugarDireccion { get; set; }

        // Nuevos indicadores de tipo de acceso
        public bool bInterno { get; set; }
        public bool bExterno { get; set; }
    }
}
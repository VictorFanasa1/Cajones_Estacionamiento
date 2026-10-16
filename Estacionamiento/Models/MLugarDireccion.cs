using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Estacionamiento.Models
{
    public class MLugarDireccion
    {
        public MLugarDireccion()
        {

        }

        public MLugarDireccion(DataRow r)
        {
            uiRegistroLugar = Convert.ToInt32(r["uiRegistroLugar"]);
            sLugar = Convert.ToString(r["sLugar"]);
            iLugar = Convert.ToInt32(r["iLugar"]);
        }

        public int uiRegistroLugar { get; set; }

        public string sLugar { get; set; }

        public int iLugar { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Estacionamiento.Models
{
    public class MLugares
    {
        public MLugares()
        {

        }

        public MLugares(DataRow r)
        {
            uiRegistroCatalogo = Convert.ToInt32(r["uiRegistroCatalogo"]);
            sLugar = Convert.ToString(r["sLugar"]);
        }

        public int uiRegistroCatalogo { get; set; }

        public string sLugar { get; set; }
    }
}
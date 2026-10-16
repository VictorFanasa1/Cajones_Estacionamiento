using System;
using System.Collections.Generic;
using System.Linq;
using Estacionamiento.Models;

namespace Estacionamiento.Helpers
{
    public class EmpleadoFiltros
    {
        public string NombreEmpleado { get; set; }
        public string Direccion { get; set; }
        public string Lugar { get; set; }
        public string FechaAsignacionDesde { get; set; }
        public string FechaAsignacionHasta { get; set; }
    }

    public static class EmpleadosFiltroHelper
    {
        public static List<MEstacionamiento> Aplicar(IEnumerable<MEstacionamiento> origen, EmpleadoFiltros filtros)
        {
            if (origen == null) return new List<MEstacionamiento>();
            var lista = origen.ToList();
            if (filtros == null) return lista;

            string nom = Norm(filtros.NombreEmpleado);
            string lug = Norm(filtros.Direccion);
            string dir = Norm(filtros.Lugar);

            DateTime tmp;
            DateTime? faDesde = DateTime.TryParse(filtros.FechaAsignacionDesde, out tmp) ? tmp.Date : (DateTime?)null;
            DateTime? faHasta = DateTime.TryParse(filtros.FechaAsignacionHasta, out tmp) ? tmp.Date : (DateTime?)null;

            if (faDesde.HasValue && faHasta.HasValue)
            {
                lista = lista.Where(x => x.dtFechaAsignacion.Date >= faDesde.Value && x.dtFechaAsignacion.Date <= faHasta.Value).ToList();
            }

            if (!string.IsNullOrEmpty(nom))
                lista = lista.Where(x => !string.IsNullOrEmpty(x.Usuarios?.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(nom)).ToList();
            if (!string.IsNullOrEmpty(dir))
                lista = lista.Where(x => !string.IsNullOrEmpty(x.LugarDireccion?.sLugar) && x.LugarDireccion.sLugar.ToLowerInvariant().Contains(dir)).ToList();
            if (!string.IsNullOrEmpty(lug))
                lista = lista.Where(x => !string.IsNullOrEmpty(x.Lugares?.sLugar) && x.Lugares.sLugar.ToLowerInvariant().Contains(lug)).ToList();

            return lista;
        }

        private static string Norm(string v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim().ToLowerInvariant();
    }
}
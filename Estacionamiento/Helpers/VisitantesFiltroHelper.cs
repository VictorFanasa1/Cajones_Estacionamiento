using System;
using System.Collections.Generic;
using System.Linq;
using Estacionamiento.Models;

namespace Estacionamiento.Helpers
{
    public class VisitanteFiltros
    {
        public string Texto { get; set; }
        public string FechaVisitaDesde { get; set; }
        public string FechaVisitaHasta { get; set; }
        public string FechaAsignacionDesde { get; set; }
        public string FechaAsignacionHasta { get; set; }
        public string NombreResponsable { get; set; }
        public string NombreVisitante { get; set; }
        public string HoraVisitaDesde { get; set; }
        public string HoraVisitaHasta { get; set; }
        public string HoraSalidaDesde { get; set; }
        public string HoraSalidaHasta { get; set; }
        public string Direccion { get; set; }
        public string Lugar { get; set; }
        public string Placa { get; set; }
    }

    public static class VisitantesFiltroHelper
    {
        public static List<MEstacionamiento> Aplicar(IEnumerable<MEstacionamiento> origen, VisitanteFiltros filtros)
        {
            if (origen == null) return new List<MEstacionamiento>();
            var lista = origen.ToList();
            if (filtros == null) return lista;

            // Normalizar strings
            string nResp = Norm(filtros.NombreResponsable);
            string q = Norm(filtros.Texto);
            string nVis = Norm(filtros.NombreVisitante);
            string lug = Norm(filtros.Direccion);
            string dir = Norm(filtros.Lugar);
            string pc = Norm(filtros.Placa);

            DateTime tmp;
            // 1. Parsear Fechas (Date portions)
            DateTime? fvDesde = null; 
            DateTime? fvHasta = null;
            DateTime? faDesde = null; 
            DateTime? faHasta = null;

            if (DateTime.TryParse(filtros.FechaVisitaDesde, out tmp)) fvDesde = tmp.Date;
            if (DateTime.TryParse(filtros.FechaVisitaHasta, out tmp)) fvHasta = tmp.Date;
            if (DateTime.TryParse(filtros.FechaAsignacionDesde, out tmp)) faDesde = tmp.Date;
            if (DateTime.TryParse(filtros.FechaAsignacionHasta, out tmp)) faHasta = tmp.Date;

            // 2. Parsear Horas
            TimeSpan ts;
            DateTime dtParse;

            TimeSpan? hvDesde = null; 
            TimeSpan? hvHasta = null;
            TimeSpan? hsDesde = null; 
            TimeSpan? hsHasta = null;


            if (!string.IsNullOrEmpty(q))
            {
                lista = lista.Where(x =>
                    (x.Usuarios != null && (
                        (!string.IsNullOrEmpty(x.Usuarios.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.Usuarios.nombreResponsable) && x.Usuarios.nombreResponsable.ToLowerInvariant().Contains(q)) ||
                        (!string.IsNullOrEmpty(x.sPlaca) && x.sPlaca.ToLowerInvariant().Contains(q)) ||
                        x.Usuarios.uiNumeroEmpleado.ToString().Contains(q)
                    ))
                ).ToList();
            }
            // Helper local para parsear entradas
            Func<string, TimeSpan?> parseInput = h => 
            {
                 if (string.IsNullOrWhiteSpace(h)) return null;
                 h = h.Trim();
                 TimeSpan t;
                 if (TimeSpan.TryParse(h, out t)) return t;
                 DateTime d;
                 if (DateTime.TryParse(h, out d)) return d.TimeOfDay;
                 return null;
            };

            hvDesde = parseInput(filtros.HoraVisitaDesde);
            hvHasta = parseInput(filtros.HoraVisitaHasta);
            hsDesde = parseInput(filtros.HoraSalidaDesde);
            hsHasta = parseInput(filtros.HoraSalidaHasta);

            // Filtro de Fecha Visita (Solo Fecha)
            if (fvDesde.HasValue) lista = lista.Where(x => x.dtFechaVisita.Date >= fvDesde.Value).ToList();
            if (fvHasta.HasValue) lista = lista.Where(x => x.dtFechaVisita.Date <= fvHasta.Value).ToList();

            // Función auxiliar para obtener TimeSpan de la hora (que viene como string en formato "HH:mm" o "hh:mm tt" o DateTime string)
            Func<string, TimeSpan?> parseHora = h => 
            { 
                if (string.IsNullOrWhiteSpace(h)) return null; 
                h = h.Trim();
                TimeSpan t; 
                if (TimeSpan.TryParse(h, out t)) return t;
                DateTime dt;
                if (DateTime.TryParse(h, out dt)) return dt.TimeOfDay;
                return null; 
            };

            // Filtro de Hora Visita (Solo Hora, TimeOfDay)
            if (hvDesde.HasValue) lista = lista.Where(x => { var h = parseHora(x.horaVisita); return h.HasValue && h.Value >= hvDesde.Value; }).ToList();
            if (hvHasta.HasValue) lista = lista.Where(x => { var h = parseHora(x.horaVisita); return h.HasValue && h.Value <= hvHasta.Value; }).ToList();

            // Filtro de Fecha Asignación
            if (faDesde.HasValue) lista = lista.Where(x => x.dtFechaAsignacion.Date >= faDesde.Value).ToList();
            if (faHasta.HasValue) lista = lista.Where(x => x.dtFechaAsignacion.Date <= faHasta.Value).ToList();

            // Filtro de Hora Salida
            if (hsDesde.HasValue) lista = lista.Where(x => { var h = parseHora(x.horaSalida); return h.HasValue && h.Value >= hsDesde.Value; }).ToList();
            if (hsHasta.HasValue) lista = lista.Where(x => { var h = parseHora(x.horaSalida); return h.HasValue && h.Value <= hsHasta.Value; }).ToList();

            if (!string.IsNullOrEmpty(nResp)) lista = lista.Where(x => !string.IsNullOrEmpty(x.Usuarios?.nombreResponsable) && x.Usuarios.nombreResponsable.ToLowerInvariant().Contains(nResp)).ToList();
            if (!string.IsNullOrEmpty(nVis)) lista = lista.Where(x => !string.IsNullOrEmpty(x.Usuarios?.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(nVis)).ToList();
            if (!string.IsNullOrEmpty(dir)) lista = lista.Where(x => !string.IsNullOrEmpty(x.LugarDireccion?.sLugar) && x.LugarDireccion.sLugar.ToLowerInvariant().Contains(dir)).ToList();
            if (!string.IsNullOrEmpty(lug)) lista = lista.Where(x => !string.IsNullOrEmpty(x.Lugares?.sLugar) && x.Lugares.sLugar.ToLowerInvariant().Contains(lug)).ToList();
            if (!string.IsNullOrEmpty(pc)) lista = lista.Where(x => !string.IsNullOrEmpty(x.sPlaca) && x.sPlaca.ToLowerInvariant().Contains(pc)).ToList();

            return lista;
        }

        private static string Norm(string v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim().ToLowerInvariant();
    }
}
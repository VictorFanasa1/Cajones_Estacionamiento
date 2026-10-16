using System;
using System.Collections.Generic;
using System.Linq;
using Estacionamiento.Models;

namespace Estacionamiento.Helpers
{
    public class AsignacionesVisitantesFiltros
    {
        public string UsuarioResponsable { get; set; }
        public string NombreVisitante { get; set; }
        public string HoraVisitaDesde { get; set; }
        public string HoraVisitaHasta { get; set; }
        public string HoraSalidaDesde { get; set; }
        public string HoraSalidaHasta { get; set; }
        public string Placa { get; set; }
        public string FechaVisitaDesde { get; set; }
        public string FechaVisitaHasta { get; set; }
    }

    public static class AsignacionesVisitantesFiltroHelper
    {
        public static List<MEstacionamiento> Aplicar(IEnumerable<MEstacionamiento> origen, AsignacionesVisitantesFiltros filtros)
        {
            if (origen == null) return new List<MEstacionamiento>();
            var lista = origen.ToList();
            if (filtros == null) return lista;

            string resp = Norm(filtros.UsuarioResponsable);
            string vis = Norm(filtros.NombreVisitante);
            string placa = Norm(filtros.Placa);

            DateTime tmp;
            DateTime? fvDesde = DateTime.TryParse(filtros.FechaVisitaDesde, out tmp) ? tmp.Date : (DateTime?)null;
            DateTime? fvHasta = DateTime.TryParse(filtros.FechaVisitaHasta, out tmp) ? tmp.Date : (DateTime?)null;

            // Parse de horas robusto: acepta HH:mm, hh:mm tt o DateTime
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

            TimeSpan? hvDesde = parseHora(filtros.HoraVisitaDesde);
            TimeSpan? hvHasta = parseHora(filtros.HoraVisitaHasta);
            TimeSpan? hsDesde = parseHora(filtros.HoraSalidaDesde);
            TimeSpan? hsHasta = parseHora(filtros.HoraSalidaHasta);

            if (fvDesde.HasValue && fvHasta.HasValue)
            {
                lista = lista.Where(x => x.dtFechaVisita.Date >= fvDesde.Value && x.dtFechaVisita.Date <= fvHasta.Value).ToList();
            }

            // Hora de visita: aplicar desde/hasta si existen (si ambos son iguales equivale a igualdad)
            if (hvDesde.HasValue)
                lista = lista.Where(x => { var h = parseHora(x.horaVisita); return h.HasValue && h.Value >= hvDesde.Value; }).ToList();
            if (hvHasta.HasValue)
                lista = lista.Where(x => { var h = parseHora(x.horaVisita); return h.HasValue && h.Value <= hvHasta.Value; }).ToList();

            // Hora de salida: aplicar desde/hasta con comparaciones correctas
            if (hsDesde.HasValue)
                lista = lista.Where(x => { var h = parseHora(x.horaSalida); return h.HasValue && h.Value >= hsDesde.Value; }).ToList();
            if (hsHasta.HasValue)
                lista = lista.Where(x => { var h = parseHora(x.horaSalida); return h.HasValue && h.Value <= hsHasta.Value; }).ToList();

            if (!string.IsNullOrEmpty(resp))
                lista = lista.Where(x => !string.IsNullOrEmpty(x.Usuarios?.nombreResponsable) && x.Usuarios.nombreResponsable.ToLowerInvariant().Contains(resp)).ToList();
            if (!string.IsNullOrEmpty(vis))
                lista = lista.Where(x => !string.IsNullOrEmpty(x.Usuarios?.sNombre) && x.Usuarios.sNombre.ToLowerInvariant().Contains(vis)).ToList();
            if (!string.IsNullOrEmpty(placa))
                lista = lista.Where(x => !string.IsNullOrEmpty(x.sPlaca) && x.sPlaca.ToLowerInvariant().Contains(placa)).ToList();

            return lista;
        }

        private static string Norm(string v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim().ToLowerInvariant();
    }
}
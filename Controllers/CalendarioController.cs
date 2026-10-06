using System.Text;
using CoordinacionOrientadores.Models;
using CoordinacionOrientadores.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoordinacionOrientadores.Controllers
{
    public class CalendarioController : Controller
    {
        private readonly DataService _data;
        public CalendarioController(DataService data) => _data = data;

        // Vista "mi calendario": un orientador ve solo lo suyo, sin saturarse con lo de los demás
        public IActionResult Individual(int? orientadorId, DateTime? semana)
        {
            var orientadores = _data.GetOrientadores();
            ViewBag.Orientadores = orientadores;

            if (!orientadorId.HasValue && orientadores.Count > 0)
            {
                orientadorId = orientadores.First().Id;
            }

            var inicioSemana = (semana ?? DateTime.Today);
            inicioSemana = inicioSemana.AddDays(-(int)inicioSemana.DayOfWeek);

            var asignaciones = _data.GetAsignaciones()
                .Where(a => a.OrientadorId == orientadorId &&
                            a.Fecha.Date >= inicioSemana.Date && a.Fecha.Date < inicioSemana.Date.AddDays(7))
                .ToList();

            var ausencias = _data.GetAusencias().Where(a => a.OrientadorId == orientadorId).ToList();

            ViewBag.OrientadorSeleccionado = orientadorId;
            ViewBag.InicioSemana = inicioSemana;
            ViewBag.Ausencias = ausencias;

            return View(asignaciones);
        }

        // Vista consolidada: la coordinadora ve todo el equipo en una sola cuadrícula para un día
        public IActionResult Consolidado(DateTime? fecha)
        {
            var dia = fecha ?? DateTime.Today;
            var asignaciones = _data.GetAsignaciones().Where(a => a.Fecha.Date == dia.Date).OrderBy(a => a.HoraInicio).ToList();
            var ausentesHoy = _data.GetOrientadores().Where(o => _data.EstaAusente(o.Id, dia)).ToList();

            ViewBag.Fecha = dia;
            ViewBag.AusentesHoy = ausentesHoy;
            return View(asignaciones);
        }

        // Reporte exportable (reemplaza el Word manual) - filtrable, descargable como CSV (se abre en Excel)
        public IActionResult Reporte(DateTime? desde, DateTime? hasta, int? orientadorId)
        {
            var asignaciones = _data.GetAsignaciones();
            if (desde.HasValue) asignaciones = asignaciones.Where(a => a.Fecha.Date >= desde.Value.Date).ToList();
            if (hasta.HasValue) asignaciones = asignaciones.Where(a => a.Fecha.Date <= hasta.Value.Date).ToList();
            if (orientadorId.HasValue) asignaciones = asignaciones.Where(a => a.OrientadorId == orientadorId).ToList();

            ViewBag.Orientadores = _data.GetOrientadores();
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.OrientadorId = orientadorId;

            return View(asignaciones);
        }

        // Descarga en CSV (compatible con Excel) de lo que se esté viendo en el Reporte
        public IActionResult ReporteCsv(DateTime? desde, DateTime? hasta, int? orientadorId)
        {
            var asignaciones = _data.GetAsignaciones();
            if (desde.HasValue) asignaciones = asignaciones.Where(a => a.Fecha.Date >= desde.Value.Date).ToList();
            if (hasta.HasValue) asignaciones = asignaciones.Where(a => a.Fecha.Date <= hasta.Value.Date).ToList();
            if (orientadorId.HasValue) asignaciones = asignaciones.Where(a => a.OrientadorId == orientadorId).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Fecha,Hora inicio,Hora fin,Proceso,Orientador,Sede,Enlace,Estado,Notas");
            foreach (var a in asignaciones)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    a.Fecha.ToString("dd/MM/yyyy"),
                    a.HoraInicio.ToString(@"hh\:mm"),
                    a.HoraFin.ToString(@"hh\:mm"),
                    CsvEscape(a.ProcesoNombre ?? ""),
                    CsvEscape(a.OrientadorNombre ?? ""),
                    CsvEscape(a.Sede ?? ""),
                    CsvEscape(a.EnlaceReunion ?? ""),
                    a.Estado.ToString(),
                    CsvEscape(a.Notas ?? "")
                }));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"calendarizacion_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}

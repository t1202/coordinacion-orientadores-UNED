using CoordinacionOrientadores.Models;

namespace CoordinacionOrientadores.Services
{
    // Punto único de acceso a los datos de la aplicación. Centraliza la lectura/escritura
    // de los 4 archivos JSON y la lógica de negocio (reparto de carga, detección de conflictos).
    public class DataService
    {
        private readonly JsonStore<Orientador> _orientadores;
        private readonly JsonStore<Proceso> _procesos;
        private readonly JsonStore<Asignacion> _asignaciones;
        private readonly JsonStore<Ausencia> _ausencias;

        public DataService(IWebHostEnvironment env)
        {
            _orientadores = new JsonStore<Orientador>("orientadores.json", env);
            _procesos = new JsonStore<Proceso>("procesos.json", env);
            _asignaciones = new JsonStore<Asignacion>("asignaciones.json", env);
            _ausencias = new JsonStore<Ausencia>("ausencias.json", env);

            SeedIfEmpty();
        }

        // ---------- Orientadores ----------
        public List<Orientador> GetOrientadores() => _orientadores.LoadAll().OrderBy(o => o.Nombre).ToList();

        public Orientador? GetOrientador(int id) => GetOrientadores().FirstOrDefault(o => o.Id == id);

        public void SaveOrientador(Orientador orientador)
        {
            var all = _orientadores.LoadAll();
            if (orientador.Id == 0)
            {
                orientador.Id = all.Count == 0 ? 1 : all.Max(o => o.Id) + 1;
                all.Add(orientador);
            }
            else
            {
                var idx = all.FindIndex(o => o.Id == orientador.Id);
                if (idx >= 0) all[idx] = orientador;
            }
            _orientadores.SaveAll(all);
        }

        public void DeleteOrientador(int id)
        {
            var all = _orientadores.LoadAll();
            all.RemoveAll(o => o.Id == id);
            _orientadores.SaveAll(all);
        }

        // ---------- Procesos ----------
        public List<Proceso> GetProcesos() => _procesos.LoadAll().OrderBy(p => p.Nombre).ToList();

        public Proceso? GetProceso(int id) => GetProcesos().FirstOrDefault(p => p.Id == id);

        public void SaveProceso(Proceso proceso)
        {
            var all = _procesos.LoadAll();
            if (proceso.Id == 0)
            {
                proceso.Id = all.Count == 0 ? 1 : all.Max(p => p.Id) + 1;
                all.Add(proceso);
            }
            else
            {
                var idx = all.FindIndex(p => p.Id == proceso.Id);
                if (idx >= 0) all[idx] = proceso;
            }
            _procesos.SaveAll(all);
        }

        public void DeleteProceso(int id)
        {
            var all = _procesos.LoadAll();
            all.RemoveAll(p => p.Id == id);
            _procesos.SaveAll(all);
        }

        // ---------- Asignaciones ----------
        public List<Asignacion> GetAsignaciones()
        {
            var asignaciones = _asignaciones.LoadAll();
            var orientadores = GetOrientadores().ToDictionary(o => o.Id);
            var procesos = GetProcesos().ToDictionary(p => p.Id);

            foreach (var a in asignaciones)
            {
                a.OrientadorNombre = orientadores.TryGetValue(a.OrientadorId, out var o) ? o.Nombre : "(eliminado)";
                a.ProcesoNombre = procesos.TryGetValue(a.ProcesoId, out var p) ? p.Nombre : "(eliminado)";
            }

            return asignaciones.OrderBy(a => a.Fecha).ThenBy(a => a.HoraInicio).ToList();
        }

        public Asignacion? GetAsignacion(int id) => GetAsignaciones().FirstOrDefault(a => a.Id == id);

        public void SaveAsignacion(Asignacion asignacion)
        {
            var all = _asignaciones.LoadAll();
            if (asignacion.Id == 0)
            {
                asignacion.Id = all.Count == 0 ? 1 : all.Max(a => a.Id) + 1;
                all.Add(asignacion);
            }
            else
            {
                var idx = all.FindIndex(a => a.Id == asignacion.Id);
                if (idx >= 0) all[idx] = asignacion;
            }
            _asignaciones.SaveAll(all);
        }

        public void DeleteAsignacion(int id)
        {
            var all = _asignaciones.LoadAll();
            all.RemoveAll(a => a.Id == id);
            _asignaciones.SaveAll(all);
        }

        // ---------- Ausencias ----------
        public List<Ausencia> GetAusencias() => _ausencias.LoadAll().OrderBy(a => a.FechaInicio).ToList();

        public Ausencia? GetAusencia(int id) => GetAusencias().FirstOrDefault(a => a.Id == id);

        // Devuelve la lista de conflictos nuevos (asignaciones que quedaron en choque) para poder avisar
        public List<Asignacion> SaveAusenciaYDetectarConflictos(Ausencia ausencia)
        {
            var all = _ausencias.LoadAll();
            if (ausencia.Id == 0)
            {
                ausencia.Id = all.Count == 0 ? 1 : all.Max(a => a.Id) + 1;
                all.Add(ausencia);
            }
            else
            {
                var idx = all.FindIndex(a => a.Id == ausencia.Id);
                if (idx >= 0) all[idx] = ausencia;
            }
            _ausencias.SaveAll(all);

            // Buscar asignaciones de ese orientador que caigan dentro del rango de ausencia
            var asignaciones = _asignaciones.LoadAll();
            var conflictos = asignaciones.Where(a =>
                    a.OrientadorId == ausencia.OrientadorId &&
                    a.Fecha.Date >= ausencia.FechaInicio.Date &&
                    a.Fecha.Date <= ausencia.FechaFin.Date &&
                    a.Estado != EstadoAsignacion.Cancelado &&
                    a.Estado != EstadoAsignacion.Sustituido)
                .ToList();

            foreach (var c in conflictos)
            {
                c.Estado = EstadoAsignacion.EnConflicto;
            }
            if (conflictos.Count > 0)
            {
                _asignaciones.SaveAll(asignaciones);
            }

            return GetAsignaciones().Where(a => conflictos.Select(c => c.Id).Contains(a.Id)).ToList();
        }

        public void DeleteAusencia(int id)
        {
            var all = _ausencias.LoadAll();
            all.RemoveAll(a => a.Id == id);
            _ausencias.SaveAll(all);
        }

        public bool EstaAusente(int orientadorId, DateTime fecha)
        {
            return GetAusencias().Any(a => a.OrientadorId == orientadorId &&
                                            fecha.Date >= a.FechaInicio.Date &&
                                            fecha.Date <= a.FechaFin.Date);
        }

        // ---------- Motor de reparto equitativo ----------
        // Sugiere, entre los orientadores disponibles ese día/hora, al que tiene menos horas
        // acumuladas en la semana de la fecha dada (para repartir la carga de forma equitativa).
        public List<(Orientador Orientador, double HorasAsignadasSemana, bool Disponible, string Motivo)> SugerirOrientadores(DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            var orientadores = GetOrientadores().Where(o => o.Activo).ToList();
            var asignaciones = GetAsignaciones();
            var inicioSemana = fecha.AddDays(-(int)fecha.DayOfWeek);
            var finSemana = inicioSemana.AddDays(6);

            var resultado = new List<(Orientador, double, bool, string)>();

            foreach (var o in orientadores)
            {
                var horasSemana = asignaciones
                    .Where(a => a.OrientadorId == o.Id &&
                                a.Fecha.Date >= inicioSemana.Date && a.Fecha.Date <= finSemana.Date &&
                                a.Estado != EstadoAsignacion.Cancelado)
                    .Sum(a => (a.HoraFin - a.HoraInicio).TotalHours);

                bool disponible = true;
                string motivo = "Disponible";

                if (!o.DiasLaborales.Contains(fecha.DayOfWeek))
                {
                    disponible = false;
                    motivo = "No labora ese día según su jornada declarada";
                }
                else if (horaInicio < o.HoraInicio || horaFin > o.HoraFin)
                {
                    disponible = false;
                    motivo = "Fuera de su horario declarado";
                }
                else if (EstaAusente(o.Id, fecha))
                {
                    disponible = false;
                    motivo = "Registrado como ausente (vacaciones/incapacidad/cita) ese día";
                }
                else if (asignaciones.Any(a => a.OrientadorId == o.Id && a.Fecha.Date == fecha.Date &&
                                                a.Estado != EstadoAsignacion.Cancelado &&
                                                horaInicio < a.HoraFin && horaFin > a.HoraInicio))
                {
                    disponible = false;
                    motivo = "Ya tiene otro proceso que choca en ese horario";
                }
                else if (horasSemana + (horaFin - horaInicio).TotalHours > o.HorasSemanalesContrato)
                {
                    disponible = false;
                    motivo = $"Excedería sus {o.HorasSemanalesContrato}h semanales de contrato";
                }

                resultado.Add((o, horasSemana, disponible, motivo));
            }

            // Primero los disponibles, ordenados por menor carga acumulada (reparto equitativo)
            return resultado
                .OrderByDescending(r => r.Item3)
                .ThenBy(r => r.Item2)
                .ToList();
        }

        private void SeedIfEmpty()
        {
            if (_orientadores.LoadAll().Count == 0)
            {
                var seed = new List<Orientador>
                {
                    new() { Id = 1, Nombre = "Magda Lorena Artavia Castrillo", Sede = "Sede Central", Correo = "magda@uned.ac.cr", TipoJornada = TipoJornada.TiempoCompleto, DiasLaborales = new(){DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday}, HoraInicio = new(8,0,0), HoraFin = new(16,0,0) },
                    new() { Id = 2, Nombre = "Luis Carlos", Sede = "Sede Central", Correo = "luiscarlos@uned.ac.cr", TipoJornada = TipoJornada.TiempoCompleto, DiasLaborales = new(){DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday}, HoraInicio = new(8,0,0), HoraFin = new(16,0,0) },
                    new() { Id = 3, Nombre = "Andrea", Sede = "Desamparados", Correo = "andrea@uned.ac.cr", TipoJornada = TipoJornada.TiempoCompleto, DiasLaborales = new(){DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday}, HoraInicio = new(8,0,0), HoraFin = new(16,0,0) },
                    new() { Id = 4, Nombre = "Karol (Carol) Vieto", Sede = "Escazú", Correo = "karol@uned.ac.cr", TipoJornada = TipoJornada.TresCuartos, DiasLaborales = new(){DayOfWeek.Tuesday,DayOfWeek.Wednesday,DayOfWeek.Thursday,DayOfWeek.Friday,DayOfWeek.Saturday}, HoraInicio = new(9,0,0), HoraFin = new(15,0,0) },
                    new() { Id = 5, Nombre = "Alba Iris", Sede = "Liberia", Correo = "alba@uned.ac.cr", TipoJornada = TipoJornada.Medio, DiasLaborales = new(){DayOfWeek.Monday,DayOfWeek.Wednesday,DayOfWeek.Friday}, HoraInicio = new(14,0,0), HoraFin = new(18,0,0) },
                    new() { Id = 6, Nombre = "Carlos Daniel", Sede = "Cartago", Correo = "carlosd@uned.ac.cr", TipoJornada = TipoJornada.Medio, DiasLaborales = new(){DayOfWeek.Monday,DayOfWeek.Tuesday,DayOfWeek.Thursday}, HoraInicio = new(18,0,0), HoraFin = new(21,0,0) },
                    new() { Id = 7, Nombre = "Jolien (Jolyn)", Sede = "Sarapiquí", Correo = "jolien@uned.ac.cr", TipoJornada = TipoJornada.UnCuarto, DiasLaborales = new(){DayOfWeek.Thursday, DayOfWeek.Saturday}, HoraInicio = new(14,0,0), HoraFin = new(18,0,0) },
                    new() { Id = 8, Nombre = "Anthony Herrera", Sede = "Sede Central", Correo = "anthonyhch12@gmail.com", TipoJornada = TipoJornada.UnCuarto, DiasLaborales = new(){DayOfWeek.Friday, DayOfWeek.Saturday}, HoraInicio = new(18,0,0), HoraFin = new(20,30,0) },
                };
                _orientadores.SaveAll(seed);
            }

            if (_procesos.LoadAll().Count == 0)
            {
                var seed = new List<Proceso>
                {
                    new() { Id = 1, Nombre = "Asesoría de admisión y matrícula", Descripcion = "Para estudiantes aspirantes, semana previa a matrícula", DuracionMinutos = 60, Modalidad = Modalidad.Virtual, RequiereEnlace = true },
                    new() { Id = 2, Nombre = "Consultas en matrícula", Descripcion = "Espacio sincrónico durante la semana de matrícula", DuracionMinutos = 60, Modalidad = Modalidad.Virtual, RequiereEnlace = true },
                    new() { Id = 3, Nombre = "Cita de orientación e inducción académica", Descripcion = "Inducción a la UNED para estudiantes de primer ingreso", DuracionMinutos = 90, Modalidad = Modalidad.Virtual, RequiereEnlace = true },
                    new() { Id = 4, Nombre = "Taller de técnicas de estudio", Descripcion = "Proceso psicoeducativo, eje académico", DuracionMinutos = 120, Modalidad = Modalidad.Virtual, RequiereEnlace = true },
                    new() { Id = 5, Nombre = "Atención individual", Descripcion = "Atención a dudas vocacionales, académicas o personales", DuracionMinutos = 45, Modalidad = Modalidad.Virtual, RequiereEnlace = true },
                    new() { Id = 6, Nombre = "Proceso presencial en sede", Descripcion = "Procesos grupales presenciales según calendarización por sede", DuracionMinutos = 120, Modalidad = Modalidad.Presencial, RequiereEnlace = false },
                    new() { Id = 7, Nombre = "Atención a población privada de libertad", Descripcion = "Siempre presencial, requiere coordinación de accesos", DuracionMinutos = 120, Modalidad = Modalidad.Presencial, RequiereEnlace = false },
                };
                _procesos.SaveAll(seed);
            }
        }
    }
}

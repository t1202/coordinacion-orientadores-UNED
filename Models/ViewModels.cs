namespace CoordinacionOrientadores.Models
{
    // Paso 1: datos del proceso/horario que se quiere programar
    public class NuevaAsignacionInput
    {
        public int ProcesoId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Sede { get; set; } = string.Empty;
        public string? EnlaceReunion { get; set; }
        public string? Notas { get; set; }
    }

    // Paso 2: una fila de la tabla de sugerencias (orientador + disponibilidad)
    public class SugerenciaOrientadorVM
    {
        public int OrientadorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public double HorasAsignadasSemana { get; set; }
        public double HorasContrato { get; set; }
        public bool Disponible { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class SugerenciasVM
    {
        public NuevaAsignacionInput Datos { get; set; } = new();
        public string ProcesoNombre { get; set; } = string.Empty;
        public List<SugerenciaOrientadorVM> Sugerencias { get; set; } = new();
    }
}

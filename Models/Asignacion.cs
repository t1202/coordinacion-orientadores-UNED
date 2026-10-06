using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoordinacionOrientadores.Models
{
    // Una sesión/proceso concreto, programado para un orientador en una fecha y hora
    public class Asignacion
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Proceso")]
        public int ProcesoId { get; set; }

        [Required]
        [Display(Name = "Orientador")]
        public int OrientadorId { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Display(Name = "Hora de inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Display(Name = "Hora de fin")]
        public TimeSpan HoraFin { get; set; }

        [Display(Name = "Sede / lugar")]
        public string Sede { get; set; } = string.Empty;

        [Display(Name = "Enlace de reunión")]
        [Url(ErrorMessage = "Debe ser una URL válida (Teams, Zoom, etc.)")]
        public string? EnlaceReunion { get; set; }

        [Display(Name = "Estado")]
        public EstadoAsignacion Estado { get; set; } = EstadoAsignacion.Programado;

        // Si esta asignación sustituye a otra persona por ausencia, referencia al orientador original
        [Display(Name = "Sustituye a (orientador original)")]
        public int? OrientadorOriginalId { get; set; }

        [Display(Name = "Notas")]
        public string? Notas { get; set; }

        // No persistido: usado solo para mostrar en vistas con los nombres ya resueltos
        [JsonIgnore]
        public string? ProcesoNombre { get; set; }
        [JsonIgnore]
        public string? OrientadorNombre { get; set; }
    }
}

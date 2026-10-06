using System.ComponentModel.DataAnnotations;

namespace CoordinacionOrientadores.Models
{
    // Vacaciones, incapacidades o citas que un orientador registra él mismo
    public class Ausencia
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Orientador")]
        public int OrientadorId { get; set; }

        [Required]
        [Display(Name = "Tipo de ausencia")]
        public TipoAusencia Tipo { get; set; } = TipoAusencia.Vacaciones;

        [Required]
        [Display(Name = "Fecha de inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Fecha de fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; } = DateTime.Today;

        [Display(Name = "Comentario")]
        public string? Comentario { get; set; }

        [Display(Name = "Registrada el")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}

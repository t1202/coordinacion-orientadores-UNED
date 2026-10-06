using System.ComponentModel.DataAnnotations;

namespace CoordinacionOrientadores.Models
{
    public class Orientador
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La sede es obligatoria")]
        [Display(Name = "Sede")]
        public string Sede { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Tipo de jornada")]
        public TipoJornada TipoJornada { get; set; } = TipoJornada.TiempoCompleto;

        // Días de la semana en que labora: 0=Domingo ... 6=Sábado
        [Display(Name = "Días laborales")]
        public List<DayOfWeek> DiasLaborales { get; set; } = new();

        [Display(Name = "Hora de inicio de jornada")]
        public TimeSpan HoraInicio { get; set; } = new TimeSpan(8, 0, 0);

        [Display(Name = "Hora de fin de jornada")]
        public TimeSpan HoraFin { get; set; } = new TimeSpan(16, 0, 0);

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Horas semanales disponibles según el tipo de jornada (referencia para el reparto equitativo)
        public double HorasSemanalesContrato => TipoJornada switch
        {
            TipoJornada.TiempoCompleto => 40,
            TipoJornada.TresCuartos => 30,
            TipoJornada.Medio => 20,
            TipoJornada.UnCuarto => 10,
            _ => 0
        };
    }
}

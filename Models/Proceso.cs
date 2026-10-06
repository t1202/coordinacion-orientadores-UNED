using System.ComponentModel.DataAnnotations;

namespace CoordinacionOrientadores.Models
{
    // Catálogo de tipos de proceso que ofrece el programa de Orientación
    // (asesorías, citas de inducción, talleres, atención individual, etc.)
    public class Proceso
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del proceso es obligatorio")]
        [Display(Name = "Nombre del proceso")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Duración (minutos)")]
        [Range(1, 480)]
        public int DuracionMinutos { get; set; } = 60;

        [Display(Name = "Modalidad")]
        public Modalidad Modalidad { get; set; } = Modalidad.Virtual;

        [Display(Name = "Requiere enlace de reunión")]
        public bool RequiereEnlace { get; set; } = true;
    }
}

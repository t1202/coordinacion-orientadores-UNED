namespace CoordinacionOrientadores.Models
{
    public enum TipoJornada
    {
        TiempoCompleto,   // 40 h/semana aprox
        TresCuartos,      // 30 h/semana aprox
        Medio,            // 20 h/semana aprox
        UnCuarto          // 10 h/semana aprox
    }

    public enum TipoAusencia
    {
        Vacaciones,
        Incapacidad,
        CitaMedica,
        Permiso,
        Otro
    }

    public enum EstadoAsignacion
    {
        Programado,
        EnConflicto,   // el orientador asignado quedó ausente, necesita sustituto
        Sustituido,    // ya se le encontró sustituto
        Cancelado,
        Realizado
    }

    public enum Modalidad
    {
        Virtual,
        Presencial
    }
}

namespace Mastermind;

public enum EstadoCheat
{
    NoUsado,
    Aplicado,
    SinSuerte,
    Rechazado
}

public sealed record ResultadoCheat(EstadoCheat Estado, string? Mensaje = null, int? Posicion = null);

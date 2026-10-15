namespace Mastermind;

public sealed record ResultadoEvaluacion(
    int BienPosicionados,
    int ColorCorrecto,
    ResultadoCheat? Cheat = null);

using Mastermind;

namespace Mastermind.Tests;

public sealed class EvaluadorMastermindTests
{
    private readonly EvaluadorMastermind _evaluador = new();

    [Theory]
    [InlineData("RGBY", "RGBY", 4, 0)]
    [InlineData("RGBY", "RBYG", 1, 3)]
    [InlineData("ABCD", "EFGH", 0, 0)]
    [InlineData("RRGB", "RGRR", 1, 2)]
    public void EvaluarIntento_CuentaCoincidenciasSinDobleConteo(
        string patronSecreto,
        string intentoJugador,
        int bienPosicionadosEsperados,
        int colorCorrectoEsperado)
    {
        var resultado = _evaluador.EvaluarIntento(patronSecreto, intentoJugador);

        Assert.Equal(bienPosicionadosEsperados, resultado.BienPosicionados);
        Assert.Equal(colorCorrectoEsperado, resultado.ColorCorrecto);
    }
}

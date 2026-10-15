namespace Mastermind;

public sealed class EvaluadorMastermind
{
    public ResultadoEvaluacion EvaluarIntento(string patronSecreto, string intentoJugador)
    {
        ArgumentNullException.ThrowIfNull(patronSecreto);
        ArgumentNullException.ThrowIfNull(intentoJugador);

        if (patronSecreto.Length != intentoJugador.Length)
        {
            throw new ArgumentException("El patrón secreto y el intento deben tener la misma longitud.");
        }

        var bienPosicionados = 0;
        var frecuenciasRestantes = new Dictionary<char, int>();

        for (var indice = 0; indice < patronSecreto.Length; indice++)
        {
            if (patronSecreto[indice] == intentoJugador[indice])
            {
                bienPosicionados++;
                continue;
            }

            var color = patronSecreto[indice];
            frecuenciasRestantes[color] = frecuenciasRestantes.GetValueOrDefault(color) + 1;
        }

        var colorCorrecto = 0;
        for (var indice = 0; indice < intentoJugador.Length; indice++)
        {
            if (patronSecreto[indice] == intentoJugador[indice])
            {
                continue;
            }

            var color = intentoJugador[indice];
            if (frecuenciasRestantes.TryGetValue(color, out var frecuencia) && frecuencia > 0)
            {
                colorCorrecto++;
                frecuenciasRestantes[color] = frecuencia - 1;
            }
        }

        return new ResultadoEvaluacion(bienPosicionados, colorCorrecto);
    }
}

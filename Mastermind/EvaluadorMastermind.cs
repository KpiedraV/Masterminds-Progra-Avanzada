using System.Globalization;
using Mastermind.Aleatoriedad;
using Mastermind.Cheats;
using Mastermind.Configuracion;
using Mastermind.Logging;

namespace Mastermind;

public sealed class EvaluadorMastermind
{
    private readonly IConfiguracionColores? _configuracionColores;
    private readonly ILogEvaluacion _log;
    private readonly Func<DateTime> _reloj;
    private readonly ICheatCodeRepository? _cheatCodeRepository;
    private readonly IGeneradorAleatorio _generadorAleatorio;
    private readonly SesionPartida _sesion;

    public EvaluadorMastermind(
        IConfiguracionColores? configuracionColores = null,
        ILogEvaluacion? log = null,
        Func<DateTime>? reloj = null,
        ICheatCodeRepository? cheatCodeRepository = null,
        IGeneradorAleatorio? generadorAleatorio = null,
        SesionPartida? sesion = null)
    {
        _configuracionColores = configuracionColores;
        _log = log ?? new LogEvaluacionArchivo(Path.Combine(AppContext.BaseDirectory, "evaluaciones.log"));
        _reloj = reloj ?? (() => DateTime.Now);
        _cheatCodeRepository = cheatCodeRepository;
        _generadorAleatorio = generadorAleatorio ?? new GeneradorAleatorio();
        _sesion = sesion ?? new SesionPartida();
    }

    public ResultadoEvaluacion EvaluarIntento(string patronSecreto, string intentoJugador, string? cheatCode = null)
    {
        ValidarEntradas(patronSecreto, intentoJugador);

        var indiceRevelado = -1;
        var resultadoCheat = new ResultadoCheat(EstadoCheat.NoUsado);

        if (cheatCode is not null)
        {
            resultadoCheat = EvaluarCheat(cheatCode, patronSecreto, intentoJugador, out indiceRevelado);
        }

        var resultado = CalcularResultado(patronSecreto, intentoJugador, indiceRevelado, resultadoCheat);
        RegistrarEvaluacion(patronSecreto, intentoJugador, resultado);
        return resultado;
    }

    private void ValidarEntradas(string patronSecreto, string intentoJugador)
    {
        if (string.IsNullOrEmpty(patronSecreto))
        {
            throw new ArgumentException("El patrón secreto no puede ser nulo o vacío.", nameof(patronSecreto));
        }

        if (string.IsNullOrEmpty(intentoJugador))
        {
            throw new ArgumentException("El intento del jugador no puede ser nulo o vacío.", nameof(intentoJugador));
        }

        if (patronSecreto.Length is not (4 or 6))
        {
            throw new ArgumentException("El patrón secreto debe tener 4 o 6 colores.", nameof(patronSecreto));
        }

        if (intentoJugador.Length is not (4 or 6))
        {
            throw new ArgumentException("El intento del jugador debe tener 4 o 6 colores.", nameof(intentoJugador));
        }

        if (patronSecreto.Length != intentoJugador.Length)
        {
            throw new ArgumentException("El patrón secreto y el intento deben tener la misma longitud.");
        }

        if (_configuracionColores is null)
        {
            return;
        }

        ValidarColores(patronSecreto, "El patrón secreto");
        ValidarColores(intentoJugador, "El intento del jugador");
    }

    private void ValidarColores(string secuencia, string nombreEntrada)
    {
        foreach (var codigo in secuencia)
        {
            if (!_configuracionColores!.CodigosValidos.Contains(codigo))
            {
                throw new ArgumentException(
                    $"{nombreEntrada} contiene un código de color no configurado: '{codigo}'.");
            }
        }
    }

    private ResultadoCheat EvaluarCheat(
        string cheatCode,
        string patronSecreto,
        string intentoJugador,
        out int indiceRevelado)
    {
        indiceRevelado = -1;
        var codigo = _cheatCodeRepository?.ObtenerPorHash(cheatCode);

        if (codigo is null)
        {
            return new ResultadoCheat(EstadoCheat.Rechazado, "El cheat code no existe o no hay un repositorio configurado.");
        }

        if (codigo.UsosDisponibles <= 0)
        {
            return new ResultadoCheat(EstadoCheat.Rechazado, "El cheat code no tiene usos disponibles.");
        }

        if (_sesion.CheatUsado)
        {
            return new ResultadoCheat(EstadoCheat.Rechazado, "El cheat code ya fue usado en esta partida.");
        }

        _sesion.MarcarCheatUsado();
        indiceRevelado = _generadorAleatorio.Next(patronSecreto.Length);

        if (patronSecreto[indiceRevelado] == intentoJugador[indiceRevelado])
        {
            var posicionYaAcertada = indiceRevelado + 1;
            indiceRevelado = -1;
            return new ResultadoCheat(
                EstadoCheat.SinSuerte,
                "Sin suerte: la posición elegida ya estaba acertada.",
                posicionYaAcertada);
        }

        return new ResultadoCheat(
            EstadoCheat.Aplicado,
            "Cheat code aplicado.",
            indiceRevelado + 1);
    }

    private static ResultadoEvaluacion CalcularResultado(
        string patronSecreto,
        string intentoJugador,
        int indiceRevelado,
        ResultadoCheat resultadoCheat)
    {
        var bienPosicionados = indiceRevelado >= 0 ? 1 : 0;
        var frecuenciasRestantes = new Dictionary<char, int>();

        for (var indice = 0; indice < patronSecreto.Length; indice++)
        {
            if (indice == indiceRevelado)
            {
                continue;
            }

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
            if (indice == indiceRevelado || patronSecreto[indice] == intentoJugador[indice])
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

        return new ResultadoEvaluacion(bienPosicionados, colorCorrecto, resultadoCheat);
    }

    private void RegistrarEvaluacion(
        string patronSecreto,
        string intentoJugador,
        ResultadoEvaluacion resultado)
    {
        var fecha = _reloj().ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        var mensaje =
            $"En la fecha {fecha} se evaluó el intento {intentoJugador} contra el patrón {patronSecreto}. " +
            $"Resultado: {resultado.BienPosicionados} bien posicionados – {resultado.ColorCorrecto} con color correcto.";
        _log.Registrar(mensaje);
    }
}

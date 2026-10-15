using Mastermind.Aleatoriedad;
using Mastermind.Cheats;
using Mastermind.Configuracion;
using Mastermind.Logging;

namespace Mastermind.Tests;

public sealed class EvaluadorMastermindAvanzadoTests
{
    private const string CheatValido = "CHEAT-VALIDO";
    private static readonly IConfiguracionColores Configuracion = new ConfiguracionColores(
        "RGBYCKPWNI1".Select(codigo => new DefinicionColor(codigo.ToString(), codigo.ToString())));

    [Theory]
    [InlineData(null, "RGBY")]
    [InlineData("", "RGBY")]
    [InlineData("RGBY", null)]
    [InlineData("RGBY", "")]
    [InlineData("RGB", "RGB")]
    [InlineData("RGBY", "RGBYK")]
    [InlineData("RGBY", "RGBYCK")]
    public void EvaluarIntento_RechazaEntradasNulasVaciasOConLongitudInvalida(
        string? patronSecreto,
        string? intentoJugador)
    {
        var evaluador = CrearEvaluador();

        Assert.Throws<ArgumentException>(() =>
            evaluador.EvaluarIntento(patronSecreto!, intentoJugador!));
    }

    [Theory]
    [InlineData("XGBY", "RGBY")]
    [InlineData("RGBY", "XGBY")]
    public void EvaluarIntento_RechazaCodigosDeColorNoConfigurados(
        string patronSecreto,
        string intentoJugador)
    {
        var evaluador = CrearEvaluador();

        var excepcion = Assert.Throws<ArgumentException>(() =>
            evaluador.EvaluarIntento(patronSecreto, intentoJugador));

        Assert.Contains("código de color no configurado", excepcion.Message);
    }

    [Fact]
    public void EvaluarIntento_UsaColoresDefinidosPorConfiguracion()
    {
        var coloresPersonalizados = new ConfiguracionColores(
        [
            new DefinicionColor("Amarillo", "A"),
            new DefinicionColor("Azul", "Z"),
            new DefinicionColor("Cian", "C"),
            new DefinicionColor("Dorado", "D")
        ]);
        var evaluador = CrearEvaluador(configuracionColores: coloresPersonalizados);

        var resultado = evaluador.EvaluarIntento("AZCD", "AZCD");

        Assert.Equal(4, resultado.BienPosicionados);
    }

    [Fact]
    public void EvaluarIntento_CheatValidoRevelaUnaPosicionYMejoraResultado()
    {
        var evaluador = CrearEvaluador(
            repositorio: new RepositorioFalso(new CodigoCheat(CheatValido, 1)),
            aleatorio: new AleatorioFalso(3));

        var resultado = evaluador.EvaluarIntento("RGBY", "RGBR", CheatValido);

        Assert.Equal(4, resultado.BienPosicionados);
        Assert.Equal(0, resultado.ColorCorrecto);
        Assert.Equal(EstadoCheat.Aplicado, resultado.Cheat!.Estado);
        Assert.Equal(4, resultado.Cheat.Posicion);
    }

    [Fact]
    public void EvaluarIntento_RechazaSegundoCheatEnLaMismaSesion()
    {
        var evaluador = CrearEvaluador(
            repositorio: new RepositorioFalso(new CodigoCheat(CheatValido, 2)),
            aleatorio: new AleatorioFalso(3));

        evaluador.EvaluarIntento("RGBY", "RGBR", CheatValido);
        var segundoResultado = evaluador.EvaluarIntento("RGBY", "RGBR", CheatValido);

        Assert.Equal(3, segundoResultado.BienPosicionados);
        Assert.Equal(EstadoCheat.Rechazado, segundoResultado.Cheat!.Estado);
        Assert.Contains("ya fue usado", segundoResultado.Cheat.Mensaje);
    }

    [Fact]
    public void EvaluarIntento_CheatSinSuerteNoMejoraResultadoPeroSeConsume()
    {
        var evaluador = CrearEvaluador(
            repositorio: new RepositorioFalso(new CodigoCheat(CheatValido, 1)),
            aleatorio: new AleatorioFalso(0));

        var primerResultado = evaluador.EvaluarIntento("RGBY", "R1CK", CheatValido);
        var segundoResultado = evaluador.EvaluarIntento("RGBY", "R1CK", CheatValido);

        Assert.Equal(1, primerResultado.BienPosicionados);
        Assert.Equal(EstadoCheat.SinSuerte, primerResultado.Cheat!.Estado);
        Assert.Equal(1, primerResultado.Cheat.Posicion);
        Assert.Equal(EstadoCheat.Rechazado, segundoResultado.Cheat!.Estado);
        Assert.Contains("ya fue usado", segundoResultado.Cheat.Mensaje);
    }

    [Theory]
    [InlineData(false, 1, "no existe")]
    [InlineData(true, 0, "no tiene usos disponibles")]
    public void EvaluarIntento_RechazaCheatInexistenteOSinUsos(
        bool existe,
        int usosDisponibles,
        string mensajeEsperado)
    {
        var codigo = existe ? new CodigoCheat(CheatValido, usosDisponibles) : null;
        var evaluador = CrearEvaluador(repositorio: new RepositorioFalso(codigo));

        var resultado = evaluador.EvaluarIntento("RGBY", "RGBR", CheatValido);

        Assert.Equal(EstadoCheat.Rechazado, resultado.Cheat!.Estado);
        Assert.Contains(mensajeEsperado, resultado.Cheat.Mensaje);
    }

    [Fact]
    public void EvaluarIntento_RegistraElFormatoExactoConRelojFijo()
    {
        var log = new LogEnMemoria();
        var relojFijo = new DateTime(2025, 3, 7, 9, 5, 4);
        var evaluador = CrearEvaluador(log: log, reloj: () => relojFijo);

        evaluador.EvaluarIntento("RGBY", "RBYG");

        Assert.Equal(
            "En la fecha 07-03-2025 09:05:04 se evaluó el intento RBYG contra el patrón RGBY. " +
            "Resultado: 1 bien posicionados – 3 con color correcto.",
            Assert.Single(log.Mensajes));
    }

    private static EvaluadorMastermind CrearEvaluador(
        IConfiguracionColores? configuracionColores = null,
        ILogEvaluacion? log = null,
        Func<DateTime>? reloj = null,
        ICheatCodeRepository? repositorio = null,
        IGeneradorAleatorio? aleatorio = null,
        SesionPartida? sesion = null)
    {
        return new EvaluadorMastermind(
            configuracionColores ?? Configuracion,
            log ?? new LogEnMemoria(),
            reloj ?? (() => DateTime.UnixEpoch),
            repositorio,
            aleatorio ?? new AleatorioFalso(0),
            sesion ?? new SesionPartida());
    }

    private sealed class LogEnMemoria : ILogEvaluacion
    {
        public List<string> Mensajes { get; } = [];

        public void Registrar(string mensaje)
        {
            Mensajes.Add(mensaje);
        }
    }

    private sealed class RepositorioFalso(CodigoCheat? codigo) : ICheatCodeRepository
    {
        public CodigoCheat? ObtenerPorHash(string hash)
        {
            return string.Equals(hash, codigo?.Hash, StringComparison.Ordinal) ? codigo : null;
        }
    }

    private sealed class AleatorioFalso(int indice) : IGeneradorAleatorio
    {
        public int Next(int maximoExclusivo)
        {
            Assert.InRange(indice, 0, maximoExclusivo - 1);
            return indice;
        }
    }
}

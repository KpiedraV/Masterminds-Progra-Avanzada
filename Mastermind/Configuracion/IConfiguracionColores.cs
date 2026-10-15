namespace Mastermind.Configuracion;

public interface IConfiguracionColores
{
    IReadOnlySet<char> CodigosValidos { get; }
}

namespace Mastermind.Aleatoriedad;

public sealed class GeneradorAleatorio : IGeneradorAleatorio
{
    private readonly Random _random;

    public GeneradorAleatorio(int? semilla = null)
    {
        _random = semilla.HasValue ? new Random(semilla.Value) : new Random();
    }

    public int Next(int maximoExclusivo)
    {
        return _random.Next(maximoExclusivo);
    }
}

using System.Text.Json;

namespace Mastermind.Cheats;

public sealed class RepositorioCheatCodesJson : ICheatCodeRepository
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _rutaArchivo;

    public RepositorioCheatCodesJson(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        _rutaArchivo = rutaArchivo;
    }

    public CodigoCheat? ObtenerPorHash(string hash)
    {
        ArgumentNullException.ThrowIfNull(hash);

        var json = File.ReadAllText(_rutaArchivo);
        var codigos = JsonSerializer.Deserialize<List<CodigoCheat>>(json, OpcionesJson)
            ?? throw new InvalidDataException("El archivo de cheat codes no contiene una lista válida.");

        return codigos.FirstOrDefault(codigo =>
            string.Equals(codigo.Hash, hash, StringComparison.Ordinal));
    }
}

using System.Text.Json;

namespace Mastermind.Configuracion;

public sealed class ConfiguracionColores : IConfiguracionColores
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlySet<char> CodigosValidos { get; }

    public ConfiguracionColores(IEnumerable<DefinicionColor> colores)
    {
        ArgumentNullException.ThrowIfNull(colores);

        var listaColores = colores.ToList();
        if (listaColores.Count == 0)
        {
            throw new ArgumentException("Debe configurarse al menos un color.", nameof(colores));
        }

        foreach (var color in listaColores)
        {
            if (color is null || string.IsNullOrWhiteSpace(color.Nombre))
            {
                throw new ArgumentException("Cada color debe tener un nombre.", nameof(colores));
            }

            if (color.Codigo is null || color.Codigo.Length != 1)
            {
                throw new ArgumentException("Cada color debe tener un código de exactamente una letra o dígito.", nameof(colores));
            }
        }

        var codigos = listaColores.Select(color => color.Codigo[0]).ToHashSet();
        if (codigos.Count != listaColores.Count)
        {
            throw new ArgumentException("Los códigos de color deben ser únicos.", nameof(colores));
        }

        CodigosValidos = codigos;
    }

    public static ConfiguracionColores DesdeArchivo(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        var json = File.ReadAllText(rutaArchivo);
        var colores = JsonSerializer.Deserialize<List<DefinicionColor>>(json, OpcionesJson)
            ?? throw new InvalidDataException("El archivo de colores no contiene una lista válida.");

        return new ConfiguracionColores(colores);
    }
}

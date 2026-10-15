namespace Mastermind.Logging;

public sealed class LogEvaluacionArchivo : ILogEvaluacion
{
    private readonly string _rutaArchivo;

    public LogEvaluacionArchivo(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        _rutaArchivo = rutaArchivo;
    }

    public void Registrar(string mensaje)
    {
        ArgumentNullException.ThrowIfNull(mensaje);
        File.AppendAllText(_rutaArchivo, mensaje + Environment.NewLine);
    }
}

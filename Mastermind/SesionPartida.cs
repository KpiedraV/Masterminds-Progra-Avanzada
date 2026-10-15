namespace Mastermind;

public sealed class SesionPartida
{
    public bool CheatUsado { get; private set; }

    internal void MarcarCheatUsado()
    {
        CheatUsado = true;
    }
}

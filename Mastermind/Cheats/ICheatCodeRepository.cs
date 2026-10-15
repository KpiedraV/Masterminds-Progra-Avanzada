namespace Mastermind.Cheats;

public interface ICheatCodeRepository
{
    CodigoCheat? ObtenerPorHash(string hash);
}

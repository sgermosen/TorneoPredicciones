using SQLite.Net.Interop;

namespace TorneoPredicciones.Interfaces
{

    public interface IConfig
    {
        string DirectoryDB { get; }

        ISQLitePlatform Platform { get; }
    }

}

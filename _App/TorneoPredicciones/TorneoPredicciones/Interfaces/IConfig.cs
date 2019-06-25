using SQLite.Net.Interop;

namespace TorneoPredicciones.Interfaces
{

    public interface IConfig
    {
        string DirectoryDb { get; }

        ISQLitePlatform Platform { get; }
    }

}

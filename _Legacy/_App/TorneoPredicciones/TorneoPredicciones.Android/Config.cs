using SQLite.Net.Interop;
using TorneoPredicciones.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(TorneoPredicciones.Droid.Config))]
namespace TorneoPredicciones.Droid
{
    public class Config : IConfig
    {
        private string _directoryDb;
        private ISQLitePlatform _platform;

        public string DirectoryDb
        {
            get {
                if (!string.IsNullOrEmpty(_directoryDb)) return _directoryDb;
                _directoryDb = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

                return _directoryDb;
            }
        }

        public ISQLitePlatform Platform => _platform ?? (_platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid());

        //private string directoryDB;
        //private ISQLitePlatform platform;

        //public string DirectoryDB
        //{
        //    get {
        //        if (string.IsNullOrEmpty(directoryDB))
        //        {
        //            directoryDB = System.Environment.GetFolderPath(
        //                System.Environment.SpecialFolder.Personal);
        //        }

        //        return directoryDB;
        //    }
        //}

        //public ISQLitePlatform Platform
        //{
        //    get {
        //        if (platform == null)
        //        {
        //            platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
        //        }

        //        return platform;
        //    }
        //}

    }
}
 
using Gcm.Client;
using Xamarin.Forms;
using Android.Util;
using TorneoPredicciones.Interfaces;

[assembly: Dependency(typeof(TorneoPredicciones.Droid.RegistrationDevice))]
namespace TorneoPredicciones.Droid
{
    public class RegistrationDevice : IRegisterDevice
    {
        #region Methods
        public void RegisterDevice()
        {
            var mainActivity = MainActivity.GetInstance();
            GcmClient.CheckDevice(mainActivity);
            GcmClient.CheckManifest(mainActivity);

            Log.Info("MainActivity", "Registering...");
            GcmClient.Register(mainActivity, Constants.SenderId);
        }
        #endregion
    }

}
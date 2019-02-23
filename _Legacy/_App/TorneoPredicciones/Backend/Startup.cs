using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Backend.Startup))]
namespace Backend
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

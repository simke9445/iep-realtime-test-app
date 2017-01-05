using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RealtimeTestApp.Startup))]
namespace RealtimeTestApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}

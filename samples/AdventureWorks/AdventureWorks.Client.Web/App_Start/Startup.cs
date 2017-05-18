using Owin;
using Xomega.Framework;

namespace AdventureWorks.Client.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AppInitializer.Initalize(new WebAppInit());
            app.Use<DIMiddleware>();
            ConfigureAuth(app);
        }
    }
}

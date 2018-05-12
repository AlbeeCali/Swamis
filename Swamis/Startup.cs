using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Swamis.Startup))]
namespace Swamis
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

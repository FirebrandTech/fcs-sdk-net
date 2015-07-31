using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CredentialsMvcTests.Startup))]
namespace CredentialsMvcTests
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

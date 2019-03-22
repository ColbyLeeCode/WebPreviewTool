using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebPreviewTool.Startup))]
namespace WebPreviewTool
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

//http://stackoverflow.com/questions/31479778/asp-net-mvc-owin-and-signalr-two-startup-cs-files
namespace WebApplication1 {
    public partial class Startup {
        public void ConfigSignalR(IAppBuilder app) {
            
                // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
                app.MapSignalR();
            
        }
    }
}

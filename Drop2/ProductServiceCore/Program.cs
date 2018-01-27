using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
//using Microsoft.AspNetCore.Hosting.Internal;

namespace ServiceSharedCore
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    var host = new WebHostBuilder()
        //        .UseKestrel()
        //        .UseContentRoot(Directory.GetCurrentDirectory())
        //        .UseIISIntegration()
        //        .UseStartup<Startup>()
        //        .Build();

        //    host.Run();

        //    //var host = new WebHostBuilder()
        //    //    .UseKestrel()
        //    //    .UseContentRoot(Directory.GetCurrentDirectory())
        //    //    .UseIISIntegration()
        //    //    .UseStartup<Startup>()
        //    //    .UseApplicationInsights()
        //    //    .CaptureStartupErrors(true)
        //    //    .UseSetting("detailedErrors", "true")
        //    //    .Build();

        //    //host.Run();
        //}

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}

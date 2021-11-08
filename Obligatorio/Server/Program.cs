using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ServerProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {


            Console.WriteLine("LLEGA");
            ServerProtocolTCP server = new ServerProtocolTCP();
            await server.RunServer();
            CreateHostBuilder(args).Build().Run();
            Console.WriteLine("Closing server application.");
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Text;
using ProtocolLibrary;
using ConsoleClientProtocol.Protocol;
using System.Threading.Tasks;

namespace ConsoleClientProtocol
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ClientProtocolTCP();
            await client.ConnectToServer();
            Console.WriteLine("Shutting down the application.");
        }
    }
}
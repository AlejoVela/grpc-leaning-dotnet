using Grpc.Core;
using PrimeNumber;
using System;
using System.IO;

namespace server
{
    class Program
    {
        const int port = 50051;
        static void Main(string[] args)
        {
            Server server = null;
            try
            {
                server = new Server() 
                { 
                    Services = {NumberService.BindService(new NumberServiceImp())},
                    Ports = {new ServerPort("localhost", port, ServerCredentials.Insecure)}
                };
                server.Start();
                Console.WriteLine($"The server in listening on localhost:{port}");
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine($"The server failed to start: {e.Message}");
            }
            finally
            {
                if (server != null) server.ShutdownAsync().Wait();
            }
        }
    }
}

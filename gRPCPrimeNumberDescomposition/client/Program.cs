using Grpc.Core;
using PrimeNumber;
using System;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50051";
        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);
            await channel.ConnectAsync().ContinueWith((task) => 
            { 
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("Client connected succesfully");
            });

            var client = new NumberService.NumberServiceClient(channel);

            var request = new NumberRequest() { Number = 120 };
            var response = client.PrimeNumberDescomposition(request);
            
            while(await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
 
            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}

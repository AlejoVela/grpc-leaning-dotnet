using Dummy;
using Greet;
using Grpc.Core;
using Sum;
using System;
using System.Linq;
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
                if(task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("Client connected succesfully");
            });
            var client = new GreetingService.GreetingServiceClient(channel);

            //DoSimpleGreet(client);
            //DoManyGreetings(client);
            //Sum(channel)
            //DoLongGreet(client);
            await DoGreetEveryone(client);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        // Unary
        public static void DoSimpleGreet(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Diego",
                LastName = "Restrepo"
            };
            var request = new GreetingRequest() { Greeting = greeting };
            var response = client.Greet(request);
            Console.WriteLine(response.Result);
        }
        public static void Sum(Channel channel)
        {
            var sumClient = new SumService.SumServiceClient(channel);
            var sumRequest = new SumRequest() { Num1 = 3, Num2 = 4 };
            var sumResponse = sumClient.Sum(sumRequest);
            Console.WriteLine($"El resultado de la suma es: {sumResponse.Result}");
        }
        // server streaming
        public static async Task DoManyGreetings(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Diego",
                LastName = "Restrepo"
            };
            var request = new GreetManyTimesRequest() { Greeting = greeting };
            var response = client.GreetManyTimes(request);
            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
        }
        // client streaming
        public static async Task DoLongGreet(GreetingService.GreetingServiceClient client)
        {
            var greeting = new Greeting()
            {
                FirstName = "Diego",
                LastName = "Restrepo"
            };
            var request = new LongGreetRequest() { Greeting = greeting };
            var stream = client.LongGreet();
            foreach (var i in Enumerable.Range(1, 10))
            {
                await stream.RequestStream.WriteAsync(request);
            }
            await stream.RequestStream.CompleteAsync();//con esto le indicamos al servidor que hemos terminado de enviar las peticiones
            var response = await stream.ResponseAsync;
            Console.WriteLine(response.Result);
        }
        // Bi Directional Streaming
        public static async Task DoGreetEveryone(GreetingService.GreetingServiceClient client)
        {
            var stream = client.GreetEveryone();
            var responseReaderTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine($"Received: {stream.ResponseStream.Current.Result}");
                }
            });

            Greeting[] greetings =
            {
                new Greeting() { FirstName = "John", LastName = "Doe" },
                new Greeting() { FirstName = "Clement", LastName = "Jean" },
                new Greeting() { FirstName = "Patricia", LastName = "Hertz"},
                new Greeting() { FirstName = "Diego", LastName = "Restrepo"}
            };

            foreach (var greeting in greetings)
            {
                Console.WriteLine($"Sending: {greeting}");
                await stream.RequestStream.WriteAsync(new GreetEveryoneRequest() 
                { 
                    Greeting = greeting 
                });
            }

            await stream.RequestStream.CompleteAsync();
            await responseReaderTask;
        }
    }
}

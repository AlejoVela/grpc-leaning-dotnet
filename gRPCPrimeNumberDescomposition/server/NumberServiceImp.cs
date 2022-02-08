using Grpc.Core;
using PrimeNumber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PrimeNumber.NumberService;

namespace server
{
    public class NumberServiceImp : NumberServiceBase
    {
        int k = 2;
        int number = 0;
        public override async Task PrimeNumberDescomposition(NumberRequest request, IServerStreamWriter<ManyNumberResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("The server got the request:");
            Console.WriteLine(request.ToString());
            number = request.Number;

            while (number > 1)
            {
                if (number % k == 0)
                {
                    await responseStream.WriteAsync(new ManyNumberResponse() { Result = $"{k}" });
                    number /= k;
                }
                else
                {
                    k += 1;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace UnityServer
{
    public class Program
    {

        static float NextFloat(Random random)
        {
            var buffer = new byte[4];
            random.NextBytes(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        static async Task Main(string[] args)
        {

            Random r = new Random();

            // Include port of the gRPC server as an application argument
            var port =  args.Length > 0 ? args[0] : "50051";

            var channel = new Channel("localhost:" + port, ChannelCredentials.Insecure);
            var client = new MultiGreeter.MultiGreeterClient(channel);

            var reply = await client.CreatePlayerAsync(new PlayerCreateRequest { Name = "A.I" });
            var playerId = reply.Id;

            for (var i =0;i<100;i++)
            {
                float rInt = NextFloat(r);
                await client.UpdatePlayerAsync(new PlayerUpdate { Id=playerId,Key="pos",Value=$"${rInt}f,0.0f,0.0f" });
              
                Thread.Sleep(500);
            }


            //if (await reply.MoveNext())
            //{
            //    Console.WriteLine("Greeting: " + reply.Current.Message);

            //}

            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;


namespace UnityServer
{
    public class MultiGreeterService : MultiGreeter.MultiGreeterBase
    {
        private static List<IServerStreamWriter<PlayerUpdate>> PlayerSubscriberStream = new List<IServerStreamWriter<PlayerUpdate>>();
        private static int PlayerIdCounter = 0;

        //add in subscription to unity for player updates
        //send in updates on update method
        public override async Task Players(PlayerRequest request, IServerStreamWriter<PlayerUpdate> responseStream, ServerCallContext context)
        {
            PlayerSubscriberStream.Add(responseStream);
        
            await Task.Run(() => {
                Thread.Sleep(3600000 * 100);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Allows user to send a player update
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<PlayerUpdate> UpdatePlayer(PlayerUpdate request, ServerCallContext context)
        {
            for (var i = 0; i < PlayerSubscriberStream.Count(); i++)
            {
                await DirectCall(PlayerSubscriberStream[i], request);
            }

            //broadcast
            return await Task.FromResult(request);
        }

        /// <summary>
        /// Allows user to send a player update
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<PlayerCreatedResponse> CreatePlayer(PlayerCreateRequest request, ServerCallContext context)
        {
            var response = new PlayerCreatedResponse
            {
                Id = PlayerIdCounter++.ToString()
            };

            //broadcast
            return await Task.FromResult(response);
        }


        private async Task DirectCall(IServerStreamWriter<PlayerUpdate> stream, PlayerUpdate message)
        {
            try
            {
                await stream.WriteAsync(message);
                //TotalMessages++;
                //Console.WriteLine($"Total Messages: {TotalMessages} Connections:{Connections}");
            }
            catch (Exception)
            {
                PlayerSubscriberStream.Remove(stream);
            }

        }
    }
}

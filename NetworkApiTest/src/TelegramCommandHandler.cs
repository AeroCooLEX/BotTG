using System;
using System.Threading.Tasks;
using Vintagestory.API.Server;

namespace BotTG
{
    internal class TelegramCommandHandler
    {
        public TelegramCommandHandler(ICoreServerAPI api)
        {
            Api = api;
        }

        public ICoreServerAPI Api { get; }

        internal void HandleCommand(string message)
        {
            throw new NotImplementedException();
        }
    }
}
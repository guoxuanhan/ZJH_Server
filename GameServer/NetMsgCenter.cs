using MyServer;
using Protocol.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class NetMsgCenter : IApplication
    {
        public void Disconnect(ClientPeer client)
        {
            throw new NotImplementedException();
        }

        public void Receive(ClientPeer client, NetMsg msg)
        {
            switch (msg.opCode)
            {
                case OpCode.Account:
                    {
                        break;
                    }
                case OpCode.Chat:
                    {
                        break;
                    }
                case OpCode.Fight:
                    {
                        break;
                    }
                case OpCode.Match:
                    {
                        break;
                    }
                default:
                    break;
            }
        }
    }
}

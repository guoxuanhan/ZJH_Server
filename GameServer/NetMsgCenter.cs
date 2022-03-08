using GameServer.Logic;
using MyServer;
using Protocol.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    /// <summary>
    /// 网络消息处理中心，分发消息到各个模块
    /// </summary>
    public class NetMsgCenter : IApplication
    {
        private AccountHandler accountHandler = new AccountHandler();
        private MatchHandler matchHandler = new MatchHandler();
        private ChatHandler chatHandler = new ChatHandler();
        private FightHandler fightHandler = new FightHandler();

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client"></param>
        public void Disconnect(ClientPeer client)
        {
            // 根据new的倒序断开
            fightHandler.Disconnect(client);
            chatHandler.Disconnect(client);
            matchHandler.Disconnect(client);
            accountHandler.Disconnect(client);
        }

        /// <summary>
        /// 接收消息并处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public void Receive(ClientPeer client, NetMsg msg)
        {
            switch (msg.opCode)
            {
                case OpCode.Account:
                    {
                        accountHandler.Receive(client, msg.subCode, msg.value);
                        break;
                    }
                case OpCode.Chat:
                    {
                        chatHandler.Receive(client, msg.subCode, msg.value);
                        break;
                    }
                case OpCode.Fight:
                    {
                        fightHandler.Receive(client, msg.subCode, msg.value);
                        break;
                    }
                case OpCode.Match:
                    {
                        matchHandler.Receive(client, msg.subCode, msg.value);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServer;
using Protocol.Code;
using Protocol.Dto;

namespace GameServer.Logic
{
    public class AccountHandler : IHandler
    {
        public void Disconnect(ClientPeer client)
        {
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case AccountCode.Register_CREQ:
                    {
                        Register(value as AccountDto);
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// 客户端注册的处理
        /// </summary>
        /// <param name="dto"></param>
        private void Register(AccountDto dto)
        {

        }
    }
}

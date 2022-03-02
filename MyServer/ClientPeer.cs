using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    public class ClientPeer
    {
        /// <summary>
        /// 客户端socket
        /// </summary>
        public Socket clientSocket { get; set; }

        /// <summary>
        /// 接收的异步套接字操作
        /// </summary>
        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        /// <summary>
        /// 接收到消息之后，存放到数据缓存区
        /// </summary>
        private List<byte> cache = new List<byte>();

        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool isProcessingReceive = false;

        /// <summary>
        /// 消息处理完成后的委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public delegate void ReceiveCompleted(ClientPeer client, NetMsg msg);
        public ReceiveCompleted receiveCompleted;

        public ClientPeer()
        {
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.UserToken = this;
            ReceiveArgs.SetBuffer(new byte[2048], 0, 2048);
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        /// <param name="packet"></param>
        public void ProcessReceive(byte[] packet)
        {
            cache.AddRange(packet);
            if (!isProcessingReceive)
            {
                ProcessData();
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            isProcessingReceive = true;
            // 解析数据包，从缓存区里取出一个完整的包
            byte[] packet = EncodeTool.DecodePacket(ref cache);
            if (packet == null)
            {
                isProcessingReceive = false;
                return;
            }

            NetMsg msg = EncodeTool.DecodeMsg(packet);
            if (receiveCompleted != null)
            {
                receiveCompleted(this, msg);
            }

            // 接着处理下一个数据
            ProcessData();
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect()
        {

        }
    }
}

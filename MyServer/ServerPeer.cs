using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    public class ServerPeer
    {
        /// <summary>
        /// 服务器socket
        /// </summary>
        private Socket serverSocket;

        /// <summary>
        /// 信号计量器，做连接的拥塞控制
        /// </summary>
        private Semaphore semaphore;

        /// <summary>
        /// 客户端对象连接池
        /// </summary>
        private ClientPeerPool clientPeerPool;

        /// <summary>
        /// 应用层
        /// </summary>
        private IApplication application { get; set; }

        /// <summary>
        /// 设置应用层参数
        /// </summary>
        /// <param name="application"></param>
        public void SetApplication(IApplication application)
        {
            this.application = application;
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="maxClient"></param>
        public void StartServer(string ip, int port, int maxClient)
        {
            try
            {
                semaphore = new Semaphore(maxClient, maxClient);

                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // 先填满客户端对象连接池，用的时候在池中取
                clientPeerPool = new ClientPeerPool(maxClient);
                for (int i = 0; i < maxClient; i++)
                {
                    ClientPeer clientPeer = new ClientPeer();
                    clientPeer.receiveCompleted = ReceiveProcessCompleted;
                    clientPeer.ReceiveArgs.Completed += ReceiveArgs_Completed;
                    clientPeerPool.Enqueue(clientPeer);
                }

                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                serverSocket.Listen(maxClient);
                Console.WriteLine("服务器启动成功");

                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region 接收客户端的连接请求

        private void StartAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e == null)
                {
                    e = new SocketAsyncEventArgs();
                    e.Completed += E_Completed;
                }

                // 为真表示正在接受连接，连接成功后会触发Completed事件
                bool result = serverSocket.AcceptAsync(e);

                // 如果为假，表示接收成功
                if (!result)
                {
                    ProcessAccept(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 异步接收客户端的连接完成后的触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /// <summary>
        /// 处理连接请求
        /// </summary>
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            semaphore.WaitOne();
            ClientPeer clientPeer = clientPeerPool.Dequeue(); //new ClientPeer();
            clientPeer.clientSocket = e.AcceptSocket;
            Console.WriteLine(clientPeer.clientSocket.RemoteEndPoint + "客户端连接成功");
            // 接收消息TODO
            StartReceive(clientPeer);

            // 循环接收连接请求
            e.AcceptSocket = null;
            StartAccept(e);
        }

        #endregion

        #region 接受数据

        /// <summary>
        /// 开始接受数据
        /// </summary>
        /// <param name="client"></param>
        private void StartReceive(ClientPeer client)
        {
            try
            {
                bool result = client.clientSocket.ReceiveAsync(client.ReceiveArgs);
                if (!result)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 异步接收收据完成后的调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        /// <summary>
        /// 处理数据的接收
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            ClientPeer client = e.UserToken as ClientPeer;

            // 判断数据是否接收成功
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);

                // 让ClientPeer自己处理接收到的数据
                client.ProcessReceive(packet);

                // 循环接收数据
                StartReceive(client);
            }
            // 断开连接
            else
            {
                // 没有传输的字节数， 断开连接了
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    // 客户端主动断开连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        Disconnect(client, "客户端主动断开连接");
                    }
                    // 因为网络异常断开连接
                    else
                    {
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 一条消息处理完成后的回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void ReceiveProcessCompleted(ClientPeer client, NetMsg msg)
        {
            // 交给应用层处理这个消息
            application.Receive(client, msg);
        }

        #endregion

        #region 断开连接

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reason">断开的理由</param>
        private void Disconnect(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("客户端为空， 无法断开连接");
                }

                Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端断开连接，原因: " + reason);
                application.Disconnect(client);
                // 让客户端处理断开连接
                client.Disconnect();

                // 将对象返回给池
                clientPeerPool.Enqueue(client);

                // 让出资源，让其他client可以进来
                semaphore.Release();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerApp
{
    class Program
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        private static string m_ServerIP = "192.168.17.128";

        /// <summary>
        /// 服务器端口
        /// </summary>
        private static int m_Prot = 1011;

        private static Socket m_ServerSocket;

        static void Main(string[] args)
        {
            //创建Socket
            //参数一：地址Ipv4
            //参数二：数据流
            //参数三：网络协议TCP
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定服务器的IP和端口
            m_ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(m_ServerIP), m_Prot));

            //监听,设置最多3000个排队连接请求
            m_ServerSocket.Listen(3000);

            //服务器开启后在控制台显示本地IP启动信息
            //m_ServerSocket.LocalEndPoint表示本地的ip和分配的端口
            Console.WriteLine("启动监听{0}服务器成功", m_ServerSocket.LocalEndPoint.ToString());

            //开启线程实现服务器的连接接受请求
            Thread mThread = new Thread(ListenClientCallBack);
            //开启
            mThread.Start();
        }

        /// <summary>
        /// 线程开启（该线程单独处理客户端连接请求）
        /// </summary>
        /// <returns></returns>
        private static void ListenClientCallBack()
        {
            //不间断接收
            while (true)
            {
                //接收客户端请求（并获取）
                Socket socket = m_ServerSocket.Accept();

                Console.WriteLine("接收到{0}的连接", socket.RemoteEndPoint.ToString());

                //ClientSocket获取当前连接的socket
                ClientSocket clientSocket = new ClientSocket(socket);

                //一个角色就相当于一个客户端连接
                Role role = new Role();
                role.m_ClientSocket = clientSocket;

                //把角色添加到集合中
                RoleManager.Instance.AllRole.Add(role);
            }
        }
    }
}

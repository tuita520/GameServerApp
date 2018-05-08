using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServerApp
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientSocket
    {
        #region 参数
        /// <summary>
        /// 客户端的Socket
        /// </summary>
        private Socket m_Socket;

        /// <summary>
        /// 接收数据的线程
        /// </summary>
        private Thread m_ReceiveThread;

        /// <summary>
        /// 客户端数据的缓存区域
        /// </summary>
        private byte[] m_ReceiveBuffer = new byte[10240];

        /// <summary>
        /// 客户端数据的读写操作
        /// </summary>
        private MMOMemoryStream m_ReceiveMS = new MMOMemoryStream();
        #endregion

        /// <summary>
        /// 构造函数（初始化）
        /// </summary>
        /// <param name="_socket"></param>
        public ClientSocket(Socket _socket)
        {
            m_Socket = _socket;

            //实例化
            m_ReceiveThread = new Thread(ReceiveMessage);
            //开启
            m_ReceiveThread.Start();
        }

        /// <summary>
        /// 接收数据处理
        /// </summary>
        private void ReceiveMessage()
        {
            //开始接收（异步）
            m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_Socket);
        }

        /// <summary>
        /// 异步数据接收的回调（实现循环）
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            //完成接收，数据才算完整
            int len = m_Socket.EndReceive(ar);

            if (len > 0)
            {
                //已经接收到数据

                //每次接收到数据都放在尾部
                m_ReceiveMS.Position = m_ReceiveMS.Length;
                //把指定长度的字节写入数据流
                m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);
                //测试（只有一条数据）
                byte[] buffer = m_ReceiveMS.ToArray();

            }
            else
            {

            }
        }
    }
}

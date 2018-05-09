using System;
using System.Net.Sockets;
using System.Threading;

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

        /// <summary>
        /// 所属角色
        /// </summary>
        private Role m_Role;
        #endregion

        /// <summary>
        /// 构造函数（初始化）
        /// </summary>
        /// <param name="_socket"></param>
        public ClientSocket(Socket _socket, Role _role)
        {
            m_Socket = _socket;
            m_Role = _role;

            m_Role.m_ClientSocket = this;

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
            try
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

                    //拆包
                    //因为包头占有两个字节
                    if (m_ReceiveMS.Length > 2)
                    {
                        //循环拆包
                        while (true)
                        {
                            //定位在数据包最开始的位置
                            m_ReceiveMS.Position = 0;

                            //获取包体的长度(就是包头的值)
                            int curMsgLen = m_ReceiveMS.ReadUshort();

                            //读取整个数据包
                            int curAllMsgLen = 2 + curMsgLen;

                            //数据流的长度大于整个数据包的长度时，说明数据完整
                            if (m_ReceiveMS.Length >= curAllMsgLen)
                            {
                                //定义一个包体的byte[]数组
                                byte[] buffer = new byte[curMsgLen];

                                //定位在2的位置（包体开始的位置）
                                m_ReceiveMS.Position = 2;

                                //Read从流中读取，长度为包体的长度
                                m_ReceiveMS.Read(buffer, 0, curMsgLen);

                                //buffer是最终读取的数据
                                using (MMOMemoryStream ms = new MMOMemoryStream(buffer))
                                {
                                    string data = ms.ReadUTF8String();
                                    Console.WriteLine(data);
                                }

                                //==============处理剩余字节==============
                                //剩余字节的长度
                                int remainLen = (int)m_ReceiveMS.Length - curAllMsgLen;

                                if (remainLen > 0)
                                {
                                    //设定在上一个数据包的尾部
                                    m_ReceiveMS.Position = curAllMsgLen;

                                    //剩余的字节数组
                                    byte[] remainBuffer = new byte[remainLen];

                                    //将剩余的字节从流中读取
                                    m_ReceiveMS.Read(remainBuffer, 0, remainLen);

                                    //清空数据流
                                    m_ReceiveMS.Position = 0;
                                    m_ReceiveMS.SetLength(0);

                                    //重新写入数据流
                                    m_ReceiveMS.Write(remainBuffer, 0, remainLen);

                                    //重新定位，在当前数据的最后位置，下次客户端传输的数据跟在该数据后
                                    m_ReceiveMS.Position = m_ReceiveMS.Length;
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("客户端{0}断开连接", m_Socket.RemoteEndPoint.ToString());

                    //集合中应当移除关闭连接的role
                    RoleManager.Instance.AllRole.Remove(m_Role);
                    Console.WriteLine("当前角色集合的长度为：{0}", RoleManager.Instance.AllRole.Count);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("客户端{0}断开连接", m_Socket.RemoteEndPoint.ToString());

                //集合中应当移除关闭连接的role
                RoleManager.Instance.AllRole.Remove(m_Role);
                Console.WriteLine("当前角色集合的长度为：{0}", RoleManager.Instance.AllRole.Count);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace GameServerApp
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientSocket
    {
        #region 单例
        private static ClientSocket instance;

        public static ClientSocket Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClientSocket();
                }
                return instance;
            }
        }

        private ClientSocket()
        {

        }
        #endregion

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
        /// 所属角色
        /// </summary>
        private Role m_Role;
        #endregion

        #region 消息接收参数
        /// <summary>
        /// 客户端数据的缓存区域
        /// </summary>
        private byte[] m_ReceiveBuffer = new byte[10240];

        /// <summary>
        /// 客户端数据的读写操作
        /// </summary>
        private MMOMemoryStream m_ReceiveMS = new MMOMemoryStream();
        #endregion

        #region 消息发送参数
        /// <summary>
        /// 消息队列
        /// </summary>
        private Queue<byte[]> m_MSgQueue = new Queue<byte[]>();

        /// <summary>
        /// 队列检查委托
        /// </summary>
        private Action m_CheckQueue;
        #endregion

        #region ClientSocket 构造函数（初始化）
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

            //委托绑定
            m_CheckQueue = CheckQueue;

            //测试
            using (MMOMemoryStream ms = new MMOMemoryStream())
            {
                ms.WriterUTF8String(string.Format("当前连接时间" + DateTime.Now.ToString()));

                SendMsg(ms.ToArray());
            }
        }
        #endregion

        //==========异步接收数据==========

        #region ReceiveMessage 接收数据处理
        /// <summary>
        /// 接收数据处理
        /// </summary>
        private void ReceiveMessage()
        {
            //开始接收（异步）
            m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_Socket);
        }
        #endregion

        #region ReceiveCallBack 异步数据接收的回调（实现循环）
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

                                //有剩余的字节长度
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

                                    //剩余空间不需要使用，应当释放
                                    remainBuffer = null;
                                }
                                //无剩余的字节长度
                                else
                                {
                                    //清空数据流
                                    m_ReceiveMS.Position = 0;
                                    m_ReceiveMS.SetLength(0);

                                    break;
                                }
                            }
                            //数据不完整，不拆包，等待下次数据写入
                            else
                            {
                                break;
                            }
                        }
                    }

                    //进行下一次数据包的接收
                    ReceiveMessage();
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
        #endregion

        //==========异步发送数据==========

        #region SendMsg 得到完整数据存入队列
        /// <summary>
        /// 消息发送（得到包体部分）
        /// </summary>
        /// <param name="_message"></param>
        public void SendMsg(byte[] _message)
        {
            //制作完整数据包
            byte[] sendBuffer = MakeAllData(_message);

            lock (m_MSgQueue)
            {
                //放置在Queue队列中
                m_MSgQueue.Enqueue(sendBuffer);

                //开启委托（委托实现）
                m_CheckQueue.BeginInvoke(null, null);
            }
        }
        #endregion

        #region MakeAllData 制作完整数据包（包头+包体）
        /// <summary>
        /// 制作完整的数据包（包头+包体）
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        private byte[] MakeAllData(byte[] _data)
        {
            byte[] retBuffer = null;

            using (MMOMemoryStream ms = new MMOMemoryStream())
            {
                //ushort占两个字节，作为包头的长度
                //将长度转换成byte，写入数据流中
                ms.WriteUshort((ushort)_data.Length);
                //将数据写入数据流
                ms.Write(_data, 0, _data.Length);

                //读取
                retBuffer = ms.ToArray();
            }

            return retBuffer;
        }
        #endregion

        #region CheckQueue 根据队列是否有值进行数据发送
        /// <summary>
        /// 检查队列的委托回调函数
        /// </summary>
        private void CheckQueue()
        {
            lock (m_MSgQueue)
            {
                //队列中至少有一条数据
                if (m_MSgQueue.Count > 0)
                {
                    //发送数据
                    Send(m_MSgQueue.Dequeue());
                }
            }
        }
        #endregion

        #region Send 用于将数据包发送到服务器
        /// <summary>
        /// 用于将数据包发送到服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void Send(byte[] buffer)
        {
            //异步发送
            m_Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, m_Socket);
        }
        #endregion

        #region SendCallBack 异步发送消息的回调函数
        /// <summary>
        /// 异步发送消息的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
            //完成异步
            m_Socket.EndSend(ar);

            //检查队列
            CheckQueue();
        }
        #endregion
    }
}
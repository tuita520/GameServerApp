using System;
using System.IO;
using System.Text;

namespace GameServerApp
{
    public class MMOMemoryStream : MemoryStream
    {
        #region 构造函数
        public MMOMemoryStream() : base()
        {

        }
        public MMOMemoryStream(byte[] _buffer) : base(_buffer)
        {

        }
        #endregion

        #region short 短整型
        /// <summary>
        /// 从流Stram中读取一个short,字节长度为2
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            byte[] buffer = new byte[2];
            base.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        /// <summary>
        /// 将short存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteShort(short value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region ushort 无符号短整型
        /// <summary>
        /// 从流Stram中读取一个ushort,字节长度为2
        /// </summary>
        /// <returns></returns>
        public ushort ReadUshort()
        {
            byte[] buffer = new byte[2];
            base.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }

        /// <summary>
        /// 将ushort存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteUshort(ushort value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region int 整型
        /// <summary>
        /// 从流Stram中读取一个int,字节长度为4
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            byte[] buffer = new byte[4];
            base.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// 将int存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region uint 无符号整型
        /// <summary>
        /// 从流Stram中读取一个uint,字节长度为4
        /// </summary>
        /// <returns></returns>
        public uint ReadUint()
        {
            byte[] buffer = new byte[4];
            base.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }

        /// <summary>
        /// 将uint存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteUint(uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region long 长整型
        /// <summary>
        /// 从流Stram中读取一个long,字节长度为8
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            byte[] buffer = new byte[8];
            base.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// 将long存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(long value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region ulong 无符号长整型
        /// <summary>
        /// 从流Stram中读取一个ulong,字节长度为8
        /// </summary>
        /// <returns></returns>
        public ulong ReadUlong()
        {
            byte[] buffer = new byte[8];
            base.Read(buffer, 0, 8);
            return BitConverter.ToUInt64(buffer, 0);
        }

        /// <summary>
        /// 将ulong存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteUlong(ulong value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region float 单精度浮点型
        /// <summary>
        /// 从流Stram中读取一个float,字节长度为4
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            byte[] buffer = new byte[4];
            base.Read(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// 将float存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region double 双精度浮点型
        /// <summary>
        /// 从流Stram中读取一个double,字节长度为8
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            byte[] buffer = new byte[8];
            base.Read(buffer, 0, 8);
            return BitConverter.ToDouble(buffer, 0);
        }

        /// <summary>
        /// 将double存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion

        #region bool 布尔型
        /// <summary>
        /// 从流Stram中读取一个bool
        /// </summary>
        /// <returns>等于1为真</returns>
        public bool ReadBool()
        {
            return base.ReadByte() == 1;
        }

        /// <summary>
        /// 将bool存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriteBool(bool value)
        {
            base.WriteByte((byte)(value == true ? 1 : 0));
        }
        #endregion

        #region string 字符串型
        /// <summary>
        /// 从流Stram中读取一个string
        /// </summary>
        /// <returns></returns>
        public string ReadUTF8String()
        {
            ushort length = ReadUshort();
            byte[] buffer = new byte[length];
            base.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// 将string存入流Stream
        /// </summary>
        /// <param name="value"></param>
        public void WriterUTF8String(string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            if (buffer.Length > 65535)
            {
                throw new InvalidCastException("字节空间超出范围！");
            }
            WriteUshort((ushort)buffer.Length);
            base.Write(buffer, 0, buffer.Length);
        }
        #endregion
    }
}

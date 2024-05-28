using System;
using System.IO;
using System.Text;

public enum ReadAndWrite : byte
{
    Read, Write
}

public class ByteBuffer
{
    MemoryStream stream = null;
    BinaryWriter writer = null;
    BinaryReader reader = null;

    public ByteBuffer()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
    }

    public ByteBuffer(byte[] data)
    {
        if (data != null)
        {
            stream = new MemoryStream(data);
            reader = new BinaryReader(stream);
        }
        else
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }
    }
    public ByteBuffer(ReadAndWrite type)
    {
        stream = new MemoryStream();
        switch (type)
        {
            case ReadAndWrite.Read:
                reader = new BinaryReader(stream);
                break;
            case ReadAndWrite.Write:
                writer = new BinaryWriter(stream);
                break;
            default:
                break;
        }

    }

    public void Reset(byte[] data = null)
    {
        if (reader == null)
        {
            reader = new BinaryReader(stream);
        }
        stream.Position = 0;
        stream.SetLength(0);
        if (data != null)
        {
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
        }
    }


    public void Close()
    {
        if (writer != null) writer.Close();
        if (reader != null) reader.Close();

        stream.Close();
        writer = null;
        reader = null;
        stream = null;
    }

    public void WriteByte(byte v)
    {
        writer.Write(v);
    }
    /// <summary>
    /// lmt
    /// </summary>
    /// <param name="v"></param>
    public void WriteSByte(sbyte v)
    {
        writer.Write(v);
    }

    public void WriteInt(int v)
    {
        writer.Write(v);
    }

    public void WriteUInt(uint v)
    {
        writer.Write(v);
    }

    public void WriteShort(short v)
    {
        writer.Write(v);
    }

    public void WriteUShort(ushort v)
    {
        writer.Write(v);
    }

    public void WriteLong(long v)
    {
        writer.Write(v);
    }

    public void WriteFloat(float v)
    {
        byte[] temp = BitConverter.GetBytes(v);
        Array.Reverse(temp);
        writer.Write(BitConverter.ToSingle(temp, 0));
    }

    public void WriteDouble(double v)
    {
        byte[] temp = BitConverter.GetBytes(v);
        Array.Reverse(temp);
        writer.Write(BitConverter.ToDouble(temp, 0));
    }

    public void WriteString(string v, ushort len)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(v);
        byte[] bytes1 = new byte[len];
        if (bytes.Length > len)
        {
            Buffer.BlockCopy(bytes, 0, bytes1, 0, len);
        }
        else
        {
            Buffer.BlockCopy(bytes, 0, bytes1, 0, bytes.Length);
        }

        writer.Write(bytes1);
    }

    /// <summary>
    /// LMT
    /// </summary>
    /// <param name="v"></param>
    public void WriteString(string v)
    {
        if (string.IsNullOrEmpty(v))
        {
            return;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(v);
        WriteUShort((ushort)bytes.Length);
        WriteBytes(bytes, (ushort)bytes.Length);
    }


    public void WriteBytes(byte[] bytes, ushort len)
    {
        byte[] bytes1 = new byte[len];
        if (bytes.Length > len)
        {
            Buffer.BlockCopy(bytes, 0, bytes1, 0, len);
        }
        else
        {
            Buffer.BlockCopy(bytes, 0, bytes1, 0, bytes.Length);
        }
        writer.Write(bytes1);
    }


    public byte ReadByte()
    {
        return reader.ReadByte();
    }
    /// <summary>
    /// LMT
    /// </summary>
    /// <returns></returns>
    public sbyte ReadSByte()
    {
        return reader.ReadSByte();
    }

    public int ReadInt()
    {
        return (int)reader.ReadInt32();
    }

    /// <summary>
    /// LMT
    /// </summary>
    /// <returns></returns>
    public uint ReadUInt()
    {
        return (uint)reader.ReadUInt32();
    }

    public short ReadShort()
    {
        return (short)reader.ReadInt16();
    }

    public ushort ReadUShort()
    {
        return (ushort)reader.ReadUInt16();
    }

    public long ReadLong()
    {
        return (long)reader.ReadInt64();
    }

    public float ReadFloat()
    {
        byte[] temp = BitConverter.GetBytes(reader.ReadSingle());
        Array.Reverse(temp);
        return BitConverter.ToSingle(temp, 0);
    }

    public double ReadDouble()
    {
        byte[] temp = BitConverter.GetBytes(reader.ReadDouble());
        Array.Reverse(temp);
        return BitConverter.ToDouble(temp, 0);
    }

    public string ReadString()
    {
        ushort len = ReadUShort();
        byte[] buffer = new byte[len];
        buffer = reader.ReadBytes(len);
        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// lmt
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    public string ReadString(ushort len)
    {
        byte[] buffer = new byte[len];
        buffer = reader.ReadBytes(len);
        string s = Encoding.UTF8.GetString(buffer);
        int idx = s.IndexOf('\0', 0, s.Length);
        if (idx < 0)
        {
            return s;
        }
        return s.Substring(0, idx);
    }

    public byte[] ReadBytes()
    {
        int len = ReadInt();
        return reader.ReadBytes(len);
    }

    //public LuaStringBuffer ReadBuffer() {
    //	byte[] bytes = ReadBytes();
    //	return new LuaStringBuffer(bytes);
    //}

    public byte[] ToBytes()
    {
        writer.Flush();
        return stream.ToArray();
    }

    public void Flush()
    {
        writer.Flush();
    }
}


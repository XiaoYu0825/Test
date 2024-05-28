using Nirvana;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MsgAdapter
{
    private static ByteBuffer read_buf = new ByteBuffer(ReadAndWrite.Read);
    private static ByteBuffer write_buf = new ByteBuffer(ReadAndWrite.Write);

    /// <summary>
    /// 每次收到服务器新的消息后，
    /// 要重新把新消息传入这个方法，
    /// 以后再使用readXXX方法解析数据
    /// </summary>
    /// <param name="_read_buf"></param>
    public static void InitReadMsg(byte[] _read_buf)
    {
        read_buf.Reset(_read_buf);
    }

    public static void InitWriteMsg()
    {
        write_buf.Reset();
    }

    #region 读数据
    public static sbyte ReadChar()
    {
        return read_buf.ReadSByte();
    }

    public static byte ReadUChar()
    {
        return read_buf.ReadByte();
    }

    public static short ReadShort()
    {
        return read_buf.ReadShort();
    }

    public static ushort ReadUShort()
    {
        return read_buf.ReadUShort();
    }

    public static int ReadInt()
    {
        return read_buf.ReadInt();
    }

    public static uint ReadUInt()
    {
        return read_buf.ReadUInt();
    }

    public static long ReadLL()
    {
        return read_buf.ReadLong();
    }

    public static float ReadFloat()
    {
        return read_buf.ReadFloat();
    }

    public static double ReadDouble()
    {
        return read_buf.ReadDouble();
    }

    public static string ReadStrN(ushort str_len)
    {
        return read_buf.ReadString(str_len);
    }
    #endregion


    #region 写数据
    public static void WriteBegin(ushort msg_type)
    {
        write_buf.WriteUShort(msg_type);
        write_buf.WriteShort(0);
    }

    public static void WriteChar(sbyte value)
    {
        write_buf.WriteSByte(value);
    }

    public static void WriteUChar(byte value)
    {
        write_buf.WriteByte(value);
    }

    public static void WriteShort(short value)
    {
        write_buf.WriteShort(value);
    }

    public static void WriteUShort(ushort value)
    {
        write_buf.WriteUShort(value);
    }

    public static void WriteInt(int value)
    {
        write_buf.WriteInt(value);
    }

    public static void WriteUInt(uint value)
    {
        write_buf.WriteUInt(value);
    }

    public static void WriteLL(long value)
    {
        write_buf.WriteLong(value);
    }

    public static void WriteFloat(float value)
    {
        write_buf.WriteFloat(value);
    }

    public static void WriteDouble(double value)
    {
        write_buf.WriteDouble(value);
    }

    public static void WriteStrN(string value, ushort len)
    {
        write_buf.WriteString(value, len);
    }

    public static void WriteStr(string value)
    {
        WriteInt(value.Length);
        WriteStrN(value, (ushort)value.Length);
        //write_buf.WriteString(value);
    }

    #endregion

    public static void Send(NetClient client = null)
    {
        NetClient net = client != null ? client : GameManager.Net.GetCurNetClient();
        if (net != null)
        {
            byte[] data = write_buf.ToBytes();
            net.SendMsg(data);
        }
    }

  /*  public static ItemDataWrapper ReadItemDataWrapper()
    {
        ItemDataWrapper info = new ItemDataWrapper();
        info.item_id = MsgAdapter.ReadUShort();
        info.num = MsgAdapter.ReadShort();
        info.is_bind = MsgAdapter.ReadShort();
        info.has_param = MsgAdapter.ReadShort();
        info.invalid_time = MsgAdapter.ReadUInt();
        info.gold_price = MsgAdapter.ReadInt();
        info.param = ReadItemParamData();
        return info;
    }



    public static ItemParam ReadItemParamData()
    {
        ItemParam itemParam = new ItemParam();
        itemParam.quality = ReadShort();
        itemParam.strengthen_level = ReadShort();
        itemParam.shen_level = ReadShort();
        itemParam.fuling_level = ReadShort();
        itemParam.star_level = ReadShort();
        itemParam.has_lucky = ReadShort();
        itemParam.fumo_id = ReadShort();
        ReadShort();
        itemParam.xianpin_type_list = new List<ushort>();
        for (int i = 0; i < 6; i++)
        {
            ushort xianpin_type = ReadUShort();
            if (xianpin_type > 0)
            {
                itemParam.xianpin_type_list.Add(xianpin_type);
            }
        }

        itemParam.param1 = ReadInt();
        itemParam.param2 = ReadInt();
        itemParam.param3 = ReadInt();

        itemParam.rand_attr_type_1 = ReadUChar();
        itemParam.rand_attr_type_2 = ReadUChar();
        itemParam.rand_attr_type_3 = ReadUChar();

        itemParam.reserve_type = ReadUChar();

        itemParam.rand_attr_val_1 = ReadUShort();
        itemParam.rand_attr_val_2 = ReadUShort();
        itemParam.rand_attr_val_3 = ReadUShort();
        itemParam.reserve = ReadUShort();

        return itemParam;
    }*/
}

/***
-- 发送消息
local send_buf = ""
function MsgAdapter.Send(net)
	send_buf = struct.pack(write_fmt, unpack(write_value_list))

    net = net or GameNet.Instance: GetCurNet()

    GameNet.Instance:GetCurNet():SendMsg(send_buf, nil)
end
	**//*//*/
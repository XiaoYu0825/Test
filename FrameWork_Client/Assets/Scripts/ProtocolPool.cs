using Nirvana;
using System;
using System.Collections.Generic;

/// <summary>
/// /-- 协议池  
/// </summary>
public class ProtocolPool : Singleton<ProtocolPool>
{
    public Dictionary<Type, BaseProtocol> protocol_list;//存储已经创建的协议实例
    public Dictionary<ushort, BaseProtocol> protocol_list_by_type;//通过消息类型快速查找和访问对应的协议实例

    public void Init()
    {
        protocol_list = new Dictionary<Type, BaseProtocol>();
        protocol_list_by_type = new Dictionary<ushort, BaseProtocol>();
    }

    public void Delete()
    {
        protocol_list.Clear();
        protocol_list_by_type.Clear();
        protocol_list = null;
        protocol_list_by_type = null;
    }

    public ushort Register<T>() where T : BaseProtocol, new()
    {
        BaseProtocol proto = AddProtocol<T>();

        if (proto != null)
        {
            protocol_list_by_type[proto.MsgType] = proto;
            return proto.MsgType;
        }
        else
        {
            return 0;
        }
    }


    public void UnRegister<T>() where T : BaseProtocol, new()
    {
        Type type = typeof(T);
        if (protocol_list.TryGetValue(type, out BaseProtocol protocol))
        {
            protocol_list.Remove(type);
            protocol_list_by_type.Remove(protocol.MsgType);
        }
    }

    public void UnRegister(Type type, ushort msgType)
    {
        if (protocol_list_by_type.TryGetValue(msgType, out BaseProtocol protocol))
        {
            protocol_list.Remove(type);
            protocol_list_by_type.Remove(protocol.MsgType);
        }
    }

    public BaseProtocol GetProtocol<T>() where T : BaseProtocol, new()
    {
        if (protocol_list.TryGetValue(typeof(T), out BaseProtocol protocol))
        {
            protocol.Init();
            return protocol;
        }
        else
        {
            protocol = AddProtocol<T>();
            if (protocol != null)
            {
                protocol.Init();
            }
            return protocol;
        }

    }
    public BaseProtocol GetProtocolByType(ushort msgType)
    {
        if (protocol_list_by_type.TryGetValue(msgType, out BaseProtocol protocol))
        {
            protocol.Init();
            return protocol;
        }
        else
        {
            //($"msgType = {msgType} has not  Registed");
            return null;
        }

    }


    public BaseProtocol AddProtocol<T>() where T : BaseProtocol, new()
    {
        if (protocol_list.TryGetValue(typeof(T), out BaseProtocol protocol))
        {
            return protocol;
        }
        BaseProtocol proto = new T();
        proto.Init();
        protocol_list[typeof(T)] = proto;
        return proto;
    }
}


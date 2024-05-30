using Nirvana;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Nirvana.NetClient;

public enum ConnectState : byte
{
    Connected, Disconnect, Connecting
}

public enum ServerEnum : byte
{
    LoginServer, GameServer, None
}

 
public class NetModule : BaseGameModule
{
    public string loginServerIP; //登录服务器的IP地址
    public int loginServerPort; //存储登录服务器的端口号

    public string gameServerIP;//存储游戏服务器的IP地址
    public int gameServerPort;//存储游戏服务器的端口号

    private NetClient _curNetClient;//存储当前的网络客户端实例

    private ServerEnum _curServerEnum;//当前连接的是哪种类型的服务器
    /// <summary>
    /// 当前的网络连接状态
    /// </summary>
    private ConnectState _crtConnectState;
    public ConnectState CrtConnectState { get => _crtConnectState; }

    /// <summary>
    /// 注册消息的操作。
    /// </summary>
    private Dictionary<ushort, List<Action<BaseProtocol>>> msg_operate_table;
    private Dictionary<ushort, Type> msg_type_map;

    public bool networkRun;

    private ReceiveDelegate receiveEvt;//处理接收到的网络消息
    private DisconnectDelegate disEvt;//处理网络断开事件

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();

        //loginServerIP = "10.161.23.56";
        loginServerIP = "10.161.29.119";
        //loginServerIP = "10.161.8.108";
        // loginServerIP = "127.0.0.1";
        loginServerPort = 10300;

        //gameServerIP = "10.161.23.56";
        //gameServerIP = "10.161.8.108";
        gameServerIP = "10.161.29.119";
        // gameServerIP = "127.0.0.1";
        gameServerPort = 4777;

        _curServerEnum = ServerEnum.None;
        ///协议池初始化
        ProtocolPool.Instance.Init();


        msg_operate_table = new Dictionary<ushort, List<Action<BaseProtocol>>>();
        msg_type_map = new Dictionary<ushort, Type>();

        _crtConnectState = ConnectState.Disconnect;//网络连接已经断开
    }
    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
    }

    /// <summary>
    ///  -- 注册协议处理函数
    /// </summary>
    /// <param name="msg_type"></param>
    /// <param name="msg_oper_func"></param>
    public void RegisterMsgOperate(ushort msg_type, Action<BaseProtocol> msg_oper_func)
    {
        if (this.msg_operate_table.TryGetValue(msg_type, out List<Action<BaseProtocol>> actList))
        {
            actList.Add(msg_oper_func);
        }
        else
        {
            List<Action<BaseProtocol>> lst = new List<Action<BaseProtocol>>();
            lst.Add(msg_oper_func);
            this.msg_operate_table[msg_type] = lst;
        }
    }
    //注册一个基于 BaseProtocol 的协议类型
    public void RegisterProtocol<T>(Action<BaseProtocol> msg_oper_func) where T : BaseProtocol, new()
    {
        ushort msg_type = ProtocolPool.Instance.Register<T>();//注册协议类型并获取消息类型
        if (msg_type <= 0)
            return;
        msg_type_map[msg_type] = typeof(T);
        this.RegisterMsgOperate(msg_type, msg_oper_func);
    }

    /// <summary>
    /// 取消注册协议
    /// </summary>
    /// <param name="msg_type">协议编号</param>
    public void UnRegisterMsgOperate(ushort msg_type)
    {
        if (this.msg_operate_table.TryGetValue(msg_type, out List<Action<BaseProtocol>> actList))
        {
            this.msg_operate_table[(ushort)msg_type].Clear();
            this.msg_operate_table.Remove(msg_type);
        }
    }


    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        base.OnModuleLateUpdate(deltaTime);
    }

    protected internal override void OnModuleStart()
    {
        base.OnModuleStart();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        networkRun = false;

        foreach (var item in msg_type_map)
        {
            ProtocolPool.Instance.UnRegister(item.Value, item.Key);
            this.UnRegisterMsgOperate(item.Key);
        }
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
    }

    #region 连接和断开
    public async Task AsyncConnect(string ip, int port)
    {
        if (!networkRun)
            return;

        ServerEnum tmpServerEnum = this._curServerEnum;
        if (this.CrtConnectState != ConnectState.Disconnect)
        {
            UnityLog.Info("正在断开之前的链接.....");
            this.Disconect();
        }

        _crtConnectState = ConnectState.Connecting;

        this._curNetClient = new NetClient();

        this._curNetClient.Connect(ip, port, async (is_succ) =>
        {
            if (is_succ)
            {
                UnityLog.Info("开始成功"); // 打印“开始成功”日志信息
                _crtConnectState = ConnectState.Connected; // 设置连接状态为已连接
                UnityLog.Info($"Async Connect to  {tmpServerEnum} server Ret: status " + is_succ); // 打印连接成功的日志信息，包含服务器类型和连接状态

                this._curNetClient.StartReceive(); // 开始接收数据

                // 定义接收消息和断开连接的事件处理函数
                receiveEvt = (byte[] message, uint length) => { NetClient_ReceiveEvent(message, length, this._curNetClient, tmpServerEnum); };
                disEvt = () => { _netClient_DisconnectEvent(this._curNetClient, tmpServerEnum); };

                // 订阅接收消息和断开连接的事件
                this._curNetClient.ReceiveEvent += receiveEvt;
                this._curNetClient.DisconnectEvent += disEvt;

                // 发布网络连接成功的消息
                GameManager.Message.Post<MessageType.NetConnected>(new MessageType.NetConnected() { serverEnum = tmpServerEnum }).Coroutine();

                await Task.Yield(); // 等待下一帧，通常用于异步方法中的协程行为
            }
            else
            {
                UnityLog.Warn("Connect Failed!!!!!!!"); // 打印连接失败的警告日志信息
                _crtConnectState = ConnectState.Disconnect; // 设置连接状态为断开连接
            }
        });
        await Task.Yield();
    }

    /// <summary>
    /// 断开和服务器的链接
    /// </summary>
    public void Disconect()
    {
        if (_crtConnectState == ConnectState.Connected)
        {
            if (_curNetClient != null)
            {
                _curNetClient.Disconnect();
            }
        }

    }
    #endregion

    //断开连接的事件
    private void _netClient_DisconnectEvent(NetClient netClient, ServerEnum serverEn)
    {
        // 打印日志信息，显示哪个服务器枚举值对应的连接已断开
        print($"net disconnected ---> " + serverEn);

        // 将当前连接状态设置为断开连接
        _crtConnectState = ConnectState.Disconnect;

        // 如果已经订阅了接收消息的事件，则取消订阅
        if (receiveEvt != null)
            netClient.ReceiveEvent -= receiveEvt;

        // 如果已经订阅了断开连接的事件，则取消订阅（尽管这个可能有些多余，因为方法本身就是断开连接的事件处理函数）
        if (disEvt != null)
            netClient.DisconnectEvent -= disEvt;

        // 发布一个网络断开连接的消息，以便游戏的其他部分可以做出响应
        GameManager.Message.Post<MessageType.NetDisconnected>(new MessageType.NetDisconnected() { serverEnum = serverEn }).Coroutine();
    }
    //接收消息的事件
    private void NetClient_ReceiveEvent(byte[] message, uint length, NetClient netClient, ServerEnum serverEnum)
    {
        // 初始化消息适配器，用于读取消息内容
        MsgAdapter.InitReadMsg(message);

        // 从消息中读取消息类型（id），通常这个id用来区分不同的消息
        ushort msgType = MsgAdapter.ReadUShort();
        // 这里又读取了一个ushort，但似乎并没有使用它，可能是为了跳过某些字段或对齐
        MsgAdapter.ReadUShort();

        // 打印接收到的消息的长度、长度（这里length参数是重复的）和消息类型
        UnityEngine.Debug.Log("收到数据长度 = " + message.Length + " lenght = " + length + " msgType = " + msgType);

        // 尝试从msg_operate_table这个字典中获取与该消息类型对应的操作列表
        if (this.msg_operate_table.TryGetValue(msgType, out List<Action<BaseProtocol>> actions))
        {
            // 根据消息类型从协议池中获取对应的协议对象
            var proto = ProtocolPool.Instance.GetProtocolByType(msgType);
            if (proto != null)
            {
                // 解码消息，填充协议对象的属性
                proto.Decode();

                // 遍历操作列表，执行每个操作（通常是回调函数）
                foreach (var act in actions)
                {
                    act?.Invoke(proto);
                }
            }
            else
            {
                // 如果协议对象不存在，则打印未知协议的错误信息
                print("Unknow protocol:" + msgType);
            }
        }

    }




    public void SendMsg(BaseProtocol proto)
    {
        if (!networkRun || proto == null)
            return;

        proto.EncodeAndSend();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public NetClient GetCurNetClient()
    {
        return _curNetClient;
    }

    /// <summary>
    /// 连接到登录服务器
    /// </summary>
    /// <returns></returns>
    public async Task ConnectLoginServer()
    {
        this._curServerEnum = ServerEnum.LoginServer;
        await this.AsyncConnect(this.loginServerIP, this.loginServerPort);
    }


    /// <summary>
    /// 链接游戏服务器
    /// </summary>
    /// <returns></returns>
    public async Task ConnectGameServer()
    {
        this._curServerEnum = ServerEnum.GameServer;
        await this.AsyncConnect(this.gameServerIP, this.gameServerPort);
    }


}

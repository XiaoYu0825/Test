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
    public string loginServerIP; //��¼��������IP��ַ
    public int loginServerPort; //�洢��¼�������Ķ˿ں�

    public string gameServerIP;//�洢��Ϸ��������IP��ַ
    public int gameServerPort;//�洢��Ϸ�������Ķ˿ں�

    private NetClient _curNetClient;//�洢��ǰ������ͻ���ʵ��

    private ServerEnum _curServerEnum;//��ǰ���ӵ����������͵ķ�����
    /// <summary>
    /// ��ǰ����������״̬
    /// </summary>
    private ConnectState _crtConnectState;
    public ConnectState CrtConnectState { get => _crtConnectState; }

    /// <summary>
    /// ע����Ϣ�Ĳ�����
    /// </summary>
    private Dictionary<ushort, List<Action<BaseProtocol>>> msg_operate_table;
    private Dictionary<ushort, Type> msg_type_map;

    public bool networkRun;

    private ReceiveDelegate receiveEvt;//������յ���������Ϣ
    private DisconnectDelegate disEvt;//��������Ͽ��¼�

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
        ///Э��س�ʼ��
        ProtocolPool.Instance.Init();


        msg_operate_table = new Dictionary<ushort, List<Action<BaseProtocol>>>();
        msg_type_map = new Dictionary<ushort, Type>();

        _crtConnectState = ConnectState.Disconnect;//���������Ѿ��Ͽ�
    }
    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
    }

    /// <summary>
    ///  -- ע��Э�鴦����
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

    public void RegisterProtocol<T>(Action<BaseProtocol> msg_oper_func) where T : BaseProtocol, new()
    {
        ushort msg_type = ProtocolPool.Instance.Register<T>();//ע��Э�����Ͳ���ȡ��Ϣ����
        if (msg_type <= 0)
            return;
        msg_type_map[msg_type] = typeof(T);
        this.RegisterMsgOperate(msg_type, msg_oper_func);
    }

    /// <summary>
    /// ȡ��ע��Э��
    /// </summary>
    /// <param name="msg_type">Э����</param>
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

    #region ���ӺͶϿ�
    public async Task AsyncConnect(string ip, int port)
    {
        if (!networkRun)
            return;

        ServerEnum tmpServerEnum = this._curServerEnum;
        if (this.CrtConnectState != ConnectState.Disconnect)
        {
            UnityLog.Info("���ڶϿ�֮ǰ������.....");
            this.Disconect();
        }

        _crtConnectState = ConnectState.Connecting;

        this._curNetClient = new NetClient();

        this._curNetClient.Connect(ip, port, async (is_succ) =>
        {
            if (is_succ)
            {
                UnityLog.Info("��ʼ�ɹ�");
                _crtConnectState = ConnectState.Connected;
                UnityLog.Info($"Async Connect to  {tmpServerEnum} server Ret: status " + is_succ);
                this._curNetClient.StartReceive();

                receiveEvt = (byte[] message, uint length) => { NetClient_ReceiveEvent(message, length, this._curNetClient, tmpServerEnum); };
                disEvt = () => { _netClient_DisconnectEvent(this._curNetClient, tmpServerEnum); };

                this._curNetClient.ReceiveEvent += receiveEvt;
                this._curNetClient.DisconnectEvent += disEvt;

                GameManager.Message.Post<MessageType.NetConnected>(new MessageType.NetConnected() { serverEnum = tmpServerEnum }).Coroutine();

                await Task.Yield();
            }
            else
            {
                UnityLog.Warn("Connect Failed!!!!!!!");
                _crtConnectState = ConnectState.Disconnect;
            }

        });

        await Task.Yield();
    }

    /// <summary>
    /// �Ͽ��ͷ�����������
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


    private void _netClient_DisconnectEvent(NetClient netClient, ServerEnum serverEn)
    {
        print($"net disconnected ---> "+ serverEn);

        _crtConnectState = ConnectState.Disconnect;

        if (receiveEvt != null)
            netClient.ReceiveEvent -= receiveEvt;

        if (disEvt != null)
            netClient.DisconnectEvent -= disEvt;

        GameManager.Message.Post<MessageType.NetDisconnected>(new MessageType.NetDisconnected() { serverEnum = serverEn }).Coroutine();
    }

    private void NetClient_ReceiveEvent(byte[] message, uint length, NetClient netClient, ServerEnum serverEnum)
    {
        MsgAdapter.InitReadMsg(message);
        ///��ȡ��Ϣ���ͣ�id������Ҫ������Ϣ���͵�Э��������ҵ�Э��
        ushort msgType = MsgAdapter.ReadUShort();
        MsgAdapter.ReadUShort();
        UnityEngine.Debug.Log("�յ����ݳ��� = " + message.Length + "  lenght = " + length + "    msgType = " + msgType);

        if (this.msg_operate_table.TryGetValue(msgType, out List<Action<BaseProtocol>> actions))
        {
            var proto = ProtocolPool.Instance.GetProtocolByType(msgType);
            if (proto != null)
            {
                proto.Decode();
                foreach (var act in actions)
                {
                    act?.Invoke(proto);
                }
            }
            else
            {
                print("Unknow protocol:"+ msgType);
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
    /// ���ӵ���¼������
    /// </summary>
    /// <returns></returns>
    public async Task ConnectLoginServer()
    {
        this._curServerEnum = ServerEnum.LoginServer;
        await this.AsyncConnect(this.loginServerIP, this.loginServerPort);
    }


    /// <summary>
    /// ������Ϸ������
    /// </summary>
    /// <returns></returns>
    public async Task ConnectGameServer()
    {
        this._curServerEnum = ServerEnum.GameServer;
        await this.AsyncConnect(this.gameServerIP, this.gameServerPort);
    }


}

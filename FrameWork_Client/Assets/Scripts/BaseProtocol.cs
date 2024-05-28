using Nirvana;

public abstract class BaseProtocol
{
    protected ushort msg_type;
    public ushort MsgType { get => msg_type; }

    public virtual void Init()
    {
        this.msg_type = 0;
    }


    /// <summary>
    /// ����
    /// </summary>
    public virtual void Encode()
    {
        MsgAdapter.InitWriteMsg();
    }

    /// <summary>
    /// ����
    /// </summary>
    public abstract void Decode();


    public void EncodeAndSend(NetClient net = null)
    {
        this.Encode();
        ///����
        MsgAdapter.Send(net);
    }

    public void Send(NetClient net = null)
    {
        MsgAdapter.Send(net);
    }




}

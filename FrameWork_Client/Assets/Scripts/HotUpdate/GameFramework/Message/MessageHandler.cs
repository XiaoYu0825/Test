using System;
using System.Threading.Tasks;


public interface IMessageHander//������ʶһ�����ܹ�����ĳ���ض����͵���Ϣ���¼�
{
    Type GetHandlerType();

}

[MessageHandler]//���Կ�����������ʱ����
public abstract class MessageHandler<T> : IMessageHander where T : struct
{
    public Type GetHandlerType()
    {
        return typeof(T);
    }

    public abstract Task HandleMessage(T arg);
}
//������Զ�����һ�������÷���AttributeUsage����ָ�����������ֻ�������ࣨClass����
//�������̳У�Inherited = true�������Ҳ�����ͬһ��Ԫ���϶��ʹ�ã�AllowMultiple = false��
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
sealed class MessageHandlerAttribute : Attribute { }
using System;
using System.Threading.Tasks;


public interface IMessageHander
{
    Type GetHandlerType();

}

[MessageHandler]
public abstract class MessageHandler<T> : IMessageHander where T : struct
{
    public Type GetHandlerType()
    {
        return typeof(T);
    }

    public abstract Task HandleMessage(T arg);
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
sealed class MessageHandlerAttribute : Attribute { }
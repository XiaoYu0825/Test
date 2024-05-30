using System;
using System.Threading.Tasks;


public interface IMessageHander//用作标识一个类能够处理某种特定类型的消息或事件
{
    Type GetHandlerType();

}

[MessageHandler]//特性可能用于运行时反射
public abstract class MessageHandler<T> : IMessageHander where T : struct
{
    public Type GetHandlerType()
    {
        return typeof(T);
    }

    public abstract Task HandleMessage(T arg);
}
//这个特性定义了一个特性用法（AttributeUsage），指定了这个特性只能用于类（Class），
//它允许被继承（Inherited = true），并且不能在同一个元素上多次使用（AllowMultiple = false）
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
sealed class MessageHandlerAttribute : Attribute { }
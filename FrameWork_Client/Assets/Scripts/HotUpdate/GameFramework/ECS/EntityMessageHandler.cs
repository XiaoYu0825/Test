using System;
using System.Threading.Tasks;


public interface IEntityMessageHandler//处理实体上的特定消息类型
{
    Type MessageType();//消息处理器能够处理的消息类型
}

public interface IEntityMessageHandler<M> : IEntityMessageHandler
{
    //Task表示消息处理可能是异步的
    Task Post(ECSEntity entity, M m);//一个 ECSEntity 类型的实体和一个泛型参数 M 类型的消息
}

[EntityMessageHandler]
public abstract class EntityMessageHandler<M> : IEntityMessageHandler<M>
{
    public abstract Task HandleMessage(ECSEntity entity, M message);

    public Type MessageType()
    {
        return typeof(M);
    }

    public async Task Post(ECSEntity entity, M m)
    {
        await HandleMessage(entity, m);
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class EntityMessageHandlerAttribute : Attribute
{
}


public interface IEntityRpcHandler
{
    Type RpcType();
}

public interface IEntityRpcResponse
{
    bool Error { get; set; }
    string ErrorMessage { get; set; }
}

public interface IEntityRpcHandler<Request, Response> : IEntityRpcHandler where Response : IEntityRpcResponse
{
    Task<Response> Post(ECSEntity entity, Request request);
}

[EntityRpcHandler]
public abstract class EntityRpcHandler<Request, Response> : IEntityRpcHandler<Request, Response> where Response : IEntityRpcResponse
{
    public abstract Task<Response> HandleRpc(ECSEntity entity, Request request);

    public async Task<Response> Post(ECSEntity entity, Request request)
    {
        return await HandleRpc(entity, request);
    }

    public Type RpcType()
    {
        return typeof(Request);
    }
}


[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class EntityRpcHandlerAttribute : Attribute
{
}


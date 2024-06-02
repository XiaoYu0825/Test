using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;


public class ECSModule : BaseGameModule
{
    public ECSWorld World { get; private set; }//ECS世界。ECSWorld可能是一个包含所有实体、组件和系统的容器或上下文

    private Dictionary<Type, IAwakeSystem> awakeSystemMap;
    private Dictionary<Type, IDestroySystem> destroySystemMap;

    private Dictionary<Type, IUpdateSystem> updateSystemMap;
    private Dictionary<IUpdateSystem, List<ECSEntity>> updateSystemRelatedEntityMap;

    private Dictionary<Type, ILateUpdateSystem> lateUpdateSystemMap;
    private Dictionary<ILateUpdateSystem, List<ECSEntity>> lateUpdateSystemRelatedEntityMap;

    private Dictionary<Type, IFixedUpdateSystem> fixedUpdateSystemMap;
    private Dictionary<IFixedUpdateSystem, List<ECSEntity>> fixedUpdateSystemRelatedEntityMap;

    private Dictionary<long, ECSEntity> entities = new Dictionary<long, ECSEntity>();
    private Dictionary<Type, List<IEntityMessageHandler>> entityMessageHandlerMap;
    private Dictionary<Type, IEntityRpcHandler> entityRpcHandlerMap;

    
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        LoadAllSystems();
        World = new ECSWorld();
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        DriveUpdateSystem();
        
    }

    protected internal override void OnModuleLateUpdate(float deltaTime)
    {
        base.OnModuleLateUpdate(deltaTime);
        DriveLateUpdateSystem();
    }

    protected internal override void OnModuleFixedUpdate(float deltaTime)
    {
        base.OnModuleFixedUpdate(deltaTime);
        DriveFixedUpdateSystem();
    }

    /// <summary>
    /// 模块初始化时调用
    /// </summary>
    public void LoadAllSystems()
    {
        awakeSystemMap = new Dictionary<Type, IAwakeSystem>();
        destroySystemMap = new Dictionary<Type, IDestroySystem>();

        updateSystemMap = new Dictionary<Type, IUpdateSystem>();
        updateSystemRelatedEntityMap = new Dictionary<IUpdateSystem, List<ECSEntity>>();

        lateUpdateSystemMap = new Dictionary<Type, ILateUpdateSystem>();
        lateUpdateSystemRelatedEntityMap = new Dictionary<ILateUpdateSystem, List<ECSEntity>>();

        fixedUpdateSystemMap = new Dictionary<Type, IFixedUpdateSystem>();
        fixedUpdateSystemRelatedEntityMap = new Dictionary<IFixedUpdateSystem, List<ECSEntity>>();

        entityMessageHandlerMap = new Dictionary<Type, List<IEntityMessageHandler>>();
        entityRpcHandlerMap = new Dictionary<Type, IEntityRpcHandler>();

        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (type.IsAbstract)
                continue;

            if (type.GetCustomAttribute<ECSSystemAttribute>(true) != null)
            {
                // AwakeSystem
                Type awakeSystemType = typeof(IAwakeSystem);//获取IAwakeSystem类型
                if (awakeSystemType.IsAssignableFrom(type))//type是否是IAwakeSystem类型
                {
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem; //创建实例
                    awakeSystemMap.Add(type, awakeSystem);
                }

                // DestroySystem
                Type destroySystemType = typeof(IDestroySystem);
                if (destroySystemType.IsAssignableFrom(type))
                {
                    if (destroySystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Destroy System:{type.FullName}");
                        continue;
                    }

                    IDestroySystem destroySytem = Activator.CreateInstance(type) as IDestroySystem;
                    destroySystemMap.Add(type, destroySytem);
                }

                // UpdateSystem
                Type updateSystemType = typeof(IUpdateSystem);
                if (updateSystemType.IsAssignableFrom(type))
                {
                    if (updateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Update System:{type.FullName}");
                        continue;
                    }

                    IUpdateSystem updateSystem = Activator.CreateInstance(type) as IUpdateSystem;
                    updateSystemMap.Add(type, updateSystem);

                    updateSystemRelatedEntityMap.Add(updateSystem, new List<ECSEntity>());
                }

                // LateUpdateSystem
                Type lateUpdateSystemType = typeof(ILateUpdateSystem);
                if (lateUpdateSystemType.IsAssignableFrom(type))
                {
                    if (lateUpdateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    ILateUpdateSystem lateUpdateSystem = Activator.CreateInstance(type) as ILateUpdateSystem;
                    lateUpdateSystemMap.Add(type, lateUpdateSystem);

                    lateUpdateSystemRelatedEntityMap.Add(lateUpdateSystem, new List<ECSEntity>());
                }

                // FixedUpdateSystem
                Type fixedUpdateSystemType = typeof(IFixedUpdateSystem);
                if (fixedUpdateSystemType.IsAssignableFrom(type))
                {
                    if (fixedUpdateSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Late update System:{type.FullName}");
                        continue;
                    }

                    IFixedUpdateSystem fixedUpdateSystem = Activator.CreateInstance(type) as IFixedUpdateSystem;
                    fixedUpdateSystemMap.Add(type, fixedUpdateSystem);

                    fixedUpdateSystemRelatedEntityMap.Add(fixedUpdateSystem, new List<ECSEntity>());
                }
            }

            if (type.GetCustomAttribute<EntityMessageHandlerAttribute>(true) != null)
            {
                // EntityMessage
                Type entityMessageType = typeof(IEntityMessageHandler);
                if (entityMessageType.IsAssignableFrom(type))
                {
                    IEntityMessageHandler entityMessageHandler = Activator.CreateInstance(type) as IEntityMessageHandler;

                    if (!entityMessageHandlerMap.TryGetValue(entityMessageHandler.MessageType(), out List<IEntityMessageHandler> list))
                    {
                        list = new List<IEntityMessageHandler>();
                        entityMessageHandlerMap.Add(entityMessageHandler.MessageType(), list);
                    }

                    list.Add(entityMessageHandler);
                }
            }

            if (type.GetCustomAttribute<EntityRpcHandlerAttribute>(true) != null)
            {
                // EntityRPC
                Type entityMessageType = typeof(IEntityRpcHandler);
                if (entityMessageType.IsAssignableFrom(type))
                {
                    IEntityRpcHandler entityRpcHandler = Activator.CreateInstance(type) as IEntityRpcHandler;

                    if (entityRpcHandlerMap.ContainsKey(entityRpcHandler.RpcType()))
                    {
                        UnityLog.Error($"Duplicate Entity Rpc, type:{entityRpcHandler.RpcType().FullName}");
                        continue;
                    }

                    entityRpcHandlerMap.Add(entityRpcHandler.RpcType(), entityRpcHandler);
                }
            }
        }
    }

    private void DriveUpdateSystem()//用于更新与updateSystemMap字典中存储的IUpdateSystem接口实例相关联的实体（ECSEntity）
    {
        // 遍历updateSystemMap字典中的所有值，即所有的IUpdateSystem接口实例
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // 从updateSystemRelatedEntityMap字典中获取与当前updateSystem相关联的实体列表
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];

            // 如果与当前updateSystem相关联的实体列表为空，则跳过当前循环，继续下一个IUpdateSystem实例
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // 从ListPool<ECSEntity>中获取一个新的实体列表，用于存储待更新的实体
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();

            // 将与updateSystem相关联的实体列表添加到新的实体列表中，但不分配新的内存
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);

            // 遍历新的实体列表中的每个实体
            foreach (var entity in entityList)
            {
                // 检查当前实体是否正在被updateSystem观察，如果不是，则跳过当前循环，继续下一个实体
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // 调用updateSystem的Update方法，更新当前实体
                updateSystem.Update(entity);
            }

            // 将使用过的实体列表释放回ListPool<ECSEntity>，以便复用
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    private void DriveLateUpdateSystem()//用于更新与lateUpdateSystemMap字典中存储的ILateUpdateSystem接口实例相关联的实体（ECSEntity）
    {
        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            List<ECSEntity> lateUpdateSystemRelatedEntities = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];
            if (lateUpdateSystemRelatedEntities.Count == 0)
                continue;

            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(lateUpdateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                if (!lateUpdateSystem.ObservingEntity(entity))
                    continue;

                lateUpdateSystem.LateUpdate(entity);
            }

            ListPool<ECSEntity>.Release(entityList);
        }
    }

    private void DriveFixedUpdateSystem()//用于更新与lfixedUpdateSystemMap字典中存储的IFixedUpdateSystem接口实例相关联的实体（ECSEntity）
    {
        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            List<ECSEntity> fixedUpdateSystemRelatedEntities = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
            if (fixedUpdateSystemRelatedEntities.Count == 0)
                continue;

            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();
            entityList.AddRangeNonAlloc(fixedUpdateSystemRelatedEntities);
            foreach (var entity in entityList)
            {
                if (!fixedUpdateSystem.ObservingEntity(entity))
                    continue;

                fixedUpdateSystem.FixedUpdate(entity);
            }

            ListPool<ECSEntity>.Release(entityList);
        }
    }
    /// <summary>
    /// 获取并返回一组实现了 IAwakeSystem 接口的系统
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <param name="list"></param>
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        foreach (var awakeSystem in awakeSystemMap.Values)
        {
            if (awakeSystem.ComponentType() == typeof(C))//对于每一个 awakeSystem，检查其组件类型
            {
                list.Add(awakeSystem);
            }
        }
    }
    /// <summary>
    /// 处理实体组件系统（ECS）中组件的唤醒逻辑
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <param name="component"></param>
    public void AwakeComponent<C>(C component) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            AwakeSystem<C> awakeSystem = item as AwakeSystem<C>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component);
            found = true;
        }

        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}>");
        }
    }

    public void AwakeComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            AwakeSystem<C, P1> awakeSystem = item as AwakeSystem<C, P1>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1);
            found = true;
        }

        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}>");
        }
    }

    public void AwakeComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IAwakeSystem> list = ListPool<IAwakeSystem>.Obtain();
        TGameFramework.Instance.GetModule<ECSModule>().GetAwakeSystems<C>(list);

        bool found = false;
        foreach (var item in list)
        {
            AwakeSystem<C, P1, P2> awakeSystem = item as AwakeSystem<C, P1, P2>;
            if (awakeSystem == null)
                continue;

            awakeSystem.Awake(component, p1, p2);
            found = true;
        }

        ListPool<IAwakeSystem>.Release(list);
        if (!found)
        {
            UnityLog.Warn($"Not found awake system:<{typeof(C).Name}, {typeof(P1).Name}, {typeof(P2).Name}>");
        }
    }

    private void GetDestroySystems<C>(List<IDestroySystem> list) where C : ECSComponent
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            if (destroySystem.ComponentType() == typeof(C))
            {
                list.Add(destroySystem);
            }
        }
    }
    private void GetDestroySystems(Type componentType, List<IDestroySystem> list)
    {
        foreach (var destroySystem in destroySystemMap.Values)
        {
            if (destroySystem.ComponentType() == componentType)
            {
                list.Add(destroySystem);
            }
        }
    }
    public void DestroyComponent<C>(C component) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            DestroySystem<C> destroySystem = item as DestroySystem<C>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component);
            component.Disposed = true;
        }

        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent(ECSComponent component)
    {
        UpdateSystemEntityList(component.Entity);

        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems(component.GetType(), list);
        foreach (var item in list)
        {
            item.Destroy(component);
            component.Disposed = true;
        }

        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1>(C component, P1 p1) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            DestroySystem<C, P1> destroySystem = item as DestroySystem<C, P1>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1);
            component.Disposed = true;
        }

        ListPool<IDestroySystem>.Release(list);
    }

    public void DestroyComponent<C, P1, P2>(C component, P1 p1, P2 p2) where C : ECSComponent
    {
        UpdateSystemEntityList(component.Entity);

        List<IDestroySystem> list = ListPool<IDestroySystem>.Obtain();
        GetDestroySystems<C>(list);
        foreach (var item in list)
        {
            DestroySystem<C, P1, P2> destroySystem = item as DestroySystem<C, P1, P2>;
            if (destroySystem == null)
                continue;

            destroySystem.Destroy(component, p1, p2);
            component.Disposed = true;
        }

        ListPool<IDestroySystem>.Release(list);
    }
    /// <summary>
    /// 更新不同系统（如 IUpdateSystem、ILateUpdateSystem 和 IFixedUpdateSystem）中跟踪的实体列表
    /// </summary>
    /// <param name="entity"></param>
    private void UpdateSystemEntityList(ECSEntity entity)
    {
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // update entity list
            List<ECSEntity> entityList = updateSystemRelatedEntityMap[updateSystem];
            if (!entityList.Contains(entity))
            {
                if (updateSystem.ObservingEntity(entity))//检查当前系统是否正在观察这个实体
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                if (!updateSystem.ObservingEntity(entity))//检查当前系统是否正在观察这个实体
                {
                    entityList.Remove(entity);
                }
            }
        }

        foreach (ILateUpdateSystem lateUpdateSystem in lateUpdateSystemMap.Values)
        {
            // update entity list
            List<ECSEntity> entityList = lateUpdateSystemRelatedEntityMap[lateUpdateSystem];
            if (!entityList.Contains(entity))
            {
                if (lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                if (!lateUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }

        foreach (IFixedUpdateSystem fixedUpdateSystem in fixedUpdateSystemMap.Values)
        {
            // update entity list
            List<ECSEntity> entityList = fixedUpdateSystemRelatedEntityMap[fixedUpdateSystem];
            if (!entityList.Contains(entity))
            {
                if (fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                if (!fixedUpdateSystem.ObservingEntity(entity))
                {
                    entityList.Remove(entity);
                }
            }
        }
    }
    /// <summary>
    /// 将传入的实体添加到集合或映射中
    /// </summary>
    /// <param name="entity"></param>
    public void AddEntity(ECSEntity entity)
    {
        entities.Add(entity.InstanceID, entity);
    }

    public void RemoveEntity(ECSEntity entity)
    {
        if (entity == null)
            return;

        entities.Remove(entity.InstanceID);
        ECSScene scene = entity.Scene;
        scene?.RemoveEntity(entity.InstanceID);
    }
    /// <summary>
    /// 根据ID查找实体
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ECSEntity FindEntity(long id)
    {
        return FindEntity<ECSEntity>(id);
    }

    public T FindEntity<T>(long id) where T : ECSEntity
    {
        entities.TryGetValue(id, out ECSEntity entity);
        return entity as T;
    }
    /// <summary>
    /// 实体ID（entityID）上查找并返回特定类型的组件（T）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityID"></param>
    /// <returns></returns>
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        return FindEntity(entityID)?.GetComponent<T>();
    }

    // 异步方法，用于向具有特定ID的实体发送消息
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        // 如果id为0，则不执行任何操作并直接返回
        if (id == 0)
            return;

        // 查找具有指定id的实体
        ECSEntity entity = FindEntity(id);
        // 如果找不到实体，则不执行任何操作并直接返回
        if (entity == null)
            return;

        // 获取消息的类型
        Type messageType = m.GetType();
        // 尝试从映射中获取与消息类型对应的消息处理器列表
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return;

        // 从对象池中获取一个IEntityMessageHandler类型的列表
        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain();
        // 将从映射中获取的消息处理器列表添加到从对象池获取的列表中
        entityMessageHandlers.AddRangeNonAlloc(list);

        // 遍历消息处理器列表，并调用每个处理器的Post方法，向实体发送消息
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers)
        {
            // 异步调用Post方法，并等待其完成
            await handler.Post(entity, m);
        }

        // 将使用过的列表释放回对象池
        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers);
    }

    // 异步方法，用于向具有特定ID的实体发送远程过程调用(RPC)，并返回响应
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request)
        where Response : IEntityRpcResponse, new()
    {
        // 如果实体ID为0，则返回包含错误的响应对象
        if (entityID == 0)
            return new Response() { Error = true };

        // 查找具有指定ID的实体
        ECSEntity entity = FindEntity(entityID);
        // 如果找不到实体，则返回包含错误的响应对象
        if (entity == null)
            return new Response() { Error = true };

        // 获取请求消息的类型
        Type messageType = request.GetType();
        // 尝试从映射中获取与消息类型对应的实体RPC处理器
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true };

        // 尝试将实体RPC处理器转换为泛型类型的处理器
        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;
        // 如果转换失败（即处理器不支持该请求类型和响应类型），则返回包含错误的响应对象
        if (handler == null)
            return new Response() { Error = true };

        // 异步调用处理器的Post方法，向实体发送RPC请求，并等待响应
        return await handler.Post(entity, request);
    }
}


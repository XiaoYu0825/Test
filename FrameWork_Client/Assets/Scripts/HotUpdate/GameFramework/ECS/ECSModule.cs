using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;


public class ECSModule : BaseGameModule
{
    public ECSWorld World { get; private set; }//ECS���硣ECSWorld������һ����������ʵ�塢�����ϵͳ��������������

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
    /// ģ���ʼ��ʱ����
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
                Type awakeSystemType = typeof(IAwakeSystem);//��ȡIAwakeSystem����
                if (awakeSystemType.IsAssignableFrom(type))//type�Ƿ���IAwakeSystem����
                {
                    if (awakeSystemMap.ContainsKey(type))
                    {
                        UnityLog.Error($"Duplicated Awake System:{type.FullName}");
                        continue;
                    }

                    IAwakeSystem awakeSystem = Activator.CreateInstance(type) as IAwakeSystem; //����ʵ��
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

    private void DriveUpdateSystem()//���ڸ�����updateSystemMap�ֵ��д洢��IUpdateSystem�ӿ�ʵ���������ʵ�壨ECSEntity��
    {
        // ����updateSystemMap�ֵ��е�����ֵ�������е�IUpdateSystem�ӿ�ʵ��
        foreach (IUpdateSystem updateSystem in updateSystemMap.Values)
        {
            // ��updateSystemRelatedEntityMap�ֵ��л�ȡ�뵱ǰupdateSystem�������ʵ���б�
            List<ECSEntity> updateSystemRelatedEntities = updateSystemRelatedEntityMap[updateSystem];

            // ����뵱ǰupdateSystem�������ʵ���б�Ϊ�գ���������ǰѭ����������һ��IUpdateSystemʵ��
            if (updateSystemRelatedEntities.Count == 0)
                continue;

            // ��ListPool<ECSEntity>�л�ȡһ���µ�ʵ���б����ڴ洢�����µ�ʵ��
            List<ECSEntity> entityList = ListPool<ECSEntity>.Obtain();

            // ����updateSystem�������ʵ���б���ӵ��µ�ʵ���б��У����������µ��ڴ�
            entityList.AddRangeNonAlloc(updateSystemRelatedEntities);

            // �����µ�ʵ���б��е�ÿ��ʵ��
            foreach (var entity in entityList)
            {
                // ��鵱ǰʵ���Ƿ����ڱ�updateSystem�۲죬������ǣ���������ǰѭ����������һ��ʵ��
                if (!updateSystem.ObservingEntity(entity))
                    continue;

                // ����updateSystem��Update���������µ�ǰʵ��
                updateSystem.Update(entity);
            }

            // ��ʹ�ù���ʵ���б��ͷŻ�ListPool<ECSEntity>���Ա㸴��
            ListPool<ECSEntity>.Release(entityList);
        }
    }

    private void DriveLateUpdateSystem()//���ڸ�����lateUpdateSystemMap�ֵ��д洢��ILateUpdateSystem�ӿ�ʵ���������ʵ�壨ECSEntity��
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

    private void DriveFixedUpdateSystem()//���ڸ�����lfixedUpdateSystemMap�ֵ��д洢��IFixedUpdateSystem�ӿ�ʵ���������ʵ�壨ECSEntity��
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
    /// ��ȡ������һ��ʵ���� IAwakeSystem �ӿڵ�ϵͳ
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <param name="list"></param>
    private void GetAwakeSystems<C>(List<IAwakeSystem> list) where C : ECSComponent
    {
        foreach (var awakeSystem in awakeSystemMap.Values)
        {
            if (awakeSystem.ComponentType() == typeof(C))//����ÿһ�� awakeSystem��������������
            {
                list.Add(awakeSystem);
            }
        }
    }
    /// <summary>
    /// ����ʵ�����ϵͳ��ECS��������Ļ����߼�
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
    /// ���²�ͬϵͳ���� IUpdateSystem��ILateUpdateSystem �� IFixedUpdateSystem���и��ٵ�ʵ���б�
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
                if (updateSystem.ObservingEntity(entity))//��鵱ǰϵͳ�Ƿ����ڹ۲����ʵ��
                {
                    entityList.Add(entity);
                }
            }
            else
            {
                if (!updateSystem.ObservingEntity(entity))//��鵱ǰϵͳ�Ƿ����ڹ۲����ʵ��
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
    /// �������ʵ����ӵ����ϻ�ӳ����
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
    /// ����ID����ʵ��
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
    /// ʵ��ID��entityID���ϲ��Ҳ������ض����͵������T��
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityID"></param>
    /// <returns></returns>
    public T FindComponentOfEntity<T>(long entityID) where T : ECSComponent
    {
        return FindEntity(entityID)?.GetComponent<T>();
    }

    // �첽����������������ض�ID��ʵ�巢����Ϣ
    public async Task SendMessageToEntity<M>(long id, M m)
    {
        // ���idΪ0����ִ���κβ�����ֱ�ӷ���
        if (id == 0)
            return;

        // ���Ҿ���ָ��id��ʵ��
        ECSEntity entity = FindEntity(id);
        // ����Ҳ���ʵ�壬��ִ���κβ�����ֱ�ӷ���
        if (entity == null)
            return;

        // ��ȡ��Ϣ������
        Type messageType = m.GetType();
        // ���Դ�ӳ���л�ȡ����Ϣ���Ͷ�Ӧ����Ϣ�������б�
        if (!entityMessageHandlerMap.TryGetValue(messageType, out List<IEntityMessageHandler> list))
            return;

        // �Ӷ�����л�ȡһ��IEntityMessageHandler���͵��б�
        List<IEntityMessageHandler> entityMessageHandlers = ListPool<IEntityMessageHandler>.Obtain();
        // ����ӳ���л�ȡ����Ϣ�������б���ӵ��Ӷ���ػ�ȡ���б���
        entityMessageHandlers.AddRangeNonAlloc(list);

        // ������Ϣ�������б�������ÿ����������Post��������ʵ�巢����Ϣ
        foreach (IEntityMessageHandler<M> handler in entityMessageHandlers)
        {
            // �첽����Post���������ȴ������
            await handler.Post(entity, m);
        }

        // ��ʹ�ù����б��ͷŻض����
        ListPool<IEntityMessageHandler>.Release(entityMessageHandlers);
    }

    // �첽����������������ض�ID��ʵ�巢��Զ�̹��̵���(RPC)����������Ӧ
    public async Task<Response> SendRpcToEntity<Request, Response>(long entityID, Request request)
        where Response : IEntityRpcResponse, new()
    {
        // ���ʵ��IDΪ0���򷵻ذ����������Ӧ����
        if (entityID == 0)
            return new Response() { Error = true };

        // ���Ҿ���ָ��ID��ʵ��
        ECSEntity entity = FindEntity(entityID);
        // ����Ҳ���ʵ�壬�򷵻ذ����������Ӧ����
        if (entity == null)
            return new Response() { Error = true };

        // ��ȡ������Ϣ������
        Type messageType = request.GetType();
        // ���Դ�ӳ���л�ȡ����Ϣ���Ͷ�Ӧ��ʵ��RPC������
        if (!entityRpcHandlerMap.TryGetValue(messageType, out IEntityRpcHandler entityRpcHandler))
            return new Response() { Error = true };

        // ���Խ�ʵ��RPC������ת��Ϊ�������͵Ĵ�����
        IEntityRpcHandler<Request, Response> handler = entityRpcHandler as IEntityRpcHandler<Request, Response>;
        // ���ת��ʧ�ܣ�����������֧�ָ��������ͺ���Ӧ���ͣ����򷵻ذ����������Ӧ����
        if (handler == null)
            return new Response() { Error = true };

        // �첽���ô�������Post��������ʵ�巢��RPC���󣬲��ȴ���Ӧ
        return await handler.Post(entity, request);
    }
}


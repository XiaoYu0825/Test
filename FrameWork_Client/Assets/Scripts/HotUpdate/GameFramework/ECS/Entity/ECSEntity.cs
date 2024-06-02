using System;
using System.Collections.Generic;

/// <summary>
///  ESC 实体
/// </summary>
public class ECSEntity : IDisposable
{
    public long InstanceID { get; private set; }//唯一标识符
    public long ParentID { get; private set; }//父实体的标识符
    public bool Disposed { get; private set; }//于表示这个实体是否已经被释放或销毁
    /// <summary>
    /// 返回当前实体的父实体
    /// </summary>
    public ECSEntity Parent
    {
        get
        {
            if (ParentID == 0)
                return default;

            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(ParentID);
        }
    }

    public long SceneID { get; set; }
    /// <summary>
    /// 返回类型为 ECSScene 的对象
    /// </summary>
    public ECSScene Scene
    {
        get
        {
            if (SceneID == 0)
                return default;

            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(SceneID) as ECSScene;
        }
    }

    private List<ECSEntity> children = new List<ECSEntity>();//存储当前实体的子实体列表
    private Dictionary<Type, ECSComponent> componentMap = new Dictionary<Type, ECSComponent>();//存储当前实体所拥有的组件

    public ECSEntity()
    {
        InstanceID = IDGenerator.NewInstanceID();
        TGameFramework.Instance.GetModule<ECSModule>().AddEntity(this);
    }

    public virtual void Dispose()
    {
        if (Disposed)
            return;

        Disposed = true;
        // 销毁Child
        for (int i = children.Count - 1; i >= 0; i--)
        {
            ECSEntity child = children[i];
            children.RemoveAt(i);
            child?.Dispose();
        }

        // 销毁Component
        List<ECSComponent> componentList = ListPool<ECSComponent>.Obtain();
        foreach (var component in componentMap.Values)
        {
            componentList.Add(component);
        }

        foreach (var component in componentList)
        {
            componentMap.Remove(component.GetType());
            TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent(component);
        }
        ListPool<ECSComponent>.Release(componentList);

        // 从父节点移除
        Parent?.RemoveChild(this);
        // 从世界中移除
        TGameFramework.Instance.GetModule<ECSModule>().RemoveEntity(this);
    }

    public bool HasComponent<C>() where C : ECSComponent
    {
        return componentMap.ContainsKey(typeof(C));
    }

    public C GetComponent<C>() where C : ECSComponent
    {
        componentMap.TryGetValue(typeof(C), out var component);
        return component as C;
    }

    public C AddNewComponent<C>() where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
        return component;
    }

    public C AddNewComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
        return component;
    }

    public C AddNewComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            RemoveComponent<C>();
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
        return component;
    }

    public C AddComponent<C>() where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component);
        return component;
    }

    public C AddComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1);
        return component;
    }

    public C AddComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        if (HasComponent<C>())
        {
            UnityLog.Error($"Duplicated Component:{typeof(C).FullName}");
            return default;
        }

        C component = new C();
        component.EntityID = InstanceID;
        componentMap.Add(typeof(C), component);
        TGameFramework.Instance.GetModule<ECSModule>().AwakeComponent(component, p1, p2);
        return component;
    }

    public void RemoveComponent<C>() where C : ECSComponent, new()
    {
        Type componentType = typeof(C);
        if (!componentMap.TryGetValue(componentType, out var component))
            return;

        componentMap.Remove(componentType);
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component);
    }

    public void RemoveComponent<C, P1>(P1 p1) where C : ECSComponent, new()
    {
        Type componentType = typeof(C);
        if (!componentMap.TryGetValue(componentType, out var component))
            return;

        componentMap.Remove(componentType);
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1);
    }

    public void RemoveComponent<C, P1, P2>(P1 p1, P2 p2) where C : ECSComponent, new()
    {
        Type componentType = typeof(C);
        if (!componentMap.TryGetValue(componentType, out var component))
            return;

        componentMap.Remove(componentType);
        TGameFramework.Instance.GetModule<ECSModule>().DestroyComponent((C)component, p1, p2);
    }

    public void AddChild(ECSEntity child)
    {
        if (child == null)
            return;

        if (child.Disposed)
            return;

        ECSEntity oldParent = child.Parent;
        if (oldParent != null)
        {
            oldParent.RemoveChild(child);
        }

        children.Add(child);
        child.ParentID = InstanceID;
    }

    public void RemoveChild(ECSEntity child)
    {
        if (child == null)
            return;

        children.Remove(child);
        child.ParentID = 0;
    }

    public T FindChild<T>(long id) where T : ECSEntity
    {
        foreach (var child in children)
        {
            if (child.InstanceID == id)
                return child as T;
        }

        return default;
    }

    public T FindChild<T>(Predicate<T> predicate) where T : ECSEntity
    {
        foreach (var child in children)
        {
            T c = child as T;
            if (c == null)
                continue;

            if (predicate.Invoke(c))
            {
                return c;
            }
        }

        return default;
    }

    public void FindChildren<T>(List<T> list) where T : ECSEntity
    {
        foreach (var child in children)
        {
            if (child is T)
            {
                list.Add(child as T);
            }
        }
    }
}


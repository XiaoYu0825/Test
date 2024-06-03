using System.Collections.Generic;
/// <summary>
/// 管理场景中的实体
/// </summary>

public class ECSScene : ECSEntity
{
    private Dictionary<long, ECSEntity> entities;

    public ECSScene()
    {
        entities = new Dictionary<long, ECSEntity>();
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public override void Dispose()
    {
        if (Disposed)//表示对象已经释放
            return;

        List<long> entityIDList = ListPool<long>.Obtain();//对象池中获取空的List<long>
        foreach (var entityID in entities.Keys)//遍历entities字典中的所有键（实体ID）
        {
            entityIDList.Add(entityID);
        }
        foreach (var entityID in entityIDList)
        {
            ECSEntity entity = entities[entityID];//获取相应的对象
            entity.Dispose();
        }
        ListPool<long>.Release(entityIDList);//列表归还给对象池

        base.Dispose(); //以确保释放基类级别的任何资源
    }
    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="entity"></param>
    public void AddEntity(ECSEntity entity)
    {
        if (entity == null)
            return;
        //从旧场景移除实体
        ECSScene oldScene = entity.Scene;//获取实体当前所属的场景
        if (oldScene != null)
        {
            oldScene.RemoveEntity(entity.InstanceID);//获取实体当前所属的场景
        }
        //将实体添加到新场景
        entities.Add(entity.InstanceID, entity);
        entity.SceneID = InstanceID;
        UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
    }
    /// <summary>
    /// 移除实体
    /// </summary>
    /// <param name="entityID"></param>
    public void RemoveEntity(long entityID)
    {
        if (entities.Remove(entityID))
        {
            UnityLog.Info($"Scene Remove Entity, Current Count:{entities.Count}");
        }
    }
    /// <summary>
    /// 查找和收集场景中所有属于指定类型T的实体的ID
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public void FindEntities<T>(List<long> list) where T : ECSEntity
    {
        foreach (var item in entities)
        {
            if (item.Value is T)
            {
                list.Add(item.Key);
            }
        }
    }
    /// <summary>
    /// 场景中的所有实体，检查是否包含指定类型的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
    {
        foreach (var item in entities)
        {
            if (item.Value.HasComponent<T>())//实体是否包含指定类型的组件
            {
                list.Add(item.Key);
            }
        }
    }
    /// <summary>
    /// 获取每个实体的ID添加到传入的列表中
    /// </summary>
    /// <param name="list"></param>
    public void GetAllEntities(List<long> list)
    {
        foreach (var item in entities)
        {
            list.Add(item.Key);
        }
    }
}

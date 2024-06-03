using System.Collections.Generic;
/// <summary>
/// �������е�ʵ��
/// </summary>

public class ECSScene : ECSEntity
{
    private Dictionary<long, ECSEntity> entities;

    public ECSScene()
    {
        entities = new Dictionary<long, ECSEntity>();
    }
    /// <summary>
    /// �ͷ���Դ
    /// </summary>
    public override void Dispose()
    {
        if (Disposed)//��ʾ�����Ѿ��ͷ�
            return;

        List<long> entityIDList = ListPool<long>.Obtain();//������л�ȡ�յ�List<long>
        foreach (var entityID in entities.Keys)//����entities�ֵ��е����м���ʵ��ID��
        {
            entityIDList.Add(entityID);
        }
        foreach (var entityID in entityIDList)
        {
            ECSEntity entity = entities[entityID];//��ȡ��Ӧ�Ķ���
            entity.Dispose();
        }
        ListPool<long>.Release(entityIDList);//�б�黹�������

        base.Dispose(); //��ȷ���ͷŻ��༶����κ���Դ
    }
    /// <summary>
    /// ���ʵ��
    /// </summary>
    /// <param name="entity"></param>
    public void AddEntity(ECSEntity entity)
    {
        if (entity == null)
            return;
        //�Ӿɳ����Ƴ�ʵ��
        ECSScene oldScene = entity.Scene;//��ȡʵ�嵱ǰ�����ĳ���
        if (oldScene != null)
        {
            oldScene.RemoveEntity(entity.InstanceID);//��ȡʵ�嵱ǰ�����ĳ���
        }
        //��ʵ����ӵ��³���
        entities.Add(entity.InstanceID, entity);
        entity.SceneID = InstanceID;
        UnityLog.Info($"Scene Add Entity, Current Count:{entities.Count}");
    }
    /// <summary>
    /// �Ƴ�ʵ��
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
    /// ���Һ��ռ���������������ָ������T��ʵ���ID
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
    /// �����е�����ʵ�壬����Ƿ����ָ�����͵����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public void FindEntitiesWithComponent<T>(List<long> list) where T : ECSComponent
    {
        foreach (var item in entities)
        {
            if (item.Value.HasComponent<T>())//ʵ���Ƿ����ָ�����͵����
            {
                list.Add(item.Key);
            }
        }
    }
    /// <summary>
    /// ��ȡÿ��ʵ���ID��ӵ�������б���
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

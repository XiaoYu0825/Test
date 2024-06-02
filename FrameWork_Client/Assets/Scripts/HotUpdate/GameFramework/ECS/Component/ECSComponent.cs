/// <summary>
/// ECS 组件
/// </summary>
public abstract class ECSComponent
{
    public long ID { get; private set; }//存储组件的唯一ID
    public long EntityID { get; set; }//存储与这个组件关联的实体的ID。
    public bool Disposed { get; set; }//记组件是否已经被释放或销毁

    /// <summary>
    /// 返回与这个组件关联的 ECSEntity 对象
    /// </summary>
    public ECSEntity Entity
    {
        get
        {
            if (EntityID == 0)
                return default;

            return TGameFramework.Instance.GetModule<ECSModule>().FindEntity(EntityID);
        }
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    public ECSComponent()
    {
        ID = IDGenerator.NewInstanceID();//生成一个新的唯一ID
        Disposed = false;//属性设置为 false，表示这个组件还没有被释放或销毁
    }
}


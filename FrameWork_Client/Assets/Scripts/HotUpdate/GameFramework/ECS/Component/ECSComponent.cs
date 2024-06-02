/// <summary>
/// ECS ���
/// </summary>
public abstract class ECSComponent
{
    public long ID { get; private set; }//�洢�����ΨһID
    public long EntityID { get; set; }//�洢��������������ʵ���ID��
    public bool Disposed { get; set; }//������Ƿ��Ѿ����ͷŻ�����

    /// <summary>
    /// ������������������ ECSEntity ����
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
    /// ���캯��
    /// </summary>
    public ECSComponent()
    {
        ID = IDGenerator.NewInstanceID();//����һ���µ�ΨһID
        Disposed = false;//��������Ϊ false����ʾ��������û�б��ͷŻ�����
    }
}


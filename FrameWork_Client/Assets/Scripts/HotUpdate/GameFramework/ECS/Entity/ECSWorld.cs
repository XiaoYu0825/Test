/// <summary>
/// ��Ϸ����򻷾�
/// </summary>
public class ECSWorld : ECSScene
{
    //����һ��Ĭ�ϵĹ��캯����ֵ�����ڴ��� ECSWorld ����ʱ��������ر����� GameScene ���ԣ�
    //�����Զ�����ʼ��Ϊһ���µ� ECSScene ʵ��
    public ECSScene GameScene { get; set; } = new ECSScene();
}


using System.Threading.Tasks;

/// <summary>
/// ����: Teddy
/// ʱ��: 2018/03/01
/// ����: 
/// </summary>
public abstract class BaseProcedure  //���������������
{
    //���ڸı䵱ǰ�ĳ������̵���һ�� BaseProcedure ���͵�ʵ��
    public async Task ChangeProcedure<T>(object value = null) where T : BaseProcedure
    {
        await GameManager.Procedure.ChangeProcedure<T>(value);
    }

    //���뵱ǰ��������ʱ������
    public virtual async Task OnEnterProcedure(object value)
    {
        await Task.Yield();
    }

    //�뿪��ǰ��������ʱ������
    public virtual async Task OnLeaveProcedure()
    {
        await Task.Yield();
    }
}

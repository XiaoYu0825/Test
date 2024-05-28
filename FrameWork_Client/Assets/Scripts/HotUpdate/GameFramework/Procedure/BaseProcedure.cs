using System.Threading.Tasks;

/// <summary>
/// 作者: Teddy
/// 时间: 2018/03/01
/// 功能: 
/// </summary>
public abstract class BaseProcedure  //用来管理程序流程
{
    //用于改变当前的程序流程到另一个 BaseProcedure 类型的实例
    public async Task ChangeProcedure<T>(object value = null) where T : BaseProcedure
    {
        await GameManager.Procedure.ChangeProcedure<T>(value);
    }

    //进入当前程序流程时被调用
    public virtual async Task OnEnterProcedure(object value)
    {
        await Task.Yield();
    }

    //离开当前程序流程时被调用
    public virtual async Task OnLeaveProcedure()
    {
        await Task.Yield();
    }
}

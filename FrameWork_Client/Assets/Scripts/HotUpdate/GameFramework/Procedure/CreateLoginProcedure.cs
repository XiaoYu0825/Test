
using System.Threading.Tasks;

public class CreateLoginProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        //await GameManager.UI.OpenUIAsync(UIViewID.CreateLoginUI, value);

        await Task.Yield();
    }
}

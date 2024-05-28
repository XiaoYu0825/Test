using System;
using System.Threading.Tasks;


public class InitProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        UnityLog.Info("enter init procedure");
       // GameManager.ECS.World.AddComponent<KnapsackComponent>();
        //GameManager.ECS.World.AddComponent<PlayerInfoComponent>();
        //GameManager.ECS.World.AddComponent<GameSceneComponent>();
        // GameManager.ECS.World.AddNewComponent<PlayerComponent>();



    }

}


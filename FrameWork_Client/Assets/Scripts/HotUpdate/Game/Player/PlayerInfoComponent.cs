using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// 玩家信息实体组件
/// </summary>
public class PlayerInfoComponent : ECSComponent
{
    public UserInfo userInfo;

}
/// <summary>
/// 用于处理 PlayerInfoComponent的唤醒Awake逻辑
/// </summary>
public class PlayerInfoComponentAwakeSystem : AwakeSystem<PlayerInfoComponent>
{
    public override void Awake(PlayerInfoComponent c)
    {
        c.userInfo = new UserInfo();

    }
}





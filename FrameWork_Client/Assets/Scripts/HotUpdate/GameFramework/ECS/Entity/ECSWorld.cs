/// <summary>
/// 游戏世界或环境
/// </summary>
public class ECSWorld : ECSScene
{
    //它有一个默认的构造函数赋值，即在创建 ECSWorld 对象时，如果不特别设置 GameScene 属性，
    //它会自动被初始化为一个新的 ECSScene 实例
    public ECSScene GameScene { get; set; } = new ECSScene();
}


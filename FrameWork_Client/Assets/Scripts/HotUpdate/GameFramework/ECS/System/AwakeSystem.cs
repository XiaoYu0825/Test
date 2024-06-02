using System;


public interface IAwakeSystem : ISystem //只是继承了ISystem的定义不用实现ISystem里的方法
{
    Type ComponentType();//于标识或返回该觉醒系统所关联的组件类型

}
/// <summary>
/// 用于定义一个通用的觉醒系统框架
/// </summary>
/// <typeparam name="C"></typeparam>
[ECSSystem]
public abstract class AwakeSystem<C> : IAwakeSystem where C : ECSComponent
{
    public abstract void Awake(C c);//处理或初始化给定类型的组件 C

    public Type ComponentType()//这个实例都能告诉调用者它所关联的组件类型
    {
        return typeof(C);
    }


    public Type SystemType()//当前实例的实际类型
    {
        return GetType();//它可以返回 AwakeSystem<C> 的具体实现类的 Type 对象
    }
}

[ECSSystem]
    public abstract class AwakeSystem<C, P1> : IAwakeSystem where C : ECSComponent
    {
        public abstract void Awake(C c, P1 p1);

        public Type ComponentType()
        {
            return typeof(C);
        }

        public Type SystemType()
        {
            return GetType();
        }
    }


    [ECSSystem]
    public abstract class AwakeSystem<C, P1, P2> : IAwakeSystem where C : ECSComponent
    {
        public abstract void Awake(C c, P1 p1, P2 p2);

        public Type ComponentType()
        {
            return typeof(C);
        }

        public Type SystemType()
        {
            return GetType();
        }
    }
 

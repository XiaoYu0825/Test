using System;


public interface IAwakeSystem : ISystem //ֻ�Ǽ̳���ISystem�Ķ��岻��ʵ��ISystem��ķ���
{
    Type ComponentType();//�ڱ�ʶ�򷵻ظþ���ϵͳ���������������

}
/// <summary>
/// ���ڶ���һ��ͨ�õľ���ϵͳ���
/// </summary>
/// <typeparam name="C"></typeparam>
[ECSSystem]
public abstract class AwakeSystem<C> : IAwakeSystem where C : ECSComponent
{
    public abstract void Awake(C c);//������ʼ���������͵���� C

    public Type ComponentType()//���ʵ�����ܸ��ߵ����������������������
    {
        return typeof(C);
    }


    public Type SystemType()//��ǰʵ����ʵ������
    {
        return GetType();//�����Է��� AwakeSystem<C> �ľ���ʵ����� Type ����
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
 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestComponent : ECSComponent
{
    
}
public class TestComponentAwakeSystem : AwakeSystem<TestComponent>
{
    public override void Awake(TestComponent c)
    {
        Debug.Log("TestComponentAwakeSystem");
    }
}
public class TestComponentUpdateSystem : UpdateSystem<TestComponent>
{
    public override void Update(ECSEntity entity)
    {
        Debug.Log("TestComponentUpdateSystem");
    }
}
public class TestComponentLateUpdateSystem : LateUpdateSystem<TestComponent>
{
    public override void LateUpdate(ECSEntity entity)
    {
        Debug.Log("TestComponentLateUpdateSystem");
    }
}
public class TestComponentFixedUpdateSystem : FixedUpdateSystem<TestComponent>
{
    public override void FixedUpdate(ECSEntity entity)
    {
       Debug.Log("TestComponentFixedUpdateSystem");
    }
}

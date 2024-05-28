using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGameFramework : MonoBehaviour
{
    public static TGameFramework Instance { get; private set; } //单例 访问器 get 属性可以在外部读取  set 限制属性的设置只能在类内部进行
    public static bool Initialized { get; private set; } // 初始化 bool 访问器 get 属性可以在外部读取  set 限制属性的设置只能在类内部进行

    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>(); //字典储存BaseGameModule类型

    public static void Initialize() //初始化
    {
        Instance = new TGameFramework(); 
    }
    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T), out BaseGameModule module))//判断字典的值是否有T这个类型 ture/false
        {
            return module as T;  //返回module 
        }

        return default(T); //返回T类型默认值
    }
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();//获取传入的module对象的类型
        if (m_modules.ContainsKey(moduleType))
        {
            Debug.Log("Module添加失败，重复:"+ moduleType.Name);
            return;
        }
        m_modules.Add(moduleType, module);
    }
    public void Update()
    {
        if (!Initialized) 
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleUpdate(deltaTime); //调用moduel(BaseGameModule)的方法
        }
    }
    public void LateUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleLateUpdate(deltaTime);
        }
    }

    public void FixedUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.fixedDeltaTime; //固定时间帧更新
        foreach (var module in m_modules.Values)
        {
            module.OnModuleFixedUpdate(deltaTime);
        }
    }
    public void InitModules()
    {
        if (Initialized)
            return;

        Initialized = true;
        //StartupModules();
        foreach (var module in m_modules.Values)
        {
            module.OnModuleInit();
        }
    }

    public void StartModules()
    {
        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        foreach (var module in m_modules.Values)
        {
            module.OnModuleStart();
        }
    }
    public void Destroy()
    {
        if (!Initialized)
            return;

        if (Instance != this)
            return;

        if (Instance.m_modules == null)
            return;

        foreach (var module in Instance.m_modules.Values)
        {
            module.OnModuleStop();
        }

        //Destroy(Instance.gameObject);
        Instance = null;//经被销毁或不再使用
        Initialized = false;//不再处于初始化状态
    }

}

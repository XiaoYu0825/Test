using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGameFramework : MonoBehaviour
{
    public static TGameFramework Instance { get; private set; } //���� ������ get ���Կ������ⲿ��ȡ  set �������Ե�����ֻ�������ڲ�����
    public static bool Initialized { get; private set; } // ��ʼ�� bool ������ get ���Կ������ⲿ��ȡ  set �������Ե�����ֻ�������ڲ�����

    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>(); //�ֵ䴢��BaseGameModule����

    public static void Initialize() //��ʼ��
    {
        Instance = new TGameFramework(); 
    }
    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T), out BaseGameModule module))//�ж��ֵ��ֵ�Ƿ���T������� ture/false
        {
            return module as T;  //����module 
        }

        return default(T); //����T����Ĭ��ֵ
    }
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();//��ȡ�����module���������
        if (m_modules.ContainsKey(moduleType))
        {
            Debug.Log("Module���ʧ�ܣ��ظ�:"+ moduleType.Name);
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
            module.OnModuleUpdate(deltaTime); //����moduel(BaseGameModule)�ķ���
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

        float deltaTime = UnityEngine.Time.fixedDeltaTime; //�̶�ʱ��֡����
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
        Instance = null;//�������ٻ���ʹ��
        Initialized = false;//���ٴ��ڳ�ʼ��״̬
    }

}

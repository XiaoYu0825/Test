using Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    [Module(1)]
    public static AssetModule Asset { get => TGameFramework.Instance.GetModule<AssetModule>(); }

    [Module(2)]
    public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }//获取特定类型的模块实例

    [Module(3)]
    public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }

    [Module(4)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }//获取特定类型的模块实例


    [Module(5)]
    public static ECSModule ECS { get => TGameFramework.Instance.GetModule<ECSModule>(); }


    [Module(6)]
    public static NetModule Net { get => TGameFramework.Instance.GetModule<NetModule>(); }//获取特定类型的模块实例

    public Button sendbut;
    //bool activing = true;
    private void Awake()
    {

        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        TGameFramework.Initialize();
        StartupModules();
        TGameFramework.Instance.InitModules();

        ConfigManager.LoadAllConfigsByAddressable("Assets/BundleAssets/Config");
        ECS.World.AddComponent<TestComponent>();
    }
    
    public void StartupModules()//查找和初始化所有从BaseGameModule派生的组件
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();//存储找到的模块属性
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);//所有公共、非公共和静态属性的信息
        Type baseCompType = typeof(BaseGameModule);//定义一个baseCompType变量，它表示BaseGameModule类型的Type对象
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            if (!baseCompType.IsAssignableFrom(property.PropertyType))//是否可以从BaseGameModule类型派生-
                continue;

            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);//方法来获取property属性上的所有ModuleAttribute类型的自定义属性 false表示只搜索当前属性
            if (attrs.Length == 0)
                continue;

            Component comp = GetComponentInChildren(property.PropertyType);//当前对象或其子对象中寻找一个与当前属性类型相匹配的组件
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;//ModuleAttribute的Module属性设置为找到的组件
            moduleAttrs.Add(moduleAttr);//ModuleAttribute对象添加到moduleAttrs列表中
        }

        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;//排序的依据是ModuleAttribute对象的Priority属性
        });

        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        sendbut.onClick.AddListener(() => {
            GameManager.Message.Post<MessageType.Game>(new MessageType.Game() { }).Coroutine();
            GameManager.UI.OpenUI(UIViewID.TestPanel);
        });
        TGameFramework.Instance.StartModules();
       
    }

    // Update is called once per frame
    private void Update()
    {
        TGameFramework.Instance.Update();
    }

    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        //TGameFramework.Instance.FixedUpdate();
    }
    private void OnDestroy()
    {
      
      TGameFramework.Instance.Destroy();
        
    }
    //AttributeTargets.Property 只能应用属性Property  Inherited  不能被子类继承  AllowMultiple 同一个属性不能多次被应用特性
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; } //只能在ModuleAttribute中设置值
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }   //可以在外部设置值

        /// <summary>
        /// 添加该特性才会被当作模块
        /// </summary>
        /// <param name="priority">控制器优先级,数值越小越先执行</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority; //Priority设置值
        }
        //优先级进行排序
        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    public class GameUIMessageHandler : MessageHandler<MessageType.Game>
    {
        public override async Task HandleMessage(MessageType.Game arg)
        {
            Debug.Log("点击按钮");
            await Task.Yield();
        }
    }
}

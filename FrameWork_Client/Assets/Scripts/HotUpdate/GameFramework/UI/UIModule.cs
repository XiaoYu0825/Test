using Config;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;


public partial class UIModule : BaseGameModule
{
    public Transform normalUIRoot;//正常的根节点
    public Transform modalUIRoot;//模式根节点
    public Transform closeUIRoot; //关闭面板的根节点
    public Image imgMask;
    public QuantumConsole prefabQuantumConsole;//预制体在Unity中创建可重复使用的游戏对象的模板

    private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING; //存储与UI视图相关联Mediator的类型
    private static Dictionary<UIViewID, Type> ASSET_MAPPING;//可能用于存储与UI视图相关联Assets的类型

    private readonly List<UIMediator> usingMediators = new List<UIMediator>();//存储当前正在使用的Mediator
    private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>();//Mediator不在使用时回收队列
    private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>();//对象池集合
    private QuantumConsole quantumConsole; //控制台或日志系统的实例

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        //quantumConsole = Instantiate(prefabQuantumConsole);
        //quantumConsole.transform.SetParentAndResetAll(transform);
        //quantumConsole.OnActivate += OnConsoleActive;
        //quantumConsole.OnDeactivate += OnConsoleDeactive;
    }

    protected internal override void OnModuleStop()
    {
        //base.OnModuleStop();
        //quantumConsole.OnActivate -= OnConsoleActive;
        //quantumConsole.OnDeactivate -= OnConsoleDeactive;
    }

    private static void CacheUIMapping()//用于缓存UI视图与Mediator和Asset之间的映射关系
    {
        Debug.Log("888888888888888");
        if (MEDIATOR_MAPPING != null)
            return;
        Debug.Log("88888888888888811111111111111");
        MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>();
        ASSET_MAPPING = new Dictionary<UIViewID, Type>();

        Type baseViewType = typeof(UIView);//获取UIView类型的 Type对象
        foreach (var type in baseViewType.Assembly.GetTypes())//遍历UIView类型所在的程序集中的所有类型
        {
            if (type.IsAbstract)
                continue;

            if (baseViewType.IsAssignableFrom(type))//查type是否是UIView的子类或实现了UIView接口
            {
                object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false);//获取当前类型上定义的UIViewAttribute自定义属性的数组
                if (attrs.Length == 0)
                {
                    Debug.Log($"{type.FullName} 没有绑定 Mediator，请使用UIMediatorAttribute绑定一个Mediator以正确使用");
                    continue;
                }

                foreach (UIViewAttribute attr in attrs)
                {
                    MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType);//ID和关联的Mediator类型添加到MEDIATOR_MAPPING字典中
                    ASSET_MAPPING.Add(attr.ID, type);//ID和视图类型添加到 ASSET_MAPPING 字典中
                    break;
                }
            }
        }
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
        uiObjectPool.UpdateLoadRequests();//处理对象池的加载请求
        foreach (var mediator in usingMediators)
        {
            mediator.Update(deltaTime);
        }
        UpdateMask(deltaTime);
    }

    private void OnConsoleActive()
    {
        //GameManager.Input.SetEnable(false);
    }

    private void OnConsoleDeactive()
    {
        //GameManager.Input.SetEnable(true);
    }

    private int GetTopMediatorSortingOrder(UIMode mode)
    {
        int lastIndexMediatorOfMode = -1;//存储与给定 UIMode 匹配的 UIMediator 在 usingMediators 列表中的最后一个索引
        for (int i = usingMediators.Count - 1; i >= 0; i--)
        {
            UIMediator mediator = usingMediators[i];//最后一个元素开始，向前遍历。usingMediators是一个包含UIMediator对象的列表
            if (mediator.UIMode != mode)//UIMode是否与给定的mode相同
                continue;

            lastIndexMediatorOfMode = i;// 更新 lastIndexMediatorOfMode 的值为当前索引 i
            break;
        }

        if (lastIndexMediatorOfMode == -1)
            return mode == UIMode.Normal ? 0 : 1000;

        return usingMediators[lastIndexMediatorOfMode].SortingOrder;
    }

    private UIMediator GetMediator(UIViewID id)
    {
        CacheUIMapping();

        if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType))//尝试查找与id相关联的UIMediator类型
        {
            UnityLog.Error($"找不到 {id} 对应的Mediator");
            return null;
        }

        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))//查找是否有现成的UIMediator对象队列
        {
            mediatorQ = new Queue<UIMediator>();
            freeMediators.Add(mediatorType, mediatorQ);
        }    

        UIMediator mediator;
        if (mediatorQ.Count == 0)
        {
            mediator = Activator.CreateInstance(mediatorType) as UIMediator;//创建一个新的UIMediator对象
        }
        else
        {
            Debug.Log("ssssssssssssssss");
            mediator = mediatorQ.Dequeue();//有可用的对象，则从队列中取出一个
        }

        return mediator;
    }

    private void RecycleMediator(UIMediator mediator)//回收UIMediator对象
    {
        if (mediator == null)
            return;

        Type mediatorType = mediator.GetType();//调用GetType()方法获取mediator的类型
        if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))//字典中获取与mediatorType相关联的Queue<UIMediator>对象
        {
            mediatorQ = new Queue<UIMediator>();
            freeMediators.Add(mediatorType, mediatorQ);
        }
        mediatorQ.Enqueue(mediator);
    }

    public UIMediator GetOpeningUIMediator(UIViewID id) //获取打开的Mediator
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);
        if (uiConfig.IsNull)
            return null;

        UIMediator mediator = GetMediator(id);
        if (mediator == null)
            return null;

        Type requiredMediatorType = mediator.GetType();
        foreach (var item in usingMediators)
        {
            if (item.GetType() == requiredMediatorType)
                return item;
        }
        return null;
    }

    public void BringToTop(UIViewID id)//将指定的UIMediator对象排序顺序更新为顶层，UI层级中显示在最前面
    {
        UIMediator mediator = GetOpeningUIMediator(id);//传入的id获取对应的UIMediator对象
        if (mediator == null)
            return;

        int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode);//获取顶层媒介的排序顺序并将其存储在topSortingOrder变量中
        if (mediator.SortingOrder == topSortingOrder)//mediator的SortingOrder是否已经是顶层排序顺序
            return;

        int sortingOrder = topSortingOrder + 10;//将sortingOrder设置为顶层排序顺序加10
        mediator.SortingOrder = sortingOrder;//sortingOrder赋值给mediator的SortingOrder属性

        usingMediators.Remove(mediator);//集合中移除mediator对象
        usingMediators.Add(mediator);//添加回集合。这样做通常是为了确保mediator对象在集合中的位置更新

        Canvas canvas = mediator.ViewObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = sortingOrder;//则将其sortingOrder属性设置为之前计算出的sortingOrder值，以确
        }
    }

    public bool IsUIOpened(UIViewID id)
    {
        return GetOpeningUIMediator(id) != null;
    }

    public UIMediator OpenUISingle(UIViewID id, object arg = null)//尝试打开一个具有特定UIViewID的UIMediator
    {
        UIMediator mediator = GetOpeningUIMediator(id);//尝试获取一个已经打开的UIMediator对象，并将其赋值给mediator变量
        if (mediator != null)
            return mediator;

        return OpenUI(id, arg);
    }

    public UIMediator OpenUI(UIViewID id, object arg = null)//打开或加载一个具有特定UIViewID的UI界面
    {
        Debug.Log("111111111111111////" + id.ToString());
        UIConfig uiConfig = UIConfig.ByID((int)id);//传入id的整数值，获取对应的UIConfig对象
        if (uiConfig.IsNull)
            return null;

        UIMediator mediator = GetMediator(id);
        if (mediator == null)
            return null;
        Debug.Log("11111111111111111111111111111111111111111111111111111111");
        GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>();
            mediator.InitMediator(newView);
        })).gameObject;

        return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
    }

    public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)//步地打开一个具有特定UIViewID的UI界面
    {
        if (!IsUIOpened(id))
        {
            yield return OpenUIAsync(id, arg);
        }
    }

    public IEnumerator OpenUIAsync(UIViewID id, object arg = null)//异步打开界面
    {
        UIConfig uiConfig = UIConfig.ByID((int)id);
        if (uiConfig.IsNull)
            yield break;

        UIMediator mediator = GetMediator(id);
        if (mediator == null)
            yield break;

        bool loadFinish = false;
        uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
        {
            GameObject uiObject = asset.gameObject;
            OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
            loadFinish = true;
        }, (obj) =>
        {
            UIView newView = obj.GetComponent<UIView>();
            mediator.InitMediator(newView);
        });
        while (!loadFinish)
        {
            yield return null;
        }
        yield return null;
        yield return null;
    }

    private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)//加载预制体
    {
        if (uiObject == null)
        {
            UnityLog.Error($"加载UI失败:{uiConfig.Asset}");
            RecycleMediator(mediator);
            return null;
        }

        UIView view = uiObject.GetComponent<UIView>();
        if (view == null)
        {
            UnityLog.Error($"UI Prefab不包含UIView脚本:{uiConfig.Asset}");
            RecycleMediator(mediator);
            uiObjectPool.UnloadGameObject(view.gameObject);
            return null;
        }

        mediator.UIMode = uiConfig.Mode;
        int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10;

        usingMediators.Add(mediator);

        Canvas canvas = uiObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        //canvas.worldCamera = GameManager.Camera.uiCamera;
        if (uiConfig.Mode == UIMode.Normal)
        {
            uiObject.transform.SetParentAndResetAll(normalUIRoot);
            canvas.sortingLayerName = "NormalUI";
        }
        else
        {
            uiObject.transform.SetParentAndResetAll(modalUIRoot);
            canvas.sortingLayerName = "ModalUI";
        }

        mediator.SortingOrder = sortingOrder;
        canvas.sortingOrder = sortingOrder;

        uiObject.SetActive(true);
        mediator.Show(uiObject, obj);
        return mediator;
    }

    public void CloseUI(UIMediator mediator) 
    {
        if (mediator != null)
        {
            // 回收View
            uiObjectPool.UnloadGameObject(mediator.ViewObject);
            mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot);

            // 回收Mediator
            mediator.Hide();
            RecycleMediator(mediator);

            usingMediators.Remove(mediator);
        }
    }

    public void CloseAllUI()
    {
        for (int i = usingMediators.Count - 1; i >= 0; i--)
        {
            CloseUI(usingMediators[i]);
        }
    }

    public void CloseUI(UIViewID id)
    {
        UIMediator mediator = GetOpeningUIMediator(id);
        if (mediator == null)
            return;

        CloseUI(mediator);
    }

    public void SetAllNormalUIVisibility(bool visible)
    {
        normalUIRoot.gameObject.SetActive(visible);
    }

    public void SetAllModalUIVisibility(bool visible)
    {
        modalUIRoot.gameObject.SetActive(visible);
    }

    public void ShowMask(float duration = 0.5f)
    {
        destMaskAlpha = 1;
        maskDuration = duration;
    }

    public void HideMask(float? duration = null)
    {
        destMaskAlpha = 0;
        if (duration.HasValue)
        {
            maskDuration = duration.Value;
        }
    }

    private float destMaskAlpha = 0;
    private float maskDuration = 0;
    private void UpdateMask(float deltaTime)
    {
        Color c = imgMask.color;
        c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha;
        c.a = Mathf.Clamp01(c.a);
        imgMask.color = c;
        imgMask.enabled = imgMask.color.a > 0;
    }

    public void ShowConsole()
    {
        quantumConsole.Activate();
    }
}


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class UIViewAttribute : Attribute
{
    public UIViewID ID { get; }
    public Type MediatorType { get; }

    public UIViewAttribute(Type mediatorType, UIViewID id)
    {
        ID = id;
        MediatorType = mediatorType;
    }
}

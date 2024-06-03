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
    public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }//��ȡ�ض����͵�ģ��ʵ��

    [Module(3)]
    public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }

    [Module(4)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }//��ȡ�ض����͵�ģ��ʵ��


    [Module(5)]
    public static ECSModule ECS { get => TGameFramework.Instance.GetModule<ECSModule>(); }


    [Module(6)]
    public static NetModule Net { get => TGameFramework.Instance.GetModule<NetModule>(); }//��ȡ�ض����͵�ģ��ʵ��

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
    
    public void StartupModules()//���Һͳ�ʼ�����д�BaseGameModule���������
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();//�洢�ҵ���ģ������
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);//���й������ǹ����;�̬���Ե���Ϣ
        Type baseCompType = typeof(BaseGameModule);//����һ��baseCompType����������ʾBaseGameModule���͵�Type����
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            if (!baseCompType.IsAssignableFrom(property.PropertyType))//�Ƿ���Դ�BaseGameModule��������-
                continue;

            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);//��������ȡproperty�����ϵ�����ModuleAttribute���͵��Զ������� false��ʾֻ������ǰ����
            if (attrs.Length == 0)
                continue;

            Component comp = GetComponentInChildren(property.PropertyType);//��ǰ��������Ӷ�����Ѱ��һ���뵱ǰ����������ƥ������
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;//ModuleAttribute��Module��������Ϊ�ҵ������
            moduleAttrs.Add(moduleAttr);//ModuleAttribute������ӵ�moduleAttrs�б���
        }

        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;//�����������ModuleAttribute�����Priority����
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
    //AttributeTargets.Property ֻ��Ӧ������Property  Inherited  ���ܱ�����̳�  AllowMultiple ͬһ�����Բ��ܶ�α�Ӧ������
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int Priority { get; private set; } //ֻ����ModuleAttribute������ֵ
        /// <summary>
        /// ģ��
        /// </summary>
        public BaseGameModule Module { get; set; }   //�������ⲿ����ֵ

        /// <summary>
        /// ��Ӹ����ԲŻᱻ����ģ��
        /// </summary>
        /// <param name="priority">���������ȼ�,��ֵԽСԽ��ִ��</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority; //Priority����ֵ
        }
        //���ȼ���������
        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    public class GameUIMessageHandler : MessageHandler<MessageType.Game>
    {
        public override async Task HandleMessage(MessageType.Game arg)
        {
            Debug.Log("�����ť");
            await Task.Yield();
        }
    }
}

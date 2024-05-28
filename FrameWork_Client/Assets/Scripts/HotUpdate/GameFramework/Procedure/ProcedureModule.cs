using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class ProcedureModule : BaseGameModule
{
    [SerializeField]   // 使用[SerializeField]属性，使得私有字段在Unity的Inspector中可见
    private  string[] proceduresNames = null;//程序的名称数组
    [SerializeField]
    private string defaultProcedureName = null; //默认程序名称

    public BaseProcedure CurrentProcedure { get; private set; }//正在运行的程序流程
    public bool IsRunning { get; private set; }//是否有程序流程正在运行
    public bool IsChangingProcedure { get; private set; }//是否正在改变程序流程

    private Dictionary<Type, BaseProcedure> procedures;
    private BaseProcedure defaultProcedure;
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null);//对象池，用于存储和管理ChangeProcedureRequest对象的创建和回收
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>(); //一个队列，可能用于按顺序处理改变程序流程的请求

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        procedures = new Dictionary<Type, BaseProcedure>();
        bool findDefaultState = false;
        for (int i = 0; i < proceduresNames.Length; i++)
        {
            string procedureTypeName = proceduresNames[i];
            if (string.IsNullOrEmpty(procedureTypeName))
                continue;

            Type procedureType = Type.GetType(procedureTypeName, true);//从程序集加载指定名称的类型
            if (procedureType == null)
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");
                continue;
            }
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;//使用 Activator.CreateInstance 方法创建指定类型的实例，并将其转换为 BaseProcedure 类型
            bool isDefaultState = procedureTypeName == defaultProcedureName;//比较当前程序流程的名称和 defaultProcedureName 来确定是否是默认的程序流程
            procedures.Add(procedureType, procedure);

            if (isDefaultState)
            {
                defaultProcedure = procedure;
                findDefaultState = true;
            }
        }
        if (!findDefaultState)
        {
            Debug.LogError($"You have to set a correct default procedure to start game");
        }

    }
    protected internal override void OnModuleStart()
    {
        base.OnModuleStart();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        changeProcedureRequestPool.Clear();
        changeProcedureQ.Clear();
        IsRunning = false;
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
    }

    public async Task StartProcedure() //启动程序流程。
    {
        if (IsRunning)//如果IsRunning标志为true，表示当前已经有程序流程在运行，则方法直接返回并不执行后续代码
            return;

        IsRunning = true;
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();//从对象池获得ChangeProcedureRequest
        changeProcedureRequest.TargetProcedure = defaultProcedure;
        changeProcedureQ.Enqueue(changeProcedureRequest);//加入队列
        await ChangeProcedureInternal();
    }
    public async Task ChangeProcedure<T>() where T : BaseProcedure//改变当前运行的程序流程到指定类型的
    {
        await ChangeProcedure<T>(null);
    }

    public async Task ChangeProcedure<T>(object value) where T : BaseProcedure 
    {
        if (!IsRunning)
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure))//使用 procedures 字典尝试获取类型为 T 的 BaseProcedure 实例
        {
            UnityLog.Error($"Change Procedure Failed, Can't find Proecedure:${typeof(T).FullName}");
            return;
        }

        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();
        changeProcedureRequest.TargetProcedure = procedure;
        changeProcedureRequest.Value = value;
        changeProcedureQ.Enqueue(changeProcedureRequest);

        if (!IsChangingProcedure)
        {
            await ChangeProcedureInternal();
        }
    }
    private async Task ChangeProcedureInternal()
    {
        if (IsChangingProcedure)// 标志为true，表示当前已经有程序流程切换正在进行中
            return;

        IsChangingProcedure = true;
        while (changeProcedureQ.Count > 0)//循环遍历 changeProcedureQ 队列，直到队列为空。在每次循环中，从队列中取出一个 ChangeProcedureRequest 请求
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue();
            if (request == null || request.TargetProcedure == null)
                continue;

            if (CurrentProcedure != null)
            {
                await CurrentProcedure.OnLeaveProcedure();//于执行一些清理工作，比如释放资源、保存状态等
            }
            CurrentProcedure = request.TargetProcedure;//将 CurrentProcedure 更新为请求中的目标程序流程 TargetProcedure
            await CurrentProcedure.OnEnterProcedure(request.Value);
        }
        IsChangingProcedure = false;//表示程序流程切换操作已经完成
    }
}

public class ChangeProcedureRequest //封装改变程序流程所需的信息
{
        public BaseProcedure TargetProcedure { get; set; }//目标程序流程 改变当前的程序流程到另一个流程时
        public object Value { get; set; } //通用的数据容器 可以用来传递数据给给新的数据程序
 }


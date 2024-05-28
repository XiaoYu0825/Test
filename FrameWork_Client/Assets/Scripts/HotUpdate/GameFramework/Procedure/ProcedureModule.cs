using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class ProcedureModule : BaseGameModule
{
    [SerializeField]   // ʹ��[SerializeField]���ԣ�ʹ��˽���ֶ���Unity��Inspector�пɼ�
    private  string[] proceduresNames = null;//�������������
    [SerializeField]
    private string defaultProcedureName = null; //Ĭ�ϳ�������

    public BaseProcedure CurrentProcedure { get; private set; }//�������еĳ�������
    public bool IsRunning { get; private set; }//�Ƿ��г���������������
    public bool IsChangingProcedure { get; private set; }//�Ƿ����ڸı��������

    private Dictionary<Type, BaseProcedure> procedures;
    private BaseProcedure defaultProcedure;
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null);//����أ����ڴ洢�͹���ChangeProcedureRequest����Ĵ����ͻ���
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>(); //һ�����У��������ڰ�˳����ı�������̵�����

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

            Type procedureType = Type.GetType(procedureTypeName, true);//�ӳ��򼯼���ָ�����Ƶ�����
            if (procedureType == null)
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");
                continue;
            }
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;//ʹ�� Activator.CreateInstance ��������ָ�����͵�ʵ����������ת��Ϊ BaseProcedure ����
            bool isDefaultState = procedureTypeName == defaultProcedureName;//�Ƚϵ�ǰ�������̵����ƺ� defaultProcedureName ��ȷ���Ƿ���Ĭ�ϵĳ�������
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

    public async Task StartProcedure() //�����������̡�
    {
        if (IsRunning)//���IsRunning��־Ϊtrue����ʾ��ǰ�Ѿ��г������������У��򷽷�ֱ�ӷ��ز���ִ�к�������
            return;

        IsRunning = true;
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();//�Ӷ���ػ��ChangeProcedureRequest
        changeProcedureRequest.TargetProcedure = defaultProcedure;
        changeProcedureQ.Enqueue(changeProcedureRequest);//�������
        await ChangeProcedureInternal();
    }
    public async Task ChangeProcedure<T>() where T : BaseProcedure//�ı䵱ǰ���еĳ������̵�ָ�����͵�
    {
        await ChangeProcedure<T>(null);
    }

    public async Task ChangeProcedure<T>(object value) where T : BaseProcedure 
    {
        if (!IsRunning)
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure))//ʹ�� procedures �ֵ䳢�Ի�ȡ����Ϊ T �� BaseProcedure ʵ��
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
        if (IsChangingProcedure)// ��־Ϊtrue����ʾ��ǰ�Ѿ��г��������л����ڽ�����
            return;

        IsChangingProcedure = true;
        while (changeProcedureQ.Count > 0)//ѭ������ changeProcedureQ ���У�ֱ������Ϊ�ա���ÿ��ѭ���У��Ӷ�����ȡ��һ�� ChangeProcedureRequest ����
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue();
            if (request == null || request.TargetProcedure == null)
                continue;

            if (CurrentProcedure != null)
            {
                await CurrentProcedure.OnLeaveProcedure();//��ִ��һЩ�������������ͷ���Դ������״̬��
            }
            CurrentProcedure = request.TargetProcedure;//�� CurrentProcedure ����Ϊ�����е�Ŀ��������� TargetProcedure
            await CurrentProcedure.OnEnterProcedure(request.Value);
        }
        IsChangingProcedure = false;//��ʾ���������л������Ѿ����
    }
}

public class ChangeProcedureRequest //��װ�ı���������������Ϣ
{
        public BaseProcedure TargetProcedure { get; set; }//Ŀ��������� �ı䵱ǰ�ĳ������̵���һ������ʱ
        public object Value { get; set; } //ͨ�õ��������� ���������������ݸ����µ����ݳ���
 }


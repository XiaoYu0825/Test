using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class MessageModule : BaseGameModule
{
    //ί������ Task ��.NET Framework�����ڱ�ʾ�첽���������� 

    public delegate Task MessageHandlerEventArgs<T>(T arg);

    private Dictionary<Type, List<object>> globalMessageHandlers;//���ڴ洢ȫ�ֵ���Ϣ���¼��������
    private Dictionary<Type, List<object>> localMessageHandlers;//���ھֲ��洢��Ϣ���¼��Ĵ������


    public Monitor Monitor { get; private set; }


    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        localMessageHandlers = new Dictionary<Type, List<object>>();
        Monitor = new Monitor();
        LoadAllMessageHandlers();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        globalMessageHandlers = null;
        localMessageHandlers = null;
    }
    private void LoadAllMessageHandlers() //���ȫ�������Ϣ����������
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes()) //��õ�ǰ������������
        {
            if (type.IsAbstract)//�ж��Ƿ��ǳ�������
                continue;

            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);//���Ҹ����Ͷ�������
            if (messageHandlerAttribute != null)
            {
                IMessageHander messageHandler = Activator.CreateInstance(type) as IMessageHander;//����������ʵ��ת���� IMessageHander
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))//����ֵ��Ƿ����������
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());//�ֵ����
                }
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);//�ֵ��������
            }
        }
    }


    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)//������Ϣ�����¼�
    {
        Type argType = typeof(T);// ��ȡ���Ͳ���T�����Ͷ���
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))
        {
            handlerList = new List<object>();// ���û���ҵ���Ӧ�Ĵ������б��򴴽�һ���µ��б�
            localMessageHandlers.Add(argType, handlerList);//�´������б���ӵ�localMessageHandler�ֵ��м�����Ϣ����
        }
        handlerList.Add(handler);//���������Ϣ������handler��ӵ���Ӧ�Ĵ������б���
    }


    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)//ע����Ϣ�¼�
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList)) //����T�����ֵ���ͬ���͵� �������б�
            return;

        handlerList.Remove(handler); //����T���ҵ��ֵ��ֵ�����Ƴ�
    } 


    public async Task Post<T>(T arg) where T : struct
    { 
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))//����ֵ��Ƿ����������
        {
            foreach (var handler in globalHandlerList) 
            {
                if (!(handler is MessageHandler<T> messageHandler))//�Ƿ����ת��Ϊ MessageHandler<T> ����
                    continue; 

                await messageHandler.HandleMessage(arg);//������ֵ
            }
        }

        if (localMessageHandlers.TryGetValue(typeof(T), out List<object> localHandlerList))
        {
            List<object> list = ListPool<object>.Obtain();
            list.AddRangeNonAlloc(localHandlerList);
            foreach (var handler in list)
            {
                if (!(handler is MessageHandlerEventArgs<T> messageHandler))
                    continue;

                await messageHandler(arg);
            }
            ListPool<object>.Release(list);
        }
    }
}

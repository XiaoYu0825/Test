using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class MessageModule : BaseGameModule
{
    //委托类型 Task 是.NET Framework中用于表示异步操作的类型 

    public delegate Task MessageHandlerEventArgs<T>(T arg);

    private Dictionary<Type, List<object>> globalMessageHandlers;//用于存储全局的消息或事件处理程序
    private Dictionary<Type, List<object>> localMessageHandlers;//用于局部存储消息或事件的处理程序


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
    private void LoadAllMessageHandlers() //添加全部标记消息处理器的类
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes()) //获得当前程序集所有类型
        {
            if (type.IsAbstract)//判断是否是抽象类型
                continue;

            MessageHandlerAttribute messageHandlerAttribute = type.GetCustomAttribute<MessageHandlerAttribute>(true);//查找该类型定义属性
            if (messageHandlerAttribute != null)
            {
                IMessageHander messageHandler = Activator.CreateInstance(type) as IMessageHander;//创建该类型实例转换成 IMessageHander
                if (!globalMessageHandlers.ContainsKey(messageHandler.GetHandlerType()))//检测字典是否包含该类型
                {
                    globalMessageHandlers.Add(messageHandler.GetHandlerType(), new List<object>());//字典添加
                }
                globalMessageHandlers[messageHandler.GetHandlerType()].Add(messageHandler);//字典添加类型
            }
        }
    }


    public void Subscribe<T>(MessageHandlerEventArgs<T> handler)//订阅消息处理事件
    {
        Type argType = typeof(T);// 获取泛型参数T的类型对象
        if (!localMessageHandlers.TryGetValue(argType, out var handlerList))
        {
            handlerList = new List<object>();// 如果没有找到对应的处理器列表，则创建一个新的列表
            localMessageHandlers.Add(argType, handlerList);//新创建的列表添加到localMessageHandler字典中键是消息类型
        }
        handlerList.Add(handler);//将传入的消息处理器handler添加到对应的处理器列表中
    }


    public void Unsubscribe<T>(MessageHandlerEventArgs<T> handler)//注册消息事件
    {
        if (!localMessageHandlers.TryGetValue(typeof(T), out var handlerList)) //根据T查找字典相同类型的 处理器列表
            return;

        handlerList.Remove(handler); //根据T查找到字典的值进行移除
    } 


    public async Task Post<T>(T arg) where T : struct
    { 
        if (globalMessageHandlers.TryGetValue(typeof(T), out List<object> globalHandlerList))//检测字典是否包含该类型
        {
            foreach (var handler in globalHandlerList) 
            {
                if (!(handler is MessageHandler<T> messageHandler))//是否可以转换为 MessageHandler<T> 类型
                    continue; 

                await messageHandler.HandleMessage(arg);//变量赋值
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

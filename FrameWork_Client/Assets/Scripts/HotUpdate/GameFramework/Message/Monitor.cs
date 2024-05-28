using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
/// <summary>
/// 等待 异步操作的状态和结果
/// </summary>
public class Monitor
{
    private readonly Dictionary<Type, object> waitObjects = new Dictionary<Type, object>();
    
    public WaitObject<T> Wait<T>() where T : struct //添加一个新的实例到字典
    {
        WaitObject<T> o = new WaitObject<T>();
        waitObjects.Add(typeof(T), o);// 新实例添加到字典中，T的类型作为键
        return o;//返回新的实例 o
    }
    public void SetResult<T>(T result) where T : struct
    {
        Type type = typeof(T);// 获取泛型参数T的Type对象
        if (!waitObjects.TryGetValue(type, out object o))//字典中获取与T类型
            return;

        waitObjects.Remove(type);/// 删除字典中T类型
        ((WaitObject<T>)o).SetResult(result);//将结果设置到找到的WaitObject<T>实例上并调用其SetResult方法
    }
    public class WaitObject<T> : INotifyCompletion where T : struct  // (struct) 只能用值类型不能用引用类型
    {
        public bool IsCompleted { get; private set; } //表示异步操作是否完成
        public T Result { get; private set; }//存储异步操作的结果

        private Action callback; //回调方法

        public void SetResult(T result)//设置异步操作的结果
        {
            Result = result; //设置结果
            IsCompleted = true;//标记操作已完成

            Action c = callback;//调用回调方法
            callback = null;
            c?.Invoke();
        }

        public WaitObject<T> GetAwaiter()//返回WaitObject<T>实现接口方法
        {
            return this;
        }

        public void OnCompleted(Action callback)//异步完成时调用的方法
        {
            this.callback = callback;
        }

        public T GetResult()//获得操作的结果
        {
            return Result;
        }
    }
}

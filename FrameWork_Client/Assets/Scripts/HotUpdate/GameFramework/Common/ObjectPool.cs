using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 泛型对象池 
/// </summary>
/// 用于缓存和管理可重用的对象实例 以避免频繁地创建和销毁对象从而提高性能
/// <typeparam name="T"><[表情]peparam>
public class ObjectPool<T> : IDisposable where T : new()
{
    public int MaxXacheCount = 32;  //最多缓存对象的个数  32

    private static LinkedList<T> cache;//静态的双向链表  共享
    private Action<T> onRelease;//委托 用于在释放对象时执行一些清理或重置操作

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="onRelease"></param>
    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>(); //新的实例
        this.onRelease = onRelease;   //接收 存储在成员变量中
    }

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <returns></returns>
    public T Obtain()
    {
        T value;
        if (cache.Count == 0)//如果对象池为空
        {
            value = new T();//创建一个新的实例
        }
        else
        {
            value = cache.First.Value;//获取第一个对象
            cache.RemoveFirst();//移除第一个元素
        }

        return value;
    }


    /// <summary>
    /// 释放对象回对象池
    /// </summary>
    /// <param name="value"></param>
    public void Release(T value)
    {
        //cache 中的对象数量已经大道最大值  则不添加新对象
        if (cache.Count >= MaxXacheCount)
            return;

        onRelease?.Invoke(value);//调用委托执行清理或重置操作
        cache.AddLast(value);//对象添加到末尾
    }

    /// <summary>
    /// 清空对象池中所有对象
    /// </summary>
    public void Clear()
    {
        cache.Clear();
    }

    /// <summary>
    /// 释放对象池占用的资源
    /// </summary>
    public void Dispose()
    {
        //设为null  帮助垃圾回收器回收对象
        cache = null;
        onRelease = null;
    }

}


public class QueuePool<T>
{
    //创建新的实例时 传入了一个委托，用于在对象被释放回对象池之前执行清理操作
    //这里，清理操作是调用 Queue<T> 对象的 Clear 方法，移除队列中的所有元素，从而可以重复使用该对象。
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());

    //调用pool 对象的 Obtain 方法 返回获取到的 Queue<T> 对象
    public static Queue<T> Obtain() => pool.Obtain();

    //调用 pool 对象的 Release 方法，将该对象释放回对象池
    public static void Release(Queue<T> value) => pool.Release(value);

    //调用了 pool 对象的 Clear 方法，从而清空对象池
    public static void Clear() => pool.Clear();
}

public class ListPool<T>
{
    private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>((value) => value.Clear());
    public static List<T> Obtain() => pool.Obtain();
    public static void Release(List<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
public class HashSetPool<T>
{
    private static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>((value) => value.Clear());
    public static HashSet<T> Obtain() => pool.Obtain();
    public static void Release(HashSet<T> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
public class DictionaryPool<K, V>
{
    private static ObjectPool<Dictionary<K, V>> pool = new ObjectPool<Dictionary<K, V>>((value) => value.Clear());
    public static Dictionary<K, V> Obtain() => pool.Obtain();
    public static void Release(Dictionary<K, V> value) => pool.Release(value);
    public static void Clear() => pool.Clear();
}
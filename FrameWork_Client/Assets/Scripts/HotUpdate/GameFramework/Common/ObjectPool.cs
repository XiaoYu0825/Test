using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���Ͷ���� 
/// </summary>
/// ���ڻ���͹�������õĶ���ʵ�� �Ա���Ƶ���ش��������ٶ���Ӷ��������
/// <typeparam name="T"><[����]peparam>
public class ObjectPool<T> : IDisposable where T : new()
{
    public int MaxXacheCount = 32;  //��໺�����ĸ���  32

    private static LinkedList<T> cache;//��̬��˫������  ����
    private Action<T> onRelease;//ί�� �������ͷŶ���ʱִ��һЩ��������ò���

    /// <summary>
    /// ���캯��
    /// </summary>
    /// <param name="onRelease"></param>
    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>(); //�µ�ʵ��
        this.onRelease = onRelease;   //���� �洢�ڳ�Ա������
    }

    /// <summary>
    /// ��ȡ����
    /// </summary>
    /// <returns></returns>
    public T Obtain()
    {
        T value;
        if (cache.Count == 0)//��������Ϊ��
        {
            value = new T();//����һ���µ�ʵ��
        }
        else
        {
            value = cache.First.Value;//��ȡ��һ������
            cache.RemoveFirst();//�Ƴ���һ��Ԫ��
        }

        return value;
    }


    /// <summary>
    /// �ͷŶ���ض����
    /// </summary>
    /// <param name="value"></param>
    public void Release(T value)
    {
        //cache �еĶ��������Ѿ�������ֵ  ������¶���
        if (cache.Count >= MaxXacheCount)
            return;

        onRelease?.Invoke(value);//����ί��ִ����������ò���
        cache.AddLast(value);//������ӵ�ĩβ
    }

    /// <summary>
    /// ��ն���������ж���
    /// </summary>
    public void Clear()
    {
        cache.Clear();
    }

    /// <summary>
    /// �ͷŶ����ռ�õ���Դ
    /// </summary>
    public void Dispose()
    {
        //��Ϊnull  �����������������ն���
        cache = null;
        onRelease = null;
    }

}


public class QueuePool<T>
{
    //�����µ�ʵ��ʱ ������һ��ί�У������ڶ����ͷŻض����֮ǰִ���������
    //�����������ǵ��� Queue<T> ����� Clear �������Ƴ������е�����Ԫ�أ��Ӷ������ظ�ʹ�øö���
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());

    //����pool ����� Obtain ���� ���ػ�ȡ���� Queue<T> ����
    public static Queue<T> Obtain() => pool.Obtain();

    //���� pool ����� Release ���������ö����ͷŻض����
    public static void Release(Queue<T> value) => pool.Release(value);

    //������ pool ����� Clear �������Ӷ���ն����
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
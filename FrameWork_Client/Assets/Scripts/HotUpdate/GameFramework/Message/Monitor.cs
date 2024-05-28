using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
/// <summary>
/// �ȴ� �첽������״̬�ͽ��
/// </summary>
public class Monitor
{
    private readonly Dictionary<Type, object> waitObjects = new Dictionary<Type, object>();
    
    public WaitObject<T> Wait<T>() where T : struct //���һ���µ�ʵ�����ֵ�
    {
        WaitObject<T> o = new WaitObject<T>();
        waitObjects.Add(typeof(T), o);// ��ʵ����ӵ��ֵ��У�T��������Ϊ��
        return o;//�����µ�ʵ�� o
    }
    public void SetResult<T>(T result) where T : struct
    {
        Type type = typeof(T);// ��ȡ���Ͳ���T��Type����
        if (!waitObjects.TryGetValue(type, out object o))//�ֵ��л�ȡ��T����
            return;

        waitObjects.Remove(type);/// ɾ���ֵ���T����
        ((WaitObject<T>)o).SetResult(result);//��������õ��ҵ���WaitObject<T>ʵ���ϲ�������SetResult����
    }
    public class WaitObject<T> : INotifyCompletion where T : struct  // (struct) ֻ����ֵ���Ͳ�������������
    {
        public bool IsCompleted { get; private set; } //��ʾ�첽�����Ƿ����
        public T Result { get; private set; }//�洢�첽�����Ľ��

        private Action callback; //�ص�����

        public void SetResult(T result)//�����첽�����Ľ��
        {
            Result = result; //���ý��
            IsCompleted = true;//��ǲ��������

            Action c = callback;//���ûص�����
            callback = null;
            c?.Invoke();
        }

        public WaitObject<T> GetAwaiter()//����WaitObject<T>ʵ�ֽӿڷ���
        {
            return this;
        }

        public void OnCompleted(Action callback)//�첽���ʱ���õķ���
        {
            this.callback = callback;
        }

        public T GetResult()//��ò����Ľ��
        {
            return Result;
        }
    }
}

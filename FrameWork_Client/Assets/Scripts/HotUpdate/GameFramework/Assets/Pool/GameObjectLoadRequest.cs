using System;
using UnityEngine;

namespace TGame.Asset
{
    public class GameObjectLoadRequest<T> where T : GameObjectPoolAsset//��Ϸ����ļ�������
    {
        public GameObjectLoadState State { get; private set; }//��Ϸ�������״̬
        public string Path { get; }//��Ϸ������Դ��·��
        public Action<GameObject> CreateNewCallback { get; }//����Ҫ�����µ���Ϸ����ʵ��ʱ���õĻص�����������һ�� GameObject ���͵Ĳ���

        private Action<T> callback;//�����������ʱ���õĻص�����

        public GameObjectLoadRequest(string path, Action<T> callback, Action<GameObject> createNewCallback)//�������ڳ�ʼ��һ�������������
        {
            Path = path;
            this.callback = callback;
            CreateNewCallback = createNewCallback;
        }

        public void LoadFinish(T obj)//���������������״̬
        {
            if (State == GameObjectLoadState.Loading)//��ǰ���������״̬�� Loading���ڼ���
            {
                callback?.Invoke(obj);
                State = GameObjectLoadState.Finish;
            }
        }
    }
}

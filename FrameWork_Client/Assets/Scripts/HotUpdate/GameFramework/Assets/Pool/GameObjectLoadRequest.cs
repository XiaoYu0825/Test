using System;
using UnityEngine;

namespace TGame.Asset
{
    public class GameObjectLoadRequest<T> where T : GameObjectPoolAsset//游戏对象的加载请求
    {
        public GameObjectLoadState State { get; private set; }//游戏对象加载状态
        public string Path { get; }//游戏对象资源的路径
        public Action<GameObject> CreateNewCallback { get; }//当需要创建新的游戏对象实例时调用的回调函数，接收一个 GameObject 类型的参数

        private Action<T> callback;//加载请求完成时调用的回调函数

        public GameObjectLoadRequest(string path, Action<T> callback, Action<GameObject> createNewCallback)//函数用于初始化一个加载请求对象
        {
            Path = path;
            this.callback = callback;
            CreateNewCallback = createNewCallback;
        }

        public void LoadFinish(T obj)//对象加载请求的完成状态
        {
            if (State == GameObjectLoadState.Loading)//当前加载请求的状态是 Loading正在加载
            {
                callback?.Invoke(obj);
                State = GameObjectLoadState.Finish;
            }
        }
    }
}

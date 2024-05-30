using System;
using UnityEngine;


public partial class AssetModule : BaseGameModule
{
    private readonly GameObjectPool<GameObjectAsset> gameObjectPool = new GameObjectPool<GameObjectAsset>();//  GameObjectAsset 类型的游戏对象

    public GameObject LoadGameObject(string path, Action<GameObject> createNewCallback = null)//加载游戏对象方法
    {
        //UnityLog.Info($"Load GameObject:{path}");
        return gameObjectPool.LoadGameObject(path, createNewCallback).gameObject;
    }
    public T LoadGameObject<T>(string path, Action<GameObject> createNewCallback = null) where T : Component//加载一个游戏对象，并返回该游戏对象上附加的特定类型的组件
    {
        //UnityLog.Info($"Load GameObject:{path}");
        GameObject go = gameObjectPool.LoadGameObject(path, createNewCallback).gameObject;//加载的游戏对象go中获取指定类型 T 的组件
        return go.GetComponent<T>();
    }

    public void LoadGameObjectAsync(string path, Action<GameObjectAsset> callback, Action<GameObject> createNewCallback = null)//异步加载游戏对象的方法
    {
        gameObjectPool.LoadGameObjectAsync(path, callback, createNewCallback);
    }

    public void UnloadCache()
    {
        gameObjectPool.UnloadAllGameObjects();
    }

    public void UnloadGameObject(GameObject go)
    {
        gameObjectPool.UnloadGameObject(go);
    }

    private void UpdateGameObjectRequests()
    {
        gameObjectPool.UpdateLoadRequests();
    }
}


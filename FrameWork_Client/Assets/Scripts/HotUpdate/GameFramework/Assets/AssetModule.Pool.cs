using System;
using UnityEngine;


public partial class AssetModule : BaseGameModule
{
    private readonly GameObjectPool<GameObjectAsset> gameObjectPool = new GameObjectPool<GameObjectAsset>();//  GameObjectAsset ���͵���Ϸ����

    public GameObject LoadGameObject(string path, Action<GameObject> createNewCallback = null)//������Ϸ���󷽷�
    {
        //UnityLog.Info($"Load GameObject:{path}");
        return gameObjectPool.LoadGameObject(path, createNewCallback).gameObject;
    }
    public T LoadGameObject<T>(string path, Action<GameObject> createNewCallback = null) where T : Component//����һ����Ϸ���󣬲����ظ���Ϸ�����ϸ��ӵ��ض����͵����
    {
        //UnityLog.Info($"Load GameObject:{path}");
        GameObject go = gameObjectPool.LoadGameObject(path, createNewCallback).gameObject;//���ص���Ϸ����go�л�ȡָ������ T �����
        return go.GetComponent<T>();
    }

    public void LoadGameObjectAsync(string path, Action<GameObjectAsset> callback, Action<GameObject> createNewCallback = null)//�첽������Ϸ����ķ���
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


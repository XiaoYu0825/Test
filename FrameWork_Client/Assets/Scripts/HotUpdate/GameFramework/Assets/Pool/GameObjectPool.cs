using System;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.AddressableAssets;

 
    public class GameObjectPool<T> where T : GameObjectPoolAsset
    {
        private readonly Dictionary<int, Queue<T>> gameObjectPool = new Dictionary<int, Queue<T>>();//来存储游戏对象池对象
       private readonly List<GameObjectLoadRequest<T>> requests = new List<GameObjectLoadRequest<T>>();//列表很追踪所有待处理的游戏对象加载请求
        private readonly Dictionary<int, GameObject> usingObjects = new Dictionary<int, GameObject>();//正在使用的对象

        public T LoadGameObject(string path, Action<GameObject> createNewCallback = null)//加载并管理游戏对象的实例
    {
            int hash = path.GetHashCode();//返回哈希码
            if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))//字典中获取对应的队列q。如果字典中不存在该哈希码对应的队列
        {
                q = new Queue<T>();
                gameObjectPool.Add(hash, q);
            }
            if (q.Count == 0)
            {
                GameObject prefab = Addressables.LoadAssetAsync<GameObject>(path).WaitForCompletion();//异步加载游戏对象资源，并等待加载完成
                GameObject go = UnityEngine.Object.Instantiate(prefab);//实例化游戏对象
                T asset = go.AddComponent<T>();
                createNewCallback?.Invoke(go);//createNewCallback 回调函数传入新创建的游戏对象作为参数
                asset.ID = hash;
                go.SetActive(false);
                q.Enqueue(asset);
                Debug.Log("--------------------------");
            }

            {
                T asset = q.Dequeue();
                OnGameObjectLoaded(asset);
                return asset;
            }
        }

    /// <summary>
    /// 异步加载游戏对象
    /// </summary>
    /// <param name="path">需要加载的资源的路径</param>
    /// <param name="callback">每次调用LoadGameObjectAsync，无论是否从缓存里取出的，都会通过这个回调进行通知</param>
    /// <param name="createNewCallback">游戏对象第一次被克隆后调用，对象池取出的复用游戏对象，不会回调</param>
    /// //加载完成后的回调函数 callback
    /// //创建新游戏对象时的回调函数 createNewCallback
    public void LoadGameObjectAsync(string path, Action<T> callback, Action<GameObject> createNewCallback = null)
    {
            GameObjectLoadRequest<T> request = new GameObjectLoadRequest<T>(path, callback, createNewCallback);
            requests.Add(request);
    }

        public void UnloadAllGameObjects()//卸载所有游戏对象
    {
            // 先将所有Request加载完毕
            while (requests.Count > 0)
            {
                //GameManager.Asset.UpdateLoader();
                UpdateLoadRequests();//确保所有加载请求都已经完成
            }

            // 将所有using Objects 卸载  避免在遍历过程中直接修改 usingObjects 字典可能导致的问题
            if (usingObjects.Count > 0)
            {
                List<int> list = new List<int>();
                foreach (var id in usingObjects.Keys)
                {
                    list.Add(id);
                }
                foreach (var id in list)
                {
                    GameObject obj = usingObjects[id];
                    UnloadGameObject(obj);
                }
            }

            // 将所有缓存清掉
            if (gameObjectPool.Count > 0)
            {
                foreach (var q in gameObjectPool.Values)
                {
                    foreach (var asset in q)
                    {
                        UnityEngine.Object.Destroy(asset.gameObject);//销毁其中的每个对象的 gameObject
                }
                    q.Clear();//清空队列
            }
                gameObjectPool.Clear();//清空字典
        }
        }

        public void UnloadGameObject(GameObject go)//卸载指定游戏对象
        {
            if (go == null)
                return;

            T asset = go.GetComponent<T>();
            if (asset == null)
            {
                UnityLog.Warn($"Unload GameObject失败，找不到GameObjectAsset:{go.name}");
                UnityEngine.Object.Destroy(go);
                return;
            }

            if (!gameObjectPool.TryGetValue(asset.ID, out Queue<T> q))
            {
                q = new Queue<T>();
                gameObjectPool.Add(asset.ID, q);
            }
            q.Enqueue(asset);
            usingObjects.Remove(go.GetInstanceID());
            go.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().releaseObjectRoot);
            go.gameObject.SetActive(false);
        }

        public void UpdateLoadRequests()//更新和处理游戏对象加载请求的
       {
            if (requests.Count > 0)
            {
                foreach (var request in requests)
                {
                    int hash = request.Path.GetHashCode();
                    if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))
                    {
                        q = new Queue<T>();
                        gameObjectPool.Add(hash, q);
                    }

                    if (q.Count == 0)
                    {
                        Addressables.LoadAssetAsync<GameObject>(request.Path).Completed += (obj) =>
                        {
                            GameObject go = UnityEngine.Object.Instantiate(obj.Result);
                            T asset = go.AddComponent<T>();
                            request.CreateNewCallback?.Invoke(go);//回调函数 Completed 来处理加载到的对象
                            asset.ID = hash;
                            go.SetActive(false);

                            OnGameObjectLoaded(asset);
                            request.LoadFinish(asset);//通知请求完成
                        };
                    }
                    else
                    {
                        T asset = q.Dequeue();
                        OnGameObjectLoaded(asset);
                        request.LoadFinish(asset);//通知请求完成
                }
                }

                requests.Clear();
            }
        }

        private void OnGameObjectLoaded(T asset)//处理游戏对象加载完成后的逻辑
    {
            Debug.Log("xxxxxxxxxxxxxxxxxxxxxx");
            asset.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().usingObjectRoot);
            int id = asset.gameObject.GetInstanceID();
            usingObjects.Add(id, asset.gameObject);
        }
    }
 

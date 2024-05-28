using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
namespace Config
{
    public partial struct ShopConfig
    {
        public static void DeserializeByAddressable(string directory)
        {
            string path = $"{directory}/ShopConfig.json";
            UnityEngine.TextAsset ta = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(path).WaitForCompletion();
            string json = ta.text;
            datas = new List<ShopConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                ShopConfig data = (ShopConfig)dataObject.ToObject(typeof(ShopConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static void DeserializeByFile(string directory)
        {
            string path = $"{directory}/ShopConfig.json";
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fs))
                {
                    datas = new List<ShopConfig>();
                    indexMap = new Dictionary<int, int>();
                    string json = reader.ReadToEnd();
                    JArray array = JArray.Parse(json);
                    Count = array.Count;
                    for (int i = 0; i < array.Count; i++)
                    {
                        JObject dataObject = array[i] as JObject;
                        ShopConfig data = (ShopConfig)dataObject.ToObject(typeof(ShopConfig));
                        datas.Add(data);
                        indexMap.Add(data.ID, i);
                    }
                }
            }
        }
        public static System.Collections.IEnumerator DeserializeByBundle(string directory, string subFolder)
        {
            string bundleName = $"{subFolder}/ShopConfig.bytes".ToLower();
            string fullBundleName = $"{directory}/{bundleName}";
            string assetName = $"assets/{bundleName}";
            #if UNITY_WEBGL && !UNITY_EDITOR
            UnityEngine.AssetBundle bundle = null;
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(fullBundleName);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                UnityEngine.Debug.LogError(request.error);
            }
            else
            {
                bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
            }
            #else
            yield return null;
            UnityEngine.AssetBundle bundle = UnityEngine.AssetBundle.LoadFromFile($"{fullBundleName}", 0, 0);
            #endif
            UnityEngine.TextAsset ta = bundle.LoadAsset<UnityEngine.TextAsset>($"{assetName}");
            string json = ta.text;
            datas = new List<ShopConfig>();
            indexMap = new Dictionary<int, int>();
            JArray array = JArray.Parse(json);
            Count = array.Count;
            for (int i = 0; i < array.Count; i++)
            {
                JObject dataObject = array[i] as JObject;
                ShopConfig data = (ShopConfig)dataObject.ToObject(typeof(ShopConfig));
                datas.Add(data);
                indexMap.Add(data.ID, i);
            }
        }
        public static int Count;
        private static List<ShopConfig> datas;
        private static Dictionary<int, int> indexMap;
        public static ShopConfig ByID(int id)
        {
            if (id <= 0)
            {
                return Null;
            }
            if (!indexMap.TryGetValue(id, out int index))
            {
                throw new System.Exception($"ShopConfig找不到ID:{id}");
            }
            return ByIndex(index);
        }
        public static ShopConfig ByIndex(int index)
        {
            return datas[index];
        }
        public bool IsNull { get; private set; }
        public static ShopConfig Null { get; } = new ShopConfig() { IsNull = true }; 
        public System.Int32 ID { get; set; }
        public string Description { get; set; }
        public System.Int32 gold { get; set; }
        public System.Int32 bind_gold { get; set; }
        public System.Int32 is_notice { get; set; }
        public System.Int32 vip_gold { get; set; }
        public System.Int32 vip_limit { get; set; }
        public System.Int32 buy_limit { get; set; }
    }
}

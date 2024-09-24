using System.Collections;
using System.Collections.Generic;
using System.IO;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace THLL.SceneSystem
{
    public class Scene : BaseGameEntity<SceneData>
    {
        #region 数据
        //父级场景ID
        public string ParentSceneID => Data.ParentSceneID;
        //父级场景
        public Scene ParentScene { get; private set; }
        //子场景哈希表
        public HashSet<Scene> ChildScenesHashSet { get; private set; } = new();
        //场景路径字典
        public Dictionary<Scene, int> PathsDict { get; private set; } = new();
        #endregion

        #region 资源
        //场景背景图片字典
        public Dictionary<string, Sprite> BackgroundsDict { get; private set; } = new();
        #endregion

        #region 构造与初始化相关
        //新增一个有参构造函数
        public Scene(string filePath, SceneDatabase sceneDb) : base(filePath)
        {
            //父类的构造函数自动执行，这时我们执行重载的配置函数
            Configure(sceneDb);
        }
        //配置函数重载
        protected void Configure(SceneDatabase sceneDb)
        {
            //针对数据中存储的连接关系进行配置
            foreach (ScenePathData scenePathData in Data.ChildScenePathDatas)
            {
                //构建场景A到场景B的路径
                if (!sceneDb.PathStorageIDVersion.ContainsKey(scenePathData.SceneAID))
                {
                    //若无键值，则创建新字典
                    sceneDb.PathStorageIDVersion[scenePathData.SceneAID] = new();
                }
                //添加
                sceneDb.PathStorageIDVersion[scenePathData.SceneAID][scenePathData.SceneBID] = scenePathData.Distance;

                //构建场景B到场景A的路径
                if (!sceneDb.PathStorageIDVersion.ContainsKey(scenePathData.SceneBID))
                {
                    //若无键值，则创建新字典
                    sceneDb.PathStorageIDVersion[scenePathData.SceneBID] = new();
                }
                //添加
                sceneDb.PathStorageIDVersion[scenePathData.SceneBID][scenePathData.SceneAID] = scenePathData.Distance;
            }
        }
        //初始化函数，需要传入一个数据库
        public void Init(SceneDatabase sceneDb)
        {
            //首先，获取到父级场景
            ParentScene = sceneDb[ParentSceneID];
            //然后，给父级的子场景哈希表添加本场景
            ParentScene.ChildScenesHashSet.Add(this);
            //最后，利用数据库中ID形式的路径存储完成路径字典构建
            foreach (KeyValuePair<string, int> kvp in sceneDb.PathStorageIDVersion[ID])
            {
                //获取另一场景
                Scene otherScene = sceneDb[kvp.Key];
                //添加路径
                PathsDict.Add(otherScene, kvp.Value);
            }
        }
        #endregion

        #region 数据读取相关
        //获取场景背景
        public IEnumerator LoadBackgrounds()
        {
            //获取到目录
            string dir = Path.Combine(DataDirectoryPath, "Backgrounds");

            //获取目录下所有图片文件
            //首先获取所有文件
            string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            //然后进行遍历
            foreach (string filePath in files)
            {
                //判断是否是图片文件
                if (TouhouLeisurelyLife.ImageExtensions.Contains(Path.GetExtension(filePath)))
                {
                    //若是，获取文件名
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);
                    //等待
                    yield return request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("Failed to load background image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        BackgroundsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //直接将对应资源标记为空
            BackgroundsDict.Clear();
        }
        #endregion
    }
}

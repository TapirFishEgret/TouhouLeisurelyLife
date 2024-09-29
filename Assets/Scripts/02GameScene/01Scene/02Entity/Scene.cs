using System.Collections.Generic;
using THLL.BaseSystem;
using UnityEngine;

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
        public Dictionary<string, Sprite> BackgroundsDict => Data.BackgroundsDict;
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
        public void LoadBackgrounds(MonoBehaviour mono)
        {
            //利用Mono启动协程
            mono.StartCoroutine(Data.LoadBackgroundsCoroutine(DataDirectoryPath));
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //使用数据中的卸载函数
            Data.UnloadAllResources();
        }
        #endregion
    }
}

using System;
using System.Collections;
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
        //场景地图
        public Map Map
        {
            get
            {
                //从数据中获取地图
                Map map = Data.Map;
                //检测地图是否为空
                if (map.IsEmpty)
                {
                    //若为空，则向上循环获取父级的地图直到找到非空地图或毫无结果为止
                    Scene parentScene = ParentScene;
                    while (parentScene != null)
                    {
                        if (!parentScene.Data.Map.IsEmpty)
                        {
                            //若父级有地图，则返回父级的地图
                            return parentScene.Data.Map;
                        }
                        else
                        {
                            //若父级没有地图，则继续向上查找
                            parentScene = parentScene.ParentScene;
                        }
                    }
                }
                //若直到最后都无可用地图，则返回当前地图
                return map;
            }
        }
        #endregion

        #region 资源
        //场景背景图片字典
        public Dictionary<string, Sprite> BackgroundsDict
        {
            get
            {
                if (Data.BackgroundsDict.Count == 0)
                {
                    //若字典为空，则向上循环获取父级的背景图字典直到找到非空字典或毫无结果为止
                    Scene parentScene = ParentScene;
                    while (parentScene != null)
                    {
                        if (parentScene.Data.BackgroundsDict.Count > 0)
                        {
                            //若父级有背景图字典，则返回父级的字典
                            return parentScene.Data.BackgroundsDict;
                        }
                        else
                        {
                            //若父级没有背景图字典，则继续向上查找
                            parentScene = parentScene.ParentScene;
                        }
                    }
                    //若字典为空，返回默认背景字典
                    return new Dictionary<string, Sprite>() { { "0", GameAssetsManager.Instance.DefaultBackground } };
                }
                else
                {
                    //若字典不为空，返回数据中的字典
                    return Data.BackgroundsDict;
                }
            }
        }
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
            //将自己添加到数据库中
            sceneDb.Add(this);

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
            //首先，判断是否有父级场景
            if (!string.IsNullOrEmpty(ParentSceneID))
            {
                //若有父级，则获取场景
                ParentScene = sceneDb[ParentSceneID];
                //然后，给父级的子场景哈希表添加本场景
                ParentScene.ChildScenesHashSet.Add(this);
            }

            //最后，利用数据库中ID形式的路径存储完成路径字典构建
            if (sceneDb.PathStorageIDVersion.ContainsKey(ID))
            {
                //遍历路径存储字典
                foreach (KeyValuePair<string, int> kvp in sceneDb.PathStorageIDVersion[ID])
                {
                    //获取另一场景
                    Scene otherScene = sceneDb[kvp.Key];
                    //添加路径
                    PathsDict.Add(otherScene, kvp.Value);
                }
            }
        }
        #endregion

        #region 数据读取相关
        //使用协程获取场景背景
        public IEnumerator LoadBackgroundsCoroutine(Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //返回协程
            yield return Data.LoadBackgroundsCoroutine(DataDirectoryPath, onBackgroundLoaded, onAllBackgroundsLoaded);
        }
        //使用异步函数获取场景背景
        public async void LoadBackgroundsAsync(Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //利用异步函数启动加载
            await Data.LoadBackgroundsAsync(DataDirectoryPath, onBackgroundLoaded, onAllBackgroundsLoaded);
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

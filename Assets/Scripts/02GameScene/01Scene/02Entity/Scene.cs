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
        //子级场景相邻状态
        public HashSet<(string, string)> ChildScenesAdjacentStates => Data.ChildScenesAdjacentStates;
        //父级场景
        public Scene ParentScene { get; private set; }
        //子场景哈希表
        public HashSet<Scene> ChildScenesHashSet { get; private set; } = new();
        //场景相邻状态哈希表
        public HashSet<Scene> AdjacentScenesHashSet { get; private set; } = new();
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
        //有参构造函数
        public Scene(string filePath, SceneDatabase sceneDb) : base(filePath)
        {
            //向数据库添加自身
            sceneDb.Add(this);
        }
        #endregion

        #region 数据读取相关
        //使用协程获取场景背景
        public IEnumerator LoadBackgroundsCoroutine(Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //返回协程
            yield return Data.LoadBackgroundsCoroutine(DataDirectory, onBackgroundLoaded, onAllBackgroundsLoaded);
        }
        //使用异步函数获取场景背景
        public async void LoadBackgroundsAsync(Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //利用异步函数启动加载
            await Data.LoadBackgroundsAsync(DataDirectory, onBackgroundLoaded, onAllBackgroundsLoaded);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using THLL.CharacterSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.SceneSystem
{
    public class Scene : BaseGameEntity<SceneData>
    {
        #region 数据
        //父级场景ID
        public string ParentSceneID => Data.ParentSceneID;
        //父级场景
        public Scene ParentScene { get; set; }
        //场景连接
        public Dictionary<Scene, int> Connections { get; private set; } = new();
        //子场景哈希表
        public HashSet<Scene> ChildScenes { get; private set; } = new();
        //场景内路径
        public List<ScenePath> PathsInScene => Data.MapData.PathsInScene;
        //场景内角色
        public List<Character> CharactersInScene { get; private set; } = new();
        //是否有地图
        public bool HasMap => !Data.MapData.IsEmpty;
        //地图数据
        public VisualElement Map => Data.MapData.GetMap();
        //全名
        public string FullName
        {
            get
            {
                //全名列表
                List<string> fullNameList = new() { Name };
                //向上查找父级场景
                Scene parentScene = ParentScene;
                while (parentScene != null)
                {
                    //添加父级场景名称
                    fullNameList.Add(parentScene.Name);
                    //向上查找
                    parentScene = parentScene.ParentScene;
                }
                //反转列表
                fullNameList.Reverse();
                //返回全名
                return string.Join("/", fullNameList);
            }
        }
        #endregion

        #region 资源
        //获取背景图
        public Sprite GetBackground(string backgroundName = "0")
        {
            //背景图变量
            Sprite background = null;
            //检测有无背景图
            if (Data.BackgroundsDict.Count == 0)
            {
                //若无背景图，向上查找
                Scene parentScene = ParentScene;
                //循环查找
                while (parentScene != null)
                {
                    //检查父级场景背景图
                    if (parentScene.Data.BackgroundsDict.Count == 0)
                    {
                        //若无背景图，继续查找
                        parentScene = parentScene.ParentScene;
                    }
                    else
                    {
                        //若有背景图，尝试获取对应背景
                        if (!parentScene.Data.BackgroundsDict.TryGetValue(backgroundName, out background))
                        {
                            //若无对应背景，尝试指定“0”
                            if (!parentScene.Data.BackgroundsDict.TryGetValue("0", out background))
                            {
                                //若无“0”背景，返回第一个背景
                                background = parentScene.Data.BackgroundsDict.Values.First();
                            }
                        }
                        //并结束循环
                        break;
                    }
                }
                //循环结束后检测背景图
                if (background == null)
                {
                    //若仍无背景图，返回默认背景
                    background = GameScene.DefaultBackground;
                }
            }
            else
            {
                //若有背景图，尝试获取对应背景
                if (!Data.BackgroundsDict.TryGetValue(backgroundName, out background))
                {
                    //若无对应背景，尝试指定“0”
                    if (!Data.BackgroundsDict.TryGetValue("0", out background))
                    {
                        //若无“0”背景，返回第一个背景
                        background = Data.BackgroundsDict.Values.First();
                    }
                }
            }
            //返回
            return background;
        }
        #endregion

        #region 构造与初始化相关
        //构造函数
        public Scene(string filePath) : base(filePath)
        {
            //向数据库添加自身
            GameScene.AddScene(this);
        }
        #endregion

        #region 数据读取相关
        //加载资源协程
        public IEnumerator LoadAllResourcesCoroutine(Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //返回协程
            yield return Data.LoadBackgroundsCoroutine(DataDirectory, onBackgroundLoaded, onAllBackgroundsLoaded);
        }
        //异步加载所有资源
        public async void LoadAllResourcesAsync(Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //异步加载
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

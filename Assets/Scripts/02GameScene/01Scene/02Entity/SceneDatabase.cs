using System.Collections.Generic;
using THLL.BaseSystem;

namespace THLL.SceneSystem
{
    public class SceneDatabase : BaseGameEntityDatabase<SceneData, Scene>
    {
        #region 新增存储
        //根场景存储
        public Dictionary<string, Scene> RootScenesStorage { get; private set; } = new();
        //相邻场景存储ID版本
        private Dictionary<string, HashSet<string>> AdjacentScenesIDStorage { get; set; } = new();
        //相邻场景存储
        private Dictionary<Scene, HashSet<Scene>> AdjacentScenesStorage { get; set; } = new();
        //父级索引
        private Dictionary<Scene, HashSet<Scene>> ParentIndex { get; set; } = new();
        #endregion

        #region 初始化相关
        //初始化
        public override void Init()
        {
            //父类初始化
            base.Init();
            //遍历所有场景
            foreach (Scene scene in Values)
            {
                //判断有无父级
                if (string.IsNullOrEmpty(scene.ParentSceneID))
                {
                    //若无父级，则加入根场景存储
                    RootScenesStorage[scene.ID] = scene;
                }
                else
                {
                    //若有父级，则尝试获取父级
                    if (TryGetValue(scene.ParentSceneID, out Scene parentScene))
                    {
                        //若存在，则将子场景加入父级的子场景集合
                        parentScene.ChildScenesHashSet.Add(scene);
                    }
                    else
                    {
                        //若不存在，游戏内打印警告
                        GameHistory.LogWarning("Scene " + scene.ID + " has a parent scene " + scene.ParentSceneID + " that does not exist in the database.");
                    }
                }

                //以其ID为键查找相邻场景
                if (AdjacentScenesIDStorage.TryGetValue(scene.ID, out HashSet<string> adjacentScenesIDSet))
                {
                    //遍历相邻场景ID
                    foreach (string adjacentSceneID in adjacentScenesIDSet)
                    {
                        //尝试获取相邻场景
                        if (TryGetValue(adjacentSceneID, out Scene adjacentScene))
                        {
                            //若存在，则将其加入相邻场景集合
                            scene.AdjacentScenesHashSet.Add(adjacentScene);
                        }
                        else
                        {
                            //若不存在，游戏内打印警告
                            GameHistory.LogWarning("Scene " + scene.ID + " has an adjacent scene " + adjacentSceneID + " that does not exist in the database.");
                        }
                    }
                }
            }
        }
        //初始化筛选器
        protected override void InitFilter()
        {
            //暂时没有查询关键字需要初始化
        }
        #endregion

        #region 方法重写
        //添加方法
        public override void Add(Scene scene)
        {
            //父类添加
            base.Add(scene);
            //存储子场景相邻状态数据
            StoreAdjacentScenesData(scene);
            //关联相邻场景
            AdjacentScenesStorage[scene] = scene.AdjacentScenesHashSet;
            //关联父级索引
            ParentIndex[scene] = scene.ChildScenesHashSet;
        }
        #endregion

        #region 新增私有方法
        //存储子场景相邻状态数据
        private void StoreAdjacentScenesData(Scene scene)
        {
            //遍历其子场景相邻状态
            foreach ((string, string) tuple in scene.ChildScenesAdjacentStates)
            {
                //尝试获取场景1存储数据
                if (!AdjacentScenesIDStorage.TryGetValue(tuple.Item1, out HashSet<string> scene1Set))
                {
                    //若不存在，则创建
                    scene1Set = new HashSet<string>();
                    //存储
                    AdjacentScenesIDStorage[tuple.Item1] = scene1Set;
                }
                //将场景2ID加入场景1存储数据
                scene1Set.Add(tuple.Item2);
                //尝试获取场景2存储数据
                if (!AdjacentScenesIDStorage.TryGetValue(tuple.Item2, out HashSet<string> scene2Set))
                {
                    //若不存在，则创建
                    scene2Set = new HashSet<string>();
                    //存储
                    AdjacentScenesIDStorage[tuple.Item2] = scene2Set;
                }
                //将场景1ID加入场景2存储数据
                scene2Set.Add(tuple.Item1);
            }
        }
        #endregion 
    }
}

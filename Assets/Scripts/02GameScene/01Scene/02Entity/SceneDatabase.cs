using System.Collections.Generic;
using THLL.BaseSystem;

namespace THLL.SceneSystem
{
    public class SceneDatabase : BaseGameEntityDatabase<SceneData, Scene>
    {
        #region 新增存储
        //根场景存储
        public Dictionary<string, Scene> RootSceneStorage { get; private set; } = new();
        //路径存储-场景ID版本
        public Dictionary<string, Dictionary<string, int>> PathStorageIDVersion { get; private set; } = new();
        //路径存储，场景版本
        public Dictionary<Scene, Dictionary<Scene, int>> PathStorage { get; private set; } = new();
        //父级索引
        public Dictionary<Scene, HashSet<Scene>> ParentIndex { get; private set; } = new();
        #endregion

        #region 初始化相关
        //初始化
        public override void Init()
        {
            //父类初始化
            base.Init();
            //针对基础存储中的每一个场景进行初始化
            foreach (Scene scene in BasicStorage.Values)
            {
                //初始化该场景
                scene.Init(this);
                //初始化场景结束后，获取该场景的路径存储
                PathStorage[scene] = scene.PathsDict;
                //及父级索引存储
                ParentIndex[scene] = scene.ChildScenesHashSet;
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
            //检测是否有父级
            if (string.IsNullOrEmpty(scene.ParentSceneID))
            {
                //若无父级，则添加到根场景存储
                RootSceneStorage[scene.ID] = scene;
            }
        }
        #endregion
    }
}

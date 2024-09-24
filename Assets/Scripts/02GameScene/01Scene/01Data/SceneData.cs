using System.Collections.Generic;
using THLL.BaseSystem;

namespace THLL.SceneSystem
{
    public class SceneData : BaseGameData
    {
        #region 数据
        //父级场景ID
        public string ParentSceneID { get; set; }
        //子级场景间路径列表
        public List<ScenePathData> ChildScenePathDatas { get; set; }
        #endregion

        #region 构造函数
        //无参
        public SceneData()
        {

        }
        //有参
        public SceneData(string id, string name, string description, int sortOrder, string parentSceneID) : base(id, name, description, sortOrder)
        {
            ParentSceneID = parentSceneID;
        }
        #endregion
    }
}

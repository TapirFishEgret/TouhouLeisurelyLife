using THLL.SceneSystem;

namespace THLL.EditorSystem.SceneEditor
{
    public class SceneSystemDataContainer : TreeViewItemDataContainer<SceneData>
    {
        #region 构造函数
        //传入文字数据时
        public SceneSystemDataContainer(string stringData, int sortOrder, TreeViewItemDataContainer<SceneData> parent)
            : base(stringData, sortOrder, parent)
        {

        }
        //传入数据时
        public SceneSystemDataContainer(SceneData data, TreeViewItemDataContainer<SceneData> parent)
            : base(data, parent)
        {

        }
        #endregion
    }
}

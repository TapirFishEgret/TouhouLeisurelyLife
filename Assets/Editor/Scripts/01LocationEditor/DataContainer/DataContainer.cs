using THLL.SceneSystem;

namespace THLL.EditorSystem.SceneEditor
{
    public class DataContainer
    {
        #region 数据
        //ID
        public int ID { get; set; } = -1;
        //显示名称
        public string StringData { get; set; } = string.Empty;
        //排序
        public int SortOrder { get; set; } = 0;
        //节点数据
        public SceneData Data { get; set; } = default;
        //父级
        public DataContainer Parent { get; set; } = null;
        #endregion

        #region 构造函数
        //传入文字数据时
        public DataContainer(string id, string stringData, int sortOrder, DataContainer parent)
        {
            ID = id.GetHashCode();
            StringData = stringData;
            SortOrder = sortOrder;
            Data = default;
            Parent = parent;
        }
        //传入数据时
        public DataContainer(SceneData data, DataContainer parent)
        {
            ID = data.ID.GetHashCode();
            StringData = data.IDPart;
            SortOrder = data.SortOrder;
            Data = data;
            Parent = parent;
        }
        #endregion
    }
}

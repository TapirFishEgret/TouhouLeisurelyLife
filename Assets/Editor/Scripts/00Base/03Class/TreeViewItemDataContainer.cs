using THLL.BaseSystem;

namespace THLL.EditorSystem
{
    public class TreeViewItemDataContainer<T> where T : BaseGameData
    {
        #region 数据
        //TreeViewItem-ID
        public int ID { get; set; } = -1;
        //TreeViewItem-显示名称
        public string StringData { get; set; } = string.Empty;
        //排序
        public int SortOrder { get; set; } = 0;
        //TreeViewItem-节点数据
        public T Data { get; set; } = default;
        //TreeViewItem-父级
        public TreeViewItemDataContainer<T> Parent { get; set; } = null;
        #endregion

        #region 构造函数
        //字符串数据类型
        public TreeViewItemDataContainer(string stringData, int sortOrder, TreeViewItemDataContainer<T> parent)
        {
            ID = stringData.GetHashCode();
            StringData = stringData;
            SortOrder = sortOrder;
            Data = default;
            Parent = parent;
        }
        //数据数据类型
        public TreeViewItemDataContainer(T data, TreeViewItemDataContainer<T> parent)
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

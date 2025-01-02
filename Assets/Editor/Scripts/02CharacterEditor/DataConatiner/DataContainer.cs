using THLL.CharacterSystem;

namespace THLL.EditorSystem.CharacterEditor
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
        public CharacterData Data { get; set; } = default;
        //父级
        public DataContainer Parent { get; set; } = null;
        //类型
        public ItemDataType Type { get; }
        #endregion

        #region 类内枚举
        //类型
        public enum ItemDataType
        {
            Series,
            Group,
            Character,
            Version
        }
        #endregion

        #region 构造函数
        //字符串数据类型
        public DataContainer(string id, string stringData, int sortOrder, DataContainer parent, ItemDataType type)
        {
            ID = id.GetHashCode();
            StringData = stringData;
            SortOrder = sortOrder;
            Data = default;
            Parent = parent;
            Type = type;
        }
        //数据数据类型
        public DataContainer(CharacterData data, DataContainer parent)
        {
            ID = data.GetHashCode();
            StringData = data.IDPart;
            SortOrder = data.SortOrder;
            Data = data;
            Parent = parent;
            Type = ItemDataType.Version;
        }
        #endregion
    }
}

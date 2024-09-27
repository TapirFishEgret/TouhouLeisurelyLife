using THLL.CharacterSystem;

namespace THLL.EditorSystem.CharacterEditor
{
    public class CharacterSystemDataContainer : TreeViewItemDataContainer<CharacterData>
    {
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

        #region 数据
        //类型
        public ItemDataType Type { get; }
        #endregion 

        #region 构造函数
        //字符串数据类型
        public CharacterSystemDataContainer(string stringData, int sortOrder, CharacterSystemDataContainer parent, ItemDataType type)
            : base(stringData, sortOrder, parent)
        {
            Type = type;
        }
        //数据数据类型
        public CharacterSystemDataContainer(CharacterData data, CharacterSystemDataContainer parent)
            : base(data, parent)
        {
            Type = ItemDataType.Version;
        }
        #endregion
    }
}

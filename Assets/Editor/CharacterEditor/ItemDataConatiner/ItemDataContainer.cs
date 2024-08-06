using THLL.CharacterSystem;

namespace THLL.GameEditor.CharacterEditor
{
    //主要为解决Unity的TreeView中不允许不同类型的ItemData的问题
    public class ItemDataContainer
    {
        //枚举，表示该物体所属类型
        public enum ItemType
        {
            OriginatingSeries,
            Affiliation,
            CharacterName,
            CharacterData
        }

        //物体类型
        public ItemType Type { get; } = ItemType.OriginatingSeries;
        //排序
        public int SortingOrder { get; } = 0;
        //字符串数据
        public string StringData { get; } = string.Empty;
        //角色数据
        public CharacterData CharacterData { get; } = null;
        //父级数据
        public ItemDataContainer Parent { get; } = null;

        //构建函数
        public ItemDataContainer(string data, ItemType itemType, ItemDataContainer parent, int sortingOrder)
        {
            Type = itemType;
            StringData = data;
            SortingOrder = sortingOrder;
            Parent = parent;
        }
        public ItemDataContainer(CharacterData characterData, ItemDataContainer parent)
        {
            Type = ItemType.CharacterData;
            SortingOrder = characterData.SortingOrder;
            CharacterData = characterData;
            StringData = characterData.Version;
            Parent = parent;
        }
    }
}

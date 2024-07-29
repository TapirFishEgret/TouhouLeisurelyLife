using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ItemType Type { get; }
        //排序
        public int SortingOrder { get; }
        //字符串数据
        public string StringData { get; }
        //角色数据
        public CharacterData CharacterData { get; }

        //构建函数
        public ItemDataContainer(string data, ItemType itemType, int sortingOrder)
        {
            Type = itemType;
            StringData = data;
            SortingOrder = sortingOrder;
        }
        public ItemDataContainer(CharacterData characterData)
        {
            Type = ItemType.CharacterData;
            SortingOrder = characterData.SortingOrder;
            CharacterData = characterData;
        }
    }
}

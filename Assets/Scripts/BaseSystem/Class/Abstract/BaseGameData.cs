using UnityEngine;

namespace THLL.BaseSystem
{
    public abstract class BaseGameData : ScriptableObject
    {
        #region 基础数据成员
        //ID
        [SerializeField]
        protected string id = string.Empty;
        public virtual string ID { get { return id; } set { id = value; } }
        //Name
        [SerializeField]
        protected string dataName = string.Empty;
        public virtual string Name { get { return dataName; } set { dataName = value; } }
        //Description
        [SerializeField]
        protected string description = string.Empty;
        public virtual string Description { get { return description; } set { description = value; } }
        //GameDataType
        [SerializeField]
        protected GameDataTypeEnum gameDataType = 0;
        public virtual GameDataTypeEnum GameDataType { get { return gameDataType; } set { gameDataType = value; } }
        //SortingOrder
        [SerializeField]
        protected int sortingOrder = 0;
        public virtual int SortingOrder { get { return sortingOrder; } set { sortingOrder = value; } }
        #endregion

#if UNITY_EDITOR
        //生成ID
        public virtual void Editor_GenerateID()
        {
            id = string.Join("_", new string[] { GameDataType.ToString(), Name });
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

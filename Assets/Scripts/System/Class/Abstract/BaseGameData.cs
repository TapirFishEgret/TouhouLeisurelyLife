using UnityEngine;

namespace THLL.BaseSystem
{
    public abstract class BaseGameData : ScriptableObject
    {
        #region 基础数据成员
        //ID
        public virtual string ID => string.Join("_", new string[] { Package, Category, Author, Name });
        //Name
        [SerializeField]
        protected string dataName;
        public virtual string Name => dataName;
        //Description
        [SerializeField]
        protected string dataDescription;
        public virtual string Description => dataDescription;
        //Author
        [SerializeField]
        protected string dataAuthor;
        public virtual string Author => dataAuthor;
        //Package
        [SerializeField]
        protected string dataPackage;
        public virtual string Package => dataPackage;
        //Category
        [SerializeField]
        protected string dataCategory;
        public virtual string Category => dataCategory;
        #endregion

#if UNITY_EDITOR
        public virtual void Editor_SetName(string name)
        {
            dataName = name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public virtual void Editor_SetDescription(string description)
        {
            dataDescription = description;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public virtual void Editor_SetAuthor(string author)
        {
            dataAuthor = author;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public virtual void Editor_SetPackage(string package)
        {
            dataPackage = package;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public virtual void Editor_SetCategory(string category)
        {
            dataCategory = category;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

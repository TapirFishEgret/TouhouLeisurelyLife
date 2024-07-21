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
        private string _name;
        public string Name { get => _name; protected set => _name = value; }
        //Description
        [SerializeField]
        private string _description;
        public string Description { get => _description; protected set => _description = value; }
        //Author
        [SerializeField]
        private string _author;
        public string Author { get => _author; protected set => _author = value; }
        //Package
        [SerializeField]
        private string _package;
        public string Package { get => _package; protected set => _package = value; }
        //Category
        [SerializeField]
        private string _category;
        public string Category { get => _category; protected set => _category = value; }
        #endregion

        #if UNITY_EDITOR 
        public virtual void Editor_SetName(string name)
        {
            _name = name;
        }
        public virtual void Editor_SetDescription(string description)
        {
            _description = description;
        }
        public virtual void Editor_SetAuthor(string author)
        {
            _author = author;
        }
        public virtual void Editor_SetPackage(string package)
        {
            _package = package;
        }
        public virtual void Editor_SetCategory(string category)
        {
            _category = category;
        }
        #endif
    }
}

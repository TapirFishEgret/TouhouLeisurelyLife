using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using THLL.LocationSystem.Tags;
using UnityEngine;


namespace THLL.LocationSystem
{
    public class LocUnitData : BaseGameData
    {
        #region 地点类型数据成员
        //重写ID
        public override string ID => string.Join("_", new List<string>() { Package, Category, Author }.Concat(FullName));
        //全名
        private List<string> _fullName;
        public List<string> FullName => _fullName;
        //父级单元
        [SerializeField]
        private LocUnitData _parentData;
        public LocUnitData ParentData => _parentData;
        //地点背景图
        [SerializeField]
        private Sprite _background;
        public Sprite Background => _background;
        //地点相连情况
        [SerializeField]
        private List<LocUnitDataConn> _connections;
        public List<LocUnitDataConn> LocUnitDataConns => _connections;
        //标签列表
        [SerializeField]
        private List<LocUnitTag> _tags;
        public List<LocUnitTag> Tags => _tags;
        #endregion

        #region 函数
        //构建函数
        public LocUnitData()
        {
            //初始化
            _connections = new();
            _tags = new();
        }
        #endregion

#if UNITY_EDITOR
        //生成全名
        public void Editor_GenerateFullName()
        {
            List<string> fullName = new();
            LocUnitData current = this;
            while (current != null)
            {
                fullName.Insert(0, current.Name);
                current = current.ParentData;
            }
            _fullName = fullName;
            //标记数据为脏
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定名称时
        public override void Editor_SetName(string name)
        {
            base.Editor_SetName(name);
            //重建全名
            Editor_GenerateFullName();
            //标记数据为脏
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定父级数据
        public void Editor_SetParent(LocUnitData parentData)
        {
            _parentData = parentData;
            //父级数据设定更改时生成全名
            Editor_GenerateFullName();
            //标记数据为脏
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定背景
        public void Editor_SetBackground(Sprite background)
        {
            _background = background;
            //标记数据为脏
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //复制数据
        public void Editor_CopyTo(LocUnitData otherData)
        {
            otherData.Editor_SetPackage(Package);
            otherData.Editor_SetCategory(Category);
            otherData.Editor_SetAuthor(Author);
            otherData.Editor_SetDescription(Description);
            otherData.Editor_SetName(Name);
            otherData.Editor_SetParent(ParentData);
            otherData.Editor_SetBackground(Background);
        }
#endif
    }
}

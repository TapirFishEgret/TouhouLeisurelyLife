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
        private LocUnitData _parentLocUnitData;
        public LocUnitData ParentLocUnitData => _parentLocUnitData;
        //地点背景图
        [SerializeField]
        private Sprite _background;
        public Sprite Background => _background;
        //地点相连情况
        [SerializeField]
        private List<LocUnitDataConn> _locUnitDataConns;
        public List<LocUnitDataConn> LocUnitDataConns => _locUnitDataConns;
        //是否为本层级出入口
        [SerializeField]
        private bool isGateway;
        public bool IsGateway => isGateway;
        //标签列表
        [SerializeField]
        private List<LocUnitTag> _locUnitDataTags;
        public List<LocUnitTag> LocUnitDataTags => _locUnitDataTags;
        #endregion

        #region 函数
        //构建函数
        public LocUnitData()
        {
            //设置默认属性
            Package = "Core";
            Category = "Location";
            Author = "TapirFishEgret";
            //初始化
            _locUnitDataConns = new();
            _locUnitDataTags = new();
        }
        //生成全名
        public void GenerateFullName()
        {
            List<string> fullName = new();
            LocUnitData current = this;

            while (current != null)
            {
                fullName.Insert(0, current.Name);
                current = current.ParentLocUnitData;
            }

            _fullName = fullName;
        }
        #endregion

#if UNITY_EDITOR
        //设定名称时
        public override void Editor_SetName(string name)
        {
            base.Editor_SetName(name);
            //重建全名
            GenerateFullName();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定父级数据
        public void Editor_SetParent(LocUnitData parentData)
        {
            _parentLocUnitData = parentData;
            //父级数据设定更改时生成全名
            GenerateFullName();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定背景
        public void Editor_SetBackground(Sprite background)
        {
            _background = background;
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
            otherData.Editor_SetParent(ParentLocUnitData);
            otherData.Editor_SetBackground(Background);
        }
#endif
    }
}

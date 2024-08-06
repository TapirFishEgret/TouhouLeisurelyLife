using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using UnityEngine;


namespace THLL.LocationSystem
{
    public class LocUnitData : BaseGameData
    {
        #region 地点类型数据成员
        //重写ID
        [SerializeField]
        private string id = string.Empty;
        public override string ID => id;
        //全名
        [SerializeField]
        private List<string> _fullName = new();
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
        private List<LocUnitData> _connectionKeys = new();
        public List<LocUnitData> ConnectionKeys => _connectionKeys;
        [SerializeField]
        private List<int> _connectionValues = new();
        public List<int> ConnectionValues => _connectionValues;
        //是否为出入口
        [SerializeField]
        private bool _isGateway = false;
        public bool IsGateway => _isGateway;
        #endregion

#if UNITY_EDITOR
        //生成ID
        public override void Editor_GenerateID()
        {
            base.Editor_GenerateID();
            id = string.Join("_", new List<string>() { Package, Category, Author }.Concat(FullName)).Replace(" ", "-");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //生成全名
        public void Editor_GenerateFullName()
        {
            //遍历父级生成全名
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
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定父级数据
        public void Editor_SetParent(LocUnitData parentData)
        {
            _parentData = parentData;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定背景
        public void Editor_SetBackground(Sprite background)
        {
            _background = background;
            //标记数据为脏
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //设定是否为出入口
        public void Editor_SetIsGateway(bool isGateway)
        {
            _isGateway = isGateway;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //添加链接
        public void Editor_AddConnection(LocUnitData locUnitData, int duration)
        {
            ConnectionKeys.Add(locUnitData);
            ConnectionValues.Add(duration);
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //移除链接
        public bool Editor_RemoveConnection(LocUnitData locUnitData)
        {
            if (ConnectionKeys.Contains(locUnitData))
            {
                int index = ConnectionKeys.IndexOf(locUnitData);
                ConnectionKeys.RemoveAt(index);
                ConnectionValues.RemoveAt(index);
                UnityEditor.EditorUtility.SetDirty(this);
                return true;
            }
            UnityEditor.EditorUtility.SetDirty(this);
            return false;
        }
        //设定通行时间
        public void Editor_SetConnDuration(LocUnitData locUnitData, int duration)
        {
            if (ConnectionKeys.Contains(locUnitData))
            {
                int index = ConnectionKeys.IndexOf(locUnitData);
                ConnectionValues[index] = duration;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using UnityEngine;


namespace THLL.GeographySystem
{
    public class LocationData : BaseGameData
    {
        #region 地点类型数据成员
        //全名
        [SerializeField]
        private List<string> _fullName = new();
        public List<string> FullName => _fullName;
        //父级单元
        [SerializeField]
        private LocationData _parentData;
        public LocationData ParentData { get { return _parentData; } set { _parentData = value; } }
        //地点背景图
        [SerializeField]
        private Sprite _background;
        public Sprite Background { get { return _background; } set { _background = value; } }
        //是否为出入口
        [SerializeField]
        private bool _isGateway = false;
        public bool IsGateway { get { return _isGateway; } set { _isGateway = value; } }
        //地点相连情况
        [SerializeField]
        private List<LocationData> _connectionKeys = new();
        public List<LocationData> ConnectionKeys => _connectionKeys;
        [SerializeField]
        private List<int> _connectionValues = new();
        public List<int> ConnectionValues => _connectionValues;
        #endregion

#if UNITY_EDITOR
        //生成ID
        public override void Editor_GenerateID()
        {
            base.Editor_GenerateID();
            id = string.Join("_", new List<string>() { GameDataType.ToString() }.Concat(FullName)).Replace(" ", "-");
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //生成全名
        public void Editor_GenerateFullName()
        {
            //遍历父级生成全名
            List<string> fullName = new();
            LocationData current = this;
            while (current != null)
            {
                fullName.Insert(0, current.Name);
                current = current.ParentData;
            }
            _fullName = fullName;
            //标记数据为脏
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //添加链接
        public void Editor_AddConnection(LocationData locUnitData, int duration)
        {
            ConnectionKeys.Add(locUnitData);
            ConnectionValues.Add(duration);
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //移除链接
        public bool Editor_RemoveConnection(LocationData locUnitData)
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
        //设定通行距离
        public void Editor_SetConnDistance(LocationData locUnitData, int distance)
        {
            if (ConnectionKeys.Contains(locUnitData))
            {
                int index = ConnectionKeys.IndexOf(locUnitData);
                ConnectionValues[index] = distance;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

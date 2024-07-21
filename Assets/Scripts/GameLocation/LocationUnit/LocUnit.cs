using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.LocationSystem.Tags;
using UnityEngine;

namespace THLL.LocationSystem
{
    public class LocUnit : BaseGameEntity<LocUnitData>
    {
        #region 地点实例数据成员
        //全名
        public List<string> FullName => baseData.FullName;
        //父级地点实例
        private LocUnit _parentLocUnit;
        public LocUnit ParentLocUnit => _parentLocUnit;
        //子级地点实例
        private readonly LocUnitDb _childrenLocUnits;
        public LocUnitDb ChildrenLocUnits => _childrenLocUnits;
        //背景图
        public Sprite Background => baseData.Background;
        //地点连接情况
        private readonly Dictionary<LocUnit, int> _locUnitConns;
        public Dictionary<LocUnit, int> LocUnitConns => _locUnitConns;
        //标签存储
        private readonly LocUnitTagDb _locUnitTags;
        public LocUnitTagDb LocUnitTags => _locUnitTags;
        //是否为地区出入口
        public bool IsGateway => baseData.IsGateway;
        #endregion

        #region 方法
        //构建函数
        public LocUnit(LocUnitData data) : base(data)
        {
            _childrenLocUnits = new();
            _locUnitConns = new();
            _locUnitTags = new();
        }
        public void Test()
        {
            Debug.Log(baseData.ParentLocUnitData.Name);
            Debug.Log(ParentLocUnit.Name);
        }
        //初始化方法
        public void Init(LocUnitDb globalData, LocUnitConnDb globalConnData)
        {
            //设定父级
            if (baseData.ParentLocUnitData != null)
            {
                _parentLocUnit = globalData[baseData.ParentLocUnitData];
            }

            //设定子级
            foreach (LocUnit locUnit in globalData.GetChildren(ID))
            {
                ChildrenLocUnits.AddData(locUnit.baseData, locUnit);
            }

            //创建连接
            InitConnections(globalData, globalConnData);
        }
        //初始化地点连接
        private void InitConnections(LocUnitDb globalData, LocUnitConnDb globalConnData)
        {
            //检测连接数据是否为空
            if (LocUnitConns.Count == 0)
            {
                return;
            }

            //遍历数据中的连接数据
            foreach (LocUnitDataConn conn in baseData.LocUnitDataConns)
            {
                //目标地点
                LocUnit targetLocUnit = globalData[conn.otherLocUnit];
                //添加自身连接
                LocUnitConns[targetLocUnit] = conn.duration;
                //添加全局双向连接
                globalConnData.AddConnection(this, targetLocUnit, conn.duration);
                globalConnData.AddConnection(targetLocUnit, this, conn.duration);
            }

            //判断自身是否为出入口
            if (IsGateway)
            {
                //若是，添加自身与全局与父级的双向连接
                LocUnitConns[ParentLocUnit] = 0;
                ParentLocUnit.LocUnitConns[this] = 0;
                globalConnData.AddConnection(this, ParentLocUnit, 0);
                globalConnData.AddConnection(ParentLocUnit, this, 0);
            }
        }
        #endregion
    }
}

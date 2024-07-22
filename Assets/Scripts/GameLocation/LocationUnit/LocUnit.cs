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
        private LocUnit _parent;
        public LocUnit Parent => _parent;
        //子级地点实例
        private readonly LocUnitDb _children;
        public LocUnitDb Children => _children;
        //背景图
        public Sprite Background => baseData.Background;
        //地点连接情况
        private readonly Dictionary<LocUnit, int> _connections;
        public Dictionary<LocUnit, int> Connections => _connections;
        //标签存储
        private readonly LocUnitTagDb _tags;
        public LocUnitTagDb Tags => _tags;
        #endregion

        #region 方法
        //构建函数
        public LocUnit(LocUnitData data) : base(data)
        {
            _children = new();
            _connections = new();
            _tags = new();
        }
        //初始化方法
        public void Init(LocUnitDb globalData, LocUnitConnDb globalConnData)
        {
            //设定父级
            if (baseData.ParentData != null)
            {
                _parent = globalData[baseData.ParentData];
            }

            //设定子级
            foreach (LocUnit locUnit in globalData.GetChildren(ID))
            {
                Children.AddValue(locUnit.baseData, locUnit);
            }

            //应用标签
            foreach (LocUnitTag locUnitTag in baseData.Tags)
            {
                locUnitTag.ApplyTag(this, globalData, globalConnData);
            }

            //创建连接
            InitConnections(globalData, globalConnData);
        }
        //初始化地点连接
        private void InitConnections(LocUnitDb globalData, LocUnitConnDb globalConnData)
        {
            //检测连接数据是否为空
            if (Connections.Count == 0)
            {
                return;
            }

            //遍历数据中的连接数据
            foreach (LocUnitDataConn conn in baseData.LocUnitDataConns)
            {
                //目标地点
                LocUnit targetLocUnit = globalData[conn.otherLocUnit];
                //添加双向连接
                Connections[targetLocUnit] = conn.duration;
                targetLocUnit.Connections[this] = conn.duration;
                //添加全局双向连接
                globalConnData.AddConnection(this, targetLocUnit, conn.duration);
                globalConnData.AddConnection(targetLocUnit, this, conn.duration);
            }
        }
        #endregion
    }
}

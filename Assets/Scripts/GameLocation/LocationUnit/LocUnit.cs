using System.Collections.Generic;
using THLL.BaseSystem;
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
        //是否为出入口
        public bool IsGateway => baseData.IsGateway;
        #endregion

        #region 方法
        //构建函数
        public LocUnit(LocUnitData data) : base(data)
        {
            _children = new();
            _connections = new();
        }
        //初始化方法
        public void Init(LocUnitDb globalData, Dictionary<LocUnit, Dictionary<LocUnit, int>> globalConnData)
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

            //将加载连接数据并放入全局
            foreach (LocUnitData locUnitData in baseData.ConnectionKeys)
            {
                //序号
                int index = baseData.ConnectionKeys.IndexOf(locUnitData);
                //耗时
                int duration = baseData.ConnectionValues[index];
                //实例
                LocUnit locUnit = globalData[locUnitData];
                //存放
                if (!globalConnData.ContainsKey(this))
                {
                    globalConnData[this] = new Dictionary<LocUnit, int>();
                }
                globalConnData[this][locUnit] = duration;
            }
        }
        #endregion
    }
}

using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.CharacterSystem;
using UnityEngine;

namespace THLL.GeographySystem
{
    public class Location : BaseGameEntity<LocationData>
    {
        #region 从数据中获取的数据成员
        //全名
        public List<string> FullName => BaseData.FullName;
        //父级地点实例
        public Location Parent { get; private set; }
        //背景图
        public Sprite Background => BaseData.Background;
        //地点连接情况
        public Dictionary<Location, int> Connections { get; } = new();
        //是否为出入口
        public bool IsGateway => BaseData.IsGateway;
        #endregion

        #region 实例特有的数据成员
        //子级地点实例
        public LocationDb Children { get; } = new();
        //当前该地点下的角色
        public CharacterDb Character { get; } = new();
        #endregion

        #region 方法
        //构建函数
        public Location(LocationData data) : base(data) { }
        //初始化方法
        public void Init(LocationDb globalData, Dictionary<Location, Dictionary<Location, int>> globalConnData)
        {
            //设定父级
            if (BaseData.ParentData != null)
            {
                Parent = globalData[BaseData.ParentData];
            }

            //设定子级
            foreach (Location locUnit in globalData.GetChildren(ID))
            {
                Children.Add(locUnit.BaseData, locUnit);
            }

            //将加载连接数据并放入全局
            foreach (LocationData locUnitData in BaseData.ConnectionKeys)
            {
                //序号
                int index = BaseData.ConnectionKeys.IndexOf(locUnitData);
                //耗时
                int duration = BaseData.ConnectionValues[index];
                //实例
                Location location = globalData[locUnitData];
                //存放在自身
                Connections[location] = duration;
                //存放入全局
                if (!globalConnData.ContainsKey(this))
                {
                    globalConnData[this] = new Dictionary<Location, int>();
                }
                globalConnData[this][location] = duration;
            }
        }
        #endregion
    }
}

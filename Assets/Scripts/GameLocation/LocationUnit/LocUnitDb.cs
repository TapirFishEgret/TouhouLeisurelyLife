using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;

namespace THLL.LocationSystem
{
    public class LocUnitDb : BaseGameEntityDb<LocUnitData, LocUnit>
    {
        #region 新增存储
        //父级名称索引存储
        private Dictionary<string, List<LocUnit>> ParentDic { get; } = new();
        #endregion

        #region 操作方法
        //增添
        public override void Add(LocUnitData key, LocUnit value)
        {
            base.Add(key, value);

            //创建父级名称索引存储
            if (key.ParentData != null)
            {
                if (!ParentDic.ContainsKey(key.ParentData.ID))
                {
                    ParentDic[key.ParentData.ID] = new List<LocUnit>();
                }
                ParentDic[key.ParentData.ID].Add(value);
            }
        }
        public override void Add(LocUnit value)
        {
            Add(value.BaseData, value);
        }
        //移除
        public override bool Remove(LocUnitData key)
        {
            //从他的父级中移除他自己
            ParentDic[key.ParentData.ID].Remove(Store[key]);
            //移除他自己
            ParentDic.Remove(key.ID);
            //使用父级方法移除ID存储与根存储
            return base.Remove(key);
        }
        public override bool Remove(LocUnit value)
        {
            return Remove(value.BaseData);
        }
        public override bool Remove(string id)
        {
            return Remove(IDDic[id]);
        }
        //获取子级地点
        public IEnumerable<LocUnit> GetChildren(LocUnitData locUnitData)
        {
            return GetChildren(locUnitData.ID);
        }
        public IEnumerable<LocUnit> GetChildren(LocUnit locUnit)
        {
            return GetChildren(locUnit.ID);
        }
        public IEnumerable<LocUnit> GetChildren(string parentID)
        {
            return ParentDic.ContainsKey(parentID) ? ParentDic[parentID] : Enumerable.Empty<LocUnit>();
        }
        #endregion

        #region 其他方法
        //更新查询方式
        protected override void InitFilters()
        {
            base.InitFilters();
            //新增父级名称查询
            Filters[QueryKeywordEnum.L_ParentName] = (datas, queryValue) =>
            {
                //检查类型
                if (queryValue is string parentName)
                {
                    return datas.Where(d => d.Parent.Name == parentName);
                }
                return datas;
            };
            //新增是否为出入口查询
            Filters[QueryKeywordEnum.L_IsGateway] = (datas, queryValue) =>
            {
                //检查类型
                if (queryValue is bool isGateway)
                {
                    return datas.Where(d => d.IsGateway == isGateway);
                }
                return datas;
            };
        }
        #endregion
    }
}

using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;

namespace THLL.LocationSystem
{
    public class LocUnitDb : BaseGameEntityDb<LocUnitData, LocUnit>
    {
        #region 新增存储
        //父级名称索引存储
        private readonly Dictionary<string, List<LocUnit>> _parentIndex;
        #endregion

        #region 操作方法
        //增添
        public override void AddData(LocUnitData key, LocUnit value)
        {
            base.AddData(key, value);

            //创建父级名称索引存储
            if (key.ParentLocUnitData != null)
            {
                if (!_parentIndex.ContainsKey(key.ParentLocUnitData.ID))
                {
                    _parentIndex[key.ParentLocUnitData.ID] = new List<LocUnit>();
                }
                _parentIndex[key.ParentLocUnitData.ID].Add(value);
            }
        }
        //获取子级地点
        public IEnumerable<LocUnit> GetChildren(string parentID)
        {
            return _parentIndex.ContainsKey(parentID) ? _parentIndex[parentID] : Enumerable.Empty<LocUnit>();
        }
        #endregion

        #region 其他方法
        //构造函数
        public LocUnitDb() : base()
        {
            _parentIndex = new Dictionary<string, List<LocUnit>>();
        }
        //更新查询方式
        protected override void InitFilters()
        {
            base.InitFilters();
            //新增父级名称查询
            filters[QueryKeywordEnum.L_ParentName] = (datas, queryValue) => datas.Where(d => d.ParentLocUnit.Name == queryValue);
        }
        #endregion
    }
}

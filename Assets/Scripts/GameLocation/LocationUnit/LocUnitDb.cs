using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;

namespace THLL.LocationSystem
{
    public class LocUnitDb : BaseGameEntityDb<LocUnitData, LocUnit>
    {
        #region 新增存储
        //ID索引存储
        private readonly Dictionary<string, LocUnit> _idStore;
        //父级名称索引存储
        private readonly Dictionary<string, List<LocUnit>> _parentIndex;
        #endregion

        #region 操作方法
        //增添
        public override void AddValue(LocUnitData key, LocUnit value)
        {
            base.AddValue(key, value);

            //向ID存储中添加同样的数据
            _idStore[value.ID] = value;

            //创建父级名称索引存储
            if (key.ParentData != null)
            {
                if (!_parentIndex.ContainsKey(key.ParentData.ID))
                {
                    _parentIndex[key.ParentData.ID] = new List<LocUnit>();
                }
                _parentIndex[key.ParentData.ID].Add(value);
            }
        }
        //获取
        public LocUnit GetValue(string id)
        {
            _idStore.TryGetValue(id, out var value);
            return value;
        }
        //获取子级地点
        public IEnumerable<LocUnit> GetChildren(string parentID)
        {
            return _parentIndex.ContainsKey(parentID) ? _parentIndex[parentID] : Enumerable.Empty<LocUnit>();
        }
        //索引器
        public LocUnit this[string id]
        {
            get
            {
                return GetValue(id);
            }
        }
        #endregion

        #region 其他方法
        //构造函数
        public LocUnitDb() : base()
        {
            _idStore = new Dictionary<string, LocUnit>();
            _parentIndex = new Dictionary<string, List<LocUnit>>();
        }
        //更新查询方式
        protected override void InitFilters()
        {
            base.InitFilters();
            //新增父级名称查询
            filters[QueryKeywordEnum.L_ParentName] = (datas, queryValue) =>
            {
                //检查类型
                if (queryValue is string parentName)
                {
                    return datas.Where(d => d.Parent.Name == parentName);
                }
                return datas;
            };
            //新增是否为出入口查询
            filters[QueryKeywordEnum.L_IsGateway] = (datas, queryValue) =>
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

using System.Collections.Generic;
using THLL.BaseSystem;

namespace THLL.LocationSystem
{
    public class LocUnitConnDb : BaseGameDb<LocUnit, Dictionary<LocUnit, int>>
    {
        #region 新增处理方法
        //增添连接
        public void AddConnection(LocUnit from, LocUnit to, int duration)
        {
            if (!store.ContainsKey(from))
            {
                store[from] = new Dictionary<LocUnit, int>();
            }
            store[from][to] = duration;
        }
        //获取连接
        public Dictionary<LocUnit, int> GetConnection(LocUnit locUnit)
        {
            return store.ContainsKey(locUnit) ? store[locUnit] : new Dictionary<LocUnit, int>();
        }
        #endregion

        #region 其他方法
        //初始化过滤器方法的实现
        protected override void InitFilters()
        {
            //空，连接应该是用不上过滤器查询
        }
        #endregion
    }
}

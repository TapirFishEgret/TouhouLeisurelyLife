using System.Linq;
using THLL.BaseSystem;

namespace THLL.LocationSystem.Tags
{
    public class LocUnitTagDb : BaseGameDataDb<LocUnitTag>
    {
        #region 方法
        //更新过滤器
        protected override void InitFilters()
        {
            base.InitFilters();
            //新增“是否可被继承”过滤器
            filters[QueryKeywordEnum.LT_IsInherited] = (datas, queryValue) => datas.Where(d => d.IsInherited);
        }
        #endregion
    }
}

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
            //新增“次级类型过滤器”
            filters[QueryKeywordEnum.LT_TagCategory] = (datas, queryValue) =>
            {
                //检查类型
                if (queryValue is string subCategory)
                {
                    return datas.Where(d => d.SubCategory == subCategory);
                }
                return datas;
            };
        }
        #endregion
    }
}

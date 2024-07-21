using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameEntityDb<TData, TEntity> : BaseGameDb<TData, TEntity> where TData : BaseGameData where TEntity : BaseGameEntity<TData>
    {
        #region 基础自身函数
        //构建函数
        public BaseGameEntityDb() : base() { }
        protected override void InitFilters()
        {
            //基础过滤器
            filters[QueryKeywordEnum.B_Package] = (datas, queryValue) => datas.Where(d => d.Package == queryValue);
            filters[QueryKeywordEnum.B_Category] = (datas, queryValue) => datas.Where(d => d.Category == queryValue);
            filters[QueryKeywordEnum.B_Author] = (datas, queryValue) => datas.Where(d => d.Author == queryValue);
            filters[QueryKeywordEnum.B_Name] = (datas, queryValue) => datas.Where(d => d.Name == queryValue);
        }
        #endregion
    }
}

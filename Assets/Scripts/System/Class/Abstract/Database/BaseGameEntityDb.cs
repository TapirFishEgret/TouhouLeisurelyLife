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
            filters[QueryKeywordEnum.B_Package] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is string package)
                {
                    return datas.Where(x => x.Package == package);
                }
                return datas;
            };
            filters[QueryKeywordEnum.B_Category] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is string category)
                {
                    return datas.Where(d => d.Category == category);
                }
                return datas;
            };
            filters[QueryKeywordEnum.B_Author] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is string author)
                {
                    return datas.Where(d => d.Author == author);
                }
                return datas;
            };
            filters[QueryKeywordEnum.B_Name] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is string name)
                {
                    return datas.Where(d => d.Name == name);
                }
                return datas;
            };
        }
        #endregion
    }
}

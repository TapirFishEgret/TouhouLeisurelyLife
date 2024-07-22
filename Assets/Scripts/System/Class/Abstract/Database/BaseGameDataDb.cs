using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDataDb<T> : BaseGameDb<string, T> where T : BaseGameData
    {
        #region 操作方法
        //增添
        public void AddValue(T data)
        {
            base.AddValue(data.ID, data);
        }
        #endregion

        #region 基础自身函数
        //构造函数
        public BaseGameDataDb() : base() { }
        //过滤器初始化
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

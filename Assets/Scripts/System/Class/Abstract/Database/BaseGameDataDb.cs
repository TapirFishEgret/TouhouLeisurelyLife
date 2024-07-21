using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDataDb<T> : BaseGameDb<string, T> where T : BaseGameData
    {
        #region 操作方法
        //增添
        public void AddData(T data)
        {
            base.AddData(data.ID, data);
        }
        #endregion

        #region 基础自身函数
        //构造函数
        public BaseGameDataDb() : base() { }
        //过滤器初始化
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

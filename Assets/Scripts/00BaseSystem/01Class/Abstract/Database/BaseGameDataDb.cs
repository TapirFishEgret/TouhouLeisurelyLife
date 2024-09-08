using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDataDb<T> : BaseGameDb<string, T> where T : BaseGameData
    {
        #region 操作方法
        //增添
        public virtual void Add(T data)
        {
            base.Add(data.ID, data);
        }
        //移除
        public virtual void Remove(T data)
        {
            base.Remove(data.ID);
        }
        #endregion

        #region 基础自身函数
        //构造函数
        public BaseGameDataDb() : base() { }
        //过滤器初始化
        protected override void InitFilters()
        {
            //基础过滤器
            Filters[QueryKeywordEnum.B_Category] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is GameDataTypeEnum gameDataType)
                {
                    return datas.Where(d => d.GameDataType == gameDataType);
                }
                return datas;
            };
            Filters[QueryKeywordEnum.B_Name] = (datas, queryValue) =>
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

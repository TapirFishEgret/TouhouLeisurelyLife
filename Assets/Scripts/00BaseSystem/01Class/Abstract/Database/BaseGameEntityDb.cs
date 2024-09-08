using System.Collections.Generic;
using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameEntityDb<TData, TEntity> : BaseGameDb<TData, TEntity> where TData : BaseGameData where TEntity : BaseGameEntity<TData>
    {
        #region 新增存储
        //ID存储
        //ID索引存储
        protected Dictionary<string, TEntity> IDDic { get; } = new();
        #endregion

        #region 基础操作方法
        //增添
        public override void Add(TData key, TEntity value)
        {
            base.Add(key, value);
            //添加到ID存储中去
            IDDic[value.ID] = value;
        }
        public virtual void Add(TEntity value)
        {
            Add(value.BaseData, value);
        }
        //获取
        public virtual TEntity Get(string id)
        {
            IDDic.TryGetValue(id, out var value);
            return value;
        }
        //移除
        public override bool Remove(TData key)
        {
            IDDic.Remove(key.ID);
            return base.Remove(key);
        }
        public virtual bool Remove(TEntity value)
        {
            return Remove(value.BaseData);
        }
        public virtual bool Remove(string id)
        {
            return Remove(IDDic[id]);
        }
        //索引器
        public TEntity this[string id]
        {
            get
            {
                return Get(id);
            }
        }
        #endregion

        #region 基础自身函数
        //构建函数
        public BaseGameEntityDb() : base() { }
        protected override void InitFilters()
        {
            //基础过滤器
            Filters[QueryKeywordEnum.B_Category] = (datas, queryValue) =>
            {
                //类型检查
                if (queryValue is GameDataTypeEnum gameDataType)
                {
                    return datas.Where(d => d.Category == gameDataType);
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

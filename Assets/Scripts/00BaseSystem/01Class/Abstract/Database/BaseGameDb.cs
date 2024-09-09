using System.Collections.Generic;
using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDb<TKey, TValue>
    {
        #region 基础数据存储
        //数据存储
        protected Dictionary<TKey, TValue> Store { get; } = new();
        //过滤器存储
        protected Dictionary<QueryKeywordEnum, FilterDelegate<TValue, object>> Filters { get; } = new();
        //查询缓存
        private Dictionary<object, IEnumerable<TValue>> QueryCache { get; } = new();
        #endregion

        #region 属性
        //个数
        public int Count => Store.Count;
        //所有值
        public Dictionary<TKey, TValue>.ValueCollection Datas => Store.Values;
        #endregion


        #region 基础操作方法
        //增添
        public virtual void Add(TKey key, TValue value)
        {
            //添加数据
            Store[key] = value;
            //清除缓存
            QueryCache.Clear();
        }
        //获取
        public virtual TValue Get(TKey key)
        {
            Store.TryGetValue(key, out TValue value);
            return value;
        }
        //移除
        public virtual bool Remove(TKey key)
        {
            //清除缓存
            QueryCache.Clear();
            //返回
            return Store.Remove(key);
        }
        //关键字查询
        public IEnumerable<TValue> QueryByKeywords(Dictionary<QueryKeywordEnum, object> queryParameters)
        {
            //获取本次查询缓存查询键
            string cacheKey = GenerateCacheKey(queryParameters);
            //查询缓存数据
            if (QueryCache.TryGetValue(cacheKey, out var cachedResult))
            {
                //若有缓存数据，则返回
                return cachedResult;
            }

            //若无缓存数据，则正常筛选
            IEnumerable<TValue> result = Store.Values;

            //筛选
            foreach (var queryParameter in queryParameters)
            {
                //获取关键词与查询语句
                QueryKeywordEnum keyword = queryParameter.Key;
                object queryValue = queryParameter.Value;

                //进行筛选
                if (Filters.TryGetValue(keyword, out var filter))
                {
                    result = filter(result, queryValue);
                }
                else
                {
                    //在游戏历史记录中报错
                    GameHistory.LogError("查询失败，请检查查询条件");
                }
            }

            //记录缓存
            QueryCache[cacheKey] = result;
            //返回筛选结果
            return result;
        }
        //索引
        public TValue this[TKey key]
        {
            get
            {
                return Get(key);
            }
        }
        #endregion

        #region 其他方法
        //构建函数
        protected BaseGameDb()
        {
            //初始化过滤器
            InitFilters();
        }
        //生成缓存Key
        private string GenerateCacheKey(Dictionary<QueryKeywordEnum, object> queryParameters)
        {
            return string.Join(";", queryParameters.Select(p => $"{p.Key}:{p.Value}"));
        }
        //初始化过滤器
        protected abstract void InitFilters();
        #endregion
    }
}

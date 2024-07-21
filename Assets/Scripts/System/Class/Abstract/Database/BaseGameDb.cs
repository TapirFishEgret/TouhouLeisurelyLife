using System.Collections.Generic;
using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDb<TKey, TValue>
    {
        #region 基础数据存储
        //数据存储
        protected readonly Dictionary<TKey, TValue> store;
        //过滤器存储
        protected readonly Dictionary<QueryKeywordEnum, FilterDelegate<TValue>> filters;
        //查询缓存
        private readonly Dictionary<string, IEnumerable<TValue>> _queryCache;
        #endregion

        #region 基础操作方法
        //增添
        public virtual void AddData(TKey key, TValue value)
        {
            //添加数据
            store[key] = value;
            //清除缓存
            _queryCache.Clear();
        }
        //获取
        public TValue GetData(TKey key)
        {
            store.TryGetValue(key, out TValue value);
            return value;
        }
        //关键字查询
        public IEnumerable<TValue> QueryByKeywords(Dictionary<QueryKeywordEnum, string> queryParameters)
        {
            //获取本次查询缓存查询键
            string cacheKey = GenerateCacheKey(queryParameters);
            //查询缓存数据
            if (_queryCache.TryGetValue(cacheKey, out var cachedResult))
            {
                //若有缓存数据，则返回
                return cachedResult;
            }

            //若无缓存数据，则正常筛选
            IEnumerable<TValue> result = store.Values;

            //筛选
            foreach (var queryParameter in queryParameters)
            {
                //获取关键词与查询语句
                QueryKeywordEnum keyword = queryParameter.Key;
                string queryValue = queryParameter.Value;

                //进行筛选
                if (filters.TryGetValue(keyword, out var filter))
                {
                    result = filter(result, queryValue);
                }
                else
                {
                    //TODO:报错
                }
            }

            //记录缓存
            _queryCache[cacheKey] = result;
            //返回筛选结果
            return result;
        }
        //获取所有值
        public IEnumerable<TValue> GetAllData()
        {
            return store.Values;
        }
        //实现可遍历
        public IEnumerator<TValue> GetEnumerator()
        {
            return store.Values.GetEnumerator();
        }
        //索引
        public TValue this[TKey key]
        {
            get
            {
                return GetData(key);
            }
        }
        #endregion

        #region 其他方法
        //构建函数
        protected BaseGameDb()
        {
            //初始化数据
            store = new Dictionary<TKey, TValue>();
            filters = new Dictionary<QueryKeywordEnum, FilterDelegate<TValue>>();
            _queryCache = new Dictionary<string, IEnumerable<TValue>>();
            //初始化过滤器
            InitFilters();
        }
        //生成缓存Key
        private string GenerateCacheKey(Dictionary<QueryKeywordEnum, string> queryParameters)
        {
            return string.Join(";", queryParameters.Select(p => $"{p.Key}:{p.Value}"));
        }
        //初始化过滤器
        protected abstract void InitFilters();
        #endregion
    }
}

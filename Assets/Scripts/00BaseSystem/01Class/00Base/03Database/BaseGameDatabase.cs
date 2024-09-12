using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDatabase<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region 数据存储
        //基础数据存储
        protected Dictionary<TKey, TValue> BasicStorage { get; } = new();
        //过滤器存储
        protected Dictionary<QueryKeyWordEnum, FilterDelegate<TValue>> FilterStorage { get; } = new();
        //查询缓存
        protected Dictionary<string, IEnumerable<TValue>> QueryCache { get; } = new();
        #endregion

        #region 其他操作方法
        //关键字查询
        public virtual IEnumerable<TValue> QueryByKeywords(Dictionary<QueryKeyWordEnum, string> searchCriteria)
        {
            //获取本次查询缓存查询键
            string cacheKey = GetQueryCacheKey(searchCriteria);
            //查询缓存数据
            if (QueryCache.TryGetValue(cacheKey, out IEnumerable<TValue> cachedResult))
            {
                //若存在缓存，则返回
                return cachedResult;
            }

            //若无缓存数据，则正常筛选，创建结果
            IEnumerable<TValue> result = BasicStorage.Values;
            //筛选
            foreach (KeyValuePair<QueryKeyWordEnum, string> searchCriterion in searchCriteria)
            {
                //获取查询关键字与查询语句
                QueryKeyWordEnum queryKeyWord = searchCriterion.Key;
                string filterValue = searchCriterion.Value;

                //进行筛选
                if (FilterStorage.TryGetValue(queryKeyWord, out FilterDelegate<TValue> filter))
                {
                    //若有该筛选器，则使用该筛选器进行筛选
                    result = filter(result, filterValue);
                }
            }

            //记录缓存
            QueryCache[cacheKey] = result;
            //返回结果
            return result;
        }
        #endregion

        #region 辅助方法
        //获取查询缓存键
        public string GetQueryCacheKey(Dictionary<QueryKeyWordEnum, string> queryParameters)
        {
            return string.Join(";", queryParameters.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }
        //初始化筛选器
        protected abstract void InitFilter();
        #endregion

        #region IDictionary<TKey, TValue> 接口实现
        //键集合
        public ICollection<TKey> Keys => BasicStorage.Keys;
        //值集合
        public ICollection<TValue> Values => BasicStorage.Values;
        //计数
        public int Count => BasicStorage.Count;
        //只读属性
        public bool IsReadOnly => true;

        //索引器
        public virtual TValue this[TKey key]
        {
            get
            {
                return BasicStorage[key];
            }
            set
            {
                QueryCache.Clear();
                BasicStorage[key] = value;
            }
        }
        //尝试获取数据
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return BasicStorage.TryGetValue(key, out value);
        }

        //添加，不过提交的是键和值
        public virtual void Add(TKey key, TValue value)
        {
            QueryCache.Clear();
            BasicStorage.Add(key, value);
        }
        //添加，不过提交的是键值对
        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            QueryCache.Clear();
            BasicStorage.Add(item.Key, item.Value);
        }
        //移除，不过以键为参数
        public virtual bool Remove(TKey key)
        {
            QueryCache.Clear();
            return BasicStorage.Remove(key);
        }
        //移除，不过以键值对为参数
        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            QueryCache.Clear();
            return BasicStorage.Remove(item.Key);
        }
        //清除
        public virtual void Clear()
        {
            QueryCache.Clear();
            BasicStorage.Clear();
        }

        //判断是否包含键值对
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return BasicStorage.Contains(item);
        }
        //判断是否包含键
        public virtual bool ContainsKey(TKey key)
        {
            return BasicStorage.ContainsKey(key);
        }

        //复制到数组
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            //检测数组
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            //检测数组索引
            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            //检测数组长度
            if (array.Length - arrayIndex < BasicStorage.Count)
            {
                throw new ArgumentException("数组长度不足以容纳全部元素");
            }
            //复制
            foreach (var item in BasicStorage)
            {
                array[arrayIndex++] = item;
            }
        }

        //获取迭代器
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return BasicStorage.GetEnumerator();
        }
        //获取迭代器
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}

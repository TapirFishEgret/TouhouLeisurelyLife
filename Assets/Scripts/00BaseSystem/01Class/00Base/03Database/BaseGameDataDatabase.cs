﻿using System.Collections.Generic;
using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameDataDatabase<TData> : BaseGameDatabase<string, TData> where TData : BaseGameData
    {
        #region 新增存储及索引
        //作者索引
        protected Dictionary<string, HashSet<TData>> AuthorIndex { get; } = new();
        #endregion

        #region 方法重载
        //添加
        public override void Add(string key, TData data)
        {
            //基础方法
            base.Add(key, data);
            //额外存储
            if (!AuthorIndex.ContainsKey(data.Author))
            {
                AuthorIndex[data.Author] = new();
            }
            AuthorIndex[data.Author].Add(data);
        }
        public override void Add(KeyValuePair<string, TData> item)
        {
            //基础方法
            base.Add(item);
            //额外存储
            if (!AuthorIndex.ContainsKey(item.Value.Author))
            {
                AuthorIndex[item.Value.Author] = new();
            }
            AuthorIndex[item.Value.Author].Add(item.Value);
        }
        //移除
        public override bool Remove(string key)
        {
            //尝试获取数据
            if (BasicStorage.TryGetValue(key, out TData removedData))
            {
                //获得了，使用基类方法
                bool result = base.Remove(key);
                //再判断要不要移除索引中的数据
                if (result)
                {
                    //要，移除
                    result = AuthorIndex[removedData.Author].Remove(removedData);
                }
                return result;
            }
            return false;
        }
        public override bool Remove(KeyValuePair<string, TData> item)
        {
            //基础方法
            bool result = base.Remove(item);
            //额外移除
            if (result)
            {
                result = AuthorIndex[item.Value.Author].Remove(item.Value);
            }
            return result;
        }
        //清空
        public override void Clear()
        {
            //基础方法
            base.Clear();
            //额外清空作者存储字段
            AuthorIndex.Clear();
        }
        //初始化筛选器
        protected override void InitFilter()
        {
            FilterStorage[QueryKeyWordEnum.B_Author] = (source, queryValue) =>
            {
                //判断
                if (source.Count() == BasicStorage.Count)
                {
                    //若传入数据与总数据相等，说明此时在第一次查询，直接返回作者存储中的值
                    return AuthorIndex[queryValue];
                }
                else
                {
                    //若不相等，则正常筛选
                    return source.Where(item => item.Author == queryValue);
                }
            };
        }
        #endregion

        #region 新增方法
        //添加
        public void Add(TData data)
        {
            Add(data.ID, data);
        }
        //移除
        public bool Remove(TData data)
        {
            return Remove(data.ID);
        }
        //包含
        public bool Contains(TData data)
        {
            return ContainsKey(data.ID);
        }
        #endregion
    }
}
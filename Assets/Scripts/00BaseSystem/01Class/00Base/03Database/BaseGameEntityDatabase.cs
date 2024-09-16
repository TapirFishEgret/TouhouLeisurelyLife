using System.Collections.Generic;
using System.Linq;

namespace THLL.BaseSystem
{
    public abstract class BaseGameEntityDatabase<TData, TEntity> : BaseGameDatabase<string, TEntity> where TEntity : BaseGameEntity<TData> where TData : BaseGameData
    {
        #region 新增存储及索引
        //包索引
        protected Dictionary<string, HashSet<TEntity>> PackageIndex { get; } = new();
        //作者索引
        protected Dictionary<string, HashSet<TEntity>> AuthorIndex { get; } = new();
        #endregion

        #region 方法重载
        //添加
        public override void Add(string key, TEntity entity)
        {
            //基础方法
            base.Add(key, entity);
            //额外存储
            if (!PackageIndex.ContainsKey(entity.Package))
            {
                PackageIndex[entity.Package] = new();
            }
            if (!AuthorIndex.ContainsKey(entity.Author))
            {
                AuthorIndex[entity.Author] = new();
            }
            //添加索引
            PackageIndex[entity.Package].Add(entity);
            AuthorIndex[entity.Author].Add(entity);
        }
        public override void Add(KeyValuePair<string, TEntity> item)
        {
            //基础方法
            base.Add(item);
            //额外存储
            if (!PackageIndex.ContainsKey(item.Value.Package))
            {
                PackageIndex[item.Value.Package] = new();
            }
            if (!AuthorIndex.ContainsKey(item.Value.Author))
            {
                AuthorIndex[item.Value.Author] = new();
            }
            //添加索引
            PackageIndex[item.Value.Package].Add(item.Value);
            AuthorIndex[item.Value.Author].Add(item.Value);
        }
        //移除
        public override bool Remove(string key)
        {
            //尝试获取数据
            if (BasicStorage.TryGetValue(key, out TEntity removedEntity))
            {
                //获得了，使用基类方法
                bool result = base.Remove(key);
                //再判断要不要移除索引中的数据
                if (result)
                {
                    //要，移除
                    PackageIndex[removedEntity.Package].Remove(removedEntity);
                    result = AuthorIndex[removedEntity.Author].Remove(removedEntity);
                }
                return result;
            }
            return false;
        }
        public override bool Remove(KeyValuePair<string, TEntity> item)
        {
            //基础方法
            bool result = base.Remove(item);
            //额外移除
            if (result)
            {
                //移除索引
                PackageIndex[item.Value.Package].Remove(item.Value);
                result = AuthorIndex[item.Value.Author].Remove(item.Value);
            }
            return result;
        }
        //清空
        public override void Clear()
        {
            //基础方法
            base.Clear();
            //额外清空作者与包存储字段
            PackageIndex.Clear();
            AuthorIndex.Clear();
        }
        //初始化筛选器
        protected override void InitFilter()
        {
            //包筛选器
            FilterStorage[QueryKeyWordEnum.B_Package] = (source, queryValue) =>
            {
                //判断
                if (source.Count() == BasicStorage.Count)
                {
                    //若传入数据与总数据相等，说明此时在第一次查询，直接返回包存储中的值
                    return PackageIndex[queryValue];
                }
                else
                {
                    //若不相等，则正常筛选
                    return source.Where(item => item.Package == queryValue);
                }
            };
            //作者筛选器
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
        public void Add(TEntity entity)
        {
            Add(entity.ID, entity);
        }
        //移除
        public bool Remove(TEntity entity)
        {
            return Remove(entity.ID);
        }
        //包含
        public bool Contains(TEntity entity)
        {
            return ContainsKey(entity.ID);
        }
        #endregion
    }
}

﻿namespace THLL.BaseSystem
{
    public abstract class BaseGameEntityDatabase<TData, TEntity> : BaseGameDatabase<string, TEntity> where TEntity : BaseGameEntity<TData> where TData : BaseGameData
    {
        #region 新增方法
        //添加
        public virtual void Add(TEntity entity)
        {
            Add(entity.ID, entity);
        }
        //移除
        public virtual bool Remove(TEntity entity)
        {
            return Remove(entity.ID);
        }
        //包含
        public virtual bool Contains(TEntity entity)
        {
            return ContainsKey(entity.ID);
        }
        #endregion
    }
}

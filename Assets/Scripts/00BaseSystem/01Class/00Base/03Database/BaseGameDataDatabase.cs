namespace THLL.BaseSystem
{
    public abstract class BaseGameDataDatabase<TData> : BaseGameDatabase<string, TData> where TData : BaseGameData
    {
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

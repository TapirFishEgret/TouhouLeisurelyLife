namespace THLL.BaseSystem
{
    public abstract class BaseGameEntity<T> where T : BaseGameData
    {
        #region 基础实例数据成员
        //数据本身
        public T BaseData { get; private set; }
        //ID
        public string ID => BaseData.ID;
        //Name
        public string Name => BaseData.Name;
        //Des
        public string Description => BaseData.Description;
        //Author
        public string Author => BaseData.Author;
        //Package
        public string Package => BaseData.Package;
        //Category
        public string Category => BaseData.Category;
        //SortingOrder
        public int SortingOrder => BaseData.SortingOrder;
        #endregion

        #region 自身函数
        //构造函数
        public BaseGameEntity(T baseData)
        {
            BaseData = baseData;
        }
        #endregion
    }
}

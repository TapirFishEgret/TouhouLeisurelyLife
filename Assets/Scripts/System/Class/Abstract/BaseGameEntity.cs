namespace THLL.BaseSystem
{
    public abstract class BaseGameEntity<T> where T : BaseGameData
    {
        #region 基础实例数据成员
        //数据本身
        protected T baseData;
        //ID
        public string ID => baseData.ID;
        //Name
        public string Name => baseData.Name;
        //Des
        public string Description => baseData.Description;
        //Author
        public string Author => baseData.Author;
        //Package
        public string Package => baseData.Package;
        //Category
        public string Category => baseData.Category;
        #endregion

        #region 自身函数
        //构造函数
        public BaseGameEntity(T baseData)
        {
            this.baseData = baseData;
        }
        #endregion
    }
}

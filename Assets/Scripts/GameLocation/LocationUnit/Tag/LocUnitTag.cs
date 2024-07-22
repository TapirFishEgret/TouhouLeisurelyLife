using THLL.BaseSystem;

namespace THLL.LocationSystem.Tags
{
    public class LocUnitTag : BaseGameData
    {
        #region 静态数据
        //由于标签名称等应该根据类型走而不是实例走，故将部分数据设置为静态数据
        //作者
        protected static string tagAuthor;
        public override string Author => tagAuthor;
        //包
        protected static string tagPackage;
        public override string Package => tagPackage;
        //分类
        protected static string tagCategory;
        public override string Category => tagCategory;
        //名称
        protected static string tagName;
        public override string Name => tagName;
        //描述
        protected static string tagDescription;
        public override string Description => tagDescription;
        //次级分类
        protected static string subCategory;
        public string SubCategory => subCategory;
        #endregion

        #region 函数
        //静态构造函数
        static LocUnitTag()
        {
            tagAuthor = "TapirFishEgret";
            tagPackage = "Core";
            tagCategory = "LocUnitTag";
            subCategory = "基础";
            tagName = "基础标签";
            tagDescription = "基础标签，代码层面作为基类来使用，并无实际作用。";
        }
        //虚函数，应用标签
        public virtual void ApplyTag(LocUnit target, LocUnitDb globalData = null, LocUnitConnDb globalConnData = null)
        {
            //将标签添加到地点实例中去
            target.Tags.AddValue(this);
        }
        #endregion
    }
}

using System;

namespace THLL.BaseSystem
{
    public abstract class BaseGameEntity<TData> where TData : BaseGameData
    {
        #region 基础游戏数据
        //原数据
        protected TData Data { get; private set; }
        //数据路径
        protected string DataPath => Data.DataPath;
        //数据存储目录路径
        protected string DataDirectory => Data.DataDirectory;
        //ID
        public string ID => Data.ID;
        //名称
        public string Name => Data.Name;
        //描述
        public string Description => Data.Description;
        //排序
        public int SortOrder => Data.SortOrder;
        #endregion

        #region 构造函数及初始化
        //有参构造函数，传入文件路径版
        public BaseGameEntity(string filePath)
        {
            //从文件路径中读取数据
            TData data = BaseGameData.LoadFromJson<TData>(filePath);
            //对数据进行检测
            if (data is not TData rawData)
            {
                //若数据类型不匹配，则抛出异常
                throw new ArgumentException("传入数据类型不匹配");
            }
            //赋值原数据
            Data = rawData;
        }
        #endregion
    }
}

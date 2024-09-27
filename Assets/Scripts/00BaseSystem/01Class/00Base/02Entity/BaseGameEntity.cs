using System;
using System.IO;

namespace THLL.BaseSystem
{
    public abstract class BaseGameEntity<TData> where TData : BaseGameData
    {
        #region 基础游戏数据
        //原数据
        protected TData Data { get; private set; }
        //数据存储目录路径
        protected string DataDirectoryPath { get; private set; }
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
            //传入文件路径时，读取
            BaseGameData baseGameData = LoadDataFromXml(filePath);
            //设定数据存储目录路径
            DataDirectoryPath = Path.GetDirectoryName(filePath);
            //进行配置
            Configure(baseGameData);
        }

        //配置
        protected virtual void Configure(BaseGameData baseGameData)
        {
            //检测传入数据
            if (baseGameData is not TData data)
            {
                //若数据类型不匹配，则抛出异常
                throw new ArgumentException("传入数据类型不匹配");
            }
            //若数据类型匹配，赋值原数据
            Data = data;
        }
        #endregion

        #region 数据驱动设计方法
        //从XML文件中读取数据
        public virtual TData LoadDataFromXml(string filePath)
        {
            //读取数据
            return BaseGameData.LoadFromXML<TData>(filePath);
        }
        #endregion
    }
}

using System;
using System.IO;
using System.Xml.Serialization;

namespace THLL.BaseSystem
{
    public abstract class BaseGameEntity<TData> where TData : BaseGameData
    {
        #region 基础游戏数据
        //ID
        public string ID { get; set; }
        //名称
        public string Name { get; set; }
        //描述
        public string Description { get; set; }
        //排序
        public int SortOrder { get; set; }
        #endregion

        #region 基础其他数据
        //包
        public string Package { get; set; }
        //作者
        public string Author { get; set; }
        //版本
        public string Version { get; set; }
        //创建时间
        public DateTime CreateTime { get; set; }
        //更新时间
        public DateTime UpdateTime { get; set; }
        #endregion

        #region 构造函数及初始化
        //无参构造函数
        public BaseGameEntity()
        {
            //游戏数据
            ID = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            SortOrder = 0;
            //其他数据
            Package = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }
        //有参构造函数，传入数据版
        public BaseGameEntity(BaseGameData baseGameData)
        {
            //传入数据时，直接配置
            Init(baseGameData);
        }
        //有参构造函数，传入文件路径版
        public BaseGameEntity(string filePath)
        {
            //传入文件路径时，读取
            BaseGameData baseGameData = LoadFromXml(filePath);
            //进行初始化
            Init(baseGameData);
        }

        public virtual void Init(BaseGameData baseGameData)
        {
            //检测传入数据
            if (baseGameData.GetType() != typeof(TData))
            {
                //若数据类型不匹配，则抛出异常
                throw new ArgumentException("传入数据类型不匹配");
            }
            //赋值基础数据
            ID = baseGameData.ID;
            Name = baseGameData.Name;
            Description = baseGameData.Description;
            SortOrder = baseGameData.SortOrder;
            //然后赋值其他数据
            Package = baseGameData.Package;
            Author = baseGameData.Author;
            Version = baseGameData.Version;
            CreateTime = baseGameData.CreateTime;
            UpdateTime = baseGameData.UpdateTime;
        }
        #endregion

        #region 数据驱动设计方法
        //从XML文件中读取
        public TData LoadFromXml(string filePath)
        {
            //反序列化
            XmlSerializer xmlSerializer = new(typeof(TData));
            //创建文件流
            using StreamReader reader = new(filePath);
            //反序列化
            TData data = (TData)xmlSerializer.Deserialize(reader);
            //返回数据
            return data;
        }
        #endregion
    }
}

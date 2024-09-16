using System;
using System.IO;
using System.Xml.Serialization;

namespace THLL.BaseSystem
{
    public abstract class BaseGameData
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

        #region 构造函数
        //无参构造函数
        public BaseGameData()
        {
            ID = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            SortOrder = 0;

            Package = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }
        //有参构造函数
        public BaseGameData(string id, string name, string description, int sortOrder,string package, string author, string version)
        {
            ID = id;
            Name = name;
            Description = description;
            SortOrder = sortOrder;

            Package = package;
            Author = author;
            Version = version;
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
        }
        #endregion

        #region 数据驱动设计方法
        //保存为XML文件
        public void SaveToXML(string filePath)
        {
            //序列化对象
            XmlSerializer serializer = new(GetType());
            //创建文件流
            using StreamWriter writer = new(filePath);
            //写入文件
            serializer.Serialize(writer, this);
        }
        #endregion
    }
}

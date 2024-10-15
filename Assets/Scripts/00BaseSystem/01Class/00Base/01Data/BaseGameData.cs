using Newtonsoft.Json;
using System.IO;

namespace THLL.BaseSystem
{
    public abstract class BaseGameData
    {
        #region 基础游戏数据
        //ID
        [JsonProperty(Order = 1)]
        public string ID { get; set; }
        //ID分段
        [JsonProperty(Order = 2)]
        public string IDPart { get; set; }
        //名称
        [JsonProperty(Order = 3)]
        public string Name { get; set; }
        //描述
        [JsonProperty(Order = 4)]
        public string Description { get; set; }
        //排序
        [JsonProperty(Order = 5)]
        public int SortOrder { get; set; }
        #endregion

        #region 暂存
        //存储地址，为什么暂存呢，为了防止有小可爱移动文件
        [JsonIgnore]
        public string JsonFileSavePath { get; set; }
        #endregion

        #region 构造函数
        //无参构造函数
        public BaseGameData()
        {
            ID = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            SortOrder = 0;
        }
        //有参构造函数
        public BaseGameData(string id, string idPart, string name, string description, int sortOrder)
        {
            ID = id;
            IDPart = idPart;
            Name = name;
            Description = description;
            SortOrder = sortOrder;
        }
        #endregion

        #region 数据驱动设计方法
        //保存为JSON文件
        public static void SaveToJson<T>(T data, string filePath) where T : BaseGameData
        {
            //确定路径存在
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            //序列化对象
            JsonSerializer serializer = new() { Formatting = Formatting.Indented };
            //创建文件流
            using StreamWriter writer = new(filePath);
            //写入文件
            serializer.Serialize(writer, data);
        }
        //从JSON文件读取数据
        public static T LoadFromJson<T>(string filePath) where T : BaseGameData
        {
            //反序列化对象
            JsonSerializer serializer = new();
            //创建文件流
            using StreamReader reader = new(filePath);
            //读取文件
            T data = (T)serializer.Deserialize(reader, typeof(T));
            //返回数据
            return data;
        }
        #endregion
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace THLL.BaseSystem
{
    public abstract class BaseGameData
    {
        #region 基础游戏数据
        //ID
        [JsonProperty(Order = 1)]
        public string ID { get; set; } = string.Empty;
        //ID分段
        [JsonProperty(Order = 2)]
        public string IDPart { get; set; } = string.Empty;
        //名称
        [JsonProperty(Order = 3)]
        public string Name { get; set; } = string.Empty;
        //描述
        [JsonProperty(Order = 4)]
        public string Description { get; set; } = string.Empty;
        //排序
        [JsonProperty(Order = 5)]
        public int SortOrder { get; set; } = 0;
        //子数据列表
        [JsonProperty(Order = 6)]
        protected HashSet<string> SubDataFiles { get; set; } = new();

        //数据路径
        [JsonProperty(Order = 51)]
        public string DataPath { get; set; } = string.Empty;
        //数据目录
        [JsonIgnore]
        public string DataDirectory => Path.GetDirectoryName(DataPath);
        #endregion

        #region 数据驱动设计方法
        //抽象方法，获取子数据
        public abstract object GetSubData(string subDataFile);
        //抽象方法，获取子数据类型
        public abstract Type GetSubDataType(string subDataFile);
        //抽象方法，设置子数据
        public abstract void SetSubData(string subDataFile, object subData);

        //保存为JSON文件
        public static void SaveToJson<T>(T data, string filePath) where T : BaseGameData
        {
            //尝试读取数据
            try
            {
                //确定路径存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                //设定存储位置
                data.DataPath = filePath;
                //获取数据字符串
                string dataJson = JsonConvert.SerializeObject(data, Formatting.Indented);
                //写入文件
                File.WriteAllText(filePath, dataJson);

                //本体数据存储完成后检查是否有子数据
                if (data.SubDataFiles.Count > 0)
                {
                    //若有，遍历子数据列表
                    foreach (string subDataFile in data.SubDataFiles)
                    {
                        //获取子数据路径
                        string subDataFilePath = Path.Combine(data.DataDirectory, "SubData", subDataFile + ".json");
                        //确保路径存在
                        Directory.CreateDirectory(Path.GetDirectoryName(subDataFilePath));
                        //从本体数据中获取子数据
                        object subData = data.GetSubData(subDataFile);
                        //检测是否获取到数据
                        if (subData != null)
                        {
                            //若是，获取数据字符串
                            string subDataJson = JsonConvert.SerializeObject(subData, Formatting.Indented);
                            //写入子文件
                            File.WriteAllText(subDataFilePath, subDataJson);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //若出现异常，记录异常
                GameHistory.LogError("SaveToJson Error: " + filePath);
                //抛出异常
                throw;
            }
        }
        //从JSON文件读取数据
        public static T LoadFromJson<T>(string filePath) where T : BaseGameData
        {
            //尝试读取数据
            try
            {
                //加载主数据
                string dataJson = File.ReadAllText(filePath);
                T data = JsonConvert.DeserializeObject<T>(dataJson);
                //设定存储位置
                data.DataPath = filePath;

                //本体数据读取完成后检查是否有子数据
                if (data.SubDataFiles.Count > 0)
                {
                    //若有，遍历子数据列表
                    foreach (string subDataFile in data.SubDataFiles)
                    {
                        //获取子数据路径
                        string subDataFilePath = Path.Combine(data.DataDirectory, "SubData", subDataFile + ".json");
                        //检测子文件是否存在
                        if (File.Exists(subDataFilePath))
                        {
                            //若存在，获取JSON字符串数据
                            string subDataJson = File.ReadAllText(subDataFilePath);
                            //读取子文件
                            object subData = JsonConvert.DeserializeObject(subDataJson, data.GetSubDataType(subDataFile));
                            //设置子数据
                            data.SetSubData(subDataFile, subData);
                        }
                    }
                }

                //返回数据
                return data;
            }
            catch (Exception)
            {
                //若出现异常，记录异常
                GameHistory.LogError("LoadFromJson Error: " + filePath);
                //抛出异常
                throw;
            }
        }
        #endregion
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using THLL.SceneSystem;

namespace THLL.BaseSystem
{
    public class MapCellsDictConverter : JsonConverter
    {
        //实现CanConvert方法
        public override bool CanConvert(Type objectType)
        {
            //检查是否可以转换为特定类型的字典，类型为 Dictionary<(int, int), MapCell>
            return objectType == typeof(Dictionary<(int, int), MapCell>);
        }

        //实现ReadJson方法
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //创建一个新的字典实例
            var dictionary = new Dictionary<(int, int), MapCell>();
            //从 JSON 读取数据并加载为 JObject
            var jsonObject = JObject.Load(reader);

            //遍历 JObject 中的属性
            foreach (var property in jsonObject.Properties())
            {
                //获取并解析键，将字符串形式的元组转换为元组 (int, int)
                var key = property.Name.Trim('(', ')').Split(',');
                var tuple = (int.Parse(key[0]), int.Parse(key[1]));
                //将解析的键值对添加到字典中
                dictionary[tuple] = property.Value.ToObject<MapCell>(serializer);
            }

            //返回填充后的字典
            return dictionary;
        }

        //实现WriteJson方法
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //将对象转换为字典类型
            var dictionary = (Dictionary<(int, int), MapCell>)value;
            //开始写入 JSON 对象
            writer.WriteStartObject();

            //遍历字典中的键值对
            foreach (var kvp in dictionary)
            {
                //将元组键格式化为字符串，并写入属性名
                writer.WritePropertyName($"({kvp.Key.Item1},{kvp.Key.Item2})");
                //写入对应的值
                serializer.Serialize(writer, kvp.Value);
            }

            //结束 JSON 对象的写入
            writer.WriteEndObject();
        }
    }
}

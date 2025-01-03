﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace THLL.SceneSystem
{
    public class SceneData : BaseGameData
    {
        #region 数据
        //父级场景ID
        [JsonProperty(Order = 101)]
        public string ParentSceneID { get; set; } = string.Empty;
        //地图数据
        [JsonIgnore]
        public MapData MapData { get; set; } = new();
        #endregion

        #region 资源
        //场景背景图片字典
        [JsonIgnore]
        public Dictionary<string, Sprite> BackgroundsDict { get; set; } = new();
        #endregion

        #region 构造函数
        //无参
        public SceneData()
        {
            //将地图数据作为子数据添加到列表中
            SubDataFiles = new HashSet<string>() { "MapData" };
        }
        #endregion

        #region 数据读取相关
        //获取场景背景
        public IEnumerator LoadBackgroundsCoroutine(string dataDirectoryPath, Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //获取到目录
            string dir = Path.Combine(dataDirectoryPath, "Backgrounds");

            //获取目录下所有图片文件
            //首先获取所有文件
            string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            //然后进行遍历
            foreach (string filePath in files)
            {
                //判断是否是图片文件
                if (TouhouLeisurelyLife.ImageExtensions.Contains(Path.GetExtension(filePath)))
                {
                    //若是，获取文件名
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    //检测是否已经加载
                    if (BackgroundsDict.ContainsKey(fileName))
                    {
                        //若已经加载，触发事件
                        onBackgroundLoaded?.Invoke(fileName, BackgroundsDict[fileName]);
                        //跳过
                        continue;
                    }
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);
                    //等待
                    yield return request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("读取此处背景图片失败: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        BackgroundsDict[fileName] = sprite;

                        //触发事件
                        onBackgroundLoaded?.Invoke(fileName, sprite);
                    }
                }
            }
            //全部加载完成，触发事件
            onAllBackgroundsLoaded?.Invoke();
        }
        public async Task LoadBackgroundsAsync(string dataDirectoryPath, Action<string, Sprite> onBackgroundLoaded = null, Action onAllBackgroundsLoaded = null)
        {
            //获取到目录
            string dir = Path.Combine(dataDirectoryPath, "Backgrounds");

            //获取目录下所有图片文件
            //首先获取所有文件
            string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            //然后进行遍历
            foreach (string filePath in files)
            {
                //判断是否是图片文件
                if (TouhouLeisurelyLife.ImageExtensions.Contains(Path.GetExtension(filePath)))
                {
                    //若是，获取文件名
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    //检测是否已经加载
                    if (BackgroundsDict.ContainsKey(fileName))
                    {
                        //若已经加载，触发事件
                        onBackgroundLoaded?.Invoke(fileName, BackgroundsDict[fileName]);
                        //跳过
                        continue;
                    }
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(new Uri(filePath).AbsoluteUri);

                    //等待
                    await request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("读取此处背景图片失败: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        BackgroundsDict[fileName] = sprite;

                        //触发事件
                        onBackgroundLoaded?.Invoke(fileName, sprite);
                    }
                }
            }
            //全部加载完成，触发事件
            onAllBackgroundsLoaded?.Invoke();
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //直接将对应资源标记为空
            BackgroundsDict.Clear();
        }
        #endregion

        #region 数据驱动相关
        //实现获取子数据方法
        public override object GetSubData(string subDataFile)
        {
            //获取前对数据进行保存
            MapData.GeneratePreview();
            //检测传入的数据名
            return subDataFile switch
            {
                "MapData" => MapData,
                _ => null,
            };
        }
        //实现获取子数据类型方法
        public override Type GetSubDataType(string subDataFile)
        {
            //检测传入的数据名并返回类型
            return subDataFile switch
            {
                "MapData" => typeof(MapData),
                _ => null,
            };
        }
        //实现设置子数据方法
        public override void SetSubData(string subDataFile, object subData)
        {
            //检测传入的数据名并赋值
            switch (subDataFile)
            {
                case "MapData":
                    MapData = subData as MapData;
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}

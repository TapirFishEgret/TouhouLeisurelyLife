using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace THLL.CharacterSystem
{
    public class CharacterData : BaseGameData
    {
        #region 数据
        //角色所属作品名
        [JsonProperty(Order = 101)]
        public string Series { get; set; }
        //角色所属组织名
        [JsonProperty(Order = 102)]
        public string Group { get; set; }
        //角色名
        [JsonProperty(Order = 103)]
        public string Chara { get; set; }
        //版本名
        [JsonProperty(Order = 104)]
        public string Version { get; set; }

        //颜色
        [JsonProperty(Order = 105)]
        public string ColorString { get; set; }
        #endregion

        #region 资源
        //头像字典
        [JsonIgnore]
        public Dictionary<string, Sprite> AvatarsDict { get; set; } = new();
        //立绘字典
        [JsonIgnore]
        public Dictionary<string, Sprite> PortraitsDict { get; set; } = new();
        #endregion

        #region 构造函数
        //无参
        public CharacterData()
        {
            Series = string.Empty;
            Group = string.Empty;
            Chara = string.Empty;
            Version = string.Empty;
            ColorString = string.Empty;
        }
        #endregion

        #region 资源加载相关方法
        //获取头像
        public IEnumerator LoadAvatarsCoroutine(string directoryPath, Action<string, Sprite> onAvatarLoaded = null, Action onAllAvatarsLoaded = null)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Avatars");

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
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);
                    //等待
                    yield return request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("Failed to load avatar image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        AvatarsDict[fileName] = sprite;

                        //触发事件
                        onAvatarLoaded?.Invoke(fileName, sprite);
                    }
                }
            }
            //所有图片加载完成后触发事件
            onAllAvatarsLoaded?.Invoke();
        }
        public async Task LoadAvatarsAsync(string directoryPath, Action<string, Sprite> onAvatarLoaded = null, Action onAllAvatarsLoaded = null)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Avatars");

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
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);

                    //等待请求完成
                    await request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("Failed to load avatar image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        AvatarsDict[fileName] = sprite;

                        //触发事件
                        onAvatarLoaded?.Invoke(fileName, sprite);
                    }
                }
            }
            //所有图片加载完成后触发事件
            onAllAvatarsLoaded?.Invoke();
        }
        //获取立绘
        public IEnumerator LoadPortraitsCoroutine(string directoryPath, Action<string, Sprite> onPortraitLoaded = null, Action onAllPortraitsLoaded = null)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Portraits");

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
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);
                    //等待
                    yield return request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("Failed to load portrait image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        PortraitsDict[fileName] = sprite;

                        //触发事件
                        onPortraitLoaded?.Invoke(fileName, sprite);
                    }
                }
            }
            //所有图片加载完成后触发事件
            onAllPortraitsLoaded?.Invoke();
        }
        public async Task LoadPortraitsAsync(string directoryPath, Action<string, Sprite> onPortraitLoaded = null, Action onAllPortraitsLoaded = null)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Portraits");

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
                    //然后，启动请求加载图片
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);

                    //等待
                    await request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("Failed to load portrait image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        PortraitsDict[fileName] = sprite;

                        //触发事件
                        onPortraitLoaded?.Invoke(fileName, sprite);
                    }
                }
            }
            //所有图片加载完成后触发事件
            onAllPortraitsLoaded?.Invoke();
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //将资源设定为空
            AvatarsDict.Clear();
            PortraitsDict.Clear();
        }
        #endregion

        #region 数据驱动相关
        //实现获取子数据方法
        public override object GetSubData(string subDataFile)
        {
            throw new NotImplementedException();
        }
        //实现获取子数据类型方法
        public override Type GetSubDataType(string subDataFile)
        {
            throw new NotImplementedException();
        }
        //实现设置子数据方法
        public override void SetSubData(string subDataFile, object subData)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

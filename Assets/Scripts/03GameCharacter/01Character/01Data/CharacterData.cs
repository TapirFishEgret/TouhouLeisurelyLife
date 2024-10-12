using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace THLL.CharacterSystem
{
    public class CharacterData : BaseGameData
    {
        #region 数据
        //角色所属作品名
        public string Series { get; set; }
        //角色所属组织名
        public string Group { get; set; }
        //角色名
        public string Chara { get; set; }
        //版本名
        public string Version { get; set; }

        //颜色
        public Color Color { get; set; }
        #endregion

        #region 资源
        //头像字典
        [XmlIgnore]
        public Dictionary<string, Sprite> AvatarsDict { get; set; } = new();
        //立绘字典
        [XmlIgnore]
        public Dictionary<string, Sprite> ProtraitsDict { get; set; } = new();
        #endregion

        #region 构造函数
        //无参
        public CharacterData()
        {

        }
        //有参
        public CharacterData(string id, string idPart, string name, string description, int sortOrder, string series, string group, string chara, string version, Color color)
            : base(id, idPart, name, description, sortOrder)
        {
            Series = series;
            Group = group;
            Chara = chara;
            Version = version;
            Color = color;
        }
        #endregion

        #region 资源加载相关方法
        //获取头像
        public IEnumerator LoadAvatarsCoroutine(string directoryPath)
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
                        AvatarsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        public async Task LoadAvatarsAsync(string directoryPath)
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
                        AvatarsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        //获取立绘
        public IEnumerator LoadProtraitsCoroutine(string directoryPath)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Protraits");

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
                        GameHistory.LogError("Failed to load protrait image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        ProtraitsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        public async Task LoadPortraitsAsync(string directoryPath)
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
                        GameHistory.LogError("Failed to load protrait image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        ProtraitsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //将资源设定为空
            AvatarsDict.Clear();
            ProtraitsDict.Clear();
        }
        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace THLL.CharacterSystem
{
    public class Character : BaseGameEntity<CharacterData>
    {
        #region 数据
        //系列
        public string Series => Data.Series;
        //组织
        public string Group => Data.Group;
        //版本
        public string Version => Data.Version;
        #endregion

        #region 资源
        //头像字典
        public Dictionary<string, Sprite> AvatarsDict { get; private set; } = new();
        //立绘字典
        public Dictionary<string, Sprite> ProtraitsDict { get; private set; } = new();
        #endregion

        #region 初始化及相关方法
        //有参构造函数
        public Character(string filePath, CharacterDatabase characterDb) : base(filePath)
        {
            //调用重载后的配置函数
            Configure(characterDb);
        }
        //配置函数重载
        protected void Configure(CharacterDatabase characterDb)
        {
            //首先完善系列索引
            if (!characterDb.SeriesIndex.ContainsKey(Series))
            {
                characterDb.SeriesIndex.Add(Series, new HashSet<Character>());
            }
            characterDb.SeriesIndex[Series].Add(this);
            //然后完善组织索引
            if (!characterDb.GroupIndex.ContainsKey(Group))
            {
                characterDb.GroupIndex.Add(Group, new HashSet<Character>());
            }
            characterDb.GroupIndex[Group].Add(this);
        }
        #endregion

        #region 资源相关方法
        //获取头像
        public IEnumerator LoadAvatars()
        {
            //获取到目录
            string dir = Path.Combine(DataDirectoryPath, "Avatars");

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
        //获取立绘
        public IEnumerator LoadProtraits()
        {
            //获取到目录
            string dir = Path.Combine(DataDirectoryPath, "Protraits");

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

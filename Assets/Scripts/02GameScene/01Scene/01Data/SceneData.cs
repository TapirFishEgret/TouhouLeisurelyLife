using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace THLL.SceneSystem
{
    public class SceneData : BaseGameData
    {
        #region 数据
        //父级场景ID
        public string ParentSceneID { get; set; }
        //子级场景间路径列表
        public List<ScenePathData> ChildScenePathDatas { get; set; }
        #endregion

        #region 资源
        //场景背景图片字典
        [XmlIgnore]
        public Dictionary<string, Sprite> BackgroundsDict { get; set; } = new();
        #endregion

        #region 构造函数
        //无参
        public SceneData()
        {

        }
        //有参
        public SceneData(string id, string idPart, string name, string description, int sortOrder, string parentSceneID)
            : base(id, idPart, name, description, sortOrder)
        {
            ParentSceneID = parentSceneID;
        }
        #endregion

        #region 数据读取相关
        //获取场景背景
        public IEnumerator LoadBackgrounds(string directoryPath)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Backgrounds");

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
                        GameHistory.LogError("Failed to load background image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        BackgroundsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        public async Task LoadBackgroundsAsync(string directoryPath)
        {
            //获取到目录
            string dir = Path.Combine(directoryPath, "Backgrounds");

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
                    UnityWebRequest request = UnityWebRequestTexture.GetTexture(new Uri(filePath).AbsoluteUri);

                    //等待
                    await request.SendWebRequest();

                    //等待结束后判断结果
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        //若未成功，游戏内记录异常
                        GameHistory.LogError("Failed to load background image: " + filePath);
                    }
                    else
                    {
                        //若成功，获取到Texture2D
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        //创建Sprite
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //添加到字典中
                        BackgroundsDict.Add(fileName, sprite);
                    }
                }
            }
        }
        //卸载所有资源
        public void UnloadAllResources()
        {
            //直接将对应资源标记为空
            BackgroundsDict.Clear();
        }
        #endregion
    }
}

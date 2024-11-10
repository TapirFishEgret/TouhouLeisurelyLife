using System.Collections.Generic;
using System.IO;
using THLL.CharacterSystem;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;

namespace THLL.EditorSystem
{
    public static class GameEditor
    {
        #region 游戏数据存储
        //场景数据是否需要被重建
        public static bool IsSceneDataDirty { get; set; } = true;
        //场景数据存储
        private static Dictionary<string, SceneData> _sceneDataDict;
        public static Dictionary<string, SceneData> SceneDataDict
        {
            get
            {
                //检测是否需要重建
                if (IsSceneDataDirty)
                {
                    //若需要，则重建
                    ReadSceneData();
                    //重建完成后清除标记
                    IsSceneDataDirty = false;
                }
                //返回场景数据字典
                return _sceneDataDict;
            }
        }
        //角色数据是否需要被重建
        public static bool IsCharacterDataDirty { get; set; } = true;
        //角色数据存储
        private static Dictionary<string, CharacterData> _characterDataDict;
        public static Dictionary<string, CharacterData> CharacterDataDict
        {
            get
            {
                //检测是否需要重建
                if (IsCharacterDataDirty)
                {
                    //若需要，则重建
                    ReadCharacterData();
                    //重建完成后清除标记
                    IsCharacterDataDirty = false;
                }
                //返回角色数据字典
                return _characterDataDict;
            }
        }
        #endregion

        #region 数据读取相关方法
        //场景数据读取
        private static void ReadSceneData()
        {
            //初始化场景数据字典
            _sceneDataDict = new Dictionary<string, SceneData>();

            //数据存储路径
            string rootPath = Path.Combine(Application.streamingAssetsPath, "Scene");
            //确认路径存在
            MakeSureFolderPathExist(rootPath);

            //读取所有场景数据
            try
            {
                //获取所有文件
                string[] filePaths = Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories);
                //遍历所有文件
                foreach (string filePath in filePaths)
                {
                    //检测是否为目标数据文件
                    if (Path.GetFileNameWithoutExtension(filePath).StartsWith("SceneData"))
                    {
                        //若是，读取数据
                        SceneData sceneData = SceneData.LoadFromJson<SceneData>(filePath);
                        //设定读取地址
                        sceneData.DataPath = filePath.Replace("\\", "/");
                        //添加到字典
                        _sceneDataDict[sceneData.ID] = sceneData;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("读取场景数据失败！" + e.Message);
            }
        }
        //角色数据读取
        private static void ReadCharacterData()
        {
            //初始化角色数据字典
            _characterDataDict = new Dictionary<string, CharacterData>();

            //数据存储路径
            string rootPath = Path.Combine(Application.streamingAssetsPath, "Character");
            //确认路径存在
            MakeSureFolderPathExist(rootPath);

            //读取所有角色数据
            try
            {
                //获取所有文件
                string[] filePaths = Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories);
                //遍历所有文件
                foreach (string filePath in filePaths)
                {
                    //检测是否为目标数据文件
                    if (Path.GetFileNameWithoutExtension(filePath).StartsWith("CharacterData"))
                    {
                        //若是，读取数据
                        CharacterData characterData = CharacterData.LoadFromJson<CharacterData>(filePath);
                        //设定读取地址
                        characterData.DataPath = filePath.Replace("\\", "/");
                        //添加到字典
                        _characterDataDict[characterData.ID] = characterData;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("读取角色数据失败！" + e.Message);
            }
        }
        #endregion

        #region 文件相关方法
        //确保目标路径存在
        public static bool MakeSureFolderPathExist(string folderPath, bool isUnityEditorFolder = false)
        {
            //检测路径类型
            if (isUnityEditorFolder)
            {
                //若是Unity编辑器路径，使用相关方法创建文件夹
                //检查路径是否存在
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    //若不存在，则进行生成
                    //分割路径
                    string[] folders = folderPath.Split("\\");
                    string currentPath = string.Empty;

                    //逐级检查并创建文件夹
                    for (int i = 0; i < folders.Length; i++)
                    {
                        //获取文件夹
                        string folder = folders[i];
                        //判断是否直接在根目录下
                        if (i == 0 && folder == "Assets")
                        {
                            //若是，指定当前路径为根目录
                            currentPath = folder;
                            //并跳过此次循环
                            continue;
                        }
                        //生成新路径
                        string newPath = Path.Combine(currentPath, folder);
                        //判断新路径是否存在
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            //若不存在，则创建
                            AssetDatabase.CreateFolder(currentPath, folder);
                        }
                        //指定当前路径为新路径
                        currentPath = newPath;
                    }
                    //检查结束后保存
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    //创建结束后表明该文件夹本来不存在，返回false
                    return false;
                }
                else
                {
                    //若已存在该文件夹，则返回true
                    return true;
                }
            }
            else
            {
                //若是普通路径，则直接判断
                if (!Directory.Exists(folderPath))
                {
                    //若不存在，则创建
                    Directory.CreateDirectory(folderPath);
                    //保存更改
                    AssetDatabase.SaveAssets();
                    //检查结束后刷新
                    AssetDatabase.Refresh();
                    //创建结束后表明该文件夹本来不存在，返回false
                    return false;
                }
                else
                {
                    //若已存在该文件夹，则返回true
                    return true;
                }
            }
        }
        //通过递归完全删除文件夹
        public static void DeleteFolder(string folderPath, bool isUnityEditorFolder = false)
        {
            //检测路径类型
            if (isUnityEditorFolder)
            {
                //若是Unity编辑器路径，使用相关方法删除文件夹
                //获取文件夹中的所有文件和子文件夹GUID
                string[] asseGUIDs = AssetDatabase.FindAssets("", new[] { folderPath });

                //针对获取的所有路径
                foreach (string assetGUID in asseGUIDs)
                {
                    //确认路径
                    string path = AssetDatabase.GUIDToAssetPath(assetGUID);

                    if (AssetDatabase.IsValidFolder(path))
                    {
                        //若是文件夹，则进行递归删除
                        DeleteFolder(path);
                    }
                    else
                    {
                        //若是文件，则删除
                        AssetDatabase.DeleteAsset(path);
                    }

                    //最后删除空文件夹
                    AssetDatabase.DeleteAsset(folderPath);
                }

                //保存更改
                AssetDatabase.SaveAssets();
                //刷新资源视图
                AssetDatabase.Refresh();
            }
            else
            {
                //若不是，获取目录名称
                string folderName = Path.GetFileName(folderPath);
                //删除meta文件，我也不知道为什么Unity要在StreamingAssets里也创建Meta文件
                File.Delete(Path.Combine(Path.GetDirectoryName(folderPath), folderName + ".meta"));
                //删除该目录
                Directory.Delete(folderPath, true);
                //保存更改
                AssetDatabase.SaveAssets();
                //刷新资源视图
                AssetDatabase.Refresh();
            }
        }
        //生成占位文本文档
        public static void GeneratePlaceHolderTextFile(string directoryPath)
        {
            //确保路径存在
            MakeSureFolderPathExist(directoryPath);
            //生成文件
            string filePath = Path.Combine(directoryPath, "PlaceHolder.txt");
            File.WriteAllText(filePath,
                "这是一个占位文件。" +
                "\n主要是为了防止Unity在构建过程中自动删除StreamingAssets中的空文件夹" +
                "\nThis file is a placeholder." +
                "\nIt is mainly used to prevent Unity from automatically deleting empty folders in StreamingAssets during the build process.");
            //保存更改
            AssetDatabase.SaveAssets();
            //刷新资源视图
            AssetDatabase.Refresh();
        }
        #endregion
    }
}

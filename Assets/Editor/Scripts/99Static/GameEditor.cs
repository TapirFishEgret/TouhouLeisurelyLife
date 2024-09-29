using System.Collections.Generic;
using System.IO;
using THLL.CharacterSystem;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem
{
    public static class GameEditor
    {
        #region 游戏数据存储
        //角色数据存储
        public static Dictionary<string, CharacterData> CharacterDataDict { get; } = new();
        //场景数据存储
        public static Dictionary<string, SceneData> SceneDataDict { get; } = new();
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

        #region 视觉元素功能扩展
        //Label视觉元素实现字体自动缩放
        public static void SingleLineLabelAdjustFontSizeToFit(Label label)
        {
            //首先检测传入
            if (label == null
                || label.text == null
                || label.style.display == DisplayStyle.None
                || float.IsNaN(label.resolvedStyle.width)
                || float.IsNaN(label.resolvedStyle.height))
            {
                //若无传入或者传入Label中无文本或Label压根儿就没显示或高和宽为NaN，则返回
                return;
            }

            //记录循环次数
            int count = 0;
            //然后计算文本长度
            Vector2 textSize = label.MeasureTextSize(
                label.text,
                float.MaxValue,
                VisualElement.MeasureMode.Undefined,
                float.MaxValue,
                VisualElement.MeasureMode.Undefined);
            //获取Label宽度
            float labelWidth = label.resolvedStyle.width;
            float labelHeight = label.resolvedStyle.height;
            //循环缩放以调整字体大小
            while (true)
            {
                //当前字体大小
                float currentFontSize = label.resolvedStyle.fontSize;
                //调整后字体大小
                float adjustedFontSize = currentFontSize;
                //检测文本宽度与Label宽度关系
                if (textSize.x > labelWidth)
                {
                    //若大于，则缩小
                    adjustedFontSize = currentFontSize - 0.1f;
                }
                else if (textSize.x < labelWidth && textSize.y < labelHeight)
                {
                    //若文本宽度小于当前宽度，且文本高度同样小于，则放大
                    adjustedFontSize = currentFontSize + 0.1f;
                }
                //设置字体大小
                label.style.fontSize = adjustedFontSize;
                //重新测量长度
                textSize = label.MeasureTextSize(
                    label.text,
                    float.MaxValue,
                    VisualElement.MeasureMode.Undefined,
                    float.MaxValue,
                    VisualElement.MeasureMode.Undefined);
                //检测是否满足跳出循环条件
                //两种情形，宽度近似而高度小，宽度小而高度近似
                if ((Mathf.Abs(textSize.x - labelWidth) < 4f && textSize.y < labelHeight)
                    || (Mathf.Abs(textSize.y - labelHeight) < 4f && textSize.x < labelWidth))
                {
                    //若满足跳出循环条件，则跳出
                    break;
                }
                //循环次数增加
                count++;
                //检测
                if (count > 100)
                {
                    //若循环次数过多，结束调整
                    break;
                }
            }
        }
        #endregion
    }
}

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace THLL.GameEditor
{
    public static class EditorExtensions
    {
        //获取文件资源的GUID的哈希值
        public static int GetAssetHashCode(this UnityEngine.Object asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "资源不能为空");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException("资源地址不可用", nameof(asset));
            }

            GUID guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            return guid.GetHashCode();
        }
        //确保目标路径存在
        public static bool MakeSureFolderPathExist(string folderPath)
        {
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
        //通过递归完全删除文件夹
        public static void DeleteFolder(string folderPath)
        {
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
        //确保目标资源包存在
        public static AddressableAssetGroup GetAddressableGroup(string groupName, string buildPath, string loadPath)
        {
            //创建返回结果
            AddressableAssetGroup group;

            //获取可寻址资源设置
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            //尝试获取包
            group = settings.FindGroup(groupName);

            //检测获取情况
            if (group == null)
            {
                //若无获取，则进行创建
                //设定构建及读取变量在设置中对应的变量名
                string buildPathVariable = groupName + "_" + "BuildPath";
                string loadPathVariable = groupName + "_" + "LoadPath";
                //设置
                SetProfileVariable(settings, buildPathVariable, buildPath);
                SetProfileVariable(settings, loadPathVariable, loadPath);
                //随后创建新资源组
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
                //配置组的参数
                //打包参数
                BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
                if (bundledSchema != null)
                {
                    //设置自定义构建路径与加载路径
                    bundledSchema.BuildPath.SetVariableByName(settings, buildPathVariable);
                    bundledSchema.LoadPath.SetVariableByName(settings, loadPathVariable);
                    //设置打包模式为分开打包
                    bundledSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
                    //设置自定义命名策略
                    bundledSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
                }
                //内容参数
                ContentUpdateGroupSchema contentSchema = group.GetSchema<ContentUpdateGroupSchema>();
                if (contentSchema != null)
                {
                    //设置为非静态资源
                    contentSchema.StaticContent = false;
                }

                //创建结束后保存并刷新
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //返回结果
            return group;
        }
        //设定文档变量
        private static void SetProfileVariable(AddressableAssetSettings settings, string variableName, string value)
        {
            //获取当前Profile的值
            AddressableAssetProfileSettings profileSettings = settings.profileSettings;
            string currentProfileID = settings.activeProfileId;
            //确认变量是否存在
            if (!profileSettings.GetVariableNames().Contains(variableName))
            {
                //若不存在，创建
                profileSettings.CreateValue(variableName, value);
            }
            //设置路径变量值
            profileSettings.SetValue(currentProfileID, variableName, value);
        }
    }
}

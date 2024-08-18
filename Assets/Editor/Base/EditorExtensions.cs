using System;
using System.IO;
using THLL.BaseSystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor
{
    public static class EditorExtensions
    {
        #region 数据
        //存放了资源组信息的资源组
        public static AddressableAssetGroup AssetGroupInfoGroup { get; private set; }
        #endregion

        #region 文件相关
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
        #endregion

        #region 可寻址资源包相关
        //确保目标资源包存在
        public static AddressableAssetGroup GetAssetGroup(string groupName, string buildPath, string loadPath)
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
        //获取资源组对应的信息文件
        public static AssetGroupInfo GetAssetGroupInfo(AddressableAssetGroup group, GameAssetTypeEnum assetType)
        {
            //创建返回结果
            AssetGroupInfo assetGroupInfo = null;

            //确保当前资源组信息组存在
            if (AssetGroupInfoGroup == null)
            {
                //组名
                string groupName = "AssetGroupInfo";
                //确认构建路径，此处采用可寻址资源包内置路径变量
                string buildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/AssetGroupInfo";
                //确认读取路径
                string loadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/AssetGroupInfo";
                //获取
                AssetGroupInfoGroup = GetAssetGroup(groupName, buildPath, loadPath);
            }

            //确保当前资源组信息组存在后，尝试获取信息资源
            //根据当前组名寻找对应的信息文件
            string[] guids = AssetDatabase.FindAssets($"t:AssetGroupInfo {group.Name}_Info", new[] { "Assets/GameData" });
            //检测获取到的数量
            if (guids.Length == 0)
            {
                //若为0，说明没有对应资源，进行创建
                AssetGroupInfo newAssetGroupInfo = ScriptableObject.CreateInstance<AssetGroupInfo>();
                //并设定类型
                newAssetGroupInfo.AssetType = assetType;
                //更改文件名
                newAssetGroupInfo.name = group.Name + "_Info";
                //获取文件夹地址
                string newFolderPath = "Assets\\GameData\\AssetGroupInfos";
                //确保文件夹地址存在
                MakeSureFolderPathExist(newFolderPath);
                //获取文件地址
                string newFilePath = (newFolderPath + $"\\{group.Name}_Info.asset").Replace("\\", "/");
                //创建资源
                AssetDatabase.CreateAsset(newAssetGroupInfo, newFilePath);
                //保存
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //由于是新建的资源，所以将其添加入信息组中
                AddressableAssetEntry entry = AddressableAssetSettingsDefaultObject
                    .GetSettings(true)
                    .CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(newFilePath), AssetGroupInfoGroup);
                //并设定索引名称
                entry.SetAddress(group.Name + "_Info");
                //并设定标签
                entry.SetLabel("AssetGroupInfo", true, true);
                //保存
                AssetGroupInfoGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                AssetDatabase.SaveAssets();

                //指定结果
                assetGroupInfo = newAssetGroupInfo;
            }
            else if (guids.Length == 1)
            {
                //若等于1，说明没问题，加载
                assetGroupInfo = AssetDatabase.LoadAssetAtPath<AssetGroupInfo>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            else
            {
                //其他情况说明出毛病了，检查一下吧
                Debug.LogWarning("当前资源组对应多个信息文件，请检查。");
            }

            //返回结果
            return assetGroupInfo;
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.LocationSystem;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class MainWindow : EditorWindow
    {
        #region 基础构成
        //自身UXML文件
        [SerializeField]
        private VisualTreeAsset _windowVisualTree;
        //数据编辑面板UXML文件
        [SerializeField]
        private VisualTreeAsset _dataEditorVisualTree;
        //永久性存储文件
        [SerializeField]
        private TextAsset _persistentDataFile;
        public TextAsset PersistentDataFile => _persistentDataFile;
        //默认地点背景图
        [SerializeField]
        private Sprite _defaultLocationBackground;
        public Sprite DefaultLocationBackground => _defaultLocationBackground;

        //UI元素
        //左侧面板
        //树形图
        public DataTreeView DataTreeView { get; private set; }
        //包输入框
        public TextField DefaultPackageField { get; private set; }
        //作者输入框
        public TextField DefaultAuthorField { get; private set; }
        //计时器Debug面板显示开关
        public Toggle TimerDebugLogToggle { get; private set; }
        //右侧面板
        //多标签页面板
        public TabView MultiTabView { get; private set; }
        //数据编辑面板
        public DataEditorPanel DataEditorPanel { get; private set; }
        //连接编辑面板
        public NodeEditorPanel NodeEditorPanel { get; private set; }

        //数据存储
        //需要进行重命名的数据
        public HashSet<LocUnitData> DataNeedToReGenerateFullNameCache { get; private set; }

        //窗口菜单
        [MenuItem("GameEditor/LocationSystem/Location")]
        public static void ShowWindow()
        {
            //窗口设置
            MainWindow window = GetWindow<MainWindow>("Location Unit Editor Window");
            window.position = new Rect(100, 100, 1440, 810);
        }
        #endregion

        #region UI生命周期
        //创建UI
        public void CreateGUI()
        {
            //加载UXML文件
            _windowVisualTree.CloneTree(rootVisualElement);

            //初始化数据
            DataNeedToReGenerateFullNameCache = new();

            //其他控件
            //获取
            DefaultPackageField = rootVisualElement.Q<TextField>("DefaultPackageField");
            DefaultAuthorField = rootVisualElement.Q<TextField>("DefaultAuthorField");
            TimerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            VisualElement dataTreeViewContainer = rootVisualElement.Q<VisualElement>("DataTreeViewContainer");
            MultiTabView = rootVisualElement.Q<TabView>("EditorContainer");

            //设置标签页面容器为可延展
            MultiTabView.contentContainer.style.flexGrow = 1;
            MultiTabView.contentContainer.style.flexShrink = 1;

            //读取持久化数据
            LoadPersistentData();

            //左侧面板
            //创建树形图面板并添加
            DataTreeView = new DataTreeView(this);
            dataTreeViewContainer.Add(DataTreeView);

            //右侧面板
            //创建数据编辑面板并添加
            DataEditorPanel = new DataEditorPanel(_dataEditorVisualTree, this);
            MultiTabView.Add(DataEditorPanel);
            //创建连接面板并添加
            NodeEditorPanel = new NodeEditorPanel(this);
            MultiTabView.Add(NodeEditorPanel);
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //手动对标记为需要进行的数据的全名的重新生成
            foreach (LocUnitData data in DataNeedToReGenerateFullNameCache)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(data)))
                {
                    data.Editor_GenerateFullName();
                }
            }
            //结束后清除数据
            DataNeedToReGenerateFullNameCache.Clear();
            //保存持久化数据到磁盘
            SavePersistentData();
            //提醒修改可寻址资源包标签
            Debug.LogWarning("窗口已被关闭，请注意修改新增数据的可寻址资源包的Key。");
        }
        #endregion

        #region 编辑器面板下的地点数据增删改方法
        //编辑器面板下增加地点数据
        public void CreateLocUnitDataFile(LocUnitData parentData, string newDataName, LocUnitData originalData = null)
        {
            //检测重名状况
            //获取父级地点的地址
            string parentDataFolder = GetDataFolderPath(parentData);
            //获取子级地点的地址
            string newDataFolder = Path.Combine(parentDataFolder, newDataName).Replace("\\", "/");
            //检测子级地点地址是否已经存在
            if (AssetDatabase.IsValidFolder(newDataFolder))
            {
                //若已经存在，发出警告并更改名称
                Debug.LogWarning("文件创建警告：新文件与同级目录下其他文件重名！");
                newDataName = "需要重命名_" + newDataName;
            }

            //在磁盘上创建资源
            //创建新资源
            LocUnitData newData = CreateInstance<LocUnitData>();
            //检测原文件数据
            if (originalData != null)
            {
                //若不为空，则继承其数据
                originalData.Editor_CopyTo(newData);
            }
            else
            {
                //若为空，正常设置所需要的元素
                newData.Editor_SetName(newDataName);
                newData.Editor_SetPackage(DefaultPackageField.text);
                newData.Editor_SetCategory("Location");
                newData.Editor_SetAuthor(DefaultAuthorField.text);
                newData.Editor_SetBackground(DefaultLocationBackground);
            }
            //更改文件名
            newData.name = newDataName;
            //更改父级
            newData.Editor_SetParent(parentData);
            //生成全名
            newData.Editor_GenerateFullName();
            //获取资源文件夹地址
            string newDataPath = Path.Combine(GetDataFolderPath(newData), $"{newDataName}.asset").Replace("\\", "/");
            //新建资源
            AssetDatabase.CreateAsset(newData, newDataPath);

            //保存文件更改
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //处理缓存数据
            //创建新实例对应的树形图数据
            List<TreeViewItemData<LocUnitData>> newChildren = new();
            TreeViewItemData<LocUnitData> newItem = new(newData.GetAssetHashCode(), newData, newChildren);
            //判断选中项是否为空
            if (parentData != null)
            {
                //当选中项不为空时，新数据作为被选中的数据的子级被添加
                DataTreeView.ChildrenDicCache[parentData.GetAssetHashCode()].Add(newItem);
                //重排
                DataTreeView.ChildrenDicCache[parentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            else
            {
                //若为空，则认定为顶级数据
                //添加到顶级数据中
                DataTreeView.RootItemCache.Add(newItem);
                //重排
                DataTreeView.RootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            //添加到其他缓存中
            DataTreeView.ItemDicCache[newData.GetAssetHashCode()] = newItem;
            DataTreeView.ChildrenDicCache[newData.GetAssetHashCode()] = newChildren;

            //重构树形图
            DataTreeView.TRefresh();

            //判断源文件是否存在
            if (originalData != null)
            {
                //若存在，判断其对应的数据项是否有子级
                if (DataTreeView.ItemDicCache[originalData.GetAssetHashCode()].hasChildren)
                {
                    //若有，对子级进行递归创建
                    foreach (TreeViewItemData<LocUnitData> childItem in DataTreeView.ChildrenDicCache[originalData.GetAssetHashCode()])
                    {
                        //此时的父数据为新创建的数据，新名称为原名称，源数据为自身
                        CreateLocUnitDataFile(newData, childItem.data.name, childItem.data);
                    }
                }
            }

            //重新生成节点与连线缓存
            NodeEditorPanel.GenerateNodesAndLines();
            //结束后注明节点面板需要重建并重建
            NodeEditorPanel.NeedToRefresh = true;
            NodeEditorPanel.NRefresh(NodeEditorPanel.ShowedNodes[0].TargetData);
        }
        //编辑器面板中删除地点数据
        public void DeleteLocUnitDataFile(LocUnitData deletedData)
        {
            //判断传入是否为空
            if (deletedData == null)
            {
                //若为空，返回
                return;
            }
            //若不为空
            TreeViewItemData<LocUnitData> item = DataTreeView.ItemDicCache[deletedData.GetAssetHashCode()];

            //将数据自身从缓存中移除
            //将数据从其父级中删除
            if (item.data.ParentData != null)
            {
                //若其父级不为空，则删除父级的子级数据
                DataTreeView.ChildrenDicCache[item.data.ParentData.GetAssetHashCode()].Remove(item);
                //重排
                DataTreeView.ChildrenDicCache[item.data.ParentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            //其他缓存
            //树状图数据的根物体缓存，考虑到Remove能自己解决没有物体的问题，所以直接移除
            DataTreeView.RootItemCache.Remove(item);
            //树状图数据的全物体缓存
            DataTreeView.ItemDicCache.Remove(item.id);
            //树状图数据的子物体缓存
            DataTreeView.ChildrenDicCache.Remove(item.id);
            //树状图数据的扩展状态缓存
            DataTreeView.ExpandedStateCache.Remove(item.id);
            //节点面板的当前显示节点缓存，考虑到需要移除视觉元素，还有丫有关的连线，所以稍微麻烦点
            Node node = NodeEditorPanel.NodeDicCache[item.id];
            if (NodeEditorPanel.ShowedNodes.Contains(node))
            {
                NodeEditorPanel.NodeView.Remove(node);
                NodeEditorPanel.ShowedNodes.Remove(node);
            }
            //节点面板的全节点缓存
            NodeEditorPanel.NodeDicCache.Remove(item.id);
            //节点面板的节点位置缓存
            NodeEditorPanel.NodePosDicCache.Remove(item.id);
            //需要重命名的数据的缓存
            DataNeedToReGenerateFullNameCache.Remove(item.data);

            //将其对应的连接中删除
            foreach (LocUnitData otherData in deletedData.ConnectionKeys)
            {
                otherData.Editor_RemoveConnection(deletedData);
            }

            //将数据从文件夹中移除
            //获取其所在的文件夹
            string removedDataFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(deletedData));
            //删除文件夹下所有文件
            DeleteFolder(removedDataFolderPath);

            //保存更改
            AssetDatabase.SaveAssets();
            //刷新资源视图
            AssetDatabase.Refresh();

            //刷新树形图
            DataTreeView.TRefresh();

            //判断是否有子级
            if (item.hasChildren)
            {
                //若有子级，对子级进行递归删除
                foreach (TreeViewItemData<LocUnitData> childItem in item.children)
                {
                    DeleteLocUnitDataFile(childItem.data);
                }
            }

            //重新生成节点与连线缓存
            NodeEditorPanel.GenerateNodesAndLines();
            //结束后注明节点面板需要重建
            NodeEditorPanel.NeedToRefresh = true;
            NodeEditorPanel.NRefresh(NodeEditorPanel.ShowedNodes[0].TargetData);
        }
        //编辑器面板中重命名物体数据
        public void RenameLocUnitDataFile(LocUnitData renamedData, string newDataName)
        {
            //获取当前文件及文件夹路径
            string assetPath = AssetDatabase.GetAssetPath(renamedData);
            string assetFolderPath = Path.GetDirectoryName(assetPath);
            //获取重命名后的文件夹路径
            string folderPathAfterRename = Path.GetDirectoryName(assetFolderPath) + $"\\{newDataName}";
            if (AssetDatabase.IsValidFolder(folderPathAfterRename))
            {
                //若已存在，则提出警告并更改名称
                Debug.LogWarning("重命名警告：重命名文件与同级目录下其他文件重名！");
                newDataName = "需要重命名_" + newDataName;
            }
            //重命名文件及文件夹与数据
            AssetDatabase.RenameAsset(assetPath, newDataName);
            AssetDatabase.RenameAsset(assetFolderPath, newDataName);
            renamedData.Editor_SetName(newDataName);
            MarkAsNeedToReGenerateFullName(renamedData);

            //标记为脏
            EditorUtility.SetDirty(renamedData);

            //保存更改
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //更名结束后对缓存中的数据进行重排
            if (renamedData.ParentData != null)
            {
                //若有父级，重排其父级的子级
                DataTreeView.ChildrenDicCache[renamedData.ParentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            else
            {
                //若无父级，重排根数据子级
                DataTreeView.RootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }

            //重构树形图
            DataTreeView.TRefresh();

            //重新生成节点与连线缓存
            NodeEditorPanel.GenerateNodesAndLines();
            //结束后注明节点面板需要重建
            NodeEditorPanel.NeedToRefresh = true;
            NodeEditorPanel.NRefresh(NodeEditorPanel.ShowedNodes[0].TargetData);
        }
        //编辑器面板中移动物体数据
        public void MoveLocUnitDataFile(LocUnitData targetData, List<LocUnitData> topLevelMovedDatas)
        {
            //移动前检测
            if (targetData != null)
            {
                //若目标数据不为空，则首先判断目标数据是否在顶级选中项之中
                if (topLevelMovedDatas.Contains(targetData))
                {
                    //如果是，返回
                    Debug.LogWarning("移动/剪切操作不允许将数据拖放到自身或其子级中");
                    return;
                }
                //若不是，获取其父级
                LocUnitData targetParentData = targetData.ParentData;
                while (targetParentData != null)
                {
                    if (topLevelMovedDatas.Contains(targetParentData))
                    {
                        Debug.LogWarning("移动/剪切操作不允许将数据拖放到自身或其子级中");
                        return;
                    }
                    //若当前父级不在被选中的物体中，进入下一级父级的判断
                    targetParentData = targetParentData.ParentData;
                }
            }

            //更新父级与缓存与本地存储
            foreach (LocUnitData movedData in topLevelMovedDatas)
            {
                //移动前，首先清除自身相关链接
                foreach (LocUnitData otherData in movedData.ConnectionKeys)
                {
                    otherData.Editor_RemoveConnection(movedData);
                }
                movedData.ConnectionKeys.Clear();
                movedData.ConnectionValues.Clear();

                //源文件夹
                string sourceFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(movedData));
                //目标父级文件夹
                string targetFolderPath;

                //更新缓存
                //获取被移动的数据对应的树形图物体
                TreeViewItemData<LocUnitData> movedItem = DataTreeView.ItemDicCache[movedData.GetAssetHashCode()];
                //判断数据的旧父级
                if (movedData.ParentData != null)
                {
                    //若有，则将数据从旧父级的子级列表中移除
                    DataTreeView.ChildrenDicCache[movedData.ParentData.GetAssetHashCode()].Remove(movedItem);
                    //重排
                    DataTreeView.ChildrenDicCache[movedData.ParentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
                }
                else
                {
                    //若无，说明为顶层，从根数据缓存中移除
                    DataTreeView.RootItemCache.Remove(movedItem);
                    //重排
                    DataTreeView.RootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
                }
                //设置新的父级
                movedData.Editor_SetParent(targetData);
                //判断目标对象
                if (targetData != null)
                {
                    //如果有目标对象，则将数据作为目标的子级
                    DataTreeView.ChildrenDicCache[targetData.GetAssetHashCode()].Add(movedItem);
                    //重排
                    DataTreeView.ChildrenDicCache[targetData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
                    //并设置父级文件夹
                    targetFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetData));
                }
                else
                {
                    //如果没有目标对象，则添加到根数据缓存中
                    DataTreeView.RootItemCache.Add(movedItem);
                    //重排
                    DataTreeView.RootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
                    //并生成父级文件夹
                    targetFolderPath = GetDataFolderPath(targetData);
                }

                //移动，此处为移动操作，需要保留GUID等元数据，因此采用Unity自带的方法
                //判断两个文件夹是否都存在
                if (AssetDatabase.IsValidFolder(sourceFolderPath) && AssetDatabase.IsValidFolder(targetFolderPath))
                {
                    //若存在，获取源文件夹名称
                    string folderName = Path.GetFileName(sourceFolderPath);
                    //组合得到最终的文件夹路径
                    string finalFolderPath = Path.Combine(targetFolderPath, folderName).Replace("\\", "/");

                    //判断是否实际上没有发生移动
                    if (sourceFolderPath.Replace("\\", "/").Equals(finalFolderPath))
                    {
                        //若是，不进行操作，返回
                        return;
                    }

                    //判断新文件夹路径是否存在
                    if (AssetDatabase.IsValidFolder(finalFolderPath))
                    {
                        //提示
                        Debug.LogWarning("目标文件夹内有同名文件，请进行重命名！");
                        //重设资源名称
                        string newName = "需要重命名_" + folderName;
                        //重命名文件
                        RenameLocUnitDataFile(movedData, newName);
                        //更改原地址
                        sourceFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(movedData));
                    }

                    //移动文件夹
                    AssetDatabase.MoveAsset(sourceFolderPath, finalFolderPath);
                }

                //移动结束后进行全名的重新生成的标记
                MarkAsNeedToReGenerateFullName(movedData);

                //标记资源为脏
                EditorUtility.SetDirty(movedData);
            }

            //保存更改
            AssetDatabase.SaveAssets();
            //刷新资源视图
            AssetDatabase.Refresh();

            //刷新树形图
            DataTreeView.TRefresh();

            //并重新生成节点
            NodeEditorPanel.GenerateNodesAndLines();
            //结束后注明节点面板需要重建
            NodeEditorPanel.NeedToRefresh = true;
            NodeEditorPanel.NRefresh(NodeEditorPanel.ShowedNodes[0].TargetData);
        }
        #endregion

        #region 文件存储与读取方法
        //保存缓存到永久性存储文件
        public void SavePersistentData()
        {
            //生成永久性实例
            PersistentData persistentData = new()
            {
                //设置其数值
                DefaultPackage = DefaultPackageField.text,
                DefaultAuthor = DefaultAuthorField.text,
                TimerDebugLogState = TimerDebugLogToggle.value,
                ExpandedState = DataTreeView.ExpandedStateCache.ToList(),
                NodePositions = new()
            };
            //生成节点位置数据
            foreach (Node node in NodeEditorPanel.NodeDicCache.Values)
            {
                //存入
                persistentData.NodePositions[node.TargetData.GetAssetHashCode()] = (node.style.left.value.value, node.style.top.value.value);
            }
            //将永久性存储实例转化为文本
            string jsonString = JsonConvert.SerializeObject(persistentData, Formatting.Indented);
            //写入文件中
            File.WriteAllText(AssetDatabase.GetAssetPath(PersistentDataFile), jsonString);
            //标记为脏
            EditorUtility.SetDirty(PersistentDataFile);
            //保存更改
            AssetDatabase.SaveAssets();
            //刷新数据
            AssetDatabase.Refresh();
        }
        //读取永久性存储文件到缓存
        private void LoadPersistentData()
        {
            //读取文件中数据
            string jsonString = File.ReadAllText(AssetDatabase.GetAssetPath(PersistentDataFile));
            //生成永久性存储实例
            PersistentData persistentData = JsonConvert.DeserializeObject<PersistentData>(jsonString);
            //分配数据
            //属性
            DefaultPackageField.SetValueWithoutNotify(persistentData.DefaultPackage);
            DefaultAuthorField.SetValueWithoutNotify(persistentData.DefaultAuthor);
            TimerDebugLogToggle.SetValueWithoutNotify(persistentData.TimerDebugLogState);
        }
        #endregion

        #region 辅助方法
        //获取数据理论存储地址，同时承担理论地址为空时进行创建
        public static string GetDataFolderPath(LocUnitData locUnitData)
        {
            //生成地址，默认为根目录的GameData文件夹下
            string assetFolderPath = "Assets\\GameData";

            //检测传入是否为空
            if (locUnitData != null)
            {
                //若不为空，进行操作
                if (locUnitData.ParentData != null)
                {
                    //若不为空有父级，则基础路径为父级路径
                    assetFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(locUnitData.ParentData));
                }
                else
                {
                    //扩展路径为
                    assetFolderPath += "\\Location";
                }

                //扩展自身地址为父级路径加上子级名称文件夹
                assetFolderPath += $"\\{locUnitData.name}";
            }
            else
            {
                //若传入为空，扩展路径为
                assetFolderPath += "\\Location";
            }

            //检查路径是否存在
            if (!AssetDatabase.IsValidFolder(assetFolderPath))
            {
                //若不存在，则进行生成
                //分割路径
                string[] folders = assetFolderPath.Split("\\");
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
            }

            //保存更改并刷新
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //返回路径
            return assetFolderPath;
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
        //重新生成全名
        public void MarkAsNeedToReGenerateFullName(LocUnitData locUnitData)
        {
            //检测数据是否为空
            if (locUnitData == null)
            {
                //返回
                return;
            }

            //数据自身生成添加
            DataNeedToReGenerateFullNameCache.Add(locUnitData);

            //检查是否拥有子级
            if (DataTreeView.ItemDicCache[locUnitData.GetAssetHashCode()].hasChildren)
            {
                //若有，则对子级进行同样操作
                foreach (var child in DataTreeView.ChildrenDicCache[locUnitData.GetAssetHashCode()])
                {
                    MarkAsNeedToReGenerateFullName(child.data);
                }
            }
        }
        #endregion
    }
}


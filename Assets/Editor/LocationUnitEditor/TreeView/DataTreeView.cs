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
    public class DataTreeView : TreeView
    {
        #region 数据内容
        //主窗口
        public MainWindow MainWindow { get; private set; }

        //根数据缓存
        public List<TreeViewItemData<LocUnitData>> RootItemCache { get; private set; } = new();
        //ID-地点查询字典缓存
        public Dictionary<int, TreeViewItemData<LocUnitData>> ItemDicCache { get; private set; } = new();
        //ID-子级查询字典缓存
        public Dictionary<int, List<TreeViewItemData<LocUnitData>>> ChildrenDicCache { get; private set; } = new();
        //展开状态缓存
        public HashSet<int> ExpandedStateCache { get; private set; } = new();

        //当前活跃选中项
        public LocUnitData ActiveSelection { get; private set; }
        #endregion

        #region 树形图的初始化及数据更新
        //构建函数
        public DataTreeView(MainWindow mainWindow)
        {
            //获取主面板
            MainWindow = mainWindow;

            //更改自身属性
            style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));

            //初始化
            Init();
        }
        //初始化树形结构
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("地点数据管理面板初始化", MainWindow.TimerDebugLogToggle.value);

            //读取数据
            LoadPersistentData();

            //设置数据源
            SetRootItems(RootItemCache);
            //设置树形图面板
            makeItem = () =>
            {
                Label label = new();
                label.AddToClassList("treeview-item-location");
                return label;
            };
            bindItem = (element, i) =>
            {
                LocUnitData locUnitData = GetItemDataForIndex<LocUnitData>(i);
                Label label = element as Label;
                label.text = locUnitData.name;
            };

            //生成树形图数据
            GenerateItems();

            //实现展开状态保存
            itemExpandedChanged += SaveExpandedState;

            //实现有选中项时获取活跃数据与打开编辑窗口
            selectionChanged += (selections) =>
            {
                //获取活跃数据
                LocUnitData activeSelection = selections.Cast<LocUnitData>().FirstOrDefault();
                //检测活跃数据
                if (activeSelection != null)
                {
                    //赋值
                    ActiveSelection = activeSelection;
                    //刷新节点面板
                    MainWindow.NodeEditorPanel.NRefresh();
                    //刷新数据编辑面板
                    MainWindow.DataEditorPanel.DRefresh();
                }
            };

            //注册快捷键
            RegisterCallback<KeyDownEvent>(RegisterShortcutKey);

            //注册点击事件
            RegisterCallback<PointerDownEvent>(OnPointerDown);

            //注册右键菜单
            RegisterContextMenu();
        }
        //刷新树形图面板
        public void TRefresh()
        {
            //设置数据源并重建
            SetRootItems(RootItemCache);
            Rebuild();
            //恢复展开状态
            RestoreExpandedState();
        }
        //生成树形结构数据方法
        private void GenerateItems()
        {
            //缓存清零
            ItemDicCache.Clear();
            ChildrenDicCache.Clear();
            RootItemCache.Clear();

            //读取数据，并进行缓存
            List<LocUnitData> locUnitDatas = AssetDatabase.FindAssets("t:LocUnitData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocUnitData>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();
            foreach (LocUnitData locUnitData in locUnitDatas)
            {
                //子级列表的创建
                List<TreeViewItemData<LocUnitData>> children = new();
                //数据的创建
                TreeViewItemData<LocUnitData> item = new(locUnitData.GetAssetHashCode(), locUnitData, children);
                //添加到字典中去
                ItemDicCache[locUnitData.GetAssetHashCode()] = item;
                ChildrenDicCache[locUnitData.GetAssetHashCode()] = children;
            }

            //构建树形结构
            foreach (TreeViewItemData<LocUnitData> item in ItemDicCache.Values)
            {
                //判断该地点数据是否有父级
                if (item.data.ParentData == null)
                {
                    //若无，则添加入根级别中
                    RootItemCache.Add(item);
                }
                else
                {
                    //若有，则获取其父级树形图数据
                    TreeViewItemData<LocUnitData> parentItem = ItemDicCache[item.data.ParentData.GetAssetHashCode()];
                    //向父级树形图数据的子级列表中添加
                    ChildrenDicCache[parentItem.id].Add(item);
                }
            }

            //结束后按文件名称进行重新排序
            RootItemCache.Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
            foreach (List<TreeViewItemData<LocUnitData>> items in ChildrenDicCache.Values)
            {
                items.Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
            }

            //刷新面板
            TRefresh();
        }
        //注册树形图右键菜单方法
        private void RegisterContextMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                //添加方法
                evt.menu.AppendAction("添加地点数据\tInsert", action => CreateItemData(), DropdownMenuAction.AlwaysEnabled);
                //移除方法
                evt.menu.AppendAction("删除地点数据\tDel", action => DeleteItemData(), actionStatus =>
                {
                    //判断是否有选择
                    return selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
            }));
        }
        //注册快捷键方法
        private void RegisterShortcutKey(KeyDownEvent e)
        {
            //实现快捷键的绑定
            if (e.keyCode == KeyCode.Insert)
            {
                //若按键为Insert，执行添加方法
                CreateItemData();
                //阻断事件传播
                e.StopImmediatePropagation();
            }
            else if (e.keyCode == KeyCode.Delete)
            {
                //若按键为Delete，检测是否有选中
                if (selectedIndices.Any())
                {
                    //若有选中，执行移除操作
                    DeleteItemData();
                }
                //阻断事件传播
                e.StopImmediatePropagation();
            }
        }
        //取消选中方法
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.ctrlKey)
            {
                if (evt.button == 0)
                {
                    //当按下Ctrl+左键时
                    //获取新的选中项数据
                    LocUnitData newSelection = selectedItems.Cast<LocUnitData>().FirstOrDefault();
                    //比较
                    if (newSelection == ActiveSelection)
                    {
                        //若与旧选中项相同，则清空选择
                        SetSelection(new int[0]);
                        ActiveSelection = null;
                    }
                }
            }
        }
        #endregion

        #region 树形图增删改方法
        //增添新地点方法
        private void CreateItemData()
        {
            //显示输入窗口
            TextInputWindow.ShowWindow(newName =>
            {
                //计时
                using ExecutionTimer timer = new("新增地点数据", MainWindow.TimerDebugLogToggle.value);

                //创建路径
                string newFolderPath = "Assets\\GameData\\Location";
                //判断选中项
                if (ActiveSelection != null)
                {
                    //不为空的情况下，路径更改为父级路径加上新文件夹名称
                    newFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ActiveSelection)) + $"\\{newName}";
                }
                else
                {
                    //为空的情况下，路径扩展为新路径
                    newFolderPath += $"\\{newName}";
                }

                //检查路径存在状态
                if (MakeSureFolderPathExist(newFolderPath))
                {
                    //若已存在，提示并返回
                    Debug.LogWarning("该地点已经存在，请重新创建！");
                    return;
                }
                else
                {
                    //若不存在，开始生成物体
                    //创建新资源
                    LocUnitData newData = ScriptableObject.CreateInstance<LocUnitData>();
                    //设置相关数据
                    newData.Editor_SetPackage(MainWindow.DefaultPackageField.text);
                    newData.Editor_SetCategory("Location");
                    newData.Editor_SetAuthor(MainWindow.DefaultAuthorField.text);
                    newData.Editor_SetName(newName);
                    newData.Editor_SetSortingOrder(999);
                    newData.Editor_SetBackground(MainWindow.DefaultLocationBackground);
                    //更改文件名
                    newData.name = newName;
                    //更改父级
                    newData.Editor_SetParent(ActiveSelection);
                    //生成全名
                    newData.Editor_GenerateFullName();
                    //生成ID
                    newData.Editor_GenerateID();
                    //获取资源文件夹地址
                    string newDataPath = Path.Combine(newFolderPath, $"{newName}.asset").Replace("\\", "/");
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
                    if (ActiveSelection != null)
                    {
                        //当选中项不为空时，新数据作为被选中的数据的子级被添加
                        ChildrenDicCache[ActiveSelection.GetAssetHashCode()].Add(newItem);
                        //并赋予序号
                        newItem.data.Editor_SetSortingOrder(ChildrenDicCache[ActiveSelection.GetAssetHashCode()].Count);
                        //重排
                        ChildrenDicCache[ActiveSelection.GetAssetHashCode()].Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
                    }
                    else
                    {
                        //若为空，则认定为顶级数据
                        //添加到顶级数据中
                        RootItemCache.Add(newItem);
                        //并赋予序号
                        newItem.data.Editor_SetSortingOrder(RootItemCache.Count);
                        //重排
                        RootItemCache.Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
                    }
                    //添加到其他缓存中
                    ItemDicCache[newData.GetAssetHashCode()] = newItem;
                    ChildrenDicCache[newData.GetAssetHashCode()] = newChildren;
                    //重构树形图
                    TRefresh();

                    //重新生成节点与连线缓存
                    MainWindow.NodeEditorPanel.GenerateNodesAndLines();
                }
            },
            "创建新地点数据",
            "请输入新地点数据的文件名",
            "新地点数据文件名",
            "新地点",
            EditorWindow.focusedWindow
            );
        }
        //删除地点方法
        private void DeleteItemData()
        {
            //判断当前选中项
            if (ActiveSelection == null)
            {
                //若当前选中项为空，考虑到删除全部是不合理的，所以返回
                return;
            }
            else
            {
                //若选中项不为空，首先进行关怀的询问
                bool confirmWnd = EditorUtility.DisplayDialog(
                    "请确认删除",
                    "您确认要删除选定的节点吗？\n这将删除选中的节点及其子节点。",
                    "确认",
                    "取消"
                    );

                //确认结果
                if (!confirmWnd)
                {
                    //若不确认，直接返回
                    return;
                }

                //若确认，则开始进行删除
                using ExecutionTimer timer = new("移除地点数据", MainWindow.TimerDebugLogToggle.value);

                //获取路径
                string deletedFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(ActiveSelection));
                //从其连接项中删除自身
                foreach (LocUnitData otherLocation in ActiveSelection.ConnectionKeys)
                {
                    otherLocation.Editor_RemoveConnection(ActiveSelection);
                }
                //删除
                DeleteFolder(deletedFolderPath);

                //由于情况复杂且不好分类，所以直接重构面板
                GenerateItems();
                MainWindow.NodeEditorPanel.GenerateNodesAndLines();
            }
        }
        #endregion

        #region 数据的保存与读取
        //保存树形图展开状态
        private void SaveExpandedState(TreeViewExpansionChangedArgs args)
        {
            //获取参数中的ID
            int id = args.id;
            //保存或删除
            if (IsExpanded(id))
            {
                //若为打开，添加到缓存
                ExpandedStateCache.Add(id);
            }
            else
            {
                //若为关闭，判断其是否存在于数据中并移除
                if (ExpandedStateCache.Contains(id))
                {
                    ExpandedStateCache.Remove(id);
                }
            }
        }
        //恢复树形图展开状态
        private void RestoreExpandedState()
        {
            //新增被移除的ID列表
            List<int> removedIDs = new();
            //根据缓存数据进行展开
            foreach (int id in ExpandedStateCache)
            {
                //判断ID是否存在
                if (ItemDicCache.ContainsKey(id))
                {
                    //若存在，展开
                    ExpandItem(id);
                }
                else
                {
                    //若不存在，说明该ID对应的物体已经消失，添加到被移除的列表中
                    removedIDs.Add(id);
                }
            }
            //结束后取差集
            ExpandedStateCache = ExpandedStateCache.Except(removedIDs).ToHashSet();
        }
        #endregion

        #region 辅助方法
        //读取数据
        private void LoadPersistentData()
        {
            //读取文件中数据
            string jsonString = File.ReadAllText(AssetDatabase.GetAssetPath(MainWindow.PersistentDataFile));
            //生成永久性存储实例
            PersistentData persistentData = JsonConvert.DeserializeObject<PersistentData>(jsonString);
            //分配数据
            //展开状态数据
            ExpandedStateCache.Clear();
            foreach (int id in persistentData.ExpandedState)
            {
                ExpandedStateCache.Add(id);
            }
        }
        //获取顶级物体列表，顶级物体指的是，该树形图物体列表中，其任意一级父级均不在列表中的物体
        public List<TreeViewItemData<LocUnitData>> GetTopLevelTreeViewItemData(List<TreeViewItemData<LocUnitData>> items)
        {
            //需要被移除的数据
            List<TreeViewItemData<LocUnitData>> itemsToRemove = new();

            //对传入列表进行遍历
            foreach (TreeViewItemData<LocUnitData> item in items)
            {
                //获取物体父级
                LocUnitData parentLocUnitData = item.data.ParentData;
                //判断物体父级
                while (parentLocUnitData != null)
                {
                    //当父级数据不为空时，获取父级物体
                    TreeViewItemData<LocUnitData> parentItem = ItemDicCache[parentLocUnitData.GetAssetHashCode()];
                    //判断父级是否在传入的列表内
                    if (items.Contains(parentItem))
                    {
                        //若在列表内，说明不是顶级数据，加入被移除数据的表中
                        itemsToRemove.Add(item);
                        //跳出循环
                        break;
                    }
                    //若不在，则指定推进到下一个父级
                    parentLocUnitData = parentLocUnitData.ParentData;
                }
            }

            //返回差集
            return items.Except(itemsToRemove).ToList();
        }
        //确保目标路径存在
        private bool MakeSureFolderPathExist(string folderPath)
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
        private void DeleteFolder(string folderPath)
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
    }
}

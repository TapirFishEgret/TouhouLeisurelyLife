using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.GeographySystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocationDataEditor
{
    public class DataTreeView : TreeView
    {
        #region 数据内容
        //主窗口
        public MainWindow MainWindow { get; private set; }

        //根数据缓存
        public List<TreeViewItemData<LocationData>> RootItemCache { get; private set; } = new();
        //ID-地点查询字典缓存
        public Dictionary<int, TreeViewItemData<LocationData>> ItemDicCache { get; private set; } = new();
        //ID-子级查询字典缓存
        public Dictionary<int, List<TreeViewItemData<LocationData>>> ChildrenDicCache { get; private set; } = new();
        //展开状态缓存
        public HashSet<int> ExpandedStateCache { get; private set; } = new();

        //当前活跃选中项
        public LocationData ActiveSelection { get; private set; }
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
                LocationData locUnitData = GetItemDataForIndex<LocationData>(i);
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
                LocationData activeSelection = selections.Cast<LocationData>().FirstOrDefault();
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
            List<LocationData> locUnitDatas = AssetDatabase.FindAssets("t:LocationData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocationData>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();
            foreach (LocationData locUnitData in locUnitDatas)
            {
                //子级列表的创建
                List<TreeViewItemData<LocationData>> children = new();
                //数据的创建
                TreeViewItemData<LocationData> item = new(locUnitData.GetAssetHashCode(), locUnitData, children);
                //添加到字典中去
                ItemDicCache[locUnitData.GetAssetHashCode()] = item;
                ChildrenDicCache[locUnitData.GetAssetHashCode()] = children;
            }

            //构建树形结构
            foreach (TreeViewItemData<LocationData> item in ItemDicCache.Values)
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
                    TreeViewItemData<LocationData> parentItem = ItemDicCache[item.data.ParentData.GetAssetHashCode()];
                    //向父级树形图数据的子级列表中添加
                    ChildrenDicCache[parentItem.id].Add(item);
                }
            }

            //结束后按文件名称进行重新排序
            RootItemCache.Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
            foreach (List<TreeViewItemData<LocationData>> items in ChildrenDicCache.Values)
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
                    LocationData newSelection = selectedItems.Cast<LocationData>().FirstOrDefault();
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
                if (EditorExtensions.MakeSureFolderPathExist(newFolderPath))
                {
                    //若已存在，提示并返回
                    Debug.LogWarning("该地点已经存在，请重新创建！");
                    return;
                }

                //若路径不存在，而包存在，则开始生成物体
                //创建新资源
                LocationData newLocData = ScriptableObject.CreateInstance<LocationData>();
                //设置相关数据
                newLocData.GameDataType = BaseSystem.GameDataTypeEnum.Location;
                newLocData.Name = newName;
                newLocData.SortingOrder = 999;
                newLocData.Background = MainWindow.DefaultLocationBackground;
                //更改文件名
                newLocData.name = newName;
                //更改父级
                newLocData.ParentData = ActiveSelection;
                //生成全名
                newLocData.Editor_GenerateFullName();
                //生成ID
                newLocData.Editor_GenerateID();
                //获取资源文件夹地址
                string newLocDataPath = Path.Combine(newFolderPath, $"{newName}.asset").Replace("\\", "/");
                //新建资源
                AssetDatabase.CreateAsset(newLocData, newLocDataPath);
                //保存文件更改
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //处理缓存数据
                //创建新实例对应的树形图数据
                List<TreeViewItemData<LocationData>> newChildren = new();
                TreeViewItemData<LocationData> newItem = new(newLocData.GetAssetHashCode(), newLocData, newChildren);
                //判断选中项是否为空
                if (ActiveSelection != null)
                {
                    //当选中项不为空时，新数据作为被选中的数据的子级被添加
                    ChildrenDicCache[ActiveSelection.GetAssetHashCode()].Add(newItem);
                    //并赋予序号
                    newItem.data.SortingOrder = ChildrenDicCache[ActiveSelection.GetAssetHashCode()].Count;
                    //重排
                    ChildrenDicCache[ActiveSelection.GetAssetHashCode()].Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
                }
                else
                {
                    //若为空，则认定为顶级数据
                    //添加到顶级数据中
                    RootItemCache.Add(newItem);
                    //并赋予序号
                    newItem.data.SortingOrder = RootItemCache.Count;
                    //重排
                    RootItemCache.Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
                }
                //添加到其他缓存中
                ItemDicCache[newLocData.GetAssetHashCode()] = newItem;
                ChildrenDicCache[newLocData.GetAssetHashCode()] = newChildren;
                //重新生成节点与连线缓存
                MainWindow.NodeEditorPanel.GenerateNodesAndLines();
                //重构树形图
                TRefresh();

                //处理资源包
                //创建新的资源索引
                AddressableAssetEntry entry = AddressableAssetSettingsDefaultObject
                .GetSettings(true)
                .CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newLocData)), MainWindow.AssetGroup);
                //设定索引名称为全名
                entry.SetAddress(string.Join("_", newLocData.FullName).Replace(" ", "-"));
                //并设定标签
                entry.SetLabel("Location", true, true);
                //保存设置
                MainWindow.AssetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                AssetDatabase.SaveAssets();
            },
            "Create New Location Unit",
            "Please Input New Location Unit Name",
            "New Location Unit Name",
            "New Name",
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
                foreach (LocationData otherLocation in ActiveSelection.ConnectionKeys)
                {
                    otherLocation.Editor_RemoveConnection(ActiveSelection);
                }
                //删除
                EditorExtensions.DeleteFolder(deletedFolderPath);

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
        //保存所有数据
        public void SaveAllData()
        {
            //遍历缓存
            foreach (TreeViewItemData<LocationData> item in ItemDicCache.Values)
            {
                //并将数据设置为脏
                EditorUtility.SetDirty(item.data);
            }
            //保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
        #endregion
    }
}

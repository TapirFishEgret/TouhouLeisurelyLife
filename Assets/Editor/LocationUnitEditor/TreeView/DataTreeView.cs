using System.Collections.Generic;
using System.Linq;
using THLL.LocationSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class DataTreeView : TreeView
    {
        #region 数据内容
        //主窗口
        private readonly MainWindow _mainWindow;
        public MainWindow MainWindow => _mainWindow;

        //根数据缓存
        private readonly List<TreeViewItemData<LocUnitData>> _rootItemCache = new();
        public List<TreeViewItemData<LocUnitData>> RootItemCache => _rootItemCache;
        //ID-地点查询字典缓存
        private readonly Dictionary<int, TreeViewItemData<LocUnitData>> _itemDicCache = new();
        public Dictionary<int, TreeViewItemData<LocUnitData>> ItemDicCache => _itemDicCache;
        //ID-子级查询字典缓存
        private readonly Dictionary<int, List<TreeViewItemData<LocUnitData>>> _childrenDicCache = new();
        public Dictionary<int, List<TreeViewItemData<LocUnitData>>> ChildrenDicCache => _childrenDicCache;
        //展开状态缓存
        private readonly HashSet<int> _expandedStateCache = new();
        public HashSet<int> ExpandedStateCache => _expandedStateCache;
        //剪切板缓存
        private readonly List<TreeViewItemData<LocUnitData>> _clipboardItemCache = new();
        public List<TreeViewItemData<LocUnitData>> ClipboardItemCache => _clipboardItemCache;
        //是否为剪切操作
        private bool _isCutOperation = false;
        public bool IsCutOperation => _isCutOperation;

        //当前活跃数据
        private LocUnitData _activeData;
        public LocUnitData ActiveData => _activeData;
        #endregion

        //构建函数
        public DataTreeView(MainWindow mainWindow) 
        {
            //获取主面板
            _mainWindow = mainWindow;

            //更改自身属性
            style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));

            //初始化
            Init();
        }

        #region 树形图的初始化及数据更新
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
                label.AddToClassList("treeview-item");
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

            //TreeView中实现拖动逻辑
            //是否允许拖动设置
            canStartDrag += OnCanStartDrag;
            //开始拖动
            setupDragAndDrop += OnSetupDragAndDrop;
            //拖动更新
            dragAndDropUpdate += OnDragAndDropUpdate;
            //拖动结束
            handleDrop += OnHandleDrop;

            //实现展开状态保存
            itemExpandedChanged += SaveExpandedState;

            //实现双击重命名
            RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.clickCount == 2)
                {
                    RenameItemData();
                }
            });

            //实现有选中项时获取活跃数据与打开编辑窗口
            selectionChanged += (selections) =>
            {
                //获取活跃数据
                _activeData = selections.Cast<LocUnitData>().FirstOrDefault();
                //检测活跃数据与打开面板状况
                if (ActiveData != null && MainWindow.IsDataEditorPanelOpen)
                {
                    //刷新数据编辑面板
                    MainWindow.DataEditorPanel.DRefresh(_activeData);
                }
                else if (ActiveData != null && !MainWindow.IsDataEditorPanelOpen)
                {
                    //向节点面板新增节点
                    MainWindow.NodeEditorPanel.Add(new Node(ItemDicCache[_activeData.GetAssetHashCode()], MainWindow.NodeEditorPanel.NodeDicCache));
                }
            };

            //注册快捷键
            RegisterCallback<KeyDownEvent>(RegisterShortcutKey);

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
            RootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            foreach (List<TreeViewItemData<LocUnitData>> items in ChildrenDicCache.Values)
            {
                items.Sort((x, y) => x.data.name.CompareTo(y.data.name));
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
                //重命名方法
                evt.menu.AppendAction("重命名地点数据\tF2", action => RenameItemData(), actionStatus =>
                {
                    //判断是否有选择
                    return selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });

                //分割线
                evt.menu.AppendSeparator();

                //剪切方法
                evt.menu.AppendAction("剪切\tCtrl + X", action => CutOrCopyItemData(isCutOperation: true), actionStatus =>
                {
                    //判断是否有选择
                    return selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //复制方法
                evt.menu.AppendAction("复制\tCtrl + C", action => CutOrCopyItemData(isCutOperation: false), actionStatus =>
                {
                    //判断是否有选择
                    return selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //粘贴方法
                evt.menu.AppendAction("粘贴\tCtrl + V", action => PasteItemData(), actionStatus =>
                {
                    //判断剪切板是否有数据
                    return ClipboardItemCache.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
            }));
        }
        //注册快捷键方法
        private void RegisterShortcutKey(KeyDownEvent e)
        {
            //实现快捷键的绑定
            //检测Ctrl是否被按下
            if (e.ctrlKey)
            {
                //若是，检测组合按键
                if (e.keyCode == KeyCode.X)
                {
                    //若为Ctrl+X，检测数据存在状态
                    if (selectedIndices.Any())
                    {
                        //若有数据选中，执行剪切操作
                        CutOrCopyItemData(isCutOperation: true);
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
                else if (e.keyCode == KeyCode.C)
                {
                    //若为Ctrl+C，检测数据存在状态
                    if (selectedIndices.Any())
                    {
                        //若有数据选中，执行复制操作
                        CutOrCopyItemData(isCutOperation: false);
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
                else if (e.keyCode == KeyCode.V)
                {
                    //若为Ctrl+V，检测剪切板缓存
                    if (ClipboardItemCache.Any())
                    {
                        //若剪切板内有数据，执行粘贴操作
                        PasteItemData();
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
            }
            else if (e.keyCode == KeyCode.Insert)
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
            else if (e.keyCode == KeyCode.F2)
            {
                //若按键为F2，检测是否有选中
                if (selectedIndices.Any())
                {
                    //若有选中，执行移除操作
                    RenameItemData();
                }
                //阻断事件传播
                e.StopImmediatePropagation();
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

                //以活跃选中项为可能的父级，创建数据
                MainWindow.CreateLocUnitDataFile(_activeData, newName);

                //刷新树形图
                TRefresh();
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
            //询问窗口
            bool confirmWnd = EditorUtility.DisplayDialog
                (
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

            //计时
            using ExecutionTimer timer = new("移除地点数据", MainWindow.TimerDebugLogToggle.value);

            //获取当前所有选中项
            List<TreeViewItemData<LocUnitData>> selections = selectedItems
                .Select(data => ItemDicCache[((LocUnitData)data).GetAssetHashCode()])
                .ToList();

            //获取顶级选中项
            List<TreeViewItemData<LocUnitData>> topLevelItems = GetTopLevelTreeViewItemData(selections);

            //判断是否有选中项
            if (selections.Count > 0)
            {
                //有选中项的情况下
                foreach (TreeViewItemData<LocUnitData> removedItem in topLevelItems)
                {
                    //删除
                    MainWindow.DeleteLocUnitDataFile(removedItem.data);
                }

                //刷新树形图
                TRefresh();
            }
        }
        //TreeView实现重命名
        private void RenameItemData()
        {
            //检测活跃数据
            if (ActiveData != null)
            {
                //显示输入窗口
                TextInputWindow.ShowWindow(newName =>
                {
                    //计时
                    using ExecutionTimer timer = new("重命名地点数据", MainWindow.TimerDebugLogToggle.value);

                    //检测新名称
                    if (newName.Equals(ActiveData.name))
                    {
                        //若未发生更改，返回
                        return;
                    }

                    //更改文件名
                    MainWindow.RenameLocUnitDataFile(ActiveData, newName);

                    //刷新树形图
                    TRefresh();
                },
                "重命名地点数据",
                "请输入新的地点数据的名称",
                "新名称",
                ActiveData.name,
                EditorWindow.focusedWindow
                );
            }
        }
        //剪切与复制操作
        private void CutOrCopyItemData(bool isCutOperation)
        {
            //清除剪切板缓存
            ClipboardItemCache.Clear();
            //设定剪切
            _isCutOperation = isCutOperation;
            //获取所有选中数据
            List<TreeViewItemData<LocUnitData>> selections = selectedItems
                .Select(data => ItemDicCache[((LocUnitData)data).GetAssetHashCode()])
                .ToList();
            //获取顶级数据
            List<TreeViewItemData<LocUnitData>> topLevelItems = GetTopLevelTreeViewItemData(selections);
            //将顶级数据存储于缓存中
            ClipboardItemCache.AddRange(topLevelItems);
        }
        //粘贴操作
        private void PasteItemData()
        {
            //判断剪切板数据
            if (ClipboardItemCache.Count == 0)
            {
                //若无数据，返回
                return;
            }

            //计时
            using ExecutionTimer timer = new("粘贴地点数据", MainWindow.TimerDebugLogToggle.value);

            //获取剪切板数据
            List<LocUnitData> _clipboardData = ClipboardItemCache.Select(item => item.data).ToList();

            //检测目标数据是否在剪切板中
            if (_clipboardData.Contains(ActiveData))
            {
                //爆出警告并返回
                Debug.LogWarning("粘贴操作不允许将数据粘贴到自身上");
                return;
            }

            //检测是否为剪切
            if (_isCutOperation)
            {
                //若为剪切，则本次操作本质上为移动操作
                MainWindow.MoveLocUnitDataFile(ActiveData, _clipboardData);
                //完成剪切后清除缓存数据
                ClipboardItemCache.Clear();
            }
            else
            {
                //若不是剪切，则为复制操作，遍历剪切板
                foreach (TreeViewItemData<LocUnitData> item in ClipboardItemCache)
                {
                    //以选中项为父级，原数据名称为新名称，原数据为源数据进行递归创建
                    MainWindow.CreateLocUnitDataFile(ActiveData, item.data.name, item.data);
                }
            }

            //刷新树形图
            TRefresh();
        }
        #endregion

        #region 树形图拖动功能的实现
        //确认是否可以开始拖动操作
        private bool OnCanStartDrag(CanStartDragArgs args)
        {
            //始终可以拖动
            return true;
        }
        //设置拖动和放置的参数
        private StartDragArgs OnSetupDragAndDrop(SetupDragAndDropArgs args)
        {
            //获取被拖动的元素
            VisualElement draggerElement = args.draggedElement;
            //检测被拖动的元素是否为空
            if (draggerElement == null)
            {
                //若为空，返回默认
                return args.startDragArgs;
            }

            //创建开始拖动的参数
            StartDragArgs startDragArgs = new("拖动地点数据", DragVisualMode.Move);
            //设置拖动源
            startDragArgs.SetGenericData("地点数据源", this);
            //获取被选择的物体的ID列表
            //判断是否有选中项
            bool hasSelection = false;
            foreach (int id in args.selectedIds)
            {
                //若有选中项，则遍历可以成立，直接设置为真
                hasSelection = true;
                break;
            }
            //设置选中项ID
            List<int> selectedIds = hasSelection ? args.selectedIds.ToList() : null;
            //设置被拖动的数据ID列表
            startDragArgs.SetGenericData("拖动数据ID列表", selectedIds);
            //返回参数
            return startDragArgs;
        }
        //拖动过程中更新拖动效果
        private DragVisualMode OnDragAndDropUpdate(HandleDragAndDropArgs args)
        {
            //任何情景下均为普通拖动
            return DragVisualMode.Move;
        }
        //拖动放置操作
        private DragVisualMode OnHandleDrop(HandleDragAndDropArgs args)
        {
            //计时
            using ExecutionTimer timer = new("拖拽操作", MainWindow.TimerDebugLogToggle.value);

            //获取数据源
            if (args.dragAndDropData.GetGenericData("地点数据源") is not TreeView sourceTreeView)
            {
                //若未正确获取数据，则什么都不发生
                return DragVisualMode.None;
            }
            //获取拖动数据ID列表
            if (args.dragAndDropData.GetGenericData("拖动数据ID列表") is not List<int> draggedIDs)
            {
                //若未正确获取数据，则什么都不发生
                return DragVisualMode.None;
            }

            //获取目标数据
            LocUnitData targetData = GetItemDataForIndex<LocUnitData>(args.insertAtIndex);

            //提取选中物体中的顶层物体
            List<TreeViewItemData<LocUnitData>> topLevelDraggedItems = GetTopLevelTreeViewItemData
                (draggedIDs.Select(id => ItemDicCache[id]).ToList());

            //获取顶层数据
            List<LocUnitData> topLevelDatas = topLevelDraggedItems.Select(item => item.data).ToList();

            //进行移动操作
            MainWindow.MoveLocUnitDataFile(targetData, topLevelDatas);

            //刷新树形图
            TRefresh();

            //正确放置，返回空
            return DragVisualMode.None;
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
            //根据缓存数据进行展开
            foreach (int id in ExpandedStateCache)
            {
                //判断ID是否存在
                if (ItemDicCache.ContainsKey(id))
                {
                    //若存在，展开
                    ExpandItem(id);
                }
            }
        }
        #endregion

        #region 辅助方法
        //读取数据
        private void LoadPersistentData()
        {
            //读取文件中数据
            string jsonString = File.ReadAllText(AssetDatabase.GetAssetPath(MainWindow.PersistentDataFile));
            //生成永久性存储实例
            PersistentData persistentData = JsonUtility.FromJson<PersistentData>(jsonString);
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
        #endregion
    }
}

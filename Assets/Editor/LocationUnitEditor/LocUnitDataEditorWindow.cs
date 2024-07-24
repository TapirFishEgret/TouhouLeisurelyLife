using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.LocationSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor
{
    public class LocUnitDataEditorWindow : EditorWindow
    {
        #region 基础构成
        //自身UXML文件
        [SerializeField]
        private VisualTreeAsset _visualTree;
        //永久性存储文件
        [SerializeField]
        private TextAsset _persistentDataFile;

        //UI元素
        //左侧面板
        //面板切换按钮
        private Button _switchPanelButton;
        //树形图
        private TreeView _locUnitDataTreeView;
        //包、作者输入框
        private TextField _defaultPackageField;
        private TextField _defaultAuthorField;
        //计时器Debug面板显示开关
        private Toggle _timerDebugLogToggle;
        //右侧面板
        //数据编辑面板
        private VisualElement _dataEditorPanel;
        //基础四项
        private TextField _packageField;
        private TextField _authorFiled;
        private ObjectField _parentDataField;
        //全名
        private Label _fullNameLabel;
        //设置控件
        private TextField _nameField;
        private TextField _descriptionField;
        private ObjectField _backgroundField;
        //连接编辑面板
        private VisualElement _connectionEditorPanel;

        //数据存储
        //根数据缓存
        private readonly List<TreeViewItemData<LocUnitData>> _rootItemCache = new();
        //ID-地点查询字典缓存
        private readonly Dictionary<int, TreeViewItemData<LocUnitData>> _itemDicCache = new();
        //ID-子级查询字典缓存
        private readonly Dictionary<int, List<TreeViewItemData<LocUnitData>>> _childrenDicCache = new();
        //ID-地点数据节点缓存
        private readonly Dictionary<int, LocUnitDataNode> _nodeDicCache = new();
        //展开状态缓存
        private readonly HashSet<int> _expandedStateCache = new();
        //剪切板缓存
        private readonly List<TreeViewItemData<LocUnitData>> _clipboardItemCache = new();
        private bool _isCutOperation = false;
        //当前活跃数据，单选
        private LocUnitData _activeData;
        //需要进行重命名的数据
        private readonly HashSet<LocUnitData> _dataNeedToReGenerateFullName = new();
        //面板的选择
        private bool _isDataEditorPanelOpen;
        //连接编辑面板的节点拖拽功能
        private Vector2 _connectionEditorDragStart;
        private bool _connectionEditorIsDragging = false;

        //窗口菜单
        [MenuItem("GameEditor/LocationSystem/Location")]
        public static void ShowWindow()
        {
            //窗口设置
            LocUnitDataEditorWindow window = GetWindow<LocUnitDataEditorWindow>("Location Unit Editor Window");
            window.position = new Rect(100, 100, 1280, 720);
        }
        #endregion

        #region UI生命周期
        //创建UI
        public void CreateGUI()
        {
            //加载UXML文件
            _visualTree.CloneTree(rootVisualElement);

            //其他控件
            //获取
            _switchPanelButton = rootVisualElement.Q<Button>("SwitchPanelButton");
            _defaultPackageField = rootVisualElement.Q<TextField>("DefaultPackageField");
            _defaultAuthorField = rootVisualElement.Q<TextField>("DefaultAuthorField");
            _timerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            //绑定
            _switchPanelButton.clicked += SwitchPanel;

            //读取持久化数据
            LoadPersistentData();

            //左侧面板
            //初始化树形图面板
            TreeView_Init();

            //右侧面板
            //初始化数据编辑面板
            DataEditor_Init();
            //初始化连接编辑面板
            ConnectionEditor_Init();

            //调整面板的打开与关闭
            if (_isDataEditorPanelOpen)
            {
                _dataEditorPanel.style.display = DisplayStyle.Flex;
                _connectionEditorPanel.style.display = DisplayStyle.None;
            }
            else
            {
                _dataEditorPanel.style.display = DisplayStyle.None;
                _connectionEditorPanel.style.display = DisplayStyle.Flex;
            }
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //手动对标记为需要进行的数据的全名的重新生成
            foreach (LocUnitData data in _dataNeedToReGenerateFullName)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(data)))
                {
                    data.Editor_GenerateFullName();
                    //生成完成后移除
                    _dataNeedToReGenerateFullName.Remove(data);
                }
            }
            //保存持久化数据到磁盘
            SavePersistentData();
            //提醒修改可寻址资源包标签
            Debug.LogWarning("窗口已被关闭，请注意修改新增数据的可寻址资源包的Key。");
        }
        #endregion

        #region 树形图的初始化及数据更新
        //初始化树形结构
        private void TreeView_Init()
        {
            //计时
            using ExecutionTimer timer = new("地点数据管理面板初始化", _timerDebugLogToggle.value);

            //获取树形图面板
            _locUnitDataTreeView = rootVisualElement.Q<TreeView>("LocUnitDataTreeView");
            //设置数据源
            _locUnitDataTreeView.SetRootItems(_rootItemCache);
            //设置树形图面板
            _locUnitDataTreeView.makeItem = () =>
            {
                Label label = new();
                label.AddToClassList("treeview-item");
                return label;
            };
            _locUnitDataTreeView.bindItem = (element, i) =>
            {
                LocUnitData locUnitData = _locUnitDataTreeView.GetItemDataForIndex<LocUnitData>(i);
                Label label = element as Label;
                label.text = locUnitData.name;
            };

            //生成树形图数据
            TreeView_GenerateItem();

            //TreeView中实现拖动逻辑
            //是否允许拖动设置
            _locUnitDataTreeView.canStartDrag += TreeView_OnCanStartDrag;
            //开始拖动
            _locUnitDataTreeView.setupDragAndDrop += TreeView_OnSetupDragAndDrop;
            //拖动更新
            _locUnitDataTreeView.dragAndDropUpdate += TreeView_OnDragAndDropUpdate;
            //拖动结束
            _locUnitDataTreeView.handleDrop += TreeView_OnHandleDrop;

            //实现展开状态保存
            _locUnitDataTreeView.itemExpandedChanged += SaveExpandedState;

            //实现双击重命名
            _locUnitDataTreeView.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.clickCount == 2)
                {
                    TreeView_RenameItemData();
                }
            });

            //实现有选中项时获取活跃数据与打开编辑窗口
            _locUnitDataTreeView.selectionChanged += (selections) =>
            {
                //获取活跃数据
                _activeData = selections.Cast<LocUnitData>().FirstOrDefault();
                //检测活跃数据与打开面板状况
                if (_activeData != null && _isDataEditorPanelOpen)
                {
                    //刷新数据编辑面板
                    DataEditor_Refresh(_activeData);
                }
                else if (_activeData != null && !_isDataEditorPanelOpen)
                {
                    //向节点面板新增节点
                    _connectionEditorPanel.Add(new LocUnitDataNode(_itemDicCache[_activeData.GetAssetHashCode()], _nodeDicCache));
                }
            };

            //注册快捷键
            _locUnitDataTreeView.RegisterCallback<KeyDownEvent>(TreeView_RegisterShortcutKey);
            //注册右键菜单
            TreeView_RegisterContextMenu();
        }
        //刷新树形图面板
        private void TreeView_Refresh()
        {
            //设置数据源并重建
            _locUnitDataTreeView.SetRootItems(_rootItemCache);
            _locUnitDataTreeView.Rebuild();
            //恢复展开状态
            RestoreExpandedState();
        }
        //生成树形结构数据方法
        private void TreeView_GenerateItem()
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
                _itemDicCache[locUnitData.GetAssetHashCode()] = item;
                _childrenDicCache[locUnitData.GetAssetHashCode()] = children;
            }

            //构建树形结构
            foreach (TreeViewItemData<LocUnitData> item in _itemDicCache.Values)
            {
                //判断该地点数据是否有父级
                if (item.data.ParentData == null)
                {
                    //若无，则添加入根级别中
                    _rootItemCache.Add(item);
                }
                else
                {
                    //若有，则获取其父级树形图数据
                    TreeViewItemData<LocUnitData> parentItem = _itemDicCache[item.data.ParentData.GetAssetHashCode()];
                    //向父级树形图数据的子级列表中添加
                    _childrenDicCache[parentItem.id].Add(item);
                }
            }

            //结束后按文件名称进行重新排序
            _rootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            foreach (List<TreeViewItemData<LocUnitData>> items in _childrenDicCache.Values)
            {
                items.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }

            //刷新面板
            TreeView_Refresh();
        }
        //注册树形图右键菜单方法
        private void TreeView_RegisterContextMenu()
        {
            _locUnitDataTreeView.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                //添加方法
                evt.menu.AppendAction("添加地点数据\tInsert", action => TreeView_CreateItemData(), DropdownMenuAction.AlwaysEnabled);
                //移除方法
                evt.menu.AppendAction("删除地点数据\tDel", action => TreeView_DeleteItemData(), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //重命名方法
                evt.menu.AppendAction("重命名地点数据\tF2", action => TreeView_RenameItemData(), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });

                //分割线
                evt.menu.AppendSeparator();

                //剪切方法
                evt.menu.AppendAction("剪切\tCtrl + X", action => TreeView_CutOrCopyItemData(isCutOperation: true), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //复制方法
                evt.menu.AppendAction("复制\tCtrl + C", action => TreeView_CutOrCopyItemData(isCutOperation: false), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //粘贴方法
                evt.menu.AppendAction("粘贴\tCtrl + V", action => TreeView_PasteItemData(), actionStatus =>
                {
                    //判断剪切板是否有数据
                    return _clipboardItemCache.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
            }));
        }
        //注册快捷键方法
        private void TreeView_RegisterShortcutKey(KeyDownEvent e)
        {
            //检测窗口是否被聚焦
            if (focusedWindow != this)
            {
                return;
            }

            //实现快捷键的绑定
            //检测Ctrl是否被按下
            if (e.ctrlKey)
            {
                //若是，检测组合按键
                if (e.keyCode == KeyCode.X)
                {
                    //若为Ctrl+X，检测数据存在状态
                    if (_locUnitDataTreeView.selectedIndices.Any())
                    {
                        //若有数据选中，执行剪切操作
                        TreeView_CutOrCopyItemData(isCutOperation: true);
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
                else if (e.keyCode == KeyCode.C)
                {
                    //若为Ctrl+C，检测数据存在状态
                    if (_locUnitDataTreeView.selectedIndices.Any())
                    {
                        //若有数据选中，执行复制操作
                        TreeView_CutOrCopyItemData(isCutOperation: false);
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
                else if (e.keyCode == KeyCode.V)
                {
                    //若为Ctrl+V，检测剪切板缓存
                    if (_clipboardItemCache.Any())
                    {
                        //若剪切板内有数据，执行粘贴操作
                        TreeView_PasteItemData();
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
            }
            else if (e.keyCode == KeyCode.Insert)
            {
                //若按键为Insert，执行添加方法
                TreeView_CreateItemData();
                //阻断事件传播
                e.StopImmediatePropagation();
            }
            else if (e.keyCode == KeyCode.Delete)
            {
                //若按键为Delete，检测是否有选中
                if (_locUnitDataTreeView.selectedIndices.Any())
                {
                    //若有选中，执行移除操作
                    TreeView_DeleteItemData();
                }
                //阻断事件传播
                e.StopImmediatePropagation();
            }
            else if (e.keyCode == KeyCode.F2)
            {
                //若按键为F2，检测是否有选中
                if (_locUnitDataTreeView.selectedIndices.Any())
                {
                    //若有选中，执行移除操作
                    TreeView_RenameItemData();
                }
                //阻断事件传播
                e.StopImmediatePropagation();
            }
        }
        #endregion

        #region 树形图增删改方法
        //增添新地点方法
        private void TreeView_CreateItemData()
        {
            //显示输入窗口
            TextInputWindow.ShowWindow(newName =>
            {
                //计时
                using ExecutionTimer timer = new("新增地点数据", _timerDebugLogToggle.value);

                //以活跃选中项为可能的父级，创建数据
                LocUnitDataFile_Create(_activeData, newName);

                //刷新树形图
                TreeView_Refresh();
            },
            "创建新地点数据",
            "请输入新地点数据的文件名",
            "新地点数据文件名",
            "新地点",
            this
            );
        }
        //删除地点方法
        private void TreeView_DeleteItemData()
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
            using ExecutionTimer timer = new("移除地点数据", _timerDebugLogToggle.value);

            //获取当前所有选中项
            List<TreeViewItemData<LocUnitData>> selectedItems = _locUnitDataTreeView.selectedItems
                .Select(data => _itemDicCache[((LocUnitData)data).GetAssetHashCode()])
                .ToList();

            //获取顶级选中项
            List<TreeViewItemData<LocUnitData>> topLevelItems = GetTopLevelTreeViewItemData(selectedItems);

            //判断是否有选中项
            if (selectedItems.Count > 0)
            {
                //有选中项的情况下
                foreach (TreeViewItemData<LocUnitData> removedItem in topLevelItems)
                {
                    //删除
                    LocUnitDataFile_Delete(removedItem.data);
                }

                //刷新树形图
                TreeView_Refresh();
            }
        }
        //TreeView实现重命名
        private void TreeView_RenameItemData()
        {
            //检测活跃数据
            if (_activeData != null)
            {
                //显示输入窗口
                TextInputWindow.ShowWindow(newName =>
                {
                    //计时
                    using ExecutionTimer timer = new("重命名地点数据", _timerDebugLogToggle.value);

                    //检测新名称
                    if (newName.Equals(_activeData.name))
                    {
                        //若未发生更改，返回
                        return;
                    }

                    //更改文件名
                    LocUnitDataFile_Rename(_activeData, newName);

                    //刷新树形图
                    TreeView_Refresh();
                },
                "重命名地点数据",
                "请输入新的地点数据的名称",
                "新名称",
                _activeData.name,
                this
                );
            }
        }
        //剪切与复制操作
        private void TreeView_CutOrCopyItemData(bool isCutOperation)
        {
            //清除剪切板缓存
            _clipboardItemCache.Clear();
            //设定剪切
            _isCutOperation = isCutOperation;
            //获取所有选中数据
            List<TreeViewItemData<LocUnitData>> selectedItems = _locUnitDataTreeView.selectedItems
                .Select(data => _itemDicCache[((LocUnitData)data).GetAssetHashCode()])
                .ToList();
            //获取顶级数据
            List<TreeViewItemData<LocUnitData>> topLevelItems = GetTopLevelTreeViewItemData(selectedItems);
            //将顶级数据存储于缓存中
            _clipboardItemCache.AddRange(topLevelItems);
        }
        //粘贴操作
        private void TreeView_PasteItemData()
        {
            //判断剪切板数据
            if (_clipboardItemCache.Count == 0)
            {
                //若无数据，返回
                return;
            }

            //计时
            using ExecutionTimer timer = new("粘贴地点数据", _timerDebugLogToggle.value);

            //获取剪切板数据
            List<LocUnitData> _clipboardData = _clipboardItemCache.Select(item => item.data).ToList();

            //检测目标数据是否在剪切板中
            if (_clipboardData.Contains(_activeData))
            {
                //爆出警告并返回
                Debug.LogWarning("粘贴操作不允许将数据粘贴到自身上");
                return;
            }

            //检测是否为剪切
            if (_isCutOperation)
            {
                //若为剪切，则本次操作本质上为移动操作
                LocUnitDataFile_Move(_activeData, _clipboardData);
                //完成剪切后清除缓存数据
                _clipboardItemCache.Clear();
            }
            else
            {
                //若不是剪切，则为复制操作，遍历剪切板
                foreach (TreeViewItemData<LocUnitData> item in _clipboardItemCache)
                {
                    //以选中项为父级，原数据名称为新名称，原数据为源数据进行递归创建
                    LocUnitDataFile_Create(_activeData, item.data.name, item.data);
                }
            }

            //刷新树形图
            TreeView_Refresh();
        }
        #endregion

        #region 树形图拖动功能的实现
        //确认是否可以开始拖动操作
        private bool TreeView_OnCanStartDrag(CanStartDragArgs args)
        {
            //始终可以拖动
            return true;
        }
        //设置拖动和放置的参数
        private StartDragArgs TreeView_OnSetupDragAndDrop(SetupDragAndDropArgs args)
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
            startDragArgs.SetGenericData("地点数据源", _locUnitDataTreeView);
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
        private DragVisualMode TreeView_OnDragAndDropUpdate(HandleDragAndDropArgs args)
        {
            //任何情景下均为普通拖动
            return DragVisualMode.Move;
        }
        //拖动放置操作
        private DragVisualMode TreeView_OnHandleDrop(HandleDragAndDropArgs args)
        {
            //计时
            using ExecutionTimer timer = new("拖拽操作", _timerDebugLogToggle.value);

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
            LocUnitData targetData = _locUnitDataTreeView.GetItemDataForIndex<LocUnitData>(args.insertAtIndex);

            //提取选中物体中的顶层物体
            List<TreeViewItemData<LocUnitData>> topLevelDraggedItems = GetTopLevelTreeViewItemData
                (draggedIDs.Select(id => _itemDicCache[id]).ToList());

            //获取顶层数据
            List<LocUnitData> topLevelDatas = topLevelDraggedItems.Select(item => item.data).ToList();

            //进行移动操作
            LocUnitDataFile_Move(targetData, topLevelDatas);

            //刷新树形图
            TreeView_Refresh();

            //正确放置，返回空
            return DragVisualMode.None;
        }
        #endregion

        #region 数据编辑面板的初始化以及数据更新
        //初始化
        private void DataEditor_Init()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板初始化", _timerDebugLogToggle.value);

            //获取UI控件
            //面板
            _dataEditorPanel = rootVisualElement.Q<VisualElement>("DataEditorPanel");
            //基础项
            _packageField = _dataEditorPanel.Q<TextField>("PackageField");
            _authorFiled = _dataEditorPanel.Q<TextField>("AuthorField");
            _parentDataField = _dataEditorPanel.Q<ObjectField>("ParentDataField");
            //全名
            _fullNameLabel = _dataEditorPanel.Q<Label>("FullNameLabel");
            //设置控件
            _nameField = _dataEditorPanel.Q<TextField>("NameField");
            _descriptionField = _dataEditorPanel.Q<TextField>("DescriptionField");
            _backgroundField = _dataEditorPanel.Q<ObjectField>("BackgroundField");
        }
        //刷新面板
        private void DataEditor_Refresh(LocUnitData locUnitData)
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板刷新", _timerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //清除旧的绑定
            DataEditor_Unbind();

            //重新绑定
            DataEditor_Bind(locUnitData);

            //设置数据
            _fullNameLabel.text = string.Join("/", locUnitData.FullName);
            if (_backgroundField.value is Sprite sprite)
            {
                _dataEditorPanel.style.backgroundImage = new StyleBackground(sprite);
                _dataEditorPanel.style.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                _dataEditorPanel.style.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                _dataEditorPanel.style.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleAndCrop);
                _dataEditorPanel.style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleAndCrop);
            }
            else
            {
                _dataEditorPanel.style.backgroundImage = null;
            }
        }
        //绑定
        private void DataEditor_Bind(LocUnitData locUnitData)
        {
            //不触发通知的情况下更改数据
            _packageField.SetValueWithoutNotify(locUnitData.Package);
            _authorFiled.SetValueWithoutNotify(locUnitData.Author);
            _parentDataField.SetValueWithoutNotify(locUnitData.ParentData);
            _nameField.SetValueWithoutNotify(locUnitData.Name);
            _descriptionField.SetValueWithoutNotify(locUnitData.Description);
            _backgroundField.SetValueWithoutNotify(locUnitData.Background);

            //检测目标是否需要重新生成全名
            if (_dataNeedToReGenerateFullName.Contains(locUnitData))
            {
                //若是，重新生成
                locUnitData.Editor_GenerateFullName();
                //生成结束后移除
                _dataNeedToReGenerateFullName.Remove(locUnitData);
            }

            //将控件绑定至新数据上
            _packageField.RegisterValueChangedCallback(DataEditor_OnPackageChanged);
            _authorFiled.RegisterValueChangedCallback(DataEditor_OnAuthorChanged);
            _nameField.RegisterValueChangedCallback(DataEditor_OnNameChanged);
            _descriptionField.RegisterValueChangedCallback(DataEditor_OnDescriptionChanged);
            _backgroundField.RegisterValueChangedCallback(DataEditor_OnBackgroundChanged);
        }
        //清除绑定
        private void DataEditor_Unbind()
        {
            //将控件从旧数据清除绑定
            _packageField.UnregisterValueChangedCallback(DataEditor_OnPackageChanged);
            _authorFiled.UnregisterValueChangedCallback(DataEditor_OnAuthorChanged);
            _nameField.UnregisterValueChangedCallback(DataEditor_OnNameChanged);
            _descriptionField.UnregisterValueChangedCallback(DataEditor_OnDescriptionChanged);
            _backgroundField.UnregisterValueChangedCallback(DataEditor_OnBackgroundChanged);
        }
        //数据处理方法
        private void DataEditor_OnPackageChanged(ChangeEvent<string> evt)
        {
            _activeData.Editor_SetPackage(evt.newValue);
        }
        private void DataEditor_OnAuthorChanged(ChangeEvent<string> evt)
        {
            _activeData.Editor_SetAuthor(evt.newValue);
        }

        private void DataEditor_OnNameChanged(ChangeEvent<string> evt)
        {
            //更改数据
            _activeData.Editor_SetName(evt.newValue);
            //更改显示
            _fullNameLabel.text = string.Join("/", _activeData.FullName);
            //检查加上重命名全名标记
            MarkAsNeedToReGenerateFullName(_activeData);
        }

        private void DataEditor_OnDescriptionChanged(ChangeEvent<string> evt)
        {
            _activeData.Editor_SetDescription(evt.newValue);
        }

        private void DataEditor_OnBackgroundChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is Sprite sprite)
            {
                _activeData.Editor_SetBackground(sprite);
                _dataEditorPanel.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                _activeData.Editor_SetBackground(null);
            }
        }
        #endregion

        #region 连接编辑面板的初始化
        private void ConnectionEditor_Init()
        {
            //计时
            using ExecutionTimer timer = new("连接编辑面板初始化", _timerDebugLogToggle.value);

            //获取面板
            _connectionEditorPanel = rootVisualElement.Q<VisualElement>("ConnectionEditorPanel");

            //添加网格背景
            //新建网格背景
            IMGUIContainer gridContainer = new();
            //将其位置修改为绝对
            gridContainer.style.position = Position.Absolute;
            //设置宽高
            gridContainer.style.width = 2560;
            gridContainer.style.height = 1440;
            //绘制网格
            gridContainer.onGUIHandler = ConnectionEditor_DrawGrid;
            //添加到面板中去
            _connectionEditorPanel.Add(gridContainer);

            //对面板进行设置
            //设置为可延伸
            _connectionEditorPanel.style.flexGrow = 1;
            //监听事件
            _connectionEditorPanel.RegisterCallback<WheelEvent>(ConnectionEditor_OnMouseWheel);
            _connectionEditorPanel.RegisterCallback<MouseDownEvent>(ConnectionEditor_OnMouseDown);
            _connectionEditorPanel.RegisterCallback<MouseMoveEvent>(ConnectionEditor_OnMouseMove);
            _connectionEditorPanel.RegisterCallback<MouseUpEvent>(ConnectionEditor_OnMouseUp);
            _connectionEditorPanel.RegisterCallback<MouseLeaveEvent>(ConnectionEditor_OnMouseLeave);
            //_connectionEditorPanel.RegisterCallback<KeyDownEvent>(ConnectionEditor_OnKeyDown);
        }
        //绘制面板网格
        private void ConnectionEditor_DrawGrid()
        {
            //网格间隔
            float gridSpacing = 20f;
            //网格透明度
            float gridOpacity = 0.2f;
            //网格颜色
            Color gridColor = Color.gray;

            //计算需要绘制的网格线条总数
            int widthDivs = Mathf.CeilToInt(_connectionEditorPanel.contentRect.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(_connectionEditorPanel.contentRect.height / gridSpacing);

            //开始IMGUI绘制
            Handles.BeginGUI();
            //设置绘制颜色
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            //绘制垂直网格线
            for (int i = 0; i < widthDivs; i++)
            {
                //绘制线条，从一段到另一端
                Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, _connectionEditorPanel.contentRect.height, 0));
            }
            //水平
            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(_connectionEditorPanel.contentRect.width, gridSpacing * j, 0));
            }

            //重置绘制颜色
            Handles.color = Color.white;
            //结束IMGUI绘制
            Handles.EndGUI();
        }
        //鼠标滚轮事件
        private void ConnectionEditor_OnMouseWheel(WheelEvent evt)
        {
            //处理缩放
            //原缩放比例
            Vector3 scale = _connectionEditorPanel.transform.scale;
            //缩放调整因子
            float zoomFactor = 1.1f;
            //检测滚轮移动
            if (evt.delta.y > 0)
            {
                //若有移动，调整缩放因子
                zoomFactor = 1 / zoomFactor;
            }
            //调整缩放
            scale *= zoomFactor;
            //应用缩放
            _connectionEditorPanel.transform.scale = scale;
        }
        //鼠标拖放事件
        private void ConnectionEditor_OnMouseDown(MouseDownEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //当鼠标左键按下时
                //记录拖动起始位置
                _connectionEditorDragStart = evt.localMousePosition;
                //状态更改为拖放中
                _connectionEditorIsDragging = true;
            }
        }
        private void ConnectionEditor_OnMouseMove(MouseMoveEvent evt)
        {
            //检测拖放状态
            if (_connectionEditorIsDragging)
            {
                //若正在拖放
                //获取位置差值
                Vector2 delta = evt.localMousePosition - _connectionEditorDragStart;
                //变更位置
                _connectionEditorPanel.transform.position += (Vector3)delta;
            }
        }
        private void ConnectionEditor_OnMouseUp(MouseUpEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //当左键抬起时
                //取消拖拽状态
                _connectionEditorIsDragging = false;
            }
        }
        private void ConnectionEditor_OnMouseLeave(MouseLeaveEvent evt)
        {
            //停止拖拽
            _connectionEditorIsDragging = false;
        }
        //快捷键事件，聚焦功能
        //private void ConnectionEditor_OnKeyDown(KeyDownEvent evt)
        //{
        //    //检测按键
        //    if (evt.keyCode == KeyCode.Home)
        //    {
        //        //当Home键被按下时
        //        //获取当基础面板中心位置
        //        Vector3 centerPosition = new(_rightPanel.contentRect.width / 2, _rightPanel.contentRect.height / 2, 0);
        //        //设置中心位置
        //        _connectionEditorPanel.transform.position = centerPosition - new Vector3(_connectionEditorPanel.contentRect.width / 2, _connectionEditorPanel.contentRect.height / 2, 0);
        //        //更改缩放
        //        _connectionEditorPanel.transform.scale = Vector3.one;
        //    }
        //}
        #endregion

        #region 地点节点的创建与数据更新

        #endregion

        #region 编辑器面板下的地点数据增删改方法
        //编辑器面板下增加地点数据
        private void LocUnitDataFile_Create(LocUnitData parentData, string newDataName, LocUnitData originalData = null)
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
                newData.Editor_SetPackage(_defaultPackageField.text);
                newData.Editor_SetCategory("Location");
                newData.Editor_SetAuthor(_defaultAuthorField.text);
            }
            //更改文件名
            newData.name = newDataName;
            //更改父级
            newData.Editor_SetParent(parentData);
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
                _childrenDicCache[parentData.GetAssetHashCode()].Add(newItem);
                //重排
                _childrenDicCache[parentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            else
            {
                //若为空，则认定为顶级数据
                //添加到顶级数据中
                _rootItemCache.Add(newItem);
                //重排
                _rootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            //添加到其他缓存中
            _itemDicCache[newData.GetAssetHashCode()] = newItem;
            _childrenDicCache[newData.GetAssetHashCode()] = newChildren;

            //重构树形图
            TreeView_Refresh();

            //判断源文件是否存在
            if (originalData != null)
            {
                //若存在，判断其对应的数据项是否有子级
                if (_itemDicCache[originalData.GetAssetHashCode()].hasChildren)
                {
                    //若有，对子级进行递归创建
                    foreach (TreeViewItemData<LocUnitData> childItem in _childrenDicCache[originalData.GetAssetHashCode()])
                    {
                        //此时的父数据为新创建的数据，新名称为原名称，源数据为自身
                        LocUnitDataFile_Create(newData, childItem.data.name, childItem.data);
                    }
                }
            }
        }
        //编辑器面板中删除地点数据
        private void LocUnitDataFile_Delete(LocUnitData deletedData)
        {
            //判断传入是否为空
            if (deletedData == null)
            {
                //若为空，返回
                return;
            }
            //若不为空
            TreeViewItemData<LocUnitData> item = _itemDicCache[deletedData.GetAssetHashCode()];

            //将数据自身从缓存中移除
            //将数据从其父级中删除
            if (item.data.ParentData != null)
            {
                //若其父级不为空，则删除父级的子级数据
                _childrenDicCache[item.data.ParentData.GetAssetHashCode()].Remove(item);
                //重排
                _childrenDicCache[item.data.ParentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            //其他缓存
            _rootItemCache.Remove(item);
            _itemDicCache.Remove(item.id);
            _childrenDicCache.Remove(item.id);
            _expandedStateCache.Remove(item.id);
            _dataNeedToReGenerateFullName.Remove(item.data);

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
            TreeView_Refresh();

            //判断是否有子级
            if (item.hasChildren)
            {
                //若有子级，对子级进行递归删除
                foreach (TreeViewItemData<LocUnitData> childItem in item.children)
                {
                    LocUnitDataFile_Delete(childItem.data);
                }
            }
        }
        //编辑器面板中重命名物体数据
        private void LocUnitDataFile_Rename(LocUnitData renamedData, string newDataName)
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
            //重命名文件及文件夹
            AssetDatabase.RenameAsset(assetPath, newDataName);
            AssetDatabase.RenameAsset(assetFolderPath, newDataName);

            //标记为脏
            EditorUtility.SetDirty(renamedData);

            //保存更改
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //更名结束后对缓存中的数据进行重排
            if (renamedData.ParentData != null)
            {
                //若有父级，重排其父级的子级
                _childrenDicCache[renamedData.ParentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }
            else
            {
                //若无父级，重排根数据子级
                _rootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
            }

            //重构树形图
            TreeView_Refresh();
        }
        //编辑器面板中移动物体数据
        private void LocUnitDataFile_Move(LocUnitData targetData, List<LocUnitData> topLevelMovedDatas)
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
                //源文件夹
                string sourceFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(movedData));
                //目标父级文件夹
                string targetFolderPath;

                //更新缓存
                //获取被移动的数据对应的树形图物体
                TreeViewItemData<LocUnitData> movedItem = _itemDicCache[movedData.GetAssetHashCode()];
                //判断数据的旧父级
                if (movedData.ParentData != null)
                {
                    //若有，则将数据从旧父级的子级列表中移除
                    _childrenDicCache[movedData.ParentData.GetAssetHashCode()].Remove(movedItem);
                    //重排
                    _childrenDicCache[movedData.ParentData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
                }
                else
                {
                    //若无，说明为顶层，从根数据缓存中移除
                    _rootItemCache.Remove(movedItem);
                    //重排
                    _rootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
                }
                //设置新的父级
                movedData.Editor_SetParent(targetData);
                //判断目标对象
                if (targetData != null)
                {
                    //如果有目标对象，则将数据作为目标的子级
                    _childrenDicCache[targetData.GetAssetHashCode()].Add(movedItem);
                    //重排
                    _childrenDicCache[targetData.GetAssetHashCode()].Sort((x, y) => x.data.name.CompareTo(y.data.name));
                    //并设置父级文件夹
                    targetFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetData));
                }
                else
                {
                    //如果没有目标对象，则添加到根数据缓存中
                    _rootItemCache.Add(movedItem);
                    //重排
                    _rootItemCache.Sort((x, y) => x.data.name.CompareTo(y.data.name));
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
                        LocUnitDataFile_Rename(movedData, newName);
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
            TreeView_Refresh();
        }
        #endregion

        #region 文件存储与读取方法
        //保存树形图展开状态
        private void SaveExpandedState(TreeViewExpansionChangedArgs args)
        {
            //获取参数中的ID
            int id = args.id;
            //保存或删除
            if (_locUnitDataTreeView.IsExpanded(id))
            {
                //若为打开，添加到缓存
                _expandedStateCache.Add(id);
            }
            else
            {
                //若为关闭，判断其是否存在于数据中并移除
                if (_expandedStateCache.Contains(id))
                {
                    _expandedStateCache.Remove(id);
                }
            }
        }
        //恢复树形图展开状态
        private void RestoreExpandedState()
        {
            //根据缓存数据进行展开
            foreach (int id in _expandedStateCache)
            {
                //判断ID是否存在
                if (_itemDicCache.ContainsKey(id))
                {
                    //若存在，展开
                    _locUnitDataTreeView.ExpandItem(id);
                }
            }
        }
        //保存缓存到永久性存储文件
        public void SavePersistentData()
        {
            //生成永久性实例
            LocUnitDataEditorPersistentData persistentData = new()
            {
                //设置其数值
                DefaultPackage = _defaultPackageField.text,
                DefaultAuthor = _defaultAuthorField.text,
                TimerDebugLogState = _timerDebugLogToggle.value,
                IsDataEditorPanelOpen = _isDataEditorPanelOpen,
                ExpandedState = _expandedStateCache.ToList(),
            };
            //将永久性存储实例转化为文本
            string jsonString = JsonUtility.ToJson(persistentData, prettyPrint: true);
            //写入文件中
            File.WriteAllText(AssetDatabase.GetAssetPath(_persistentDataFile), jsonString);
            //标记为脏
            EditorUtility.SetDirty(_persistentDataFile);
            //保存更改
            AssetDatabase.SaveAssets();
            //刷新数据
            AssetDatabase.Refresh();
        }
        //读取永久性存储文件到缓存
        private void LoadPersistentData()
        {
            //读取文件中数据
            string jsonString = File.ReadAllText(AssetDatabase.GetAssetPath(_persistentDataFile));
            //生成永久性存储实例
            LocUnitDataEditorPersistentData persistentData = JsonUtility.FromJson<LocUnitDataEditorPersistentData>(jsonString);
            //分配数据
            //属性
            _defaultPackageField.SetValueWithoutNotify(persistentData.DefaultPackage);
            _defaultAuthorField.SetValueWithoutNotify(persistentData.DefaultAuthor);
            _timerDebugLogToggle.SetValueWithoutNotify(persistentData.TimerDebugLogState);
            _isDataEditorPanelOpen = persistentData.IsDataEditorPanelOpen;
            //缓存
            //展开状态数据
            _expandedStateCache.Clear();
            foreach (int id in persistentData.ExpandedState)
            {
                _expandedStateCache.Add(id);
            }
        }
        #endregion

        #region 辅助方法
        //获取数据理论存储地址，同时承担理论地址为空时进行创建
        private string GetDataFolderPath(LocUnitData locUnitData)
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
        //获取顶级物体列表，顶级物体指的是，该树形图物体列表中，其任意一级父级均不在列表中的物体
        private List<TreeViewItemData<LocUnitData>> GetTopLevelTreeViewItemData(List<TreeViewItemData<LocUnitData>> items)
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
                    TreeViewItemData<LocUnitData> parentItem = _itemDicCache[parentLocUnitData.GetAssetHashCode()];
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
        //重新生成全名
        private void MarkAsNeedToReGenerateFullName(LocUnitData locUnitData)
        {
            //检测数据是否为空
            if (locUnitData == null)
            {
                //返回
                return;
            }

            //数据自身生成添加
            _dataNeedToReGenerateFullName.Add(locUnitData);

            //检查是否拥有子级
            if (_itemDicCache[locUnitData.GetAssetHashCode()].hasChildren)
            {
                //若有，则对子级进行同样操作
                foreach (var child in _childrenDicCache[locUnitData.GetAssetHashCode()])
                {
                    MarkAsNeedToReGenerateFullName(child.data);
                }
            }
        }
        //切换面板
        private void SwitchPanel()
        {
            //检测面板打开状态
            if (_isDataEditorPanelOpen)
            {
                //若数据编辑面板为打开，则关闭，并打开连接编辑面板
                _dataEditorPanel.style.display = DisplayStyle.None;
                _connectionEditorPanel.style.display = DisplayStyle.Flex;
                _isDataEditorPanelOpen = false;
            }
            else
            {
                //反之反之，并刷新面板
                _dataEditorPanel.style.display = DisplayStyle.Flex;
                _connectionEditorPanel.style.display = DisplayStyle.None;
                _isDataEditorPanelOpen = true;
                DataEditor_Refresh(_activeData);
            }
        }
        #endregion
    }
}


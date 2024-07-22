using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.LocationSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor
{
    public class LocUnitDataEditorWindow : EditorWindow
    {
        #region 基础构成
        //UXML文件
        [SerializeField]
        private VisualTreeAsset _visualTree;
        //永久性存储文件
        [SerializeField]
        private TextAsset _persistentDataFile;

        //UI元素
        //包、分类、作者输入框
        private TextField _packageTextField;
        private TextField _categoryTextField;
        private TextField _authorTextField;
        //树形图
        private TreeView _locUnitDataTreeView;

        //数据存储
        //根数据缓存
        private readonly List<TreeViewItemData<LocUnitData>> _rootItemCache = new();
        //ID-地点查询字典缓存
        private readonly Dictionary<int, TreeViewItemData<LocUnitData>> _itemDicCache = new();
        //ID-子级查询字典缓存
        private readonly Dictionary<int, List<TreeViewItemData<LocUnitData>>> _childrenDicCache = new();
        //展开状态缓存
        private readonly HashSet<int> _expandedStateCache = new();
        //剪切板缓存
        private readonly List<TreeViewItemData<LocUnitData>> _clipboardItemCache = new();
        private bool _isCutOperation = false;

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

            //属性面板
            //获取与绑定
            _packageTextField = rootVisualElement.Q<TextField>("PackageTextField");
            _categoryTextField = rootVisualElement.Q<TextField>("CategoryTextField");
            _authorTextField = rootVisualElement.Q<TextField>("AuthorTextField");

            //左侧面板
            //初始化树形图面板
            InitTreeViewData();

            //右侧面板
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //保存持久化数据到磁盘
            SavePersistentData();
            //提醒修改可寻址资源包标签
            Debug.LogWarning("窗口已被关闭，请注意修改新增数据的可寻址资源包的Key。");
        }
        #endregion

        #region 树形图的初始化及数据更新
        //初始化树形结构
        private void InitTreeViewData()
        {
            //计时
            using ExecutionTimer timer = new("地点数据管理面板初始化");

            //获取树形图面板
            _locUnitDataTreeView = rootVisualElement.Q<TreeView>("LocUnitTreeView");
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
            GenerateTreeView();

            //TreeView中实现拖动逻辑
            //是否允许拖动设置
            _locUnitDataTreeView.canStartDrag += OnCanStartDrag;
            //开始拖动
            _locUnitDataTreeView.setupDragAndDrop += OnSetupDragAndDrop;
            //拖动更新
            _locUnitDataTreeView.dragAndDropUpdate += OnDragAndDropUpdate;
            //拖动结束
            _locUnitDataTreeView.handleDrop += OnHandleDrop;

            //实现展开状态保存
            _locUnitDataTreeView.itemExpandedChanged += SaveExpandedState;

            //实现双击重命名
            _locUnitDataTreeView.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (evt.clickCount == 2)
                {
                    RenameTreeViewItemData();
                }
            });

            //注册快捷键
            _locUnitDataTreeView.RegisterCallback<KeyDownEvent>(RegisterShortcutKey);

            //注册右键菜单
            RegisterTreeViewContextMenu();

            //读取持久化数据
            LoadPersistentData();
        }
        //刷新树形图面板
        private void RefreshLocUnitDataTreeView()
        {
            //设置数据源并重建
            _locUnitDataTreeView.SetRootItems(_rootItemCache);
            _locUnitDataTreeView.Rebuild();
            //恢复展开状态
            RestoreExpandedState();
        }
        //生成树形结构数据方法
        private void GenerateTreeView()
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
            RefreshLocUnitDataTreeView();
        }
        //注册树形图右键菜单方法
        private void RegisterTreeViewContextMenu()
        {
            _locUnitDataTreeView.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                //添加方法
                evt.menu.AppendAction("添加地点数据\tInsert", action => CreateTreeViewItemData(), DropdownMenuAction.AlwaysEnabled);
                //移除方法
                evt.menu.AppendAction("删除地点数据\tDel", action => DeleteTreeViewItemData(), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //重命名方法
                evt.menu.AppendAction("重命名地点数据\tF2", action => RenameTreeViewItemData(), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });

                //分割线
                evt.menu.AppendSeparator();

                //剪切方法
                evt.menu.AppendAction("剪切\tCtrl + X", action => CutOrCopyTreeViewItemData(isCutOperation: true), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //复制方法
                evt.menu.AppendAction("复制\tCtrl + C", action => CutOrCopyTreeViewItemData(isCutOperation: false), actionStatus =>
                {
                    //判断是否有选择
                    return _locUnitDataTreeView.selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
                //粘贴方法
                evt.menu.AppendAction("粘贴\tCtrl + V", action => PasteTreeViewItemData(), actionStatus =>
                {
                    //判断剪切板是否有数据
                    return _clipboardItemCache.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
            }));
        }
        //注册快捷键方法
        private void RegisterShortcutKey(KeyDownEvent e)
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
                        CutOrCopyTreeViewItemData(isCutOperation: true);
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
                        CutOrCopyTreeViewItemData(isCutOperation: false);
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
                        PasteTreeViewItemData();
                    }
                    //阻断事件传播
                    e.StopImmediatePropagation();
                }
            }
            else if (e.keyCode == KeyCode.Insert)
            {
                //若按键为Insert，执行添加方法
                CreateTreeViewItemData();
                //阻断事件传播
                e.StopImmediatePropagation();
            }
            else if (e.keyCode == KeyCode.Delete)
            {
                //若按键为Delete，检测是否有选中
                if (_locUnitDataTreeView.selectedIndices.Any())
                {
                    //若有选中，执行移除操作
                    DeleteTreeViewItemData();
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
                    RenameTreeViewItemData();
                }
                //阻断事件传播
                e.StopImmediatePropagation();
            }
        }
        #endregion

        #region 树形图增删改方法
        //增添新地点方法
        private void CreateTreeViewItemData()
        {
            //显示输入窗口
            TextInputWindow.ShowWindow(newName =>
            {
                //计时
                using ExecutionTimer timer = new("新增地点数据");

                //获取当次选中项
                LocUnitData selectedData = _locUnitDataTreeView.selectedIndices
                    .Select(index => _locUnitDataTreeView.GetItemDataForIndex<LocUnitData>(index))
                    .FirstOrDefault();

                //创建数据
                CreateLocUnitData(selectedData, newName);

                //刷新树形图
                RefreshLocUnitDataTreeView();
            },
            "创建新地点数据",
            "请输入新地点数据的文件名",
            "新地点数据文件名",
            "新地点",
            this
            );
        }
        //删除地点方法
        private void DeleteTreeViewItemData()
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
            using ExecutionTimer timer = new("移除地点数据");

            //获取当前选中项
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
                    DeleteLocUnitData(removedItem.data);
                }

                //刷新树形图
                RefreshLocUnitDataTreeView();
            }
        }
        //TreeView实现重命名
        private void RenameTreeViewItemData()
        {
            //获取选中项
            LocUnitData selectedData = _locUnitDataTreeView.selectedIndices
                .Select(index => _locUnitDataTreeView.GetItemDataForIndex<LocUnitData>(index))
                .FirstOrDefault();

            //检测选中项
            if (selectedData != null)
            {
                //显示输入窗口
                TextInputWindow.ShowWindow(newName =>
                {
                    //计时
                    using ExecutionTimer timer = new("重命名地点数据");

                    //检测新名称
                    if (newName.Equals(selectedData.name))
                    {
                        //若未发生更改，返回
                        return;
                    }

                    //更改文件名
                    RenameLocUnitData(selectedData, newName);

                    //刷新树形图
                    RefreshLocUnitDataTreeView();
                },
                "重命名地点数据",
                "请输入新的地点数据的名称",
                "新名称",
                selectedData.name,
                this
                );
            }
        }
        //剪切与复制操作
        private void CutOrCopyTreeViewItemData(bool isCutOperation)
        {
            //清除剪切板缓存
            _clipboardItemCache.Clear();
            //设定剪切
            _isCutOperation = isCutOperation;
            //获取选中数据
            List<TreeViewItemData<LocUnitData>> selectedItems = _locUnitDataTreeView.selectedItems
                .Select(data => _itemDicCache[((LocUnitData)data).GetAssetHashCode()])
                .ToList();
            //获取顶级数据
            List<TreeViewItemData<LocUnitData>> topLevelItems = GetTopLevelTreeViewItemData(selectedItems);
            //将顶级数据存储于缓存中
            _clipboardItemCache.AddRange(topLevelItems);
        }
        //粘贴操作
        private void PasteTreeViewItemData()
        {
            //判断剪切板数据
            if (_clipboardItemCache.Count == 0)
            {
                //若无数据，返回
                return;
            }

            //计时
            using ExecutionTimer timer = new("粘贴地点数据");

            //获取目标数据
            LocUnitData targetData = _locUnitDataTreeView.selectedIndices
                .Select(index => _locUnitDataTreeView.GetItemDataForIndex<LocUnitData>(index))
                .FirstOrDefault();

            //获取剪切板数据
            List<LocUnitData> _clipboardData = _clipboardItemCache.Select(item => item.data).ToList();

            //检测目标数据是否在剪切板中
            if (_clipboardData.Contains(targetData))
            {
                //爆出警告并返回
                Debug.LogWarning("粘贴操作不允许将数据粘贴到自身上");
                return;
            }

            //检测是否为剪切
            if (_isCutOperation)
            {
                //若为剪切，则本次操作本质上为移动操作
                MoveLocUnitData(targetData, _clipboardData);
                //完成剪切后清除缓存数据
                _clipboardItemCache.Clear();
            }
            else
            {
                //若不是剪切，则为复制操作，遍历剪切板
                foreach (TreeViewItemData<LocUnitData> item in _clipboardItemCache)
                {
                    //以选中项为父级，原数据名称为新名称，原数据为源数据进行递归创建
                    CreateLocUnitData(targetData, item.data.name, item.data);
                }
            }

            //刷新树形图
            RefreshLocUnitDataTreeView();
        }
        #endregion

        #region 编辑器面板下的地点数据增删改方法
        //编辑器面板下增加地点数据
        private void CreateLocUnitData(LocUnitData parentData, string newDataName, LocUnitData originalData = null)
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
                newData.Editor_SetPackage(_packageTextField.text);
                newData.Editor_SetCategory(_categoryTextField.text);
                newData.Editor_SetAuthor(_authorTextField.text);
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
            RefreshLocUnitDataTreeView();

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
                        CreateLocUnitData(newData, childItem.data.name, childItem.data);
                    }
                }
            }
        }
        //编辑器面板中删除地点数据
        private void DeleteLocUnitData(LocUnitData deletedData)
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
            RefreshLocUnitDataTreeView();

            //判断是否有子级
            if (item.hasChildren)
            {
                //若有子级，对子级进行递归删除
                foreach (TreeViewItemData<LocUnitData> childItem in item.children)
                {
                    DeleteLocUnitData(childItem.data);
                }
            }
        }
        //编辑器面板中重命名物体数据
        private void RenameLocUnitData(LocUnitData renamedData, string newDataName)
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
            RefreshLocUnitDataTreeView();
        }
        //编辑器面板中移动物体数据
        private void MoveLocUnitData(LocUnitData targetData, List<LocUnitData> topLevelMovedDatas)
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
                        RenameLocUnitData(movedData, newName);
                        //更改原地址
                        sourceFolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(movedData));
                    }

                    //移动文件夹
                    AssetDatabase.MoveAsset(sourceFolderPath, finalFolderPath);
                }

                //标记资源为脏
                EditorUtility.SetDirty(movedData);
            }

            //保存更改
            AssetDatabase.SaveAssets();
            //刷新资源视图
            AssetDatabase.Refresh();

            //刷新树形图
            RefreshLocUnitDataTreeView();
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
        private DragVisualMode OnDragAndDropUpdate(HandleDragAndDropArgs args)
        {
            //任何情景下均为普通拖动
            return DragVisualMode.Move;
        }
        //拖动放置操作
        private DragVisualMode OnHandleDrop(HandleDragAndDropArgs args)
        {
            //计时
            using ExecutionTimer timer = new("拖拽操作");

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
            MoveLocUnitData(targetData, topLevelDatas);

            //刷新树形图
            RefreshLocUnitDataTreeView();

            //正确放置，返回空
            return DragVisualMode.None;
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
                Package = _packageTextField.text,
                Category = _categoryTextField.text,
                Author = _authorTextField.text,
                ExpandedState = _expandedStateCache.ToList()
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
            _packageTextField.SetValueWithoutNotify(persistentData.Package);
            _categoryTextField.SetValueWithoutNotify(persistentData.Category);
            _authorTextField.SetValueWithoutNotify(persistentData.Author);
            //缓存
            //展开状态数据
            _expandedStateCache.Clear();
            foreach (int id in persistentData.ExpandedState)
            {
                _expandedStateCache.Add(id);
            }
            RestoreExpandedState();
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
                    //若无父级，则检测分类属性栏是否有数据
                    if (!string.IsNullOrEmpty(_categoryTextField.text))
                    {
                        //若有，扩展路径
                        assetFolderPath += $"\\{_categoryTextField.text}";
                    }
                }

                //扩展自身地址为父级路径加上子级名称文件夹
                assetFolderPath += $"\\{locUnitData.name}";
            }
            else
            {
                //若传入为空，则检测属性栏是否有数据
                if (!string.IsNullOrEmpty(_categoryTextField.text))
                {
                    //若有，扩展路径
                    assetFolderPath += $"\\{_categoryTextField.text}";
                }
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
        #endregion
    }
}


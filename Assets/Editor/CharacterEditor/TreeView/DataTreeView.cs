using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.CharacterSystem;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.CharacterEditor
{
    public class DataTreeView : TreeView
    {
        #region 构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //根缓存
        public List<TreeViewItemData<ItemDataContainer>> RootItemCache { get; } = new();
        //系列-组织缓存
        public Dictionary<int, List<TreeViewItemData<ItemDataContainer>>> SeriesAffiliationDicCache { get; } = new();
        //组织-角色缓存
        public Dictionary<int, List<TreeViewItemData<ItemDataContainer>>> AffiliationCharacterDicCache { get; } = new();
        //角色-版本缓存
        public Dictionary<int, List<TreeViewItemData<ItemDataContainer>>> CharacterVersionDicCache { get; } = new();
        //ID-数据缓存
        public Dictionary<int, TreeViewItemData<ItemDataContainer>> ItemDicCache { get; } = new();

        //名称-排序永久性存储
        public Dictionary<string, int> StringSortingOrderPersistentData { get; } = new();
        //展开状态永久性存储
        public HashSet<int> ExpandedStatePersistentData { get; private set; } = new();

        //当前活跃选中项
        public ItemDataContainer ActiveSelection { get; private set; }
        #endregion

        #region 构造函数与刷新与初始化
        //构造函数
        public DataTreeView(MainWindow window)
        {
            //赋值
            MainWindow = window;

            //更改自身样式为我全都要
            style.width = new Length(100, LengthUnit.Percent);
            style.height = new Length(100, LengthUnit.Percent);

            //更改颜色为皇帝的新衣
            style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0));

            //初始化
            Init();
        }
        //刷新方法
        public void TRefresh()
        {
            //重设数据源
            SetRootItems(RootItemCache);
            //重建
            Rebuild();
            //恢复展开状态
            RestoreExpandedState();
        }
        //初始化方法
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("角色树形图初始化", MainWindow.TimerDebugLogToggle.value);

            //读取数据
            LoadPersistentData();

            //总之先生成物体
            GenerateItems();

            //然后事件绑定
            RegisterEvents();
        }
        //生成物体方法
        private void GenerateItems()
        {
            //清除现有缓存
            RootItemCache.Clear();
            SeriesAffiliationDicCache.Clear();
            AffiliationCharacterDicCache.Clear();
            CharacterVersionDicCache.Clear();
            ItemDicCache.Clear();

            //数据存储路径
            string rootPath = "Assets/GameData/Character";

            //读取文件中所有的CharacterData类
            List<CharacterData> characterDatas = AssetDatabase.FindAssets("t:CharacterData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            //构建数据结构
            //总字典
            Dictionary<string, Dictionary<string, Dictionary<string, List<CharacterData>>>> totalDataDic = new();
            //构建字典，此处依靠文件夹构建，以免出现空文件夹所代表的数据未被识别的情况
            if (AssetDatabase.IsValidFolder(rootPath))
            {
                //若存在，获取第一级的所有文件夹地址，也就是系列作品文件夹
                string[] firstLevelDirs = AssetDatabase.GetSubFolders(rootPath);
                //并对其进行遍历
                foreach (string firstLevelDir in firstLevelDirs)
                {
                    //从地址中获取名称
                    string firstLevelName = Path.GetFileName(firstLevelDir);
                    //并创建第一级对应的值，也就是包含了第二级的字典
                    Dictionary<string, Dictionary<string, List<CharacterData>>> secondLevelDic = new();

                    //获取第二级所有文件夹地址，也就是组织文件夹
                    string[] secondLevelDirs = AssetDatabase.GetSubFolders(firstLevelDir);
                    //遍历
                    foreach (var secondLevelDir in secondLevelDirs)
                    {
                        //从地址中获取名称
                        string secondLevelName = Path.GetFileName(secondLevelDir);
                        //创建第二级对应的值，也就是包含了第三级的字典
                        Dictionary<string, List<CharacterData>> thirdLevelDic = new();

                        //获取第三级所有文件夹地址，也就是角色名文件夹
                        string[] thirdLevelDirs = AssetDatabase.GetSubFolders(secondLevelDir);
                        //遍历
                        foreach (string thirdLevelDir in thirdLevelDirs)
                        {
                            //获取第三级文件夹地址名称
                            string thirdLevelName = Path.GetFileName(thirdLevelDir);
                            //创建第三级对应的值，也就是那个列表
                            List<CharacterData> targetDatas = new();
                            //添加到字典中
                            thirdLevelDic[thirdLevelName] = targetDatas;
                        }
                        //添加到字典中
                        secondLevelDic[secondLevelName] = thirdLevelDic;
                    }
                    //添加到字典中
                    totalDataDic[firstLevelName] = secondLevelDic;
                }
            }
            //填充字典，如果是从编辑器创建的数据，那么此处应该是正确无误的
            foreach (CharacterData characterData in characterDatas)
            {
                //添加数据
                totalDataDic[characterData.OriginatingSeries][characterData.Affiliation][characterData.Name].Add(characterData);
            }

            //随后开始构建树形图物体
            //遍历第一层
            foreach (var series in totalDataDic)
            {
                //获取字符串排序
                int seriesSortingOrder;
                if (StringSortingOrderPersistentData.ContainsKey(series.Key))
                {
                    seriesSortingOrder = StringSortingOrderPersistentData[series.Key];
                }
                else
                {
                    seriesSortingOrder = RootItemCache.Count + 1;
                }
                //创造物体容器
                ItemDataContainer seriesItemDataContainer = new(
                    series.Key,
                    ItemDataContainer.ItemType.OriginatingSeries,
                    null,
                    seriesSortingOrder
                    );
                //创建系列名称的子级的集合，即存储组织名称的地方
                List<TreeViewItemData<ItemDataContainer>> seriesChildren = new();
                //创建物体
                TreeViewItemData<ItemDataContainer> seriesItem = new(
                    series.Key.GetHashCode(),
                    seriesItemDataContainer,
                    seriesChildren
                    );
                //设置对应缓存
                SeriesAffiliationDicCache[series.Key.GetHashCode()] = seriesChildren;
                //添加到总集缓存与一层缓存中
                ItemDicCache[series.Key.GetHashCode()] = seriesItem;
                RootItemCache.Add(seriesItem);

                //填充其子级
                //遍历该数据的第二层
                foreach (var affiliation in series.Value)
                {
                    //获取字符串排序
                    int affiliationSortingOrder;
                    if (StringSortingOrderPersistentData.ContainsKey(affiliation.Key))
                    {
                        affiliationSortingOrder = StringSortingOrderPersistentData[affiliation.Key];
                    }
                    else
                    {
                        affiliationSortingOrder = seriesChildren.Count + 1;
                    }
                    //创建物体容器
                    ItemDataContainer affiliationItemDataContainer = new(
                        affiliation.Key,
                        ItemDataContainer.ItemType.Affiliation,
                        seriesItemDataContainer,
                        affiliationSortingOrder
                        );
                    //创建组织名称的子级的集合，即存储角色名称的地方
                    List<TreeViewItemData<ItemDataContainer>> affiliationChildren = new();
                    //创建物体
                    TreeViewItemData<ItemDataContainer> affiliationItem = new(
                        affiliation.Key.GetHashCode(),
                        affiliationItemDataContainer,
                        affiliationChildren
                        );
                    //添加到上层子级中
                    seriesChildren.Add(affiliationItem);
                    //对对应的存储角色名称的地方设定为缓存
                    AffiliationCharacterDicCache[affiliation.Key.GetHashCode()] = affiliationChildren;
                    //添加到总集缓存与存储组织名称的地方的缓存中
                    ItemDicCache[affiliation.Key.GetHashCode()] = affiliationItem;

                    //填充其子级
                    //遍历该数据的第三层，即角色名称
                    foreach (var character in affiliation.Value)
                    {
                        //获取字符串的排序
                        int characterSortingOrder;
                        if (StringSortingOrderPersistentData.ContainsKey(character.Key))
                        {
                            characterSortingOrder = StringSortingOrderPersistentData[character.Key];
                        }
                        else
                        {
                            characterSortingOrder = affiliationChildren.Count + 1;
                        }
                        //创建角色名称物体数据容器
                        ItemDataContainer characterItemDataContainer = new(
                            character.Key,
                            ItemDataContainer.ItemType.CharacterName,
                            affiliationItemDataContainer,
                            characterSortingOrder
                            );
                        //创建其对应的树状图物体的子级数据
                        List<TreeViewItemData<ItemDataContainer>> characterChildren = new();
                        //创建角色名称物体
                        TreeViewItemData<ItemDataContainer> characterItem = new(
                            character.Key.GetHashCode(),
                            characterItemDataContainer,
                            characterChildren
                            );
                        //添加到上层子级中
                        affiliationChildren.Add(characterItem);
                        //设置对应的存储不同版本的实际角色数据的地方为缓存
                        CharacterVersionDicCache[character.Key.GetHashCode()] = characterChildren;
                        //添加到总集缓存中
                        ItemDicCache[character.Key.GetHashCode()] = characterItem;

                        //填充其子级集合
                        //遍历最底层数据，即存储着不同版本角色数据的地方
                        foreach (var version in character.Value)
                        {
                            //创建物体容器
                            ItemDataContainer versionItemContainer = new(
                                version,
                                characterItemDataContainer
                                );
                            //创建物体
                            TreeViewItemData<ItemDataContainer> versionItem = new(
                                version.GetAssetHashCode(),
                                versionItemContainer,
                                null
                                );
                            //添加到上层子级中
                            characterChildren.Add(versionItem);
                            //添加到总集缓存中
                            ItemDicCache[version.GetAssetHashCode()] = versionItem;
                        }
                        //结束后对子级集合进行排序
                        characterChildren.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
                    }
                    //结束之后对角色数据集合进行排序
                    affiliationChildren.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
                }
                //结束之后对组织名称集合进行排序
                seriesChildren.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
            }
            //对一层缓存进行排序
            RootItemCache.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));

            //构建完成后设置树形图根物体
            SetRootItems(RootItemCache);

            //自定义显示逻辑
            //物体采用普通标签显示
            makeItem = () =>
            {
                Label label = new();
                label.AddToClassList("treeview-item-character");
                return label;
            };
            //绑定逻辑
            bindItem = (element, i) =>
            {
                //获取数据与物体展示形态
                ItemDataContainer itemDataContainer = GetItemDataForIndex<ItemDataContainer>(i);
                Label label = element as Label;

                //设置展示形态显示内容
                if (itemDataContainer.Type == ItemDataContainer.ItemType.CharacterData)
                {
                    //若为角色数据，则设置显示名称为版本名
                    label.text = $"{itemDataContainer.CharacterData.Version}";
                }
                else
                {
                    //若不是，则显示名称为字段名称
                    label.text = itemDataContainer.StringData;
                }
            };

            //结束之后刷新
            TRefresh();
        }
        #endregion

        #region 鼠标与键盘与事件绑定
        //一个把大家聚集到一起的方法
        private void RegisterEvents()
        {
            //首先绑定的是，当选择发生更改时，更改活跃选中项
            selectionChanged += (selections) =>
            {
                ItemDataContainer newSelection = selectedItems.Cast<ItemDataContainer>().FirstOrDefault();
                if (newSelection != null)
                {
                    ActiveSelection = newSelection;
                    MainWindow.DataEditorPanel.DRefresh();
                }
            };

            //然后绑定的是，展开状态发生更改时，记录
            itemExpandedChanged += SaveExpandedState;

            //右键菜单
            RegisterContextMenu();

            //快捷键
            RegisterCallback<KeyDownEvent>(RegisterShortcutKey);

            //取消选择的方法
            RegisterCallback<PointerDownEvent>(OnPointerDown);
        }
        //首先入场的是，Ctrl+鼠标左键点击取消选择
        private void OnPointerDown(PointerDownEvent evt)
        {
            //判断Ctrl键是否被按下
            if (evt.ctrlKey)
            {
                //判断左键是否被按下
                if (evt.button == 0)
                {
                    //若是，获取当前选中项
                    ItemDataContainer newSelection = selectedItems.Cast<ItemDataContainer>().FirstOrDefault();
                    //判断与活跃选中项是否相同
                    if (newSelection == ActiveSelection)
                    {
                        //若是，取消选中
                        SetSelection(new int[0]);
                        ActiveSelection = null;
                    }
                }
            }
        }
        //接下来入场的是，右键菜单
        private void RegisterContextMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                //添加方法
                evt.menu.AppendAction("添加数据\tInsert", action => CreateData(), DropdownMenuAction.AlwaysEnabled);
                //删除方法
                evt.menu.AppendAction("删除数据\tDel", action => DeleteData(), actionStatus =>
                {
                    //判断是否有选择
                    return selectedIndices.Any() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
                });
            }));
        }
        //最后入场的是，快捷键
        private void RegisterShortcutKey(KeyDownEvent evt)
        {
            //检测按键
            if (evt.keyCode == KeyCode.Insert)
            {
                //若为Insert，执行添加方法
                CreateData();
                //阻断事件传播
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.Delete)
            {
                //若为Delete，执行删除方法
                DeleteData();
                //阻断事件传播
                evt.StopImmediatePropagation();
            }
        }
        #endregion

        #region 数据的增加与删除
        //数据的增加
        private void CreateData()
        {
            //判断当前选中项
            if (ActiveSelection == null)
            {
                //若当前选中项为空，则说明要创建根部物体，即系列名称，创建文本输入窗口
                TextInputWindow.ShowWindow(newSeriesName =>
                {
                    //计时
                    using ExecutionTimer timer = new("创建新系列名称", MainWindow.TimerDebugLogToggle.value);

                    //构建新路径
                    string newFolderPath = $"Assets\\GameData\\Character" +
                    $"\\{newSeriesName}";

                    //检查新路径存在状态
                    if (MakeSureFolderPathExist(newFolderPath))
                    {
                        //若存在，提示并返回
                        Debug.LogWarning("新系列名称已经存在，请重新创建！");
                        return;
                    }
                    else
                    {
                        //若不存在，则生成物体容器
                        ItemDataContainer newDataContainer = new(
                            newSeriesName,
                            ItemDataContainer.ItemType.OriginatingSeries,
                            null,
                            RootItemCache.Count + 1
                            );
                        //生成子级列表
                        List<TreeViewItemData<ItemDataContainer>> newChildren = new();
                        //生成树状图物体
                        TreeViewItemData<ItemDataContainer> newItem = new(
                            newSeriesName.GetHashCode(),
                            newDataContainer,
                            newChildren
                            );
                        //添加到总缓存中
                        ItemDicCache[newSeriesName.GetHashCode()] = newItem;
                        //添加到根缓存中
                        RootItemCache.Add(newItem);
                        //新增系列-组织键值对
                        SeriesAffiliationDicCache[newSeriesName.GetHashCode()] = newChildren;

                        //刷新
                        TRefresh();
                    }
                },
                "创建新系列名称",
                "请输入新的系列的名称",
                "新系列名称",
                "名称",
                EditorWindow.focusedWindow
                );
            }
            else
            {
                //若选中项不为空，则逐一判断情况
                if (ActiveSelection.Type == ItemDataContainer.ItemType.OriginatingSeries)
                {
                    //选中系列的情况下，创建组织
                    TextInputWindow.ShowWindow(newAffiliationName =>
                    {
                        //计时
                        using ExecutionTimer timer = new("创建组织名称", MainWindow.TimerDebugLogToggle.value);

                        //构建新路径
                        string newFolderPath = $"Assets\\GameData\\Character" +
                        $"\\{ActiveSelection.StringData}\\{newAffiliationName}";

                        //检查新路径存在状态
                        if (MakeSureFolderPathExist(newFolderPath))
                        {
                            //若存在，提示并返回
                            Debug.LogWarning("新组织名称已经存在，请重新创建！");
                            return;
                        }
                        else
                        {
                            //若不存在，则生成物体容器
                            ItemDataContainer newDataContainer = new(
                                newAffiliationName,
                                ItemDataContainer.ItemType.Affiliation,
                                ActiveSelection,
                                SeriesAffiliationDicCache[ActiveSelection.StringData.GetHashCode()].Count + 1
                                );
                            //生成子级列表
                            List<TreeViewItemData<ItemDataContainer>> newChildren = new();
                            //生成树状图物体
                            TreeViewItemData<ItemDataContainer> newItem = new(
                                newAffiliationName.GetHashCode(),
                                newDataContainer,
                                newChildren
                                );
                            //添加到总缓存中
                            ItemDicCache[newAffiliationName.GetHashCode()] = newItem;
                            //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                            SeriesAffiliationDicCache[ActiveSelection.StringData.GetHashCode()].Add(newItem);
                            //将本数据添加到对应级别缓存中
                            AffiliationCharacterDicCache[newAffiliationName.GetHashCode()] = newChildren;

                            //刷新
                            TRefresh();
                        }
                    },
                    "创建新组织名称",
                    "请输入新的组织的名称",
                    "新组织名称",
                    "名称",
                    EditorWindow.focusedWindow
                    );
                }
                else if (ActiveSelection.Type == ItemDataContainer.ItemType.Affiliation)
                {
                    //选中组织的情况下，创建角色
                    TextInputWindow.ShowWindow(newCharacterName =>
                    {
                        //计时
                        using ExecutionTimer timer = new("创建新角色名称", MainWindow.TimerDebugLogToggle.value);

                        //构建新路径
                        string newFolderPath = $"Assets\\GameData\\Character" +
                        $"\\{ActiveSelection.Parent.StringData}\\{ActiveSelection.StringData}\\{newCharacterName}";

                        //检查新路径存在状态
                        if (MakeSureFolderPathExist(newFolderPath))
                        {
                            //若存在，提示并返回
                            Debug.LogWarning("新角色名称已经存在，请重新创建！");
                            return;
                        }
                        else
                        {
                            //若不存在，则生成物体容器
                            ItemDataContainer newDataContainer = new(
                                newCharacterName,
                                ItemDataContainer.ItemType.CharacterName,
                                ActiveSelection,
                                AffiliationCharacterDicCache[ActiveSelection.StringData.GetHashCode()].Count + 1
                                );
                            //生成子级列表
                            List<TreeViewItemData<ItemDataContainer>> newChildren = new();
                            //生成树状图物体
                            TreeViewItemData<ItemDataContainer> newItem = new(
                                newCharacterName.GetHashCode(),
                                newDataContainer,
                                newChildren
                                );
                            //添加到总缓存中
                            ItemDicCache[newCharacterName.GetHashCode()] = newItem;
                            //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                            AffiliationCharacterDicCache[ActiveSelection.StringData.GetHashCode()].Add(newItem);
                            //将本数据添加到对应级别缓存中
                            CharacterVersionDicCache[newCharacterName.GetHashCode()] = newChildren;

                            //刷新
                            TRefresh();
                        }
                    },
                    "创建新角色名称",
                    "请输入新的角色的名称",
                    "新角色名称",
                    "名称",
                    EditorWindow.focusedWindow
                    );
                }
                else if (ActiveSelection.Type == ItemDataContainer.ItemType.CharacterName)
                {
                    //若选中了角色名称，创建新版本，也就是正经的角色数据实例
                    CreateNewVersionCharacterData(ActiveSelection);
                }
                else if (ActiveSelection.Type == ItemDataContainer.ItemType.CharacterData)
                {
                    //选中的是具体的版本的角色时，同样是创建新版本角色
                    CreateNewVersionCharacterData(ActiveSelection.Parent);
                }
            }
        }
        //数据的删除
        private void DeleteData()
        {
            //判断当前选中项
            if (ActiveSelection == null)
            {
                //若选中项为空，返回，总不能全部删除吧……
                return;
            }
            else
            {
                //选中项不为空，首先进行询问
                bool confirmWnd = EditorUtility.DisplayDialog(
                    "请确认删除",
                    "您确认要删除选中的数据吗？\n这会删除包括其子级在内的所有数据，无法撤销哦",
                    "确认",
                    "取消"
                    );
                //确认结果
                if (!confirmWnd)
                {
                    //若不确认，返回
                    return;
                }

                //若确认，则进行进一步判断
                if (ActiveSelection.Type == ItemDataContainer.ItemType.OriginatingSeries)
                {
                    //若为作品系列，删除
                    //计时
                    using ExecutionTimer timer = new("删除系列作品数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedFolderPath = $"Assets\\GameData\\Character" +
                        $"\\{ActiveSelection.StringData}";

                    //删除
                    DeleteFolder(deletedFolderPath);

                    //顺带一提，由于改动太多，所以直接重建
                    GenerateItems();

                    //刷新
                    TRefresh();
                }
                else if (ActiveSelection.Type == ItemDataContainer.ItemType.Affiliation)
                {
                    //若为组织，删除
                    //计时
                    using ExecutionTimer timer = new("删除组织数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedFolderPath = $"Assets\\GameData\\Character" +
                        $"\\{ActiveSelection.Parent.StringData}\\{ActiveSelection.StringData}";

                    //删除
                    DeleteFolder(deletedFolderPath);

                    //顺带一提，由于改动太多，所以直接重建
                    GenerateItems();

                    //刷新
                    TRefresh();
                }
                else if (ActiveSelection.Type == ItemDataContainer.ItemType.CharacterName)
                {
                    //若为角色名称，删除
                    //计时
                    using ExecutionTimer timer = new("删除角色数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedFolderPath = $"Assets\\GameData\\Character" +
                        $"\\{ActiveSelection.Parent.Parent.StringData}\\{ActiveSelection.Parent.StringData}" +
                        $"\\{ActiveSelection.StringData}";

                    //清除缓存
                    //父级存储的它
                    AffiliationCharacterDicCache[ActiveSelection.Parent.StringData.GetHashCode()]
                        .Remove(ItemDicCache[ActiveSelection.StringData.GetHashCode()]);
                    //它自己的子级缓存
                    CharacterVersionDicCache.Remove(ActiveSelection.StringData.GetHashCode());
                    //总缓存
                    ItemDicCache.Remove(ActiveSelection.StringData.GetHashCode());

                    //删除文件
                    DeleteFolder(deletedFolderPath);

                    //刷新
                    TRefresh();
                }
                else if (ActiveSelection.Type == ItemDataContainer.ItemType.CharacterData)
                {
                    //若为具体角色版本，删除
                    //计时
                    using ExecutionTimer timer = new("删除角色版本数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedFolderPath = $"Assets\\GameData\\Character" +
                        $"\\{ActiveSelection.Parent.Parent.Parent.StringData}\\{ActiveSelection.Parent.Parent.StringData}" +
                        $"\\{ActiveSelection.Parent.StringData}\\{ActiveSelection.StringData}";

                    //清除缓存
                    //父级存储的它
                    CharacterVersionDicCache[ActiveSelection.Parent.StringData.GetHashCode()]
                        .Remove(ItemDicCache[ActiveSelection.CharacterData.GetAssetHashCode()]);
                    //总缓存
                    ItemDicCache.Remove(ActiveSelection.CharacterData.GetAssetHashCode());

                    //删除文件
                    DeleteFolder(deletedFolderPath);

                    //刷新
                    TRefresh();
                }
            }

            //删除结束后，将活跃数据设为空
            ActiveSelection = null;
        }
        #endregion

        #region 树形图数据的保存与读取
        //记录树形图展开状态
        private void SaveExpandedState(TreeViewExpansionChangedArgs args)
        {
            //获取参数中的ID
            int id = args.id;
            //保存或删除
            if (IsExpanded(id))
            {
                //若为打开，添加到缓存
                ExpandedStatePersistentData.Add(id);
            }
            else
            {
                //若为关闭，判断其是否存在于数据中并移除
                if (ExpandedStatePersistentData.Contains(id))
                {
                    ExpandedStatePersistentData.Remove(id);
                }
            }
        }
        //恢复树形图展开状态
        private void RestoreExpandedState()
        {
            //新增被移除的ID列表
            List<int> removedIDs = new();
            //根据缓存数据进行展开
            foreach (int id in ExpandedStatePersistentData)
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
            ExpandedStatePersistentData = ExpandedStatePersistentData.Except(removedIDs).ToHashSet();
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
            ExpandedStatePersistentData.Clear();
            foreach (int id in persistentData.ExpandedState)
            {
                ExpandedStatePersistentData.Add(id);
            }
        }
        //生成新版本角色数据
        private void CreateNewVersionCharacterData(ItemDataContainer characterNameItemDataContainer)
        {
            TextInputWindow.ShowWindow(newVersionName =>
            {
                //计时
                using ExecutionTimer timer = new("创建新角色版本名称", MainWindow.TimerDebugLogToggle.value);

                //构建新路径
                string newFolderPath = $"Assets\\GameData\\Character" +
                $"\\{characterNameItemDataContainer.Parent.Parent.StringData}\\{characterNameItemDataContainer.Parent.StringData}" +
                $"\\{characterNameItemDataContainer.StringData}\\{newVersionName}";

                //检查新路径存在状态
                if (MakeSureFolderPathExist(newFolderPath))
                {
                    //若存在，提示并返回
                    Debug.LogWarning("新角色版本名称已经存在，请重新创建！");
                    return;
                }
                else
                {
                    //若不存在，开始生成物体
                    //创建新资源
                    CharacterData newCharacterData = ScriptableObject.CreateInstance<CharacterData>();
                    //设置相关数据
                    newCharacterData.Editor_SetPackage(MainWindow.DefaultPackageField.value);
                    newCharacterData.Editor_SetCategory("Character");
                    newCharacterData.Editor_SetAuthor(MainWindow.DefaultAuthorField.value);
                    newCharacterData.Editor_SetName(characterNameItemDataContainer.StringData);
                    newCharacterData.Editor_SetOriginatingSeries(characterNameItemDataContainer.Parent.Parent.StringData);
                    newCharacterData.Editor_SetAffiliation(characterNameItemDataContainer.Parent.StringData);
                    newCharacterData.Editor_SetVersion(newVersionName);
                    newCharacterData.Editor_SetSortingOrder(ItemDicCache[characterNameItemDataContainer.StringData.GetHashCode()].children.Count() + 1);
                    newCharacterData.Editor_SetAvatar(MainWindow.DefaultCharacterAvatar);
                    newCharacterData.Editor_SetPortarit(MainWindow.DefaultCharacterPortrait);
                    newCharacterData.Editor_GenerateID();
                    //更改文件名
                    newCharacterData.name = newVersionName;
                    //获取文件地址
                    string newFilePath = Path.Combine(newFolderPath, $"{newCharacterData.name}.asset").Replace("\\", "/");
                    //创建资源
                    AssetDatabase.CreateAsset(newCharacterData, newFilePath);
                    //保存
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    //随后生成树状图物体
                    //新建物体容器
                    ItemDataContainer newDataContainer = new(
                        newCharacterData,
                        characterNameItemDataContainer
                        );
                    //生成子级列表
                    List<TreeViewItemData<ItemDataContainer>> newChildren = new();
                    //生成树状图物体
                    TreeViewItemData<ItemDataContainer> newItem = new(
                        newCharacterData.GetAssetHashCode(),
                        newDataContainer,
                        newChildren
                        );
                    //添加到总缓存中
                    ItemDicCache[newCharacterData.GetAssetHashCode()] = newItem;
                    //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                    CharacterVersionDicCache[characterNameItemDataContainer.StringData.GetHashCode()].Add(newItem);

                    //刷新
                    TRefresh();
                }
            },
            "创建新角色版本名称",
            "请输入新的角色版本的名称",
            "新角色版本名称",
            "版本名称",
            EditorWindow.focusedWindow
            );
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
        //完全删除某个文件夹
        public void DeleteFolder(string folderPath)
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
            }
            //最后删除空文件夹
            AssetDatabase.DeleteAsset(folderPath);

            //保存更改
            AssetDatabase.SaveAssets();
            //刷新资源视图
            AssetDatabase.Refresh();
        }
        #endregion
    }
}

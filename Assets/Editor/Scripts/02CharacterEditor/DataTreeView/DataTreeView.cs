using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.CharacterSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.CharacterEditor
{
    public class DataTreeView : TreeView
    {
        #region 构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //根缓存
        public List<TreeViewItemData<CharacterSystemDataContainer>> RootItemCache { get; } = new();
        //系列-组织缓存
        public Dictionary<int, List<TreeViewItemData<CharacterSystemDataContainer>>> SeriesGroupDicCache { get; } = new();
        //组织-角色缓存
        public Dictionary<int, List<TreeViewItemData<CharacterSystemDataContainer>>> GroupCharacterDicCache { get; } = new();
        //角色-版本缓存
        public Dictionary<int, List<TreeViewItemData<CharacterSystemDataContainer>>> CharacterVersionDicCache { get; } = new();
        //ID-数据缓存
        public Dictionary<int, TreeViewItemData<CharacterSystemDataContainer>> ItemDicCache { get; } = new();

        //名称-排序永久性存储
        public Dictionary<string, int> StringSortingOrderPersistentData { get; } = new();
        //展开状态永久性存储
        public HashSet<int> ExpandedStatePersistentData { get; private set; } = new();

        //当前活跃选中项
        public CharacterSystemDataContainer ActiveSelection { get; private set; }
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

            //自定义树状图显示逻辑
            //物体采用普通标签显示
            makeItem = () =>
            {
                Label label = new();
                return label;
            };
            //绑定逻辑
            bindItem = (element, i) =>
            {
                //获取数据与物体展示形态
                CharacterSystemDataContainer itemDataContainer = GetItemDataForIndex<CharacterSystemDataContainer>(i);
                Label label = element as Label;
                //设置展示形态显示内容
                label.text = itemDataContainer.StringData;
            };

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
            SeriesGroupDicCache.Clear();
            GroupCharacterDicCache.Clear();
            CharacterVersionDicCache.Clear();
            ItemDicCache.Clear();

            //数据存储路径
            string rootPath = Application.streamingAssetsPath + "/Character";
            //确认路径存在
            GameEditor.MakeSureFolderPathExist(rootPath);

            //针对文件夹遍历并获取所有数据
            try
            {
                //首先获取第一层文件夹
                string[] firstLevelDirs = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly);
                //遍历
                foreach (string firstLevelDir in firstLevelDirs)
                {
                    //获取文件夹名称作为系列名称
                    string seriesName = Path.GetFileName(firstLevelDir);
                    //创建系列名称物体数据容器
                    CharacterSystemDataContainer seriesItemDataContainer = new(
                        seriesName,
                        RootItemCache.Count,
                        null,
                        CharacterSystemDataContainer.ItemDataType.Series
                        );
                    //创建其对应的子级集合
                    List<TreeViewItemData<CharacterSystemDataContainer>> seriesChildren = new();
                    //创建对应树状图物体
                    TreeViewItemData<CharacterSystemDataContainer> seriesItem = new(
                        seriesItemDataContainer.ID,
                        seriesItemDataContainer,
                        seriesChildren
                        );
                    //设置对应缓存
                    SeriesGroupDicCache[seriesItemDataContainer.ID] = seriesChildren;
                    //添加到总集缓存
                    ItemDicCache[seriesItemDataContainer.ID] = seriesItem;
                    //考虑到是根层级，添加到一层缓存中
                    RootItemCache.Add(seriesItem);

                    //获取第二层文件夹
                    string[] secondLevelDirs = Directory.GetDirectories(firstLevelDir, "*", SearchOption.TopDirectoryOnly);
                    //遍历
                    foreach (string secondLevelDir in secondLevelDirs)
                    {
                        //获取文件夹名称作为组织名称
                        string groupName = Path.GetFileName(secondLevelDir);
                        //创建组织名称物体数据容器
                        CharacterSystemDataContainer groupItemDataContainer = new(
                            groupName,
                            RootItemCache.Count,
                            seriesItemDataContainer,
                            CharacterSystemDataContainer.ItemDataType.Group
                            );
                        //创建其对应的子级集合
                        List<TreeViewItemData<CharacterSystemDataContainer>> groupChildren = new();
                        //创建对应树状图物体
                        TreeViewItemData<CharacterSystemDataContainer> groupItem = new(
                            groupItemDataContainer.ID,
                            groupItemDataContainer,
                            groupChildren
                            );
                        //设置对应缓存
                        GroupCharacterDicCache[groupItemDataContainer.ID] = groupChildren;
                        //添加到总集缓存
                        ItemDicCache[groupItemDataContainer.ID] = groupItem;
                        //将自己添加到上层的子级中
                        seriesChildren.Add(groupItem);

                        //获取第三层文件夹
                        string[] thirdLevelDirs = Directory.GetDirectories(secondLevelDir, "*", SearchOption.TopDirectoryOnly);
                        //遍历
                        foreach (string thirdLevelDir in thirdLevelDirs)
                        {
                            //获取文件夹名称作为角色名称
                            string characterName = Path.GetFileName(thirdLevelDir);
                            //创建角色名称物体数据容器
                            CharacterSystemDataContainer characterItemDataContainer = new(
                                characterName,
                                RootItemCache.Count,
                                groupItemDataContainer,
                                CharacterSystemDataContainer.ItemDataType.Character
                                );
                            //创建其对应的子级集合
                            List<TreeViewItemData<CharacterSystemDataContainer>> characterChildren = new();
                            //创建对应树状图物体
                            TreeViewItemData<CharacterSystemDataContainer> characterItem = new(
                                characterItemDataContainer.ID,
                                characterItemDataContainer,
                                characterChildren
                                );
                            //设置对应缓存
                            CharacterVersionDicCache[characterItemDataContainer.ID] = characterChildren;
                            //添加到总集缓存
                            ItemDicCache[characterItemDataContainer.ID] = characterItem;
                            //将自己添加到上层的子级中
                            groupChildren.Add(characterItem);

                            //获取第四层文件夹
                            string[] fourthLevelDirs = Directory.GetDirectories(thirdLevelDir, "*", SearchOption.TopDirectoryOnly);
                            //遍历
                            foreach (string fourthLevelDir in fourthLevelDirs)
                            {
                                //获取当前目录内的所有文件
                                string[] filePaths = Directory.GetFiles(fourthLevelDir, "*", SearchOption.TopDirectoryOnly);
                                //遍历
                                foreach (string filePath in filePaths)
                                {
                                    //找出其中以XMl结尾的文件，以Character开头的文件
                                    if (Path.GetExtension(filePath) == ".xml" && Path.GetFileNameWithoutExtension(filePath).StartsWith("Character"))
                                    {
                                        //创建新角色版本数据
                                        CharacterData versionData = CharacterData.LoadFromXML<CharacterData>(filePath);
                                        //设定其路径
                                        versionData.SavePath = filePath;
                                        //创建角色数据物体数据容器
                                        CharacterSystemDataContainer versionItemDataContainer = new(
                                            versionData,
                                            characterItemDataContainer
                                            );
                                        //创建角色数据物体
                                        TreeViewItemData<CharacterSystemDataContainer> versionItem = new(
                                            versionItemDataContainer.ID,
                                            versionItemDataContainer,
                                            null
                                            );
                                        //添加到总集缓存中
                                        ItemDicCache[versionItemDataContainer.ID] = versionItem;
                                        //添加到上层子级中
                                        characterChildren.Add(versionItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                //处理异常时报错
                Debug.LogError(e.Message);
            }

            ////构建数据结构
            ////总字典
            //Dictionary<string, Dictionary<string, Dictionary<string, List<CharacterData>>>> totalDataDic = new();
            ////总路径存储
            //List<string> characterDataPaths = new();
            ////构建字典，此处依靠文件夹构建，以免出现空文件夹所代表的数据未被识别的情况
            //if (AssetDatabase.IsValidFolder(rootPath))
            //{
            //    //若存在，获取第一级的所有文件夹地址，也就是系列作品文件夹，作为总字典第一层Key
            //    string[] firstLevelDirs = AssetDatabase.GetSubFolders(rootPath);
            //    //并对其进行遍历
            //    foreach (string firstLevelDir in firstLevelDirs)
            //    {
            //        //从地址中获取名称，也就是Key
            //        string firstLevelName = Path.GetFileName(firstLevelDir);
            //        //并针对每一个Key创建第一级对应的值，也就是包含了第二级的字典，即组织-角色字典
            //        Dictionary<string, Dictionary<string, List<CharacterData>>> secondLevelDic = new();

            //        //获取第二级所有文件夹地址，也就是组织文件夹
            //        string[] secondLevelDirs = AssetDatabase.GetSubFolders(firstLevelDir);
            //        //遍历
            //        foreach (var secondLevelDir in secondLevelDirs)
            //        {
            //            //从地址中获取名称作为Key
            //            string secondLevelName = Path.GetFileName(secondLevelDir);
            //            //并针对每一个Key创建第三级对应的值，即角色-版本字典
            //            Dictionary<string, List<CharacterData>> thirdLevelDic = new();

            //            //获取第三级所有文件夹地址，也就是角色文件夹
            //            string[] thirdLevelDirs = AssetDatabase.GetSubFolders(secondLevelDir);
            //            //遍历
            //            foreach (var thirdLevelDir in thirdLevelDirs)
            //            {
            //                //从地址中获取名称作为Key
            //                string thirdLevelName = Path.GetFileName(thirdLevelDir);
            //                //并针对每一个Key创建第四级对应的值，即角色数据列表
            //                List<CharacterData> characterDatas = new();
            //                //并添加键值对
            //                thirdLevelDic[thirdLevelName] = characterDatas;
            //            }
            //            //添加键值对
            //            secondLevelDic[secondLevelName] = thirdLevelDic;
            //        }
            //        //添加到字典中键值对
            //        totalDataDic[firstLevelName] = secondLevelDic;
            //    }
            //}
            ////填充字典，如果是从编辑器创建的数据，那么此处应该是正确无误的
            //foreach (CharacterData versionData in characterDatas)
            //{
            //    //添加数据
            //    totalDataDic[versionData.OriginatingSeries][versionData.Affiliation].Add(versionData);
            //}

            ////随后开始构建树形图物体
            ////遍历第一层
            //foreach (var series in totalDataDic)
            //{
            //    //获取字符串排序
            //    int seriesSortingOrder;
            //    if (StringSortingOrderPersistentData.ContainsKey(series.Key))
            //    {
            //        seriesSortingOrder = StringSortingOrderPersistentData[series.Key];
            //    }
            //    else
            //    {
            //        seriesSortingOrder = RootItemCache.Count + 1;
            //    }
            //    //创造物体容器
            //    CharacterSystemDataContainer seriesItemDataContainer = new(
            //        series.Key,
            //        CharacterSystemDataContainer.Type.OriginatingSeries,
            //        null,
            //        seriesSortingOrder
            //        );
            //    //创建系列名称的子级的集合，即存储组织名称的地方
            //    List<TreeViewItemData<CharacterSystemDataContainer>> seriesChildren = new();
            //    //创建物体
            //    TreeViewItemData<CharacterSystemDataContainer> seriesItem = new(
            //        series.Key.GetHashCode(),
            //        seriesItemDataContainer,
            //        seriesChildren
            //        );
            //    //设置对应缓存
            //    SeriesGroupDicCache[series.Key.GetHashCode()] = seriesChildren;
            //    //添加到总集缓存与一层缓存中
            //    ItemDicCache[series.Key.GetHashCode()] = seriesItem;
            //    RootItemCache.Add(seriesItem);

            //    //填充其子级
            //    //遍历该数据的第二层
            //    foreach (var affiliation in series.Value)
            //    {
            //        //获取字符串排序
            //        int affiliationSortingOrder;
            //        if (StringSortingOrderPersistentData.ContainsKey(affiliation.Key))
            //        {
            //            affiliationSortingOrder = StringSortingOrderPersistentData[affiliation.Key];
            //        }
            //        else
            //        {
            //            affiliationSortingOrder = seriesChildren.Count + 1;
            //        }
            //        //创建物体容器
            //        CharacterSystemDataContainer characterItemDataContainer = new(
            //            affiliation.Key,
            //            CharacterSystemDataContainer.Type.Affiliation,
            //            seriesItemDataContainer,
            //            affiliationSortingOrder
            //            );
            //        //创建组织名称的子级的集合，即存储角色名称的地方
            //        List<TreeViewItemData<CharacterSystemDataContainer>> affiliationChildren = new();
            //        //创建物体
            //        TreeViewItemData<CharacterSystemDataContainer> affiliationItem = new(
            //            affiliation.Key.GetHashCode(),
            //            characterItemDataContainer,
            //            affiliationChildren
            //            );
            //        //添加到上层子级中
            //        seriesChildren.Add(affiliationItem);
            //        //对对应的存储角色名称的地方设定为缓存
            //        GroupCharacterDicCache[affiliation.Key.GetHashCode()] = affiliationChildren;
            //        //添加到总集缓存与存储组织名称的地方的缓存中
            //        ItemDicCache[affiliation.Key.GetHashCode()] = affiliationItem;

            //        //填充其子级
            //        //遍历该数据的第三层，即角色
            //        foreach (var character in affiliation.Value)
            //        {
            //            //创建角色名称物体数据容器
            //            CharacterSystemDataContainer characterItemDataContainer = new(
            //                character,
            //                characterItemDataContainer
            //                );
            //            //创建角色名称物体
            //            TreeViewItemData<CharacterSystemDataContainer> characterItem = new(
            //                character.GetAssetHashCode(),
            //                characterItemDataContainer,
            //                null
            //                );
            //            //添加到上层子级中
            //            affiliationChildren.Add(characterItem);
            //            //添加到总集缓存中
            //            ItemDicCache[character.GetAssetHashCode()] = characterItem;
            //        }

            //        //结束之后对角色数据集合进行排序
            //        affiliationChildren.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
            //    }

            //    //结束之后对组织名称集合进行排序
            //    seriesChildren.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
            //}

            //对一层缓存进行排序
            RootItemCache.Sort((a, b) => a.data.SortOrder.CompareTo(b.data.SortOrder));

            //构建完成后设置树形图根物体
            SetRootItems(RootItemCache);

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
                CharacterSystemDataContainer newSelection = selectedItems.Cast<CharacterSystemDataContainer>().FirstOrDefault();
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
                    CharacterSystemDataContainer newSelection = selectedItems.Cast<CharacterSystemDataContainer>().FirstOrDefault();
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
                TextInputWindow.ShowWindow((System.Action<string>)(newSeriesName =>
                {
                    //计时
                    using ExecutionTimer timer = new("创建新系列", MainWindow.TimerDebugLogToggle.value);

                    //构建新路径
                    string newFolderPath = Path.Combine(
                        Application.streamingAssetsPath,
                        "Character",
                        newSeriesName
                        );

                    //检查新路径存在状态
                    if (GameEditor.MakeSureFolderPathExist(newFolderPath))
                    {
                        //若存在，提示并返回
                        Debug.LogWarning("系列名称已经存在，请重新创建！");
                        return;
                    }
                    else
                    {
                        //若不存在，则生成物体容器
                        CharacterSystemDataContainer newDataContainer = new(
                            newSeriesName,
                            RootItemCache.Count + 1,
                            null,
                            CharacterSystemDataContainer.ItemDataType.Series
                            );
                        //生成子级列表
                        List<TreeViewItemData<CharacterSystemDataContainer>> newChildren = new();
                        //生成树状图物体
                        TreeViewItemData<CharacterSystemDataContainer> newItem = new(
                            newDataContainer.ID,
                            newDataContainer,
                            newChildren
                            );
                        //添加到总缓存中
                        ItemDicCache[newDataContainer.ID] = newItem;
                        //添加到根缓存中
                        RootItemCache.Add(newItem);
                        //新增系列-组织键值对
                        SeriesGroupDicCache[newDataContainer.ID] = newChildren;

                        //刷新
                        TRefresh();
                    }
                }),
                "Create New Series",
                "Please Input New Series ID Part",
                "New Series ID Part",
                "New ID Part",
                EditorWindow.focusedWindow
                );
            }
            else
            {
                //若选中项不为空，则逐一判断情况
                if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Series)
                {
                    //选中系列的情况下，创建组织
                    TextInputWindow.ShowWindow((System.Action<string>)(newAffiliationName =>
                    {
                        //计时
                        using ExecutionTimer timer = new("创建新组", MainWindow.TimerDebugLogToggle.value);

                        //构建新路径
                        string newFolderPath = Path.Combine(
                            Application.streamingAssetsPath,
                            "Character",
                            ActiveSelection.StringData,
                            newAffiliationName
                            );

                        //检查新路径存在状态
                        if (GameEditor.MakeSureFolderPathExist(newFolderPath))
                        {
                            //若存在，提示并返回
                            Debug.LogWarning("组名称已经存在，请重新创建！");
                            return;
                        }
                        else
                        {
                            //若不存在，则生成物体容器
                            CharacterSystemDataContainer newDataContainer = new(
                                newAffiliationName,
                                SeriesGroupDicCache[ActiveSelection.ID].Count + 1,
                                ActiveSelection,
                                CharacterSystemDataContainer.ItemDataType.Group
                                );
                            //生成子级列表
                            List<TreeViewItemData<CharacterSystemDataContainer>> newChildren = new();
                            //生成树状图物体
                            TreeViewItemData<CharacterSystemDataContainer> newItem = new(
                                newDataContainer.ID,
                                newDataContainer,
                                newChildren
                                );
                            //添加到总缓存中
                            ItemDicCache[newDataContainer.ID] = newItem;
                            //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                            SeriesGroupDicCache[ActiveSelection.ID].Add(newItem);
                            //将本数据添加到对应级别缓存中
                            GroupCharacterDicCache[newDataContainer.ID] = newChildren;

                            //刷新
                            TRefresh();
                        }
                    }),
                    "Create New Group",
                    "Please Input New Group ID Part",
                    "New Group ID Part",
                    "New ID Part",
                    EditorWindow.focusedWindow
                    );
                }
                else if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Group)
                {
                    //选中组织的情况下，创建角色
                    TextInputWindow.ShowWindow((System.Action<string>)(newCharacterName =>
                    {
                        //计时
                        using ExecutionTimer timer = new("创建新角色", MainWindow.TimerDebugLogToggle.value);

                        //构建新路径
                        string newFolderPath = Path.Combine(
                            Application.streamingAssetsPath,
                            "Character",
                            ActiveSelection.Parent.StringData,
                            ActiveSelection.StringData,
                            newCharacterName
                            );

                        //检查新路径存在状态
                        if (GameEditor.MakeSureFolderPathExist(newFolderPath))
                        {
                            //若存在，提示并返回
                            Debug.LogWarning("角色名称已经存在，请重新创建！");
                            return;
                        }
                        else
                        {
                            //若不存在，则生成物体容器
                            CharacterSystemDataContainer newDataContainer = new(
                                newCharacterName,
                                GroupCharacterDicCache[ActiveSelection.ID].Count + 1,
                                ActiveSelection,
                                CharacterSystemDataContainer.ItemDataType.Group
                                );
                            //生成子级列表
                            List<TreeViewItemData<CharacterSystemDataContainer>> newChildren = new();
                            //生成树状图物体
                            TreeViewItemData<CharacterSystemDataContainer> newItem = new(
                                newDataContainer.ID,
                                newDataContainer,
                                newChildren
                                );
                            //添加到总缓存中
                            ItemDicCache[newDataContainer.ID] = newItem;
                            //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                            GroupCharacterDicCache[ActiveSelection.ID].Add(newItem);
                            //将本数据添加到对应级别缓存中
                            CharacterVersionDicCache[newDataContainer.ID] = newChildren;

                            //刷新
                            TRefresh();
                        }
                    }),
                    "Create New Character",
                    "Please Input New Character ID Part",
                    "New Character ID Part",
                    "New ID Part",
                    EditorWindow.focusedWindow
                    );
                }
                else if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Character)
                {
                    //若选中了角色，创建角色
                    CreateNewCharacterData(ActiveSelection);
                }
                else if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Version)
                {
                    //若选中了版本，同样创建角色
                    CreateNewCharacterData(ActiveSelection.Parent as CharacterSystemDataContainer);
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
                if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Series)
                {
                    //若为作品系列，删除
                    //计时
                    using ExecutionTimer timer = new("删除系列作品数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedDirectory = Path.Combine(
                        Application.streamingAssetsPath,
                        "Character",
                        ActiveSelection.StringData
                        );

                    //删除
                    GameEditor.DeleteFolder(deletedDirectory);

                    //顺带一提，由于改动太多，所以直接重建
                    GenerateItems();

                    //刷新
                    TRefresh();
                }
                else if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Group)
                {
                    //若为组织，删除
                    //计时
                    using ExecutionTimer timer = new("删除组织数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedDirectory = Path.Combine(
                        Application.streamingAssetsPath,
                        "Character",
                        ActiveSelection.Parent.StringData,
                        ActiveSelection.StringData
                        );

                    //删除
                    GameEditor.DeleteFolder(deletedDirectory);

                    //顺带一提，由于改动太多，所以直接重建
                    GenerateItems();

                    //刷新
                    TRefresh();
                }
                else if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Character)
                {
                    //若为角色名称，删除
                    //计时
                    using ExecutionTimer timer = new("删除角色数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedDirectory = Path.Combine(
                        Application.streamingAssetsPath,
                        "Character",
                        ActiveSelection.Parent.Parent.StringData,
                        ActiveSelection.Parent.StringData,
                        ActiveSelection.StringData
                        );

                    //删除文件
                    GameEditor.DeleteFolder(deletedDirectory);

                    //顺带一提，由于改动太多，所以直接重建
                    GenerateItems();

                    //刷新
                    TRefresh();
                }
                else if (ActiveSelection.Type == CharacterSystemDataContainer.ItemDataType.Version)
                {
                    //若为角色版本，删除
                    //计时
                    using ExecutionTimer timer = new("删除角色版本数据", MainWindow.TimerDebugLogToggle.value);

                    //获取路径
                    string deletedDirectory = Path.Combine(
                        Application.streamingAssetsPath,
                        "Character",
                        ActiveSelection.Parent.Parent.Parent.StringData,
                        ActiveSelection.Parent.Parent.StringData,
                        ActiveSelection.Parent.StringData,
                        ActiveSelection.StringData
                        );

                    //清除缓存
                    //父级存储的它
                    CharacterVersionDicCache[ActiveSelection.Parent.ID]
                        .Remove(ItemDicCache[ActiveSelection.ID]);
                    //总缓存
                    ItemDicCache.Remove(ActiveSelection.ID);

                    //删除文件
                    GameEditor.DeleteFolder(deletedDirectory);

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
        private void CreateNewCharacterData(CharacterSystemDataContainer characterItemDataContainer)
        {
            TextInputWindow.ShowWindow(newCharacterVersionName =>
            {
                //计时
                using ExecutionTimer timer = new("创建新角色版本名称", MainWindow.TimerDebugLogToggle.value);

                //构建新路径
                string newFolderPath = Path.Combine(
                    Application.streamingAssetsPath,
                    "Character",
                    ActiveSelection.Parent.Parent.StringData,
                    ActiveSelection.Parent.StringData,
                    ActiveSelection.StringData,
                    newCharacterVersionName
                    );

                //检查新路径存在状态
                if (GameEditor.MakeSureFolderPathExist(newFolderPath))
                {
                    //若存在，提示并返回
                    Debug.LogWarning("新角色版本名称已经存在，请重新创建！");
                    return;
                }
                else
                {
                    //若不存在，开始生成数据
                    CharacterData newCharacterData = new(
                        //ID，由系列、组织、角色、版本组成，用下划线连接，并将空格替换为-
                        string.Join("_", new string[] {
                            "CharacterEditor",
                            characterItemDataContainer.Parent.Parent.StringData,
                            characterItemDataContainer.Parent.StringData,
                            characterItemDataContainer.StringData,
                            newCharacterVersionName}).Replace(" ", "-"),
                        //IDPart,此处应该是版本名称
                        newCharacterVersionName,
                        //Name，暂时由角色名称代替
                        characterItemDataContainer.StringData,
                        //Description，暂时为空
                        string.Empty,
                        //排序，由角色版本数量决定
                        CharacterVersionDicCache[characterItemDataContainer.ID].Count,
                        //系列名称，从容器中获取
                        characterItemDataContainer.Parent.Parent.StringData,
                        //组名称，从容器中获取
                        characterItemDataContainer.Parent.StringData,
                        //角色名称，从容器中获取
                        characterItemDataContainer.StringData,
                        //版本名称，直接获取
                        newCharacterVersionName
                        );

                    //随后将其保存在硬盘上并创建标准配备文件夹
                    //首先获取存放路径
                    string filePath = Path.Combine(newFolderPath, $"CharacterData.xml");
                    //创建文件
                    CharacterData.SaveToXML<CharacterData>(newCharacterData, filePath);
                    //设定其存放路径
                    newCharacterData.SavePath = filePath;
                    //创建配备文件夹
                    Directory.CreateDirectory(Path.Combine(newFolderPath, "Avatars"));
                    Directory.CreateDirectory(Path.Combine(newFolderPath, "Portraits"));

                    //随后处理树状图与缓存
                    //新建物体容器
                    CharacterSystemDataContainer newDataContainer = new(
                        newCharacterData,
                        characterItemDataContainer
                        );
                    //生成树状图物体
                    TreeViewItemData<CharacterSystemDataContainer> newItem = new(
                        newDataContainer.ID,
                        newDataContainer,
                        null
                        );
                    //添加到总缓存中
                    ItemDicCache[newDataContainer.ID] = newItem;
                    //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                    CharacterVersionDicCache[characterItemDataContainer.ID].Add(newItem);
                    //刷新
                    TRefresh();

                    ////若不存在，开始生成物体
                    ////创建新资源
                    //CharacterData newCharacterData = ScriptableObject.CreateInstance<CharacterData>();
                    ////设置相关数据
                    //newCharacterData.GameDataType = GameDataTypeEnum.CharacterEditor;
                    //newCharacterData.Name = newCharacterVersionName;
                    //newCharacterData.OriginatingSeries = characterItemDataContainer.Parent.StringData;
                    //newCharacterData.Affiliation = characterItemDataContainer.StringData;
                    //newCharacterData.SortingOrder = ItemDicCache[characterItemDataContainer.StringData.GetHashCode()].children.Count() + 1;
                    //newCharacterData.Avatar = MainWindow.DefaultCharacterAvatar;
                    //newCharacterData.Portrait = MainWindow.DefaultCharacterPortrait;
                    //newCharacterData.Editor_GenerateID();
                    ////更改文件名
                    //newCharacterData.name = newCharacterVersionName;
                    ////获取文件地址
                    //string newFilePath = Path.Combine(newFolderPath, $"{newCharacterData.name}.asset").Replace("\\", "/");
                    ////创建资源
                    //AssetDatabase.CreateAsset(newCharacterData, newFilePath);
                    ////保存
                    //AssetDatabase.SaveAssets();
                    //AssetDatabase.Refresh();

                    ////随后生成树状图物体
                    ////新建物体容器
                    //CharacterSystemDataContainer newDataContainer = new(
                    //    newCharacterData,
                    //    characterItemDataContainer
                    //    );
                    ////生成树状图物体
                    //TreeViewItemData<CharacterSystemDataContainer> newItem = new(
                    //    newCharacterData.GetAssetHashCode(),
                    //    newDataContainer,
                    //    null
                    //    );
                    ////添加到总缓存中
                    //ItemDicCache[newCharacterData.GetAssetHashCode()] = newItem;
                    ////添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                    //GroupCharacterDicCache[characterItemDataContainer.StringData.GetHashCode()].Add(newItem);
                    ////刷新
                    //TRefresh();

                    ////随后处理资源组问题
                    ////创建新的资源索引
                    //AddressableAssetEntry entry = AddressableAssetSettingsDefaultObject
                    //.GetSettings(true)
                    //.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newCharacterData)), MainWindow.AssetGroup);
                    ////设定索引名称为全名
                    //entry.SetAddress($"{newCharacterData.OriginatingSeries}_{newCharacterData.Affiliation}_{newCharacterData.Name}".Replace(" ", "-"));
                    ////并设定标签
                    //entry.SetLabel("CharacterEditor", true, true);
                    ////保存设置
                    //MainWindow.AssetGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                    //AssetDatabase.SaveAssets();
                }
            },
            "Create New Character Version",
            "Please Input New Character Version",
            "New Character Version",
            "New Version",
            EditorWindow.focusedWindow
            );
        }
        #endregion
    }
}

﻿using Newtonsoft.Json;
using System;
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
        public List<TreeViewItemData<DataContainer>> RootItemCache { get; } = new();
        //系列-组织缓存
        public Dictionary<int, List<TreeViewItemData<DataContainer>>> SeriesGroupDicCache { get; } = new();
        //组织-角色缓存
        public Dictionary<int, List<TreeViewItemData<DataContainer>>> GroupCharaDicCache { get; } = new();
        //角色-版本缓存
        public Dictionary<int, List<TreeViewItemData<DataContainer>>> CharaVersionDicCache { get; } = new();
        //ID-数据缓存
        public Dictionary<int, TreeViewItemData<DataContainer>> ItemDicCache { get; } = new();
        //角色-排序
        public Dictionary<int, int> CharaSortDic { get; } = new();

        //展开状态永久性存储
        public HashSet<int> ExpandedStatePersistentData { get; private set; } = new();

        //当前活跃选中项
        public DataContainer ActiveSelection { get; private set; }
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
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

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
                DataContainer itemDataContainer = GetItemDataForIndex<DataContainer>(i);
                Label label = element as Label;
                //设置展示形态显示内容
                label.text = itemDataContainer.StringData;
                //检查容器数据
                if (itemDataContainer.Type == DataContainer.ItemDataType.Version)
                {
                    //若为版本数据，更改标签名称
                    label.name = "Version";
                }
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
            GroupCharaDicCache.Clear();
            CharaVersionDicCache.Clear();
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
                    DataContainer seriesItemDataContainer = new(
                        seriesName,
                        seriesName,
                        RootItemCache.Count,
                        null,
                        DataContainer.ItemDataType.Series
                        );
                    //创建其对应的子级集合
                    List<TreeViewItemData<DataContainer>> seriesChildren = new();
                    //创建对应树状图物体
                    TreeViewItemData<DataContainer> seriesItem = new(
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
                        DataContainer groupItemDataContainer = new(
                            seriesName + "_" + groupName,
                            groupName,
                            SeriesGroupDicCache[seriesItemDataContainer.ID].Count,
                            seriesItemDataContainer,
                            DataContainer.ItemDataType.Group
                            );
                        //创建其对应的子级集合
                        List<TreeViewItemData<DataContainer>> groupChildren = new();
                        //创建对应树状图物体
                        TreeViewItemData<DataContainer> groupItem = new(
                            groupItemDataContainer.ID,
                            groupItemDataContainer,
                            groupChildren
                            );
                        //设置对应缓存
                        GroupCharaDicCache[groupItemDataContainer.ID] = groupChildren;
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
                            string charaName = Path.GetFileName(thirdLevelDir);
                            //创建角色名称物体数据容器
                            DataContainer charaItemDataContainer = new(
                                (seriesName + "_" + groupName + "_" + charaName).Replace(" ", "-"),
                                charaName,
                                GroupCharaDicCache[groupItemDataContainer.ID].Count,
                                groupItemDataContainer,
                                DataContainer.ItemDataType.Character
                                );
                            //创建其对应的子级集合
                            List<TreeViewItemData<DataContainer>> characterChildren = new();
                            //创建对应树状图物体
                            TreeViewItemData<DataContainer> characterItem = new(
                                charaItemDataContainer.ID,
                                charaItemDataContainer,
                                characterChildren
                                );
                            //设置对应缓存
                            CharaVersionDicCache[charaItemDataContainer.ID] = characterChildren;
                            //添加到总集缓存
                            ItemDicCache[charaItemDataContainer.ID] = characterItem;
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
                                    //找出其中以Json结尾的文件，以Character开头的文件
                                    if (Path.GetExtension(filePath) == ".json" && Path.GetFileNameWithoutExtension(filePath).StartsWith("CharacterData"))
                                    {
                                        //创建新角色版本数据
                                        CharacterData versionData = CharacterData.LoadFromJson<CharacterData>(filePath);
                                        //设定其路径
                                        versionData.DataPath = filePath.Replace("\\", "/");
                                        //创建角色数据物体数据容器
                                        DataContainer versionItemDataContainer = new(
                                            versionData,
                                            charaItemDataContainer
                                            );
                                        //创建角色数据物体
                                        TreeViewItemData<DataContainer> versionItem = new(
                                            versionItemDataContainer.ID,
                                            versionItemDataContainer,
                                            null
                                            );
                                        //添加到总集缓存中
                                        ItemDicCache[versionItemDataContainer.ID] = versionItem;
                                        //设定排序数值
                                        CharaSortDic[charaItemDataContainer.ID] = versionData.SortOrder;
                                        //添加到上层子级中
                                        characterChildren.Add(versionItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //处理异常时报错
                Debug.LogError(e.Message);
            }

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
            selectionChanged += async (selections) =>
            {
                DataContainer newSelection = selectedItems.Cast<DataContainer>().FirstOrDefault();
                if (newSelection != null)
                {
                    ActiveSelection = newSelection;
                    MainWindow.DataEditorPanel.DRefresh();
                    await MainWindow.AssetsEditorPanel.ARefresh();
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
                    DataContainer newSelection = selectedItems.Cast<DataContainer>().FirstOrDefault();
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
                        DataContainer newDataContainer = new(
                            newSeriesName,
                            newSeriesName,
                            RootItemCache.Count + 1,
                            null,
                            DataContainer.ItemDataType.Series
                            );
                        //生成子级列表
                        List<TreeViewItemData<DataContainer>> newChildren = new();
                        //生成树状图物体
                        TreeViewItemData<DataContainer> newItem = new(
                            newDataContainer.ID,
                            newDataContainer,
                            newChildren
                            );
                        //添加到总缓存中
                        ItemDicCache[newDataContainer.ID] = newItem;
                        //添加到根缓存中
                        RootItemCache.Add(newItem);
                        //对应子级列表
                        SeriesGroupDicCache[newDataContainer.ID] = newChildren;

                        //刷新
                        TRefresh();
                    }
                },
                "创建新系列",
                "请输入新系列名称",
                "新系列名称",
                "名称",
                EditorWindow.focusedWindow
                );
            }
            else
            {
                //若选中项不为空，则逐一判断情况
                if (ActiveSelection.Type == DataContainer.ItemDataType.Series)
                {
                    //选中系列的情况下，创建组织
                    TextInputWindow.ShowWindow(newAffiliationName =>
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
                            DataContainer newDataContainer = new(
                                (ActiveSelection.StringData + "_" + newAffiliationName).Replace(" ", "-"),
                                newAffiliationName,
                                SeriesGroupDicCache[ActiveSelection.ID].Count + 1,
                                ActiveSelection,
                                DataContainer.ItemDataType.Group
                                );
                            //生成子级列表
                            List<TreeViewItemData<DataContainer>> newChildren = new();
                            //生成树状图物体
                            TreeViewItemData<DataContainer> newItem = new(
                                newDataContainer.ID,
                                newDataContainer,
                                newChildren
                                );
                            //添加到总缓存中
                            ItemDicCache[newDataContainer.ID] = newItem;
                            //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                            SeriesGroupDicCache[ActiveSelection.ID].Add(newItem);
                            //对应子级列表
                            GroupCharaDicCache[newDataContainer.ID] = newChildren;

                            //刷新
                            TRefresh();
                        }
                    },
                    "创建新组",
                    "请输入新组名称",
                    "新组名称",
                    "名称",
                    EditorWindow.focusedWindow
                    );
                }
                else if (ActiveSelection.Type == DataContainer.ItemDataType.Group)
                {
                    //选中组织的情况下，创建角色
                    TextInputWindow.ShowWindow(newCharaName =>
                    {
                        //计时
                        using ExecutionTimer timer = new("创建新角色", MainWindow.TimerDebugLogToggle.value);

                        //构建新路径
                        string newFolderPath = Path.Combine(
                            Application.streamingAssetsPath,
                            "Character",
                            ActiveSelection.Parent.StringData,
                            ActiveSelection.StringData,
                            newCharaName
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
                            DataContainer newDataContainer = new(
                                (ActiveSelection.Parent.StringData + "_" + ActiveSelection.StringData + "_" + newCharaName).Replace(" ", "-"),
                                newCharaName,
                                GroupCharaDicCache[ActiveSelection.ID].Count + 1,
                                ActiveSelection,
                                DataContainer.ItemDataType.Character
                                );
                            //生成子级列表
                            List<TreeViewItemData<DataContainer>> newChildren = new();
                            //生成树状图物体
                            TreeViewItemData<DataContainer> newItem = new(
                                newDataContainer.ID,
                                newDataContainer,
                                newChildren
                                );
                            //添加到总缓存中
                            ItemDicCache[newDataContainer.ID] = newItem;
                            //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                            GroupCharaDicCache[ActiveSelection.ID].Add(newItem);
                            //对应子级列表
                            CharaVersionDicCache[newDataContainer.ID] = newChildren;
                            //设定排序数值
                            CharaSortDic[newDataContainer.ID] = CharaSortDic.Count;

                            //直接新建角色
                            CreateNewCharacterData(newDataContainer);

                            //刷新
                            TRefresh();
                        }
                    },
                    "创建新角色",
                    "请输入新角色名称",
                    "新角色名称",
                    "名称",
                    EditorWindow.focusedWindow
                    );
                }
                else if (ActiveSelection.Type == DataContainer.ItemDataType.Character)
                {
                    //若选中了角色，创建角色
                    CreateNewCharacterData(ActiveSelection);
                }
                else if (ActiveSelection.Type == DataContainer.ItemDataType.Version)
                {
                    //若选中了版本，同样创建角色
                    CreateNewCharacterData(ActiveSelection.Parent);
                }
            }

            //保存数据
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
                if (ActiveSelection.Type == DataContainer.ItemDataType.Series)
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
                else if (ActiveSelection.Type == DataContainer.ItemDataType.Group)
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
                else if (ActiveSelection.Type == DataContainer.ItemDataType.Character)
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
                else if (ActiveSelection.Type == DataContainer.ItemDataType.Version)
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
                    CharaVersionDicCache[ActiveSelection.Parent.ID]
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

            //将角色数据标记为脏
            GameEditor.IsCharacterDataDirty = true;

            //保存数据
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
        private async void CreateNewCharacterData(DataContainer charaItemDataContainer)
        {
            //声明新版本名称
            string newCharacterVersionName;
            //判断当前版本数量
            if (CharaVersionDicCache[charaItemDataContainer.ID].Count == 0)
            {
                //若版本数量为0，则直接命名为0
                newCharacterVersionName = "0";
            }
            else
            {
                //若版本数量不为0，则显示输入窗口并获取返回值
                newCharacterVersionName = await TextInputWindow.ShowWindowWithResult
                    ("创建新角色版本",
                    "请输入新角色版本名称",
                    "新角色版本名称",
                    "名称",
                    EditorWindow.focusedWindow);
                //检查输入是否合法
                if (string.IsNullOrEmpty(newCharacterVersionName))
                {
                    //若为空，提示并返回
                    EditorUtility.DisplayDialog("提示", "请输入一个版本名称", "OK");
                    return;
                }
            }
            //构建新路径
            string newFolderPath = Path.Combine(
                Application.streamingAssetsPath,
                "Character",
                charaItemDataContainer.Parent.Parent.StringData,
                charaItemDataContainer.Parent.StringData,
                charaItemDataContainer.StringData,
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
                CharacterData newCharacterData = new()
                {
                    //ID，由系列、组织、角色组成，用下划线连接，并将空格替换为-
                    ID = string.Join("_", new string[]
                    {
                            "Character",
                            charaItemDataContainer.Parent.Parent.StringData,
                            charaItemDataContainer.Parent.StringData,
                            charaItemDataContainer.StringData,
                    })
                    .Replace(" ", "-"),
                    //IDPart,此处应该是版本名称
                    IDPart = newCharacterVersionName,
                    //Name，暂时由角色名称代替
                    Name = charaItemDataContainer.StringData,
                    //Description，暂时为空
                    Description = string.Empty,
                    //排序，从字典中获取
                    SortOrder = CharaSortDic[charaItemDataContainer.ID],
                    //系列名称，从容器中获取
                    Series = charaItemDataContainer.Parent.Parent.StringData,
                    //组名称，从容器中获取
                    Group = charaItemDataContainer.Parent.StringData,
                    //角色名称，从容器中获取
                    Chara = charaItemDataContainer.StringData,
                    //版本名称，直接获取
                    Version = newCharacterVersionName,
                    //颜色，默认白色
                    Color = Color.white,
                };

                //随后将其保存在硬盘上并创建标准配备文件夹
                //首先获取存放路径
                string JsonFilePath = Path.Combine(newFolderPath, "CharacterData.json");
                //设定其存放路径
                newCharacterData.DataPath = JsonFilePath.Replace("\\", "/");
                //创建文件
                CharacterData.SaveToJson(newCharacterData, JsonFilePath);
                //新增占位文件
                GameEditor.GeneratePlaceHolderTextFile(newFolderPath);
                //创建配备文件夹并生成占位符
                Directory.CreateDirectory(Path.Combine(newFolderPath, "SubData"));
                GameEditor.GeneratePlaceHolderTextFile(Path.Combine(newFolderPath, "SubData"));
                Directory.CreateDirectory(Path.Combine(newFolderPath, "Avatars"));
                GameEditor.GeneratePlaceHolderTextFile(Path.Combine(newFolderPath, "Avatars"));
                Directory.CreateDirectory(Path.Combine(newFolderPath, "Portraits"));
                GameEditor.GeneratePlaceHolderTextFile(Path.Combine(newFolderPath, "Portraits"));

                //随后处理树状图与缓存
                //新建物体容器
                DataContainer newDataContainer = new(
                    newCharacterData,
                    charaItemDataContainer
                    );
                //生成树状图物体
                TreeViewItemData<DataContainer> newItem = new(
                    newDataContainer.ID,
                    newDataContainer,
                    null
                    );
                //添加到总缓存中
                ItemDicCache[newDataContainer.ID] = newItem;
                //添加到其父级的子级中，由于该数据不可直接更改，而缓存中保存的是同一个引用，因此从缓存中进行更改
                CharaVersionDicCache[charaItemDataContainer.ID].Add(newItem);
                //刷新
                TRefresh();

                //将角色数据标记为脏
                GameEditor.IsCharacterDataDirty = true;

                //保存数据
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        #endregion
    }
}

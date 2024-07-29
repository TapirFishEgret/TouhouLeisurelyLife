using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THLL.CharacterSystem;
using UnityEditor;
using UnityEngine.TextCore.Text;
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
        public HashSet<int> ExpandedStatePersistentData { get; } = new();

        //当前活跃选中项
        public ItemDataContainer ActiveSelection { get; private set; }
        #endregion

        #region 构造函数与刷新与初始化
        //构造函数
        public DataTreeView(MainWindow window)
        {
            //赋值
            MainWindow = window;

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
        }
        //初始化方法
        private void Init()
        {
            //总之先生成物体
            GenerateItems();

            //然后事件绑定
            RegisterEvents();
        }
        //生成物体方法
        private void GenerateItems()
        {
            //读取文件中所有的CharacterData类
            List<CharacterData> characterDatas = AssetDatabase.FindAssets("t:CharacterData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            //构建数据结构
            //总字典
            Dictionary<string, Dictionary<string, Dictionary<string, List<CharacterData>>>> totalDataDic = new();
            //构建
            foreach (CharacterData characterData in characterDatas)
            {
                //检测总字典是否存在系列作品数据
                if (!totalDataDic.ContainsKey(characterData.OriginatingSeries))
                {
                    //不存在，则创建
                    totalDataDic[characterData.OriginatingSeries] = new();
                }

                //检测是否存在组织数据
                if (!totalDataDic[characterData.OriginatingSeries].ContainsKey(characterData.Affiliation))
                {
                    //不存在，创建
                    totalDataDic[characterData.OriginatingSeries][characterData.Affiliation] = new();
                }

                //检测是否存在角色名数据
                if (!totalDataDic[characterData.OriginatingSeries][characterData.Affiliation].ContainsKey(characterData.Name))
                {
                    //不存在，创建
                    totalDataDic[characterData.OriginatingSeries][characterData.Affiliation][characterData.Name] = new();
                }

                //添加数据
                totalDataDic[characterData.OriginatingSeries][characterData.Affiliation][characterData.Name].Add(characterData);
            }

            //随后开始构建树形图物体
            //遍历第一层
            foreach (var series in totalDataDic)
            {
                //创建第一层的子级
                List<TreeViewItemData<ItemDataContainer>> seriesChildren = new();
                //创建二层缓存
                SeriesAffiliationDicCache[series.Key.GetHashCode()] = new();
                //遍历该数据的第二层
                foreach (var affiliation in series.Value)
                {
                    //创建第二层子级
                    List<TreeViewItemData<ItemDataContainer>> affiliationChildren = new();
                    //创建三层缓存
                    AffiliationCharacterDicCache[affiliation.Key.GetHashCode()] = new();
                    //遍历该数据的第三层
                    foreach (var character in affiliation.Value)
                    {
                        //创建第三层子级
                        List<TreeViewItemData<ItemDataContainer>> characterChildren = new();
                        //创建四层缓存
                        CharacterVersionDicCache[character.Key.GetHashCode()] = new();
                        //遍历最底层
                        foreach (var version in character.Value)
                        {
                            //创建物体容器
                            ItemDataContainer versionItemContainer = new(version);
                            //创建物体
                            TreeViewItemData<ItemDataContainer> versionItem = new(version.GetAssetHashCode(), versionItemContainer, null);
                            //添加到上层子级中
                            characterChildren.Add(versionItem);
                            //添加到总集缓存与四层缓存中
                            ItemDicCache[version.GetAssetHashCode()] = versionItem;
                            CharacterVersionDicCache[character.Key.GetHashCode()].Add(versionItem);
                        }
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
                        //创建物体容器
                        ItemDataContainer characterItemContainer = new(character.Key, ItemDataContainer.ItemType.CharacterName, characterSortingOrder);
                        //创建物体
                        TreeViewItemData<ItemDataContainer> characterItem = new(character.Key.GetHashCode(), characterItemContainer, characterChildren);
                        //添加到上层子级中
                        affiliationChildren.Add(characterItem);
                        //对对应的四层缓存数据排序
                        CharacterVersionDicCache[character.Key.GetHashCode()].Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
                        //添加到总集缓存与三层缓存中
                        ItemDicCache[character.Key.GetHashCode()] = characterItem;
                        AffiliationCharacterDicCache[affiliation.Key.GetHashCode()].Add(characterItem);
                    }
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
                    ItemDataContainer affiliationItemContainer = new(affiliation.Key, ItemDataContainer.ItemType.Affiliation, affiliationSortingOrder);
                    //创建物体
                    TreeViewItemData<ItemDataContainer> affiliationItem = new(affiliation.Key.GetHashCode(), affiliationItemContainer, affiliationChildren);
                    //添加到上层子级中
                    seriesChildren.Add(affiliationItem);
                    //对对应的三层缓存进行排序
                    AffiliationCharacterDicCache[affiliation.Key.GetHashCode()].Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
                    //添加到总集缓存与二层缓存中
                    ItemDicCache[affiliation.Key.GetHashCode()] = affiliationItem;
                    SeriesAffiliationDicCache[series.Key.GetHashCode()].Add(affiliationItem);
                }
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
                ItemDataContainer seriesItemContainer = new(series.Key, ItemDataContainer.ItemType.OriginatingSeries, seriesSortingOrder);
                //创建物体
                TreeViewItemData<ItemDataContainer> seriesItem = new(series.Key.GetHashCode(), seriesItemContainer, seriesChildren);
                //对对应的二层缓存进行排序
                SeriesAffiliationDicCache[series.Key.GetHashCode()].Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
                //添加到总集缓存与一层缓存中
                ItemDicCache[series.Key.GetHashCode()] = seriesItem;
                RootItemCache.Add(seriesItem);
            }
            //对一层缓存进行排序
            RootItemCache.Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));

            //构建完成后设置树形图根物体
            SetRootItems(RootItemCache);

            //自定义显示逻辑
            //物体采用普通标签显示
            makeItem = () => new Label();
            //绑定逻辑
            bindItem = (element, i) =>
            {
                //获取数据与物体展示形态
                ItemDataContainer itemDataContainer = GetItemDataForIndex<ItemDataContainer>(i);
                Label label = element as Label;

                //设置展示形态显示内容
                if (itemDataContainer.Type == ItemDataContainer.ItemType.CharacterData)
                {
                    //若为角色数据，则设置显示名称为角色名+版本名
                    label.text = $"{itemDataContainer.CharacterData.Name}_{itemDataContainer.CharacterData.Version}";
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
                }
            };

            //然后绑定的是，取消选择的方法
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
        #endregion

        #region 数据的增加与删除
        #endregion
    }
}

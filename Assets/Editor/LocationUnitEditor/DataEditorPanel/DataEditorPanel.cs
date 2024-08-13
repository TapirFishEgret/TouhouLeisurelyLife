using THLL.LocationSystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class DataEditorPanel : Tab
    {
        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //基层面板
        private VisualElement BasePanel { get; set; }
        //组及父级信息
        private ObjectField AssetGroupField { get; set; }
        private ObjectField ParentDataField { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //排序位置
        private IntegerField SortingOrderField { get; set; }
        //设置控件
        private TextField DescriptionField { get; set; }
        private ObjectField BackgroundField { get; set; }
        //连接展示框
        private MultiColumnListView ConnectionsShowView { get; set; }
        #endregion

        #region 数据编辑面板的初始化以及数据更新
        //构建函数
        public DataEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //获取面板
            VisualElement panel = visualTree.CloneTree();
            Add(panel);

            //指定主窗口
            MainWindow = mainWindow;

            //初始化
            Init();
        }
        //初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //设置标签页容器可延展
            style.flexGrow = 1;
            contentContainer.style.flexGrow = 1;

            //设定名称
            label = "数据编辑窗口";

            //获取UI控件
            //基层面板
            BasePanel = this.Q<VisualElement>("DataEditorPanel");
            //基础项
            AssetGroupField = this.Q<ObjectField>("AssetGroupField");
            ParentDataField = this.Q<ObjectField>("ParentDataField");
            //全名
            FullNameLabel = this.Q<Label>("FullNameLabel");
            //排序位置
            SortingOrderField = this.Q<IntegerField>("SortingOrderField");
            //设置控件
            DescriptionField = this.Q<TextField>("DescriptionField");
            BackgroundField = this.Q<ObjectField>("BackgroundField");
            //连接展示框
            ConnectionsShowView = this.Q<MultiColumnListView>("ConnectionsShowView");

            //添加连接显示框的内容
            //添加全名列
            ConnectionsShowView.columns.Add(new Column
            {
                name = "FullName",
                title = "FullName",
                makeCell = () => new Label(),
                width = new Length(60, LengthUnit.Percent)
            });
            //添加索引列
            ConnectionsShowView.columns.Add(new Column
            {
                name = "DataObject",
                title = "DataObject",
                makeCell = () => new ObjectField(),
                width = new Length(20, LengthUnit.Percent)
            });
            //添加耗时列
            ConnectionsShowView.columns.Add(new Column
            {
                name = "Distance",
                title = "Distance",
                makeCell = () => new Label(),
                width = new Length(20, LengthUnit.Percent)
            });

            //注册事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        //刷新面板
        public void DRefresh()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有
                //清除旧的绑定
                Unbind();
                //重新绑定
                Bind(MainWindow.DataTreeView.ActiveSelection);

                //设置数据
                //设置全名显示
                FullNameLabel.text = string.Join("/", MainWindow.DataTreeView.ActiveSelection.FullName);
                //检测背景图状态
                if (BackgroundField.value == null)
                {
                    //若无背景图，则设置为默认
                    BackgroundField.value = MainWindow.DefaultLocationBackground;
                }
                //随后设置面板背景图
                style.backgroundImage = new StyleBackground(BackgroundField.value as Sprite);
                //设置背景图延展模式为切削
                style.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                style.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                style.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleAndCrop);
                style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleAndCrop);
                //设置当前那选中数据资源组
                SetAddressableAssetGroup();

                //设置双列列表视图
                //数据源的设置
                ConnectionsShowView.itemsSource = MainWindow.DataTreeView.ActiveSelection.ConnectionKeys;
                //数据的重新绑定
                ConnectionsShowView.columns[0].bindCell = (element, i) => (element as Label).text = string.Join("/", MainWindow.DataTreeView.ActiveSelection.ConnectionKeys[i].FullName);
                ConnectionsShowView.columns[1].bindCell = (element, i) => (element as ObjectField).value = MainWindow.DataTreeView.ActiveSelection.ConnectionKeys[i];
                ConnectionsShowView.columns[2].bindCell = (element, i) => (element as Label).text = MainWindow.DataTreeView.ActiveSelection.ConnectionValues[i].ToString();
                //刷新
                ConnectionsShowView.RefreshItems();
            }
        }
        //绑定
        private void Bind(LocUnitData locUnitData)
        {
            //不触发通知的情况下更改数据
            ParentDataField.SetValueWithoutNotify(locUnitData.ParentData);
            DescriptionField.SetValueWithoutNotify(locUnitData.Description);
            BackgroundField.SetValueWithoutNotify(locUnitData.Background);
            SortingOrderField.SetValueWithoutNotify(locUnitData.SortingOrder);

            //将控件绑定至新数据上
            DescriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
            BackgroundField.RegisterValueChangedCallback(OnBackgroundChanged);
            SortingOrderField.RegisterValueChangedCallback(OnSortingOrderChanged);
        }
        //清除绑定
        private void Unbind()
        {
            //将控件从旧数据清除绑定
            DescriptionField.UnregisterValueChangedCallback(OnDescriptionChanged);
            BackgroundField.UnregisterValueChangedCallback(OnBackgroundChanged);
            SortingOrderField.UnregisterValueChangedCallback(OnSortingOrderChanged);
        }
        //几何图形改变时手动调整窗口大小
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //检测当前多标签页面板选中标签是否为自身
            if (MainWindow.MultiTabView.activeTab == this)
            {
                //若是，则触发更改
                BasePanel.style.width = evt.newRect.width;
                BasePanel.style.height = evt.newRect.height;
            }
        }
        #endregion

        #region 数据处理方法
        //描述改变时
        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveSelection.Editor_SetDescription(evt.newValue);
        }
        //背景改变时
        private void OnBackgroundChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is Sprite sprite)
            {
                MainWindow.DataTreeView.ActiveSelection.Editor_SetBackground(sprite);
                style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                MainWindow.DataTreeView.ActiveSelection.Editor_SetBackground(null);
            }
        }
        //排序改变时
        private void OnSortingOrderChanged(ChangeEvent<int> evt)
        {
            //设置排序
            MainWindow.DataTreeView.ActiveSelection.Editor_SetSortingOrder(evt.newValue);
            //重排
            if (MainWindow.DataTreeView.ActiveSelection.ParentData == null)
            {
                MainWindow.DataTreeView.RootItemCache.Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
            }
            else
            {
                MainWindow.DataTreeView.ChildrenDicCache[MainWindow.DataTreeView.ActiveSelection.ParentData.GetAssetHashCode()]
                    .Sort((x, y) => x.data.SortingOrder.CompareTo(y.data.SortingOrder));
            }
            //刷新
            MainWindow.DataTreeView.TRefresh();
        }
        //设置所属资源组
        private void SetAddressableAssetGroup()
        {
            //获取设置
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            //获取当前选中项的GUID
            string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(MainWindow.DataTreeView.ActiveSelection));
            //遍历所有组，找到资源所在组
            foreach (AddressableAssetGroup group in settings.groups)
            {
                //检测是否为空
                if (group == null) continue;

                //若不为空，查找条目
                AddressableAssetEntry entry = group.GetAssetEntry(assetGUID);
                //检测是否为空
                if (entry != null)
                {
                    //若不为空，设置组，并结束方法
                    AssetGroupField.SetValueWithoutNotify(group);
                    return;
                }
            }
            //若没能获取结果，提出警告
            Debug.LogWarning("该资源没有对应的可寻址资源组，请检测并排查问题！");
        }
        #endregion
    }
}

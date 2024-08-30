using THLL.LocationSystem;
using UnityEditor;
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
        //背景显示
        private VisualElement BackgroundView { get; set; }
        //父级信息
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
            //背景显示
            BackgroundView = this.Q<VisualElement>("BackgroundView");
            //基础项
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

            //绑定UI控件
            Bind();

            //设置背景图延展模式为切削
            BackgroundView.style.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
            BackgroundView.style.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
            BackgroundView.style.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleAndCrop);
            BackgroundView.style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleAndCrop);

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
                //设置数据
                //不触发通知的情况下更改数据
                ParentDataField.SetValueWithoutNotify(MainWindow.DataTreeView.ActiveSelection.ParentData);
                DescriptionField.SetValueWithoutNotify(MainWindow.DataTreeView.ActiveSelection.Description);
                BackgroundField.SetValueWithoutNotify(MainWindow.DataTreeView.ActiveSelection.Background);
                SortingOrderField.SetValueWithoutNotify(MainWindow.DataTreeView.ActiveSelection.SortingOrder);
                //设置全名显示
                FullNameLabel.text = string.Join("/", MainWindow.DataTreeView.ActiveSelection.FullName);
                //并调整全名大小
                EditorExtensions.SingleLineLabelAdjustFontSizeToFit(FullNameLabel);
                //检测背景图状态
                if (BackgroundField.value == null)
                {
                    //若无背景图，则设置为默认
                    BackgroundField.value = MainWindow.DefaultLocationBackground;
                }
                //随后设置面板背景图
                BackgroundView.style.backgroundImage = new StyleBackground(BackgroundField.value as Sprite);

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
        private void Bind()
        {
            //将控件绑定至新数据上
            DescriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
            BackgroundField.RegisterValueChangedCallback(OnBackgroundChanged);
            SortingOrderField.RegisterValueChangedCallback(OnSortingOrderChanged);
        }
        //几何图形改变时手动调整窗口大小
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //触发更改
            BasePanel.style.width = evt.newRect.width;
            BasePanel.style.height = evt.newRect.height;
            //并调整全名大小
            EditorExtensions.SingleLineLabelAdjustFontSizeToFit(FullNameLabel);
        }
        #endregion

        #region 数据处理方法
        //描述改变时
        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveSelection.Description = evt.newValue;
        }
        //背景改变时
        private void OnBackgroundChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is Sprite sprite)
            {
                MainWindow.DataTreeView.ActiveSelection.Background = sprite;
                BackgroundView.style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                MainWindow.DataTreeView.ActiveSelection.Background = null;
            }
        }
        //排序改变时
        private void OnSortingOrderChanged(ChangeEvent<int> evt)
        {
            //设置排序
            MainWindow.DataTreeView.ActiveSelection.SortingOrder = evt.newValue;
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
        #endregion
    }
}

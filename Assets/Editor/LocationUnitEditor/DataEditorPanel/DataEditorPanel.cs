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
        private VisualElement _dataEditorPanel;
        //基础四项
        private TextField _packageField;
        private TextField _authorFiled;
        private ObjectField _parentDataField;
        //全名
        private Label _fullNameLabel;
        //排序位置
        private IntegerField _sortingOrderField;
        //设置控件
        private TextField _descriptionField;
        private ObjectField _backgroundField;
        //连接展示框
        private MultiColumnListView _connectionsShowView;
        #endregion

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

        #region 数据编辑面板的初始化以及数据更新
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
            _dataEditorPanel = this.Q<VisualElement>("DataEditorPanel");
            //基础项
            _packageField = this.Q<TextField>("PackageField");
            _authorFiled = this.Q<TextField>("AuthorField");
            _parentDataField = this.Q<ObjectField>("ParentDataField");
            //全名
            _fullNameLabel = this.Q<Label>("FullNameLabel");
            //排序位置
            _sortingOrderField = this.Q<IntegerField>("SortingOrderField");
            //设置控件
            _descriptionField = this.Q<TextField>("DescriptionField");
            _backgroundField = this.Q<ObjectField>("BackgroundField");
            //连接展示框
            _connectionsShowView = this.Q<MultiColumnListView>("ConnectionsShowView");

            //添加连接显示框的内容
            //添加全名列
            _connectionsShowView.columns.Add(new Column
            {
                name = "FullName",
                title = "FullName",
                makeCell = () => new Label(),
                width = new Length(60, LengthUnit.Percent)
            });
            //添加索引列
            _connectionsShowView.columns.Add(new Column
            {
                name = "DataObject",
                title = "DataObject",
                makeCell = () => new ObjectField(),
                width = new Length(20, LengthUnit.Percent)
            });
            //添加耗时列
            _connectionsShowView.columns.Add(new Column
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

            //清除旧的绑定
            Unbind();
            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有
                //重新绑定
                Bind(MainWindow.DataTreeView.ActiveSelection);

                //设置数据
                //设置全名显示
                _fullNameLabel.text = string.Join("/", MainWindow.DataTreeView.ActiveSelection.FullName);
                //检测背景图状态
                if (_backgroundField.value == null)
                {
                    //若无背景图，则设置为默认
                    _backgroundField.value = MainWindow.DefaultLocationBackground;
                }
                //随后设置面板背景图
                style.backgroundImage = new StyleBackground(_backgroundField.value as Sprite);
                //设置背景图延展模式为切削
                style.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                style.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                style.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleAndCrop);
                style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleAndCrop);

                //设置双列列表视图
                //数据源的设置
                _connectionsShowView.itemsSource = MainWindow.DataTreeView.ActiveSelection.ConnectionKeys;
                //数据的重新绑定
                _connectionsShowView.columns[0].bindCell = (element, i) => (element as Label).text = string.Join("/", MainWindow.DataTreeView.ActiveSelection.ConnectionKeys[i].FullName);
                _connectionsShowView.columns[1].bindCell = (element, i) => (element as ObjectField).value = MainWindow.DataTreeView.ActiveSelection.ConnectionKeys[i];
                _connectionsShowView.columns[2].bindCell = (element, i) => (element as Label).text = MainWindow.DataTreeView.ActiveSelection.ConnectionValues[i].ToString();
                //刷新
                _connectionsShowView.RefreshItems();
            }
        }
        //绑定
        private void Bind(LocUnitData locUnitData)
        {
            //不触发通知的情况下更改数据
            _packageField.SetValueWithoutNotify(locUnitData.Package);
            _authorFiled.SetValueWithoutNotify(locUnitData.Author);
            _parentDataField.SetValueWithoutNotify(locUnitData.ParentData);
            _descriptionField.SetValueWithoutNotify(locUnitData.Description);
            _backgroundField.SetValueWithoutNotify(locUnitData.Background);
            _sortingOrderField.SetValueWithoutNotify(locUnitData.SortingOrder);

            //将控件绑定至新数据上
            _packageField.RegisterValueChangedCallback(OnPackageChanged);
            _authorFiled.RegisterValueChangedCallback(OnAuthorChanged);
            _descriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
            _backgroundField.RegisterValueChangedCallback(OnBackgroundChanged);
            _sortingOrderField.RegisterValueChangedCallback(OnSortingOrderChanged);
        }
        //清除绑定
        private void Unbind()
        {
            //将控件从旧数据清除绑定
            _packageField.UnregisterValueChangedCallback(OnPackageChanged);
            _authorFiled.UnregisterValueChangedCallback(OnAuthorChanged);
            _descriptionField.UnregisterValueChangedCallback(OnDescriptionChanged);
            _backgroundField.UnregisterValueChangedCallback(OnBackgroundChanged);
            _sortingOrderField.UnregisterValueChangedCallback(OnSortingOrderChanged);
        }
        //几何图形改变时手动调整窗口大小
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //检测当前多标签页面板选中标签是否为自身
            if (MainWindow.MultiTabView.activeTab == this)
            {
                //若是，则触发更改
                _dataEditorPanel.style.width = evt.newRect.width;
                _dataEditorPanel.style.height = evt.newRect.height;
            }
        }
        //数据处理方法
        private void OnPackageChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveSelection.Editor_SetPackage(evt.newValue);
        }
        private void OnAuthorChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveSelection.Editor_SetAuthor(evt.newValue);
        }

        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveSelection.Editor_SetDescription(evt.newValue);
        }

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
        #endregion
    }
}

using THLL.SceneSystem;
using UnityEditor;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class DataEditorPanel : Tab
    {
        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示的场景
        public SceneData ShowedScene { get => MainWindow.DataTreeView.ActiveSelection.Data; }

        //基层面板
        private VisualElement BasePanel { get; set; }
        //背景显示
        private VisualElement BackgroundView { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //排序位置
        private IntegerField SortingOrderField { get; set; }
        //设置控件
        private TextField DescriptionField { get; set; }
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
            //全名
            FullNameLabel = this.Q<Label>("FullNameLabel");
            //排序位置
            SortingOrderField = this.Q<IntegerField>("SortingOrderField");
            //设置控件
            DescriptionField = this.Q<TextField>("DescriptionField");

            //绑定UI控件
            Bind();

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
                DescriptionField.SetValueWithoutNotify(ShowedScene.Description);
                SortingOrderField.SetValueWithoutNotify(ShowedScene.SortOrder);
                //设置全名显示
                //FullNameLabel.text = string.Join("/", ShowedScene.FullName);
                //并调整全名大小
                GameEditor.SingleLineLabelAdjustFontSizeToFit(FullNameLabel);
            }
        }
        //绑定
        private void Bind()
        {
            //将控件绑定至新数据上
            DescriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
        }
        //几何图形改变时手动调整窗口大小
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //触发更改
            BasePanel.style.width = evt.newRect.width;
            BasePanel.style.height = evt.newRect.height;
            //并调整全名大小
            GameEditor.SingleLineLabelAdjustFontSizeToFit(FullNameLabel);
        }
        #endregion

        #region 数据处理方法
        //描述改变时
        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            ShowedScene.Description = evt.newValue;
        }
        #endregion
    }
}

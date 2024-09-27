using THLL.CharacterSystem;
using UnityEditor;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.CharacterEditor
{
    public class DataEditorPanel : Tab
    {
        #region 基础构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示数据
        public CharacterData ShowedCharacter { get { return MainWindow.DataTreeView.ActiveSelection.Data; } }

        //基础面板
        private VisualElement EditorRootPanel { get; set; }
        //信息显示
        private Label FullInfoLabel { get; set; }
        //数据编辑
        private TextField DescriptionField { get; set; }
        private IntegerField SortingOrderField { get; set; }
        #endregion

        #region 构造及初始化
        //构造函数
        public DataEditorPanel(VisualTreeAsset visualTree, MainWindow window)
        {
            //获取面板
            VisualElement panel = visualTree.CloneTree();
            Add(panel);

            //指定主窗口
            MainWindow = window;

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

            //设置标签页名称
            label = "数据编辑面板";

            //获取UI控件
            EditorRootPanel = this.Q<VisualElement>("DataEditorPanel");
            FullInfoLabel = this.Q<Label>("FullInfoLabel");
            DescriptionField = this.Q<TextField>("DescriptionField");
            SortingOrderField = this.Q<IntegerField>("SortingOrderField");

            //绑定事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        //面板大小更改事件
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //面板大小更改时，数据编辑面板大小同步更改
            EditorRootPanel.style.width = evt.newRect.width;
            EditorRootPanel.style.height = evt.newRect.height;
            //同时更改Label字体大小
            GameEditor.SingleLineLabelAdjustFontSizeToFit(FullInfoLabel);
        }
        #endregion

        #region 刷新与绑定与反绑定
        //刷新
        public void DRefresh()
        {
            //刷新前保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //判断当前数据情况
            if (ShowedCharacter != null)
            {
                //若不为空，进行刷新
                //计时
                using ExecutionTimer timer = new("数据编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

                //然后进行绑定
                Bind();
            }
        }
        //绑定
        private void Bind()
        {
            //绑定前以不通知的形式设置显示数据
            DescriptionField.SetValueWithoutNotify(ShowedCharacter.Description);
            SortingOrderField.SetValueWithoutNotify(ShowedCharacter.SortOrder);

            //显示全部信息
            FullInfoLabel.text = ($"{ShowedCharacter.Series}" +
                $"_{ShowedCharacter.Group}" +
                $"_{ShowedCharacter.Name}" +
                $"_{ShowedCharacter.Version}").Replace(" ", "-");
            //同时更改Label字体大小
            GameEditor.SingleLineLabelAdjustFontSizeToFit(FullInfoLabel);

            //绑定
            DescriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
            SortingOrderField.RegisterValueChangedCallback(OnSortingOrderChanged);
        }
        #endregion

        #region 数据更改事件与事件
        //描述更改
        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            ShowedCharacter.Description = evt.newValue;
        }
        //排序更改
        private void OnSortingOrderChanged(ChangeEvent<int> evt)
        {
            //设置排序
            ShowedCharacter.SortOrder = evt.newValue;
            //重排
            MainWindow.DataTreeView.CharacterVersionDicCache
                [ShowedCharacter.Character.GetHashCode()]
                .Sort((a, b) => a.data.SortOrder.CompareTo(b.data.SortOrder));
            //刷新
            MainWindow.DataTreeView.TRefresh();
        }
        #endregion
    }
}

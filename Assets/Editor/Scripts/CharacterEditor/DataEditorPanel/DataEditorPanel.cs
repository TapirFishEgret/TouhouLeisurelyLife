using THLL.CharacterSystem;
using THLL.GeographySystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.CharacterDataEditor
{
    public class DataEditorPanel : Tab
    {
        #region 基础构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示数据
        public CharacterData ShowedCharacter { get { return MainWindow.DataTreeView.ActiveSelection.CharacterData; } }

        //基础面板
        private VisualElement EditorRootPanel { get; set; }
        //角色头像与立绘
        private VisualElement CharacterAvatar { get; set; }
        private VisualElement CharacterPortrait { get; set; }
        //信息显示
        private Label FullInfoLabel { get; set; }
        //数据编辑
        private TextField DescriptionField { get; set; }
        private IntegerField SortingOrderField { get; set; }
        private ObjectField AvatarField { get; set; }
        private ObjectField PortraitField { get; set; }
        private ObjectField LivingAreaField { get; set; }
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

            //设置标签页为可延展
            style.flexGrow = 1;
            contentContainer.style.flexGrow = 1;

            //设置标签页名称
            label = "数据编辑面板";

            //获取UI控件
            EditorRootPanel = this.Q<VisualElement>("DataEditorPanel");
            CharacterAvatar = this.Q<VisualElement>("CharacterAvatar");
            CharacterPortrait = this.Q<VisualElement>("CharacterPortrait");
            FullInfoLabel = this.Q<Label>("FullInfoLabel");
            DescriptionField = this.Q<TextField>("DescriptionField");
            SortingOrderField = this.Q<IntegerField>("SortingOrderField");
            AvatarField = this.Q<ObjectField>("AvatarField");
            PortraitField = this.Q<ObjectField>("PortraitField");
            LivingAreaField = this.Q<ObjectField>("LivingAreaField");

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
            EditorExtensions.SingleLineLabelAdjustFontSizeToFit(FullInfoLabel);
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
            SortingOrderField.SetValueWithoutNotify(ShowedCharacter.SortingOrder);
            AvatarField.SetValueWithoutNotify(ShowedCharacter.Avatar);
            PortraitField.SetValueWithoutNotify(ShowedCharacter.Portrait);
            LivingAreaField.SetValueWithoutNotify(ShowedCharacter.LivingArea);

            //显示角色头像与立绘
            CharacterAvatar.style.backgroundImage = new StyleBackground(ShowedCharacter.Avatar);
            CharacterPortrait.style.backgroundImage = new StyleBackground(ShowedCharacter.Portrait);

            //显示全部信息
            FullInfoLabel.text = ($"{ShowedCharacter.OriginatingSeries}" +
                $"_{ShowedCharacter.Affiliation}" +
                $"_{ShowedCharacter.Name}").Replace(" ", "-");
            //同时更改Label字体大小
            EditorExtensions.SingleLineLabelAdjustFontSizeToFit(FullInfoLabel);

            //绑定
            DescriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
            SortingOrderField.RegisterValueChangedCallback(OnSortingOrderChanged);
            AvatarField.RegisterValueChangedCallback(OnAvatarChanged);
            PortraitField.RegisterValueChangedCallback(OnPortraitChanged);
            LivingAreaField.RegisterValueChangedCallback(OnLivingAreaChanged);
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
            ShowedCharacter.SortingOrder = evt.newValue;
            //重排
            MainWindow.DataTreeView.AffiliationCharacterDicCache
                [ShowedCharacter.Affiliation.GetHashCode()]
                .Sort((a, b) => a.data.SortingOrder.CompareTo(b.data.SortingOrder));
            //刷新
            MainWindow.DataTreeView.TRefresh();
        }
        //头像更改
        private void OnAvatarChanged(ChangeEvent<Object> evt)
        {
            //检测传入数据
            if (evt.newValue is Sprite avatar)
            {
                //设置头像
                ShowedCharacter.Avatar = avatar;
                //设置显示
                CharacterAvatar.style.backgroundImage = new StyleBackground(avatar);
            }
            else
            {
                //若不是，均设置为空
                ShowedCharacter.Avatar = null;
                CharacterAvatar.style.backgroundImage = null;
            }
        }
        //立绘更改
        private void OnPortraitChanged(ChangeEvent<Object> evt)
        {
            //检测传入数据
            if (evt.newValue is Sprite portrait)
            {
                //设置头像
                ShowedCharacter.Portrait = portrait;
                //设置显示
                CharacterPortrait.style.backgroundImage = new StyleBackground(portrait);
            }
            else
            {
                //若不是，均设置为空
                ShowedCharacter.Portrait = null;
                CharacterPortrait.style.backgroundImage = null;
            }
        }
        //居住地区更改
        private void OnLivingAreaChanged(ChangeEvent<Object> evt)
        {
            //检测传入数据
            if (evt.newValue is LocationData livingArea)
            {
                //设置头像
                ShowedCharacter.LivingArea = livingArea;
                //设置显示
                //EditorRootPanel.style.backgroundImage = new StyleBackground(livingArea.BackgroundView);
            }
            else
            {
                //若不是，均设置为空
                ShowedCharacter.LivingArea = null;
                //EditorRootPanel.style.backgroundImage = null;
            }
        }
        #endregion
    }
}

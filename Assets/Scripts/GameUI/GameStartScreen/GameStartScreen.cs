using THLL.BaseSystem;
using UnityEngine.UIElements;

namespace THLL.GameUI
{
    public class GameStartScreen : Singleton<GameStartScreen>
    {
        #region 数据
        //UI界面
        //UIDocument组件
        public UIDocument MainUI { get; private set; }

        //UI控件
        //TopButtonsPanel
        public VisualElement TopButtonsPanel { get; private set; }
        //BottomPanelChangeButtonGroup
        public ToggleButtonGroup BottomPanelChangeButtonGroup { get; private set; }
        //GamePlayButton
        public Button GamePlayButton { get; private set; }
        //AssetGroupPanel
        public Button GamePatchesButton { get; private set; }
        //GamePlayButtonsPanel
        public VisualElement GamePlayButtonsPanel { get; private set; }
        //NewGameButton
        public Button NewGameButton { get; private set; }
        //LoadGameButton
        public Button LoadGameButton { get; private set; }
        //SettingsButton
        public Button SettingsButton { get; private set; }
        //QuitGameButton
        public Button QuitGameButton { get; private set; }
        //GamePatchesButtonsPanel
        public VisualElement GamePatchesButtonsPanel { get; private set; }
        //LocationPatchesButton
        public VisualElement LocationPatchesButton { get; private set; }
        //CharacterPatchesButton
        public VisualElement CharacterPatchesButton { get; private set; }
        #endregion

        #region Unity周期函数
        //Start
        protected override void Start()
        {
            //父类Start
            base.Start();

            //获取组件
            MainUI = GetComponent<UIDocument>();

            //初始化
            Init();
        }
        #endregion

        #region 初始化与相关方法
        //初始化
        private void Init()
        {
            //获取控件
            GetVisualElements();
            //绑定方法
            RegisterMethods();
        }
        //获取UI控件
        private void GetVisualElements()
        {
            TopButtonsPanel = MainUI.rootVisualElement.Q<VisualElement>("TopButtonsPanel");
            BottomPanelChangeButtonGroup = MainUI.rootVisualElement.Q<ToggleButtonGroup>("BottomPanelChangeButtonGroup");
            GamePlayButton = BottomPanelChangeButtonGroup.Q<Button>("GamePlayButton");
            GamePatchesButton = BottomPanelChangeButtonGroup.Q<Button>("GamePatchesButton");
            GamePlayButtonsPanel = MainUI.rootVisualElement.Q<VisualElement>("GamePlayButtonsPanel");
            NewGameButton = GamePlayButtonsPanel.Q<Button>("NewGameButton");
            LoadGameButton = GamePlayButtonsPanel.Q<Button>("LoadGameButton");
            SettingsButton = GamePlayButtonsPanel.Q<Button>("SettingsButton");
            QuitGameButton = GamePlayButtonsPanel.Q<Button>("QuitGameButton");
            GamePatchesButtonsPanel = MainUI.rootVisualElement.Q<VisualElement>("GamePatchesButtonsPanel");
            LocationPatchesButton = GamePatchesButtonsPanel.Q<Button>("LocationPatchesButton");
            CharacterPatchesButton = GamePatchesButtonsPanel.Q<Button>("CharacterPatchesButton");
        }
        //绑定控件方法
        private void RegisterMethods()
        {
            BottomPanelChangeButtonGroup.RegisterValueChangedCallback(OnBottonGroupSelectionChanged);
        }
        #endregion

        #region UI方法
        //当面板切换时
        private void OnBottonGroupSelectionChanged(ChangeEvent<ToggleButtonGroupState> evt)
        {
            //检测面板打开状态
            if (evt.newValue[0])
            {
                //若0号位为激活，按照面板设置，GamePlay相关按钮激活
                GamePlayButtonsPanel.style.display = DisplayStyle.Flex;
                GamePatchesButtonsPanel.style.display = DisplayStyle.None;
            }
            else if (evt.newValue[1])
            {
                //若1号位为激活，按照面板设置，AssetGroup相关按钮激活
                GamePlayButtonsPanel.style.display = DisplayStyle.None;
                GamePatchesButtonsPanel.style.display = DisplayStyle.Flex;
            }
        }
        #endregion
    }
}

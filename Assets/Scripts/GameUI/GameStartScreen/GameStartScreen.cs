using THLL.BaseSystem;
using UnityEngine.UIElements;

namespace THLL.GameUI
{
    public class GameStartScreen : Singleton<GameStartScreen>
    {
        #region ����
        //UI����
        //UIDocument���
        public UIDocument MainUI { get; private set; }

        //UI�ؼ�
        //TopButtonsPanel
        public VisualElement TopButtonsPanel { get; private set; }
        //BottomPanelChangeButtonGroup
        public ToggleButtonGroup BottomPanelChangeButtonGroup { get; private set; }
        //GamePlayButton
        public Button GamePlayButton { get; private set; }
        //AssetGroupPanel
        public Button AssetGroupButton { get; private set; }
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
        //AssetGroupButtonsPanel
        public VisualElement AssetGroupButtonsPanel { get; private set; }
        //LocationAssetGroupButton
        public VisualElement LocationAssetGroupButton { get; private set; }
        //CharacterAssetGroupButton
        public VisualElement CharacterAssetGroupButton { get; private set; }
        #endregion

        #region Unity���ں���
        //Start
        protected override void Start()
        {
            //����Start
            base.Start();

            //��ȡ���
            MainUI = GetComponent<UIDocument>();

            //��ʼ��
            Init();
        }
        #endregion

        #region ��ʼ������ط���
        //��ʼ��
        private void Init()
        {
            //��ȡ�ؼ�
            GetVisualElements();
            //�󶨷���
            RegisterMethods();
        }
        //��ȡUI�ؼ�
        private void GetVisualElements()
        {
            TopButtonsPanel = MainUI.rootVisualElement.Q<VisualElement>("TopButtonsPanel");
            BottomPanelChangeButtonGroup = MainUI.rootVisualElement.Q<ToggleButtonGroup>("BottomPanelChangeButtonGroup");
            GamePlayButton = BottomPanelChangeButtonGroup.Q<Button>("GamePlayButton");
            AssetGroupButton = BottomPanelChangeButtonGroup.Q<Button>("AssetGroupButton");
            GamePlayButtonsPanel = MainUI.rootVisualElement.Q<VisualElement>("GamePlayButtonsPanel");
            NewGameButton = GamePlayButtonsPanel.Q<Button>("NewGameButton");
            LoadGameButton = GamePlayButtonsPanel.Q<Button>("LoadGameButton");
            SettingsButton = GamePlayButtonsPanel.Q<Button>("SettingsButton");
            QuitGameButton = GamePlayButtonsPanel.Q<Button>("QuitGameButton");
            AssetGroupButtonsPanel = MainUI.rootVisualElement.Q<VisualElement>("AssetGroupButtonsPanel");
            LocationAssetGroupButton = AssetGroupButtonsPanel.Q<Button>("LocationAssetGroupButton");
            CharacterAssetGroupButton = AssetGroupButtonsPanel.Q<Button>("CharacterAssetGroupButton");
        }
        //�󶨿ؼ�����
        private void RegisterMethods()
        {
            BottomPanelChangeButtonGroup.RegisterValueChangedCallback(OnBottonGroupSelectionChanged);
        }
        #endregion

        #region UI����
        //������л�ʱ
        private void OnBottonGroupSelectionChanged(ChangeEvent<ToggleButtonGroupState> evt)
        {
            //�������״̬
            if (evt.newValue[0])
            {
                //��0��λΪ�������������ã�GamePlay��ذ�ť����
                GamePlayButtonsPanel.style.display = DisplayStyle.Flex;
                AssetGroupButtonsPanel.style.display = DisplayStyle.None;
            }
            else if (evt.newValue[1])
            {
                //��1��λΪ�������������ã�AssetGroup��ذ�ť����
                GamePlayButtonsPanel.style.display = DisplayStyle.None;
                AssetGroupButtonsPanel.style.display = DisplayStyle.Flex;
            }
        }
        #endregion
    }
}

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
                GamePatchesButtonsPanel.style.display = DisplayStyle.None;
            }
            else if (evt.newValue[1])
            {
                //��1��λΪ�������������ã�AssetGroup��ذ�ť����
                GamePlayButtonsPanel.style.display = DisplayStyle.None;
                GamePatchesButtonsPanel.style.display = DisplayStyle.Flex;
            }
        }
        #endregion
    }
}

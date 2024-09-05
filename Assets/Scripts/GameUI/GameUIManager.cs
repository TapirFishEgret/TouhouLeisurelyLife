using THLL.BaseSystem;

namespace THLL.UISystem
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region Unity���ں���
        protected override void Start()
        {
            //���û��ຯ��
            base.Start();

            //�趨����Ϊ������
            DontDestroyOnLoad(this);

            //��ʼ��
            Init();
        }
        #endregion

        #region ��ʼ������ط���
        //��ʼ��
        private void Init()
        {
            //��ȡ�������
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<Settings.GameSystemSettings>();
            GameUI.GamePlaySettingsInterface = GetComponentInChildren<Settings.GamePlaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<Settings.GamePatchesSettings>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();

            //��ʾ�����
            GameUI.ShowInterface(GameUI.MainTitleInterface, false);
        }
        #endregion
    }
}

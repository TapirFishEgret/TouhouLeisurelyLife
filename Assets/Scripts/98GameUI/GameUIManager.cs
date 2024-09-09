using THLL.BaseSystem;
using THLL.UISystem.Settings;

namespace THLL.UISystem
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region Unity���ں���
        //Awake
        protected override void Awake()
        {
            //���û���
            base.Awake();

            //UI�������Ƚ����⣬ֱ���趨Ϊ���ð�
            enabled = true;

            //��ȡ���
            GetInterfaces();
        }
        //Start
        private void Start()
        {
            //��ʾ�����
            GameUI.ShowInterface(GameUI.MainTitleInterface, false);
        }
        #endregion

        #region ��ʼ����ط���
        //��ȡ���
        private void GetInterfaces()
        {
            //ϵͳ������
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<GameSystemSettings>();
            GameUI.GameplaySettingsInterface = GetComponentInChildren<GameplaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<GamePatchesSettings>();
            //�������
            GameUI.BasicPlayInterface = GetComponentInChildren<BasicPlay>();
            //�������
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();
        }
        #endregion
    }
}

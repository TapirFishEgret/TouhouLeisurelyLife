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
        }
        #endregion

        #region ��ʼ������ط���
        //��ʼ��
        protected override void Init()
        {
            //��ȡ�������
            //ϵͳ������
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<GameSystemSettings>();
            GameUI.GamePlaySettingsInterface = GetComponentInChildren<GamePlaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<GamePatchesSettings>();
            //�������
            GameUI.PlayInterface = GetComponentInChildren<Play>();
            //�������
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();

            //��ʾ�����
            GameUI.ShowInterface(GameUI.MainTitleInterface, false);
        }
        #endregion
    }
}

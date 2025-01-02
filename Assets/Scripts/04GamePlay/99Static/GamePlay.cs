using THLL.CharacterSystem;
using THLL.SceneSystem;
using THLL.UISystem;

namespace THLL.PlaySystem
{
    public static class GamePlay
    {
        #region ������
        //��ҽ�ɫ
        public static Character Player => GameCharacter.GetCharacter("Character_System_Main_Player");
        //�����������
        public static Scene CurrentScene => Player.CurrentScene;
        //��ǰѡ�н�ɫ
        public static Character SelectedCharacter { get; private set; }
        #endregion

        #region �������
        //ѡ���ɫ
        public static void SelectCharacter(Character character)
        {
            //��ֵ
            SelectedCharacter = character;
            //��������л���ɫ
            GameUI.BasicPlayInterface.SwitchCharacter(character);
        }
        #endregion
    }
}
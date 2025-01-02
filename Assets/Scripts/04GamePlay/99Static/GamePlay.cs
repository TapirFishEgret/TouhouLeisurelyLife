using THLL.CharacterSystem;
using THLL.SceneSystem;
using THLL.UISystem;

namespace THLL.PlaySystem
{
    public static class GamePlay
    {
        #region 玩家相关
        //玩家角色
        public static Character Player => GameCharacter.GetCharacter("Character_System_Main_Player");
        //玩家所处场景
        public static Scene CurrentScene => Player.CurrentScene;
        //当前选中角色
        public static Character SelectedCharacter { get; private set; }
        #endregion

        #region 操作相关
        //选择角色
        public static void SelectCharacter(Character character)
        {
            //赋值
            SelectedCharacter = character;
            //游玩界面切换角色
            GameUI.BasicPlayInterface.SwitchCharacter(character);
        }
        #endregion
    }
}
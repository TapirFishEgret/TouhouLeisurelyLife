using THLL.BaseSystem;

namespace THLL.CharacterSystem
{
    public class GameCharacterManager : Singleton<GameCharacterManager>
    {
        #region 游戏内周期函数
        protected override void OnSecondChanged(int count)
        {
            //更新所有角色移动
            GameCharacter.UpdateAllCharacterMove(count);
        }
        #endregion
    }
}

using THLL.BaseSystem;

namespace THLL.CharacterSystem
{
    public class GameCharacterManager : Singleton<GameCharacterManager>
    {
        #region ��Ϸ�����ں���
        protected override void OnSecondChanged(int count)
        {
            //�������н�ɫ�ƶ�
            GameCharacter.UpdateAllCharacterMove(count);
        }
        #endregion
    }
}

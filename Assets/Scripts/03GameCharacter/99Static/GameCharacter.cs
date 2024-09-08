namespace THLL.CharacterSystem
{
    public static class GameCharacter
    {
        //存储游戏内所有角色的数据库
        public static CharacterDb CharacterDb { get; } = new();
    }
}

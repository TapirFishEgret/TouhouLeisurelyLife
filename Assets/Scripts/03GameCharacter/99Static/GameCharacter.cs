using System.Collections.Generic;
using THLL.BaseSystem;
using UnityEngine;

namespace THLL.CharacterSystem
{
    public static class GameCharacter
    {
        #region 游戏资源
        //默认角色头像
        public static Sprite DefaultAvatar => GameAssetsManager.Instance.DefaultAvatar;
        //默认角色立绘
        public static Sprite DefaultPortrait => GameAssetsManager.Instance.DefaultPortrait;
        #endregion

        #region 数据存储
        //当前使用的角色存储
        public static Dictionary<string, Character> Storage { get; private set; } = new();
        //完整的角色存储
        private static Dictionary<string, Dictionary<string, Character>> AllCharacters { get; set; } = new();
        //系列索引
        private static Dictionary<string, List<Character>> Index_Series { get; set; } = new();
        //组织索引
        private static Dictionary<string, List<Character>> Index_Group { get; set; } = new();
        //角色索引
        private static Dictionary<string, List<Character>> Index_Chara { get; set; } = new();
        //总计数
        public static int TotalCount { get; private set; } = 0;
        //重复角色计数
        public static int DuplicateCharacterCount { get; private set; } = 0;
        #endregion

        #region 内部操作方法
        //添加角色
        public static void AddCharacter(Character character)
        {
            //添加到当前使用的角色存储
            Storage[character.ID] = character;
            //计数
            TotalCount++;

            //添加到完整的角色存储
            if (!AllCharacters.ContainsKey(character.ID))
            {
                AllCharacters[character.ID] = new Dictionary<string, Character>();
            }
            else
            {
                //添加重复角色计数
                DuplicateCharacterCount++;
            }
            AllCharacters[character.ID][character.Version] = character;

            //添加到系列索引
            if (!Index_Series.ContainsKey(character.Series))
            {
                Index_Series[character.Series] = new List<Character>();
            }
            Index_Series[character.Series].Add(character);

            //添加到组织索引
            if (!Index_Group.ContainsKey(character.Group))
            {
                Index_Group[character.Group] = new List<Character>();
            }
            Index_Group[character.Group].Add(character);

            //添加到角色索引
            if (!Index_Chara.ContainsKey(character.Chara))
            {
                Index_Chara[character.Chara] = new List<Character>();
            }
            Index_Chara[character.Chara].Add(character);
        }
        #endregion

        #region 外部查询方法
        //获取角色
        public static Character GetCharacter(string characterID)
        {
            if (Storage.ContainsKey(characterID))
            {
                return Storage[characterID];
            }
            return null;
        }
        //尝试获取角色
        public static bool TryGetCharacter(string characterID, out Character character)
        {
            if (Storage.ContainsKey(characterID))
            {
                character = Storage[characterID];
                return true;
            }
            character = null;
            return false;
        }
        //检测是否有角色
        public static bool HasCharacter(string characterID)
        {
            return Storage.ContainsKey(characterID);
        }
        #endregion

        #region 角色统一操作方法
        //更新所有角色移动
        public static void UpdateAllCharacterMove(int passedSeconds)
        {
            foreach (Character character in Storage.Values)
            {
                character.UpdateMove(passedSeconds);
            }
        }
        #endregion
    }
}

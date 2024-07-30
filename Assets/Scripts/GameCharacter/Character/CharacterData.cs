using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.LocationSystem;
using UnityEngine;

namespace THLL.CharacterSystem
{
    public class CharacterData : BaseGameData
    {
        #region 角色数据类成员
        //重写ID
        public override string ID => string.Join("_", new List<string>() { Package, Category, Author, OriginatingSeries, Affiliation, Name, Version });
        //角色所属系列
        [SerializeField]
        private string _originatingSeries = string.Empty;
        public string OriginatingSeries => _originatingSeries;
        //所属组织
        [SerializeField]
        private string _affiliation = string.Empty;
        public string Affiliation => _affiliation;
        //角色版本
        [SerializeField]
        private string _version = string.Empty;
        public string Version => _version;
        //角色头像
        [SerializeField]
        private Sprite _avatar = null;
        public Sprite Avatar => _avatar;
        //角色立绘
        [SerializeField]
        private Sprite _portrait = null;
        public Sprite Portrait => _portrait;
        //居住地区
        [SerializeField]
        private LocUnitData _livingArea = null;
        public LocUnitData LivingArea => _livingArea;
        #endregion

#if UNITY_EDITOR
        //更改角色所属作品
        public void Editor_SetOriginatingSeries(string sourceWork)
        {
            _originatingSeries = sourceWork;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //更改角色所属组织
        public void Editor_SetAffiliation(string affiliation)
        {
            _affiliation = affiliation;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //更改角色版本
        public void Editor_SetVersion(string version)
        {
            _version = version;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //更改角色头像
        public void Editor_SetAvatar(Sprite avatar)
        {
            _avatar = avatar;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //更改角色立绘
        public void Editor_SetPortarit(Sprite portrait)
        {
            _portrait = portrait;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        //更改角色居住地
        public void Editor_SetLivingArea(LocUnitData locUnitData)
        {
            _livingArea = locUnitData;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

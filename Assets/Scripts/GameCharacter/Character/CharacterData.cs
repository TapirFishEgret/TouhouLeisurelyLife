using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.LocationSystem;
using UnityEngine;

namespace THLL.CharacterSystem
{
    public class CharacterData : BaseGameData
    {
        #region 角色数据类成员
        //角色所属系列
        [SerializeField]
        private string _originatingSeries = string.Empty;
        public string OriginatingSeries { get { return _originatingSeries; } set { _originatingSeries = value; } }
        //所属组织
        [SerializeField]
        private string _affiliation = string.Empty;
        public string Affiliation { get { return _affiliation; } set { _affiliation = value; } }
        //角色头像
        [SerializeField]
        private Sprite _avatar = null;
        public Sprite Avatar { get { return _avatar; } set { _avatar = value; } }
        //角色立绘
        [SerializeField]
        private Sprite _portrait = null;
        public Sprite Portrait { get { return _portrait; } set { _portrait = value; } }
        //居住地区
        [SerializeField]
        private LocUnitData _livingArea = null;
        public LocUnitData LivingArea { get { return _livingArea; } set { _livingArea = value; } }
        #endregion

#if UNITY_EDITOR
        //生成ID
        public override void Editor_GenerateID()
        {
            base.Editor_GenerateID();
            id = string.Join("_", new List<string>() { GameDataType.ToString(), OriginatingSeries, Affiliation, Name }).Replace(" ", "-");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

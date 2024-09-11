using UnityEngine;
using THLL.BaseSystem;

namespace THLL.PlaySystem
{
    public class InteractionData : BaseGameData
    {
        #region 数据
        //交互类型
        [SerializeField]
        private InteractionTypeEnum interactionType;
        public InteractionTypeEnum InteractionType { get; set; }
        #endregion

        #region 方法

        #endregion

#if UNITY_EDITOR
        //生成ID
        public override void Editor_GenerateID()
        {
            id = string.Join("_", new string[] { GameDataType.ToString(), InteractionType.ToString(), Name });
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}

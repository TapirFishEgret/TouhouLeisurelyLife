using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace THLL.GameEditor
{
    public class AddressableAssetInfoGroupSchema : AddressableAssetGroupSchema
    {
        //包描述
        [SerializeField]
        private string _description = string.Empty;
        public string Description => _description;

#if UNITY_EDITOR
        //更改描述
        public void SetDescription(string description)
        {
            _description = description;
            SetDirty(this);
        }
#endif
    }
}

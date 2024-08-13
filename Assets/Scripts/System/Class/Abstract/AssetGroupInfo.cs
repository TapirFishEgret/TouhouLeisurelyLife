using System.Collections.Generic;
using UnityEngine;

namespace THLL.BaseSystem
{
    public class AssetGroupInfo : ScriptableObject
    {
        //组的类型
        [SerializeField]
        private GameAssetTypeEnum _assetType = 0;
        public GameAssetTypeEnum AssetType { get => _assetType; set => _assetType = value; }
        //组的信息
        [SerializeField]
        private string _description = string.Empty;
        public string Description { get => _description; set => _description = value; }
        //组内所含资源地址集合
        [SerializeField]
        private List<string> _assetAddresses = new();
        public List<string> AssetAddresses => _assetAddresses;
    }
}

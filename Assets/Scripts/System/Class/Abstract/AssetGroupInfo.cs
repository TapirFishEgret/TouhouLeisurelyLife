using System.Collections.Generic;
using UnityEngine;

namespace THLL.BaseSystem
{
    public class AssetGroupInfo : ScriptableObject
    {
        //�������
        [SerializeField]
        private GameAssetTypeEnum _assetType = 0;
        public GameAssetTypeEnum AssetType { get => _assetType; set => _assetType = value; }
        //�����Ϣ
        [SerializeField]
        private string _description = string.Empty;
        public string Description { get => _description; set => _description = value; }
        //����������Դ��ַ����
        [SerializeField]
        private List<string> _assetAddresses = new();
        public List<string> AssetAddresses => _assetAddresses;
    }
}

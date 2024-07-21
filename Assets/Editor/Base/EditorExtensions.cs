using System;
using UnityEditor;

namespace THLL.GameEditor
{
    public static class EditorExtensions
    {
        //获取文件资源的GUID的哈希值
        public static int GetAssetHashCode(this UnityEngine.Object asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "资源不能为空");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new ArgumentException("资源地址不可用", nameof(asset));
            }

            GUID guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            return guid.GetHashCode();
        }
    }
}

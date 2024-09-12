using System.Collections.Generic;
using UnityEngine;

namespace THLL.BaseSystem
{
    public class BaseGameAsset<T> : ScriptableObject where T : Object
    {
        //存储的Unity资源对象
        public T asset;
        //作者名称
        public string author;
        //作者邮箱
        public string email;
        //作者社交媒体账号
        public List<string> socialMediaUrls;
    }
}

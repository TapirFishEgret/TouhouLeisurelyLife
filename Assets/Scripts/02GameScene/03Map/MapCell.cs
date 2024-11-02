using Newtonsoft.Json;
using System;
using UnityEngine;

namespace THLL.SceneSystem
{
    [Serializable]
    public class MapCell
    {
        #region 数据
        //存储数据
        public string StringData { get; set; } = "佔";
        //颜色字符串
        public string ColorString { get; set; } = "000000FF";
        //文字颜色
        [JsonIgnore]
        public Color TextColor
        {
            get
            {
                if (ColorUtility.TryParseHtmlString("#" + ColorString, out Color color))
                {
                    return color;
                }
                else
                {
                    return Color.white;
                }
            }
            set
            {
                ColorString = ColorUtility.ToHtmlStringRGBA(value);
            }
        }
        #endregion

        #region 构造函数
        //无参构造函数
        public MapCell() { }
        #endregion 
    }
}

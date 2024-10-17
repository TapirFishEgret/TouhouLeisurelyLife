using Newtonsoft.Json;
using System;
using UnityEngine;

namespace THLL.SceneSystem
{
    [Serializable]
    public class MapCell
    {
        #region 数据
        //X坐标
        public int X { get; set; }
        //Y坐标
        public int Y { get; set; }

        //文字
        public string Text { get; set; }
        //文字颜色字符串
        public string TextColorString { get; set; }
        //文字颜色
        [JsonIgnore]
        public Color TextColor
        {
            get
            {
                if (ColorUtility.TryParseHtmlString("#" + TextColorString, out Color color))
                {
                    return color;
                }
                else
                {
                    return Color.white;
                }
            }
        }
        #endregion

        #region 构造函数
        //无参构造函数
        public MapCell() { }
        #endregion 
    }
}

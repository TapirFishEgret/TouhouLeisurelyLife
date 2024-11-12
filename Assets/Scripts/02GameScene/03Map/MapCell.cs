using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.SceneSystem
{
    [Serializable]
    public class MapCell
    {
        #region 常量
        //默认单元格大小
        public const int CellSize = 40;
        //默认字体大小
        public const int FontSize = 36;
        #endregion

        #region 数据
        //单元格显示文字
        public string Data { get; set; } = "佔";
        //单元格颜色字符串
        public string ColorString { get; set; } = "FFFFFFFF";
        //单元格颜色
        [JsonIgnore]
        public Color Color
        {
            get
            {
                return ColorUtility.TryParseHtmlString("#" + ColorString, out Color color) ? color : Color.white;
            }
            set
            {
                ColorString = ColorUtility.ToHtmlStringRGBA(value);
            }
        }
        #endregion

        #region 视觉显示
        //对应视觉元素
        [JsonIgnore]
        private VisualElement CellView { get; set; }
        #endregion

        #region 构造函数
        //无参构造函数
        public MapCell() { }
        #endregion

        #region 公共方法
        //获取视觉元素
        public VisualElement GetCell()
        {
            return CellView ?? GenerateCell();
        }
        //粉刷单元格
        public void Brush(string data, Color color)
        {
            //设置数据
            Data = data;
            Color = color;
            //刷新视觉元素
            CellView.Q<Label>("CellLabel").text = Data;
            CellView.Q<Label>("CellLabel").style.color = Color;
        }
        #endregion

        #region 私有方法
        //生成视觉元素
        private VisualElement GenerateCell()
        {
            //创建单元格元素作为容器
            CellView = new VisualElement()
            {
                //设置名称
                name = "CellView",
                //设置样式
                style =
                {
                    //设置宽度
                    width = CellSize,
                    //设置高度
                    height = CellSize,
                    //设置窄边框
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                }
            };

            //声明单元格文字
            string text = string.Empty;
            //检测文本长度
            if (Data.Length > 1)
            {
                //大于1，说明应该为场景ID，检测运行环境
#if UNITY_EDITOR
                //编辑器模式下，按照_对ID进行分割，随后使用-二次分割，取首字母大写作为显示文字
                foreach (string word in Data.Split("_").Last().Split("-"))
                {
                    text += word[..1].ToUpper();
                }
#else
                //非编辑器模式下，尝试获取场景数据
                if (GameScene.SceneDB.TryGetValue(Data, out Scene scene))
                {
                    //获取场景名称
                    text = scene.Name;
                }
                else
                {
                    //未找到场景数据，等同编辑器进行处理
                    foreach (string word in Data.Split("_").Last().Split("-"))
                    {
                        text += word[..1].ToUpper();
                    }   
                    //游戏内报错
                    GameHistory.LogError("未找到场景数据：" + Data);
                }
#endif
            }
            else
            {
                //长度为1，直接显示
                text = Data;
            }
            //计算文字大小
            float fontSize = text.Length > 1 ? FontSize / text.Length : FontSize;
            //创建标签作为单元格内容
            Label label = new()
            {
                //命名
                name = "CellLabel",
                //设置文字
                text = text,
                //设置用户数据为自身
                userData = this,
                //设置样式
                style =
                {
                    //位置设置为绝对位置
                    position = Position.Absolute,
                    //所有间距设为0
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0,
                    //设置字体大小
                    fontSize = fontSize,
                    //设置文本颜色
                    color = Color,
                    //设置文字居中
                    unityTextAlign = TextAnchor.MiddleCenter,
                    //设置边距
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    //设置内边距
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                }
            };
#if UNITY_EDITOR
            //为标签注册鼠标移入事件
            label.RegisterCallback<MouseEnterEvent>(evt =>
            {
                //高亮单元格
                Highlight();
            });
            //为标签注册鼠标移出事件
            label.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //取消高亮单元格
                Unhighlight();
                //刷新视觉元素
                RefreshCell();
            });
#endif
            //添加到容器
            CellView.Add(label);

            //返回视觉元素
            return CellView;
        }
        //刷新单元格
        private void RefreshCell()
        {
            //刷新视觉元素
            CellView.Q<Label>("CellLabel").text = Data;
            CellView.Q<Label>("CellLabel").style.color = Color;
        }
        //高亮单元格
        private void Highlight()
        {
            //设置样式
            CellView.style.borderTopColor = Color.white;
            CellView.style.borderBottomColor = Color.white;
            CellView.style.borderLeftColor = Color.white;
            CellView.style.borderRightColor = Color.white;
        }
        //取消高亮单元格
        private void Unhighlight()
        {
            //设置样式
            CellView.style.borderTopColor = Color.clear;
            CellView.style.borderBottomColor = Color.clear;
            CellView.style.borderLeftColor = Color.clear;
            CellView.style.borderRightColor = Color.clear;
        }
        #endregion
    }
}

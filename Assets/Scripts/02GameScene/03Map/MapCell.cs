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
        //默认单字体大小
        public const int SingleFontSize = 36;
        //默认多字体大小
        public const int MultiFontSize = 24;
        #endregion

        #region 数据
        //单元格数据
        public string Data { get; set; } = "佔";
        //是否表示场景
        public bool IsScene { get; set; } = false;
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
        //对应标签
        [JsonIgnore]
        private Label CellLabel { get; set; }
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
            CellView.Q<Label>("CellLabel").text = GetText(out float fontSize);
            CellView.Q<Label>("CellLabel").style.fontSize = fontSize;
            CellView.Q<Label>("CellLabel").style.color = Color;
        }
        //获取显示文字
        public string GetText(out float fontSize)
        {
            //声明单元格文字
            string text = string.Empty;

            //检测是否为场景ID
            if (IsScene)
            {
                //若是，首先尝试从场景数据库中获取数据
                if (GameScene.TryGetScene(Data, out Scene scene))
                {
                    //若成功获取，则取场景名称
                    text = scene.Name;
                }
                else
                {
                    //在编辑器模式下不会获取成功，此时对ID进行分割处理并显示
                    foreach (string word in Data.Split("_").Last().Split("-"))
                    {
                        text += word[..1].ToUpper();
                    }
                }
            }
            else
            {
                //若不是，则直接显示数据的第一个字符
                text = Data[0].ToString();
            }

            //计算文字大小
            if (text.Length > 1)
            {
                fontSize = MultiFontSize;
            }
            else
            {
                fontSize = SingleFontSize;
            }

            //返回显示文字
            return text;
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
                    //子元素垂直居中
                    alignItems = Align.Center,
                    //子元素水平居中
                    justifyContent = Justify.Center,
                    //不延展
                    flexGrow = 0,
                }
            };

            //获取显示文本
            string text = GetText(out float fontSize);
            //创建标签作为单元格内容
            CellLabel = new()
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
                    //设置字体大小
                    fontSize = fontSize,
                    //设置文本颜色
                    color = Color,
                    //设置文字Warp属性
                    whiteSpace = WhiteSpace.NoWrap,
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
                    //设置边框
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    //设置边框颜色
                    borderTopColor = Color.clear,
                    borderBottomColor = Color.clear,
                    borderLeftColor = Color.clear,
                    borderRightColor = Color.clear,
                }
            };
            //检测是否为场景单元格
            if (IsScene)
            {
                //如果是，更改标签为Absolute
                CellLabel.style.position = Position.Absolute;
            }
            //注册事件
            RegisterEvent(CellLabel);
            //添加到容器
            CellView.Add(CellLabel);

            //返回视觉元素
            return CellView;
        }
        //为标签注册事件
        private void RegisterEvent(Label label)
        {
            //首先对运行环境进行检查
#if UNITY_EDITOR
            //编辑器环境下，为所有单元格注册鼠标移入事件
            label.RegisterCallback<MouseEnterEvent>(evt =>
            {
                //高亮单元格
                Highlight();
            });
            //注册鼠标移出事件
            label.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //取消高亮单元格
                Unhighlight();
                //刷新视觉元素
                RefreshCell();
            });
#else
            //非编辑器环境下，仅对场景单元格注册鼠标移入移出事件
            if (IsScene)
            {
                label.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    //高亮单元格
                    Highlight();
                });
                label.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    //取消高亮单元格
                    Unhighlight();
                    //刷新视觉元素
                    RefreshCell();
                });
            }
#endif
        }
        //刷新单元格
        private void RefreshCell()
        {
            //刷新视觉元素
            CellLabel.text = GetText(out float fontSize);
            CellLabel.style.fontSize = fontSize;
            CellLabel.style.color = Color;
        }
        //高亮单元格
        private void Highlight()
        {
            //设置样式
            CellLabel.style.borderTopColor = Color.white;
            CellLabel.style.borderBottomColor = Color.white;
            CellLabel.style.borderLeftColor = Color.white;
            CellLabel.style.borderRightColor = Color.white;
        }
        //取消高亮单元格
        private void Unhighlight()
        {
            //设置样式
            CellLabel.style.borderTopColor = Color.clear;
            CellLabel.style.borderBottomColor = Color.clear;
            CellLabel.style.borderLeftColor = Color.clear;
            CellLabel.style.borderRightColor = Color.clear;
        }
        #endregion
    }
}

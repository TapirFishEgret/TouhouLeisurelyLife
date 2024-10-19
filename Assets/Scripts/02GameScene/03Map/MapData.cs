using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.SceneSystem
{
    [Serializable]
    public class MapData
    {
        #region 静态数据
        //单元格大小
        public static float CellSize = 30f;
        #endregion

        #region 数据
        //列数
        public int Cols { get; set; }
        //行数
        public int Rows { get; set; }
        //单元格数据字典，键值对关系为 (列,行)-单元格 数据，并应用转换器
        [JsonConverter(typeof(MapCellsDictConverter))]
        public Dictionary<(int, int), MapCell> Cells { get; set; } = new();
        #endregion

        #region 面板
        //地图面板
        private VisualElement Map { get; set; }
        //是否在拖动
        private bool IsDragging { get; set; } = false;
        //拖动起始位置
        private Vector2 DragStartPos { get; set; } = Vector2.zero;
        #endregion

        #region 构造函数与初始化
        //无参构造函数
        public MapData()
        {
            //创建地图
            Map = new VisualElement()
            {
                //命名
                name = "Map",
                //设置样式
                style =
                {
                    //内容物排布方向为向右
                    flexDirection = FlexDirection.Row,
                    //添加灰色边框
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomColor = Color.gray,
                    borderLeftColor = Color.gray,
                    borderRightColor = Color.gray,
                    borderTopColor = Color.gray,
                }
            };
            //为地图添加滚轮缩放功能，本质上是通过滚轮更改地图面板大小
            Map.RegisterCallback<WheelEvent>(evt =>
            {
                //首先，禁止事件传递以取消其他面板，比如滚轴面板的事件响应
                evt.StopPropagation();

                //然后，获取缩放数值
                float zoomDelta = -evt.delta.y * 0.01f;
                float currentScale = Map.resolvedStyle.scale.value.x * (zoomDelta + 1);
                //限制缩放范围
                currentScale = Mathf.Clamp(currentScale, 0.1f, 10f);
                //设置缩放
                Map.style.scale = new StyleScale(new Scale(new Vector2(currentScale, currentScale)));

                //结束后调整地图位置
                AdjustMapPos();
            });
            //为地图添加右键拖动功能
            Map.RegisterCallback<MouseDownEvent>(evt =>
            {
                //检测鼠标按键
                if (evt.button == 1)
                {
                    //若为右键，开始拖动
                    IsDragging = true;
                    //记录起始位置
                    DragStartPos = evt.localMousePosition;
                }
            });
            Map.RegisterCallback<MouseUpEvent>(evt =>
            {
                //检测鼠标按键
                if (evt.button == 1)
                {
                    //结束拖动
                    IsDragging = false;
                }
            });
            Map.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //检测是否在拖动
                if (IsDragging)
                {
                    //计算偏移量
                    Vector2 offset = evt.localMousePosition - DragStartPos;
                    //设置偏移
                    AdjustMapPos(offset);
                }
            });
            Map.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //鼠标离开地图，结束拖动
                IsDragging = false;
            });
        }
        #endregion

        #region 公共方法
        //根据数据获取地图视觉元素
        public VisualElement GetMap()
        {
            //检测是否为第一次获取
            if (Map.childCount == 0)
            {
                //刷新地图
                RefreshMap();
            }

            //检查地图单元格数据与行列数值是否匹配
            if (Cells.Count != Cols * Rows)
            {
                //如果不匹配，则重设地图大小
                ResizeMap();
            }

            //返回地图面板
            return Map;
        }
        #endregion

        #region 私有方法
        //重设地图大小
        private void ResizeMap()
        {
            //获取需要移除的元素
            List<ValueTuple<int, int>> keysToRemove = Cells.Keys.Where(k => k.Item1 >= Cols || k.Item2 >= Rows).ToList();
            //移除元素
            foreach (ValueTuple<int, int> key in keysToRemove)
            {
                Cells.Remove(key);
            }

            //遍历列与行
            for (int i = 0; i < Cols; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    //检测是否有单元格数据
                    if (!Cells.ContainsKey((i, j)))
                    {
                        //如果没有，则新建一个占位单元格
                        Cells[(i, j)] = new MapCell() { Text = "占", TextColor = Color.white };
                    }
                }
            }

            //刷新地图
            RefreshMap();
        }
        //刷新
        private void RefreshMap()
        {
            //调用此方法时首先清空地图
            Map.Clear();

            //地图是否为空
            if (Cols == 0 || Rows == 0)
            {
                //如果没有地图，生成标签表示无地图
                Label noMapLabel = new()
                {
                    //设置文本
                    text = "无地图",
                    //设置样式
                    style =
                    {
                        //设置字体大小
                        fontSize = new StyleLength(new Length(CellSize, LengthUnit.Pixel)),
                        //设置文字居中
                        unityTextAlign = TextAnchor.MiddleCenter,
                        //设置边距
                        marginTop = 10,
                        marginBottom = 10,
                        marginLeft = 10,
                        marginRight = 10,
                        //设置内边距
                        paddingTop = 10,
                        paddingBottom = 10,
                        paddingLeft = 10,
                        paddingRight = 10,
                    }
                };
                //添加到地图面板
                Map.Add(noMapLabel);
            }
            else
            {
                //如果有地图，则进行生成，创建单元格元素，首先对列进行遍历，也就是x
                for (int x = 0; x < Cols; x++)
                {
                    //创建视觉元素容器
                    VisualElement colContainer = new()
                    {
                        //命名
                        name = $"Col_{x}",
                        //设置样式
                        style =
                        {
                            //内容物排布方向为向下 
                            flexDirection = FlexDirection.Column,
                        }
                    };
                    //对每列的单元格进行遍历，也就是y
                    for (int y = 0; y < Rows; y++)
                    {
                        //获取文字
                        string text = Cells.ContainsKey((x, y)) ? Cells[(x, y)].Text : "占";
                        //获取文字长度
                        int textLength = text.Length;
                        //计算文字大小
                        float fontSize = textLength > 1 ? CellSize / textLength : CellSize;
                        //获取文字颜色
                        Color textColor = Cells.ContainsKey((x, y)) ? Cells[(x, y)].TextColor : Color.white;

                        //创建单元格元素
                        Label cellElement = new()
                        {
                            //命名
                            name = $"Cell_({x},{y})",
                            //设置文本
                            text = text,
                            //存储数据
                            userData = (x, y),
                            //设置样式
                            style =
                            {
                                //设置宽度
                                width = new StyleLength(new Length(CellSize, LengthUnit.Pixel)),
                                //设置高度
                                height = new StyleLength(new Length(CellSize, LengthUnit.Pixel)),
                                //设置字体大小
                                fontSize = new StyleLength(new Length(fontSize, LengthUnit.Pixel)),
                                //设置文本颜色
                                color = textColor,
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
                        //添加到单元格容器
                        colContainer.Add(cellElement);
                    }
                    //添加到根元素
                    Map.Add(colContainer);
                }
            }
        }
        //调整地图位置
        private void AdjustMapPos(Vector2 offset = default)
        {
            //检测是否有偏移量
            if (offset != default)
            {
                //获取偏移后位置
                Vector2 newPos = (Vector2)Map.resolvedStyle.translate + offset;
                //设置偏移
                Map.style.translate = new StyleTranslate(new Translate(newPos.x, newPos.y));
            }

            //偏移后获取视觉元素与其父级的Rect
            Rect mapRect = Map.localBound;
            Rect parentRect = Map.parent.localBound;

            //对高度进行判断
            if (mapRect.height < parentRect.height)
            {
                //若高度小于父级，则限制在父级内
                //判断上侧是否超出屏幕
                if (mapRect.y < 0)
                {
                    //如果超出，则向下移动
                    Map.style.translate = new StyleTranslate(new Translate(Map.resolvedStyle.translate.x, (mapRect.height - parentRect.height) / 2));
                }
                //判断下侧是否超出屏幕
                if (mapRect.yMax > parentRect.height)
                {
                    //如果超出，则向上移动
                    Map.style.translate = new StyleTranslate(new Translate(Map.resolvedStyle.translate.x, (parentRect.height - mapRect.height) / 2));
                }
            }
            else
            {
                //若高度大于父级，则将边界限制在父级外
                //判断上侧是否完全进入屏幕内
                if (mapRect.y > 0)
                {
                    //如果超出，则向下移动
                    Map.style.translate = new StyleTranslate(new Translate(Map.resolvedStyle.translate.x, (mapRect.height - parentRect.height) / 2));
                }
                //判断下侧是否完全进入屏幕内
                if (mapRect.yMax < parentRect.height)
                {
                    //如果超出，则向上移动
                    Map.style.translate = new StyleTranslate(new Translate(Map.resolvedStyle.translate.x, (parentRect.height - mapRect.height) / 2));
                }
            }

            //对宽度进行判断
            if (mapRect.width < parentRect.width)
            {
                //若宽度小于父级，则限制在父级内
                //判断左侧是否超出屏幕
                if (mapRect.x < 0)
                {
                    //如果超出，则向右移动
                    Map.style.translate = new StyleTranslate(new Translate((mapRect.width - parentRect.width) / 2, Map.resolvedStyle.translate.y));
                }
                //判断右侧是否超出屏幕
                if (mapRect.xMax > parentRect.width)
                {
                    //如果超出，则向左移动
                    Map.style.translate = new StyleTranslate(new Translate((parentRect.width - mapRect.width) / 2, Map.resolvedStyle.translate.y));
                }
            }
            else
            {
                //若宽度大于父级，则将边界限制在父级外
                //判断左侧是否完全进入屏幕内
                if (mapRect.x > 0)
                {
                    //如果超出，则向右移动
                    Map.style.translate = new StyleTranslate(new Translate((mapRect.width - parentRect.width) / 2, Map.resolvedStyle.translate.y));
                }
                //判断右侧是否完全进入屏幕内
                if (mapRect.xMax < parentRect.width)
                {
                    //如果超出，则向左移动
                    Map.style.translate = new StyleTranslate(new Translate((parentRect.width - mapRect.width) / 2, Map.resolvedStyle.translate.y));
                }
            }
        }
        #endregion
    }
}

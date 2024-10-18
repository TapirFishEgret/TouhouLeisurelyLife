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
    public class Map
    {
        #region 数据
        //列数
        public int Cols { get; set; }
        //行数
        public int Rows { get; set; }
        //单元格数据字典，键值对关系为 (列,行)-单元格 数据，并应用转换器
        [JsonConverter(typeof(MapCellsDictConverter))]
        public Dictionary<(int, int), MapCell> Cells { get; set; } = new();
        //地图单元格大小
        [JsonIgnore]
        public float CellSize { get; set; } = 50f;
        //是否为空地图
        [JsonIgnore]
        public bool IsEmpty => Cols == 0 || Rows == 0;
        #endregion

        #region 面板
        //地图面板
        [JsonIgnore]
        public VisualElement Panel { get; set; }
        #endregion

        #region 构造函数与初始化
        //无参构造函数
        public Map()
        {
            GenerateMapPanel();
        }
        //创建地图面板
        public void GenerateMapPanel()
        {
            //创建地图面板
            Panel = new VisualElement()
            {
                //命名
                name = "Map",
                //设置样式
                style =
                {
                    //自身位置为居中
                    alignSelf = Align.Center,
                    //内容物排布方向为向右
                    flexDirection = FlexDirection.Row
                }
            };
            //为地图面板添加滚轮缩放功能，本质上是通过滚轮更改单元格大小
            Panel.RegisterCallback<WheelEvent>(evt =>
            {
                //首先，禁止事件传递以取消其他面板，比如滚轴面板的事件响应
                evt.StopPropagation();

                //然后，获取缩放数值
                float zoomDelta = -evt.delta.y * 0.01f;
                CellSize *= 1 + zoomDelta;
                //应用缩放
                GetMap();
            });
        }
        #endregion

        #region 公共方法
        //根据数据获取地图视觉元素
        public VisualElement GetMap()
        {
            //首先，清空地图
            Panel.Clear();

            //检测是否真的有地图
            if (IsEmpty)
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
                Panel.Add(noMapLabel);
                //返回地图面板
                return Panel;
            }

            //然后检测行单元格总数是否正确
            if (Cells.Count != Cols * Rows)
            {
                //报错
                GameHistory.LogError($"该地图单元格总数不正确，存储总数为{Cells.Count}，理论行列为{Rows}行{Cols}列。地图已自动重新生成。");
                //如果行单元格总数不正确，则重新缩放地图
                ResizeMap((Cols, Rows));
            }

            //创建单元格元素，首先对列进行遍历，也就是x
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
                    //获取文字长度
                    int textLength = Cells[(x, y)].Text.Length;
                    //计算文字大小
                    float fontSize = textLength > 1 ? CellSize / textLength : CellSize;
                    //创建单元格元素
                    Label cellElement = new()
                    {
                        //命名
                        name = $"Cell_({x},{y})",
                        //设置文本
                        text = Cells[(x, y)].Text,
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
                            color = ColorUtility.TryParseHtmlString("#" + Cells[(x, y)].TextColorString, out Color color) ? color : Color.white,
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
                Panel.Add(colContainer);
            }

            //返回地图面板
            return Panel;
        }
        //重设地图大小
        public void ResizeMap(ValueTuple<int, int> newSize)
        {
            //更新行列数
            Cols = newSize.Item1;
            Rows = newSize.Item2;

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
                        Cells[(i, j)] = new MapCell() { Text = "占", TextColorString = "000000" };
                    }
                }
            }
        }
        #endregion
    }
}

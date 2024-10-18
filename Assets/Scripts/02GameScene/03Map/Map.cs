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
        [JsonIgnore]
        private int _cols = 0;
        public int Cols
        {
            get
            {
                return _cols;
            }
            set
            {
                _cols = value;
                ResizeMap();
            }
        }
        //行数
        [JsonIgnore]
        private int _rows = 0;
        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
                ResizeMap();
            }
        }
        //单元格数据字典，键值对关系为 (列,行)-单元格 数据，并应用转换器
        [JsonConverter(typeof(MapCellsDictConverter))]
        public Dictionary<(int, int), MapCell> Cells { get; private set; } = new();
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
            GenerateMap();
        }
        //有参构造函数
        public Map(int cols, int rows)
        {
            Cols = cols;
            Rows = rows;
            GenerateMap();
        }
        //根据数据创建地图
        private void GenerateMap()
        {
            //创建根视觉元素
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
            //根据行数和列数创建占位单元格数据字典
            for (int i = 0; i < Cols; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    Cells[(i, j)] = new MapCell() { Text = "占", TextColorString = "000000" };
                }
            }
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
                //如果行单元格总数不正确，则重新缩放地图
                ResizeMap();
                //并报错
                GameHistory.LogError("该地图单元格总数不正确，已自动重新生成。");
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
                    //创建单元格元素
                    Label cellElement = new()
                    {
                        //命名
                        name = $"Cell_({x},{y})",
                        //设置文本
                        text = Cells[(x, y)].Text,
                        //设置样式
                        style =
                        {
                            //设置宽度
                            width = new StyleLength(new Length(CellSize, LengthUnit.Pixel)),
                            //设置高度
                            height = new StyleLength(new Length(CellSize, LengthUnit.Pixel)),
                            //设置字体大小
                            fontSize = new StyleLength(new Length(CellSize / Cells[(x, y)].Text.Length, LengthUnit.Pixel)),
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

            ////创建单元格元素，首先对列进行遍历，也就是x
            //foreach (var col in Cells)
            //{
            //    //对每列创建视觉元素容器
            //    VisualElement colContainer = new()
            //    {
            //        //命名
            //        name = $"Col_{col.Key}",
            //        //设置样式
            //        style =
            //        {
            //            //内容物排布方向为向下 
            //            flexDirection = FlexDirection.Column,
            //        }
            //    };
            //    //对每列的单元格进行遍历，也就是y
            //    foreach (var cell in col.Value)
            //    {
            //        //创建单元格元素
            //        Label cellElement = new()
            //        {
            //            //命名
            //            name = $"Cell_({col.Key},{cell.Key})",
            //            //设置文本
            //            text = cell.Value.Text,
            //            //设置样式
            //            style =
            //            {
            //                //设置宽度
            //                width = new StyleLength(new Length(cellSize, LengthUnit.Pixel)),
            //                //设置高度
            //                height = new StyleLength(new Length(cellSize, LengthUnit.Pixel)),
            //                //设置字体大小
            //                fontSize = new StyleLength(new Length(cellSize / cell.Value.Text.Length, LengthUnit.Pixel)),
            //                //设置文本颜色
            //                color = ColorUtility.TryParseHtmlString("#" + cell.Value.TextColorString, out Color color) ? color : Color.white,
            //                //设置文字居中
            //                unityTextAlign = TextAnchor.MiddleCenter,
            //                //设置边距
            //                marginTop = 0,
            //                marginBottom = 0,
            //                marginLeft = 0,
            //                marginRight = 0,
            //                //设置内边距
            //                paddingTop = 0,
            //                paddingBottom = 0,
            //                paddingLeft = 0,
            //                paddingRight = 0,
            //            }
            //        };
            //        //添加到单元格容器
            //        colContainer.Add(cellElement);
            //    }
            //    //添加到根元素
            //    root.Add(colContainer);
            //}

            //返回地图面板
            return Panel;
        }
        #endregion

        #region 辅助方法
        //重设地图大小
        private void ResizeMap()
        {
            //保存原来的列数与行数
            int oldCols = Cells.Count > 0 ? Cells.Keys.Max(k => k.Item1) + 1 : 0;
            int oldRows = Cells.Count > 0 ? Cells.Keys.Max(k => k.Item2) + 1 : 0;

            //新建字典保存调整后的单元格数据
            Dictionary<(int, int), MapCell> newCells = new();

            //根据行数和列数创建占位单元格数据字典
            for (int i = 0; i < Cols; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    //如果原来有单元格数据，则复制过来
                    if (Cells.ContainsKey((i, j)))
                    {
                        newCells[(i, j)] = Cells[(i, j)];
                    }
                    //否则，新建一个占位单元格
                    else
                    {
                        newCells[(i, j)] = new MapCell() { Text = "占", TextColorString = "000000" };
                    }
                }
            }

            //用新字典替换旧字典
            Cells = newCells;

            ////遍历列数
            //for (int i = 0; i < Cols; i++)
            //{
            //    //检测列是否存在
            //    if (!Cells.ContainsKey(i))
            //    {
            //        //如果不存在，则创建列
            //        Cells[i] = new Dictionary<int, MapCell>();
            //    }

            //    //遍历本列单元格
            //    for (int j = 0; j < Rows; j++)
            //    {
            //        //检测单元格是否存在
            //        if (!Cells[i].ContainsKey(j))
            //        {
            //            //如果不存在，则占位单元格
            //            Cells[i][j] = new MapCell() { Text = "占", TextColorString = "000000" };
            //        }
            //    }

            //    //删除多余的单元格
            //    List<int> cellsToRemove = Cells[i].Keys.Where(k => k >= Rows).ToList();
            //    foreach (int key in cellsToRemove)
            //    {
            //        Cells[i].Remove(key);
            //    }
            //}

            ////删除多余的列
            //List<int> colsToRemove = Cells.Keys.Where(k => k >= Cols).ToList();
            //foreach (int key in colsToRemove)
            //{
            //    Cells.Remove(key);
            //}
        }
        #endregion
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THLL.BaseSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.SceneSystem
{
    [Serializable]
    public class Map
    {
        #region 数据
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
        //单元格数据字典
        public Dictionary<int, Dictionary<int, MapCell>> Cells { get; } = new();
        //是否为空地图
        [JsonIgnore]
        public bool IsEmpty => Rows == 0 || Cols == 0;
        #endregion

        #region 构造函数与初始化
        //无参构造函数
        public Map() 
        {
            GenerateMap();
        }
        //有参构造函数
        public Map(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            GenerateMap();
        }
        //根据数据创建地图
        private void GenerateMap()
        {
            //根据行数和列数创建单元格数据字典
            for (int i = 0; i < Rows; i++)
            {
                Cells[i] = new Dictionary<int, MapCell>();
                for (int j = 0; j < Cols; j++)
                {
                    Cells[i][j] = new MapCell() { Text = "占", TextColorString = "000000" };
                }
            }
        }
        #endregion

        #region 公共方法
        //根据数据获取地图视觉元素
        public VisualElement GetMap(VisualElement container)
        {
            //创建根元素
            VisualElement root = new()
            {
                //命名
                name = "Map",
                //设置样式
                style =
                {
                    //自身位置为居中
                    alignSelf = Align.Center,
                    //内容物排布方向为向下
                    flexDirection = FlexDirection.Column
                }
            };
            //获取单元格大小
            float cellSize = GetSuitableCellSize(container);

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
                        fontSize = new StyleLength(new Length(20, LengthUnit.Pixel)),
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
                //添加到根元素
                root.Add(noMapLabel);
                //返回根元素
                return root;
            }

            //创建单元格元素，首先对行进行遍历
            foreach (var row in Cells)
            {
                //对每行创建视觉元素容器
                VisualElement rowContainer = new()
                {
                    //命名
                    name = row.Key.ToString(),
                    //设置样式
                    style =
                    {
                        //内容物排布方向为向右
                        flexDirection = FlexDirection.Row,
                    }
                };
                //对每行的单元格进行遍历
                foreach (var cell in row.Value)
                {
                    //创建单元格元素
                    Label cellElement = new()
                    {
                        //命名
                        name = $"({cell.Key},{row.Key})",
                        //设置文本
                        text = cell.Value.Text,
                        //设置样式
                        style =
                        {
                            //设置宽度
                            width = new StyleLength(new Length(cellSize, LengthUnit.Pixel)),
                            //设置高度
                            height = new StyleLength(new Length(cellSize, LengthUnit.Pixel)),
                            //设置字体大小
                            fontSize = new StyleLength(new Length(cellSize, LengthUnit.Pixel)),
                            //设置文本颜色
                            color = ColorUtility.TryParseHtmlString(cell.Value.TextColorString, out Color color) ? color : Color.white,
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
                    rowContainer.Add(cellElement);
                }
                //添加到根元素
                root.Add(rowContainer);
            }

            //返回根元素
            return root;
        }
        #endregion

        #region 辅助方法
        //重设地图大小
        private void ResizeMap()
        {
            //遍历行数
            for (int i = 0; i < Rows; i++)
            {
                //检测行是否存在
                if (!Cells.ContainsKey(i))
                {
                    //如果不存在，则创建行
                    Cells[i] = new Dictionary<int, MapCell>();
                }

                //遍历本行单元格
                for (int j = 0; j < Cols; j++)
                {
                    //检测列是否存在
                    if (!Cells[i].ContainsKey(j))
                    {
                        //如果不存在，则占位单元格
                        Cells[i][j] = new MapCell() { Text = "占", TextColorString = "000000" };
                    }
                }

                //删除多余的单元格
                List<int> cellsToRemove = Cells[i].Keys.Where(k => k >= Cols).ToList();
                foreach (int key in cellsToRemove)
                {
                    Cells[i].Remove(key);
                }
            }

            //删除多余的行
            List<int> rowsToRemove = Cells.Keys.Where(k => k >= Rows).ToList();
            foreach (int key in rowsToRemove)
            {
                Cells.Remove(key);
            }
        }
        //获取合适的单元格大小
        public float GetSuitableCellSize(VisualElement container)
        {
            //获取容器的宽度和高度
            float containerWidth = container.layout.width;
            float containerHeight = container.layout.height;
            //计算单元格大小
            float cellSize = Math.Min(containerWidth / Cols, containerHeight / Rows);
            //返回单元格大小
            return cellSize;
        }
        #endregion
    }
}

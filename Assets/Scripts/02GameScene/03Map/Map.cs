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
        public int Rows { get; set; } = 5;
        //列数
        public int Cols { get; set; } = 10;
        //单元格数据字典
        public Dictionary<int, Dictionary<int, MapCell>> Cells { get; } = new();
        #endregion

        #region 构造函数
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
        #endregion

        #region 公共方法
        //根据数据创建地图
        private void GenerateMap()
        {
            //清空单元格数据字典
            Cells.Clear();
            //根据行数和列数创建单元格数据字典
            for (int i = 0; i < Rows; i++)
            {
                Cells.Add(i, new Dictionary<int, MapCell>());
                for (int j = 0; j < Cols; j++)
                {
                    Cells[i].Add(j, new MapCell() { Text = "空", TextColorString = "000000" });
                }
            }
        }
        //根据数据获取地图视觉元素
        public VisualElement GetMap()
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
                        flexDirection = FlexDirection.Row
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
                            //设置文本颜色
                            color = ColorUtility.TryParseHtmlString(cell.Value.TextColorString, out Color color) ? color : Color.white,
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
        //获取合适的单元格大小
        private float GetSuitableCellSize(VisualElement container)
        {
            //获取容器的宽度和高度
            float containerWidth = container.resolvedStyle.width;
            float containerHeight = container.resolvedStyle.height;
            //获取单元格的宽度和高度
            float cellWidth = containerWidth / Cols;
            float cellHeight = containerHeight / Rows;
            //获取单元格的最小边长
            float minCellSize = Math.Min(cellWidth, cellHeight);
            //返回单元格的最小边长
            return minCellSize;
        }
        #endregion
    }
}

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using THLL.BaseSystem;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine;

namespace THLL.SceneSystem
{
    [Serializable]
    public class MapData
    {
        #region 数据
        //是否为空
        public bool IsEmpty { get; set; } = true;
        //地图预览
        public string Preview { get; set; } = string.Empty;
        //单元格字典
        [JsonConverter(typeof(MapCellsDictConverter))]
        public Dictionary<(int, int), MapCell> Cells { get; set; } = new();
        #endregion

        #region 视觉显示
        //地图视觉元素
        [JsonIgnore]
        public VisualElement MapView { get; set; }
        //已经标注的场景
        [JsonIgnore]
        public Dictionary<string, MapCell> DisplayedScenes { get; set; } = new();
        //是否在拖动状态
        [JsonIgnore]
        public bool IsDragging { get; set; } = false;
        #endregion

        #region 公开方法
        //创建地图
        public void CreateMap(int cols, int rows)
        {
            //检测行列数数据
            if (cols <= 0 || rows <= 0)
            {
                //若有其一为0，则清空数据
                Cells.Clear();
                //清空地图元素
                MapView = null;
                //清空地图预览
                Preview = string.Empty;
                //设定地图为空
                IsEmpty = true;
                //返回
                return;
            }
            else
            {
                //否则设定地图为非空
                IsEmpty = false;
                //创建新单元格字典
                Dictionary<(int, int), MapCell> newCells = new();
                //对行列进行遍历
                for (int i = 0; i < cols; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        //尝试获取单元格
                        if (Cells.TryGetValue((i, j), out MapCell cell))
                        {
                            //单元格存在则直接添加
                            newCells[(i, j)] = cell;
                        }
                        else
                        {
                            //单元格不存在则创建
                            cell = new MapCell();
                            //添加到单元格字典
                            newCells[(i, j)] = cell;
                        }
                    }
                }
                //更新单元格字典
                Cells = newCells;
                //生成地图预览
                GeneratePreview();
                //生成地图视觉元素
                GenerateMap();
            }
        }
        //获取地图
        public VisualElement GetMap()
        {
            return MapView ?? GenerateMap();
        }
        //生成地图预览
        public void GeneratePreview()
        {
            //清空预览字符串
            Preview = string.Empty;
            //检测地图是否为空
            if (IsEmpty)
            {
                //若为空，返回
                return;
            }
            //获取列行数
            int cols = Cells.Keys.Max(cell => cell.Item1) + 1;
            int rows = Cells.Keys.Max(cell => cell.Item2) + 1;
            //设置预览字符串
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //尝试获取单元格
                    if (!Cells.TryGetValue((j, i), out MapCell cell))
                    {
                        //单元格不存在则添加占位符
                        Preview += "ㅇ";
                    }
                    else if (cell.Data.Length > 1)
                    {
                        //单元格存在且大于一，以特标记
                        Preview += "特";
                    }
                    else
                    {
                        //单元格存在且等于一，则添加
                        Preview += cell.Data[0];
                    }
                }
                Preview += "\n";
            }
        }
        #endregion

        #region 私有方法
        //生成地图
        private VisualElement GenerateMap()
        {
            //创建视觉元素
            MapView = new VisualElement()
            {
                //设置名称
                name = "Map",
                //设置样式
                style =
                {
                    //排布方式为横向
                    flexDirection = FlexDirection.Row,
                    //设置边框为1
                    borderTopWidth = 1,
                    borderRightWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    //设置边框颜色为灰色
                    borderTopColor = Color.gray,
                    borderRightColor = Color.gray,
                    borderBottomColor = Color.gray,
                    borderLeftColor = Color.gray,
                }
            };
            //注册地图事件
            RegisterMapEvents();

            //获取列行数
            int cols = Cells.Keys.Max(cell => cell.Item1) + 1;
            int rows = Cells.Keys.Max(cell => cell.Item2) + 1;
            //遍历列数
            for (int i = 0; i < cols; i++)
            {
                //创建列视觉元素
                VisualElement colView = new()
                {
                    //设置名称
                    name = "Column" + i,
                };
                //遍历行数
                for (int j = 0; j < rows; j++)
                {
                    //尝试获取单元格
                    if (!Cells.TryGetValue((i, j), out MapCell cell))
                    {
                        //单元格不存在则创建
                        cell = new MapCell();
                        //添加到单元格字典
                        Cells[(i, j)] = cell;
                    }
                    //获取单元格视觉元素
                    VisualElement cellView = cell.GetCell();
                    //添加到列视觉元素
                    colView.Add(cellView);
                }
                //添加到地图视觉元素
                MapView.Add(colView);
            }

            //返回视觉元素
            return MapView;
        }
        //注册地图事件
        private void RegisterMapEvents()
        {
            //为视觉元素注册滚轴缩放事件
            MapView.RegisterCallback<WheelEvent>(evt =>
            {
                //首先，禁止事件传递
                evt.StopPropagation();

                //然后，获取缩放差值
                float zoomDelta = -evt.delta.y * 0.01f;
                //获取新缩放
                float newScale = MapView.resolvedStyle.scale.value.x * (1 + zoomDelta);
                //限制其大小
                newScale = Mathf.Clamp(newScale, 0.1f, 10f);
                //设置新缩放
                MapView.style.scale = new StyleScale(new Scale(new Vector2(newScale, newScale)));

                //最后，重新调整地图位置
                AdjustMapPosition();
            });
            //为视觉元素创建鼠标中键点击开始拖动事件
            MapView.RegisterCallback<MouseDownEvent>(evt =>
            {
                //然后，检测是否是中键点击
                if (evt.button == 2)
                {
                    //若是，则设定拖动状态
                    IsDragging = true;
                }
            });
            //为视觉元素注册中键拖动事件
            MapView.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //然后，检测中键是否被按下
                if (IsDragging)
                {
                    //若是，获取偏移量
                    Vector2 offset = evt.mouseDelta;
                    //设置偏移
                    AdjustMapPosition(offset);
                }
            });
            //为视觉元素注册放开鼠标中键结束拖动事件
            MapView.RegisterCallback<MouseUpEvent>(evt =>
            {
                //然后，检测是否是中键放开
                if (evt.button == 2)
                {
                    //若是，则取消拖动状态
                    IsDragging = false;
                }
            });
            //为视觉元素注册鼠标移开元素范围时结束拖动事件
            MapView.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //然后，取消拖动状态
                IsDragging = false;
            });
        }
        //调整地图位置
        private void AdjustMapPosition(Vector2 offset = default)
        {
            //检测是否有偏移量
            if (offset != default)
            {
                //获取偏移后位置
                Vector2 newPos = (Vector2)MapView.resolvedStyle.translate + offset;
                //设置偏移
                MapView.style.translate = new StyleTranslate(new Translate(newPos.x, newPos.y));
            }

            //偏移后获取视觉元素与其父级的Rect
            Rect mapRect = MapView.localBound;
            Rect parentRect = MapView.parent.localBound;

            //对高度进行判断
            if (mapRect.height < parentRect.height)
            {
                //若高度小于父级，则限制在父级内
                //判断上侧是否超出屏幕
                if (mapRect.y < 0)
                {
                    //如果超出，则向下移动
                    MapView.style.translate = new StyleTranslate(new Translate(MapView.resolvedStyle.translate.x, (mapRect.height - parentRect.height) / 2));
                }
                //判断下侧是否超出屏幕
                if (mapRect.yMax > parentRect.height)
                {
                    //如果超出，则向上移动
                    MapView.style.translate = new StyleTranslate(new Translate(MapView.resolvedStyle.translate.x, (parentRect.height - mapRect.height) / 2));
                }
            }
            else
            {
                //若高度大于父级，则将边界限制在父级外
                //判断上侧是否完全进入屏幕内
                if (mapRect.y > 0)
                {
                    //如果超出，则向下移动
                    MapView.style.translate = new StyleTranslate(new Translate(MapView.resolvedStyle.translate.x, (mapRect.height - parentRect.height) / 2));
                }
                //判断下侧是否完全进入屏幕内
                if (mapRect.yMax < parentRect.height)
                {
                    //如果超出，则向上移动
                    MapView.style.translate = new StyleTranslate(new Translate(MapView.resolvedStyle.translate.x, (parentRect.height - mapRect.height) / 2));
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
                    MapView.style.translate = new StyleTranslate(new Translate((mapRect.width - parentRect.width) / 2, MapView.resolvedStyle.translate.y));
                }
                //判断右侧是否超出屏幕
                if (mapRect.xMax > parentRect.width)
                {
                    //如果超出，则向左移动
                    MapView.style.translate = new StyleTranslate(new Translate((parentRect.width - mapRect.width) / 2, MapView.resolvedStyle.translate.y));
                }
            }
            else
            {
                //若宽度大于父级，则将边界限制在父级外
                //判断左侧是否完全进入屏幕内
                if (mapRect.x > 0)
                {
                    //如果超出，则向右移动
                    MapView.style.translate = new StyleTranslate(new Translate((mapRect.width - parentRect.width) / 2, MapView.resolvedStyle.translate.y));
                }
                //判断右侧是否完全进入屏幕内
                if (mapRect.xMax < parentRect.width)
                {
                    //如果超出，则向左移动
                    MapView.style.translate = new StyleTranslate(new Translate((parentRect.width - mapRect.width) / 2, MapView.resolvedStyle.translate.y));
                }
            }
        }
        #endregion
    }
}

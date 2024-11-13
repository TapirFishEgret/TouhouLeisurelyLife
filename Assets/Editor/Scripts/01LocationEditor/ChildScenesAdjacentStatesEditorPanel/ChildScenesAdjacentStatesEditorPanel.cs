using System.Collections.Generic;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class ChildScenesAdjacentStatesEditorPanel : VisualElement
    {
        #region 常量
        //弦图半径
        private const float ChordDiagramRadius = 300f;
        #endregion

        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示的场景
        public SceneData ShowedScene
        {
            get
            {
                //判断是否有数据被选中
                if (MainWindow.DataTreeView.ActiveSelection == null)
                {
                    return null;
                }
                //获取选中数据
                return MainWindow.DataTreeView.ActiveSelection.Data;
            }
        }

        //基层面板
        private VisualElement ChildScenesAdjacentStatesEditorRootPanel { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //弦图数据点容器
        private VisualElement ChordDiagramDataPointContainer { get; set; }
        //弦图连线容器
        private VisualElement ChordDiagramLineContainer { get; set; }
        #endregion

        #region 数据
        //弦图数据点字典
        private Dictionary<string, Label> ChordDiagramDataPoints { get; set; } = new();
        //连线字典
        private Dictionary<(string, string), VisualElement> ChordDiagramLines { get; set; } = new();
        //当前正在绘制的连线
        private VisualElement CurrentDrawingLine { get; set; } = null;
        //当前绘制连线的起点
        private Vector2 StartDrawLinePoint { get; set; } = Vector2.zero;
        //是否正在绘制连线
        private bool IsDrawingLine { get; set; } = false;
        #endregion 

        #region 数据编辑面板的初始化以及数据更新
        //构建函数
        public ChildScenesAdjacentStatesEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //设置自身为可扩展并隐藏
            style.flexGrow = 1;
            style.display = DisplayStyle.None;

            //获取面板
            visualTree.CloneTree(this);

            //指定主窗口
            MainWindow = mainWindow;

            //初始化
            Init();
        }
        //初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("子场景相邻状态编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //获取UI控件
            //基层面板
            ChildScenesAdjacentStatesEditorRootPanel = this.Q<VisualElement>("ChildScenesAdjacentStatesEditorRootPanel");
            //全名
            FullNameLabel = ChildScenesAdjacentStatesEditorRootPanel.Q<Label>("FullNameLabel");
            //弦图容器
            ChordDiagramDataPointContainer = ChildScenesAdjacentStatesEditorRootPanel.Q<VisualElement>("ChordDiagramDataPointContainer");
            //弦图连线容器
            ChordDiagramLineContainer = ChildScenesAdjacentStatesEditorRootPanel.Q<VisualElement>("ChordDiagramLineContainer");

            //注册事件
            RegisterEvents();
        }
        //刷新面板
        public void CSASRefresh()
        {
            //计时
            using ExecutionTimer timer = new("子场景相邻状态编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有，设置全名
                SetFullName();
                //生成弦图数据点
                GenerateChordDiagramDataPoints();
                //生成弦图连线
                GenerateChordDiagramLines();
            }
        }
        //注册事件
        private void RegisterEvents()
        {
            //为数据层创建鼠标左键点击开始连线事件
            ChordDiagramDataPointContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                //检测鼠标按键
                if (evt.button == 0)
                {
                    //若为左键，检测鼠标捕获目标是否为数据点
                    if (evt.target is Label label && ChordDiagramDataPoints.ContainsKey(label.userData.ToString()))
                    {
                        //若是，则开始绘制连线
                        IsDrawingLine = true;
                        //设置当前正在绘制的连线
                        CurrentDrawingLine = new VisualElement()
                        {
                            //设置userData，暂存起始数据点ID
                            userData = label.userData.ToString(),
                            //设置样式
                            style =
                            {
                                //设置位置为绝对
                                position = Position.Absolute,
                                //顶端位置为目标调整坐标后的top
                                top = label.localBound.center.y,
                                //左侧位置为目标调整坐标后的left
                                left = label.localBound.center.x,
                                //将中心位置设置在左上角
                                transformOrigin = new StyleTransformOrigin(new TransformOrigin(0,0)),
                                //设置宽度为0
                                width = 0,
                                //设置高度为3
                                height = 3f,
                                //设置背景色为随机
                                backgroundColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)),
                            }
                        };
                        //记录起始位置
                        StartDrawLinePoint = new Vector2(label.localBound.center.x, label.localBound.center.y);
                        //添加到容器中
                        ChordDiagramDataPointContainer.Add(CurrentDrawingLine);
                        //将起始点提前
                        label.BringToFront();
                    }
                }
            });
            //为连线层创建鼠标移动事件
            ChordDiagramDataPointContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //若正在绘制连线
                if (IsDrawingLine)
                {
                    //获取鼠标位置
                    Vector2 mousePosition = evt.localMousePosition;
                    //计算长度
                    float length = Vector2.Distance(StartDrawLinePoint, mousePosition);
                    //计算角度
                    float angle = Mathf.Atan2(mousePosition.y - StartDrawLinePoint.y, mousePosition.x - StartDrawLinePoint.x);
                    //设置当前正在绘制的连线的宽度
                    CurrentDrawingLine.style.width = length;
                    //设置当前正在绘制的连线的旋转角度
                    CurrentDrawingLine.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Radian)));
                }
            });
            //为连线层创建鼠标抬起事件
            ChordDiagramDataPointContainer.RegisterCallback<MouseUpEvent>(evt =>
            {
                //检测是否在绘制连线
                if (IsDrawingLine)
                {
                    //若是，则尝试获取目标数据点
                    if (evt.target is Label label && ChordDiagramDataPoints.ContainsKey(label.userData.ToString()))
                    {
                        //若获取成功，则尝试获取目标数据点的ID
                        string targetSceneID = label.userData.ToString();
                        //获取连线ID
                        (string, string) lineID = GetLineID(CurrentDrawingLine.userData.ToString(), targetSceneID);
                        //检测ID是否相同
                        if (CurrentDrawingLine.userData.ToString() == targetSceneID)
                        {
                            //若相同，则提示
                            Debug.Log("不能连接到自己");
                            //中断绘制
                            IsDrawingLine = false;
                        }
                        //检测是否已存在该连线
                        else if (ChordDiagramLines.ContainsKey(lineID))
                        {
                            //若是，则提示
                            Debug.Log("该连线已存在");
                            //中断绘制
                            IsDrawingLine = false;
                        }
                        else
                        {
                            //若不同，则生成连线
                            GenerateChordDiagramLine(CurrentDrawingLine.userData.ToString(), targetSceneID, CurrentDrawingLine.style.backgroundColor.value);
                            //并创建数据
                            CreateAdjacentState(CurrentDrawingLine.userData.ToString(), targetSceneID);
                        }
                    }
                    //移除当前正在绘制的连线
                    ChordDiagramDataPointContainer.Remove(CurrentDrawingLine);
                    CurrentDrawingLine = null;
                }
                else
                {
                    //若不是，则中断绘制
                    IsDrawingLine = false;
                    //移除当前正在绘制的连线
                    if (CurrentDrawingLine != null)
                    {
                        ChordDiagramDataPointContainer.Remove(CurrentDrawingLine);
                        CurrentDrawingLine = null;
                    }
                }
                //设置绘制状态为否
                IsDrawingLine = false;
            });

            //为连线层创建窗口大小更改重生成事件
            ChordDiagramDataPointContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                //刷新弦图连线
                GenerateChordDiagramLines();
            });
        }
        #endregion

        #region 公共方法
        //新增数据点
        public void AddChordDiagramDataPoint(string sceneID)
        {
            //生成数据点
            GenerateChordDiagramDataPoint(sceneID);
            //更新弦图数据点位置
            UpdateChordDiagramDataPointsPosition();
        }
        //删除数据点
        public void RemoveChordDiagramDataPoint(string sceneID)
        {
            //移除数据
            ChordDiagramDataPoints.Remove(sceneID);
            //更新弦图数据点位置
            UpdateChordDiagramDataPointsPosition();
        }
        #endregion

        #region 弦图数据点生成
        //生成弦图数据点
        private void GenerateChordDiagramDataPoints()
        {
            //清空当前弦图容器
            ChordDiagramDataPointContainer.Clear();
            //清空数据点列表
            ChordDiagramDataPoints.Clear();
            //检测当前场景是否为空
            if (ShowedScene == null)
            {
                //若为空，则退出
                return;
            }
            //若不为空，获取其子场景ID数据
            List<string> childSceneIDs = MainWindow.DataTreeView.ChildrenDicCache[ShowedScene.ID.GetHashCode()]
                .Select(x => x.data.Data.ID)
                .ToList();

            //遍历子场景ID
            for (int i = 0; i < childSceneIDs.Count; i++)
            {
                //为场景ID生成Label作为数据点
                GenerateChordDiagramDataPoint(childSceneIDs[i]);
            }
            //更新弦图数据点位置
            UpdateChordDiagramDataPointsPosition();
        }
        //生成数据点
        private Label GenerateChordDiagramDataPoint(string sceneID)
        {
            //生成数据点标签
            Label dataPoint = new()
            {
                //设置名称
                name = "ChordDiagramDataPoint" + sceneID,
                //设置文本
                text = sceneID.Split("_").Last(),
                //把全段的ID数据暂存在userData中
                userData = sceneID,
                //设置样式
                style =
                {
                    //设置位置为绝对
                    position = Position.Absolute,
                    //文字排布为居中
                    unityTextAlign = TextAnchor.MiddleCenter,
                    //设置字体大小为16
                    fontSize = 16,
                    //设置内外边距为0
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    //设置边框为1
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    //设置背景色为灰
                    backgroundColor = Color.grey,
                    //设置字体颜色为白色
                    color = Color.white,
                }
            };
            //注册鼠标进入事件
            dataPoint.RegisterCallback<MouseEnterEvent>(evt =>
            {
                //高亮数据点
                Highlight(dataPoint);
            });
            //注册鼠标离开事件
            dataPoint.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //取消高亮数据点
                UnHighlight(dataPoint);
            });
            //添加到容器中
            ChordDiagramDataPointContainer.Add(dataPoint);
            //添加到数据点列表
            ChordDiagramDataPoints[sceneID] = dataPoint;
            //返回数据点标签
            return dataPoint;
        }
        //更新弦图数据点的位置
        private void UpdateChordDiagramDataPointsPosition()
        {
            //计算角度步长
            float angleStep = 360f / ChordDiagramDataPoints.Count;
            //个数记录
            int count = 0;
            //遍历数据点
            foreach (Label dataPoint in ChordDiagramDataPoints.Values)
            {
                //计算弧度
                float radian = angleStep * count * Mathf.Deg2Rad;
                //计算坐标
                Vector2 position = new(ChordDiagramRadius * Mathf.Cos(radian), ChordDiagramRadius * Mathf.Sin(radian));
                //设置坐标
                dataPoint.style.translate = new StyleTranslate(new Translate(position.x, position.y));
                //个数自增
                count++;
            }
        }
        #endregion

        #region 弦图连线生成
        //生成弦图连线
        private void GenerateChordDiagramLines()
        {
            //清空当前连线
            foreach (VisualElement line in ChordDiagramLines.Values)
            {
                if (ChordDiagramDataPointContainer.Contains(line))
                {
                    ChordDiagramDataPointContainer.Remove(line);
                }
            }
            //清空连线列表
            ChordDiagramLines.Clear();
            //检测当前场景是否为空
            if (ShowedScene == null)
            {
                //若为空，则退出
                return;
            }
            //若不为空，获取子场景相邻状态数据
            List<(string, string)> adjacentStates = MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()]
                .data
                .Data
                .ChildScenesAdjacentStates
                .ToList();

            //遍历相邻状态数据
            foreach ((string fromSceneID, string toSceneID) in adjacentStates)
            {
                //生成连线
                GenerateChordDiagramLine(fromSceneID, toSceneID);
            }
        }
        //生成连线视觉元素
        private void GenerateChordDiagramLine(string fromSceneID, string toSceneID, Color color = default)
        {
            //尝试获取数据点
            if (ChordDiagramDataPoints.TryGetValue(fromSceneID, out Label fromDataPoint) && ChordDiagramDataPoints.TryGetValue(toSceneID, out Label toDataPoint))
            {
                //获取起始点与终点位置
                Vector2 fromPosition = fromDataPoint.localBound.center;
                Vector2 toPosition = toDataPoint.localBound.center;
                //计算连线角度
                float angle = Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
                //计算连线长度
                float length = Vector2.Distance(fromPosition, toPosition);
                //生成连线视觉元素
                VisualElement line = new()
                {
                    //设置名称
                    name = "ChordDiagramLine" + fromSceneID + toSceneID,
                    //设置样式
                    style =
                    {
                        //设置位置为绝对
                        position = Position.Absolute,
                        //顶端位置为起始点y
                        top = fromPosition.y,
                        //左侧位置为起始点x
                        left = fromPosition.x,
                        //将中心位置设置在左上角
                        transformOrigin = new StyleTransformOrigin(new TransformOrigin(0,0)),
                        //设置宽度为连线长度
                        width = length,
                        //设置高度为3
                        height = 3f,
                        //边框宽度为1
                        borderTopWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                    }
                };
                //注册鼠标进入事件
                line.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    //高亮线条
                    Highlight(line);
                });
                //注册鼠标离开事件
                line.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    //取消高亮线条
                    UnHighlight(line);
                });
                //注册右键删除事件
                line.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 1)
                    {
                        //删除连线
                        RemoveAdjacentState(fromSceneID, toSceneID);
                        //移除线条
                        ChordDiagramDataPointContainer.Remove(line);
                        //移除线条列表
                        ChordDiagramLines.Remove(GetLineID(fromSceneID, toSceneID));
                    }
                });
                //检测颜色
                if (color == default)
                {
                    //若未指定颜色，则随机生成
                    color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                }
                //设置背景色
                line.style.backgroundColor = color;
                //设置旋转角度
                line.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Radian)));
                //添加到容器中
                ChordDiagramDataPointContainer.Add(line);
                //添加到连线列表
                ChordDiagramLines[GetLineID(fromSceneID, toSceneID)] = line;
                //将数据点提前
                fromDataPoint.BringToFront();
                toDataPoint.BringToFront();
            }
        }
        #endregion

        #region 数据更改
        //创建相邻状态
        private void CreateAdjacentState(string fromSceneID, string toSceneID)
        {
            //获取连线ID
            (string, string) lineID = GetLineID(fromSceneID, toSceneID);
            //尝试获取相邻状态数据
            if (MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Contains(lineID))
            {
                //若存在，则退出
                return;
            }
            //若不存在，则创建相邻状态数据
            MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Add(lineID);
        }
        //删除相邻状态
        private void RemoveAdjacentState(string fromSceneID, string toSceneID)
        {
            //获取连线ID
            (string, string) lineID = GetLineID(fromSceneID, toSceneID);
            //尝试获取相邻状态数据
            if (MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Contains(lineID))
            {
                //若存在，则删除
                MainWindow.DataTreeView.ItemDicCache[ShowedScene.ID.GetHashCode()].data.Data.ChildScenesAdjacentStates.Remove(lineID);
            }
        }
        #endregion

        #region 辅助方法
        //获取场景的全名
        private void SetFullName()
        {
            //全名列表
            List<string> names = new();
            //将当前名称插入
            names.Insert(0, ShowedScene.Name);
            //获取父级ID
            string parnetID = ShowedScene.ParentSceneID;
            //若有父级
            while (!string.IsNullOrEmpty(parnetID))
            {
                //尝试获取父级数据
                if (MainWindow.DataTreeView.ItemDicCache.ContainsKey(parnetID.GetHashCode()))
                {
                    //获取父级数据
                    SceneData parentData = MainWindow.DataTreeView.ItemDicCache[parnetID.GetHashCode()].data.Data;
                    //将父级名称插入
                    names.Insert(0, parentData.Name);
                    //更新父级ID
                    parnetID = parentData.ParentSceneID;
                }
                else
                {
                    //若无数据，则退出循环
                    break;
                }
            }
            //设置全名显示
            FullNameLabel.text = string.Join("/", names);
        }
        //获取连线ID
        private (string, string) GetLineID(string str1, string str2)
        {
            return str1.CompareTo(str2) < 0 ? (str1, str2) : (str2, str1);
        }
        //高亮方法
        private void Highlight(VisualElement ve)
        {
            //高亮，形式为显示白色边框
            ve.style.borderTopColor = Color.white;
            ve.style.borderBottomColor = Color.white;
            ve.style.borderLeftColor = Color.white;
            ve.style.borderRightColor = Color.white;
        }
        //取消高亮方法
        private void UnHighlight(VisualElement ve)
        {
            //高亮，形式为显示透明边框
            ve.style.borderTopColor = Color.clear;
            ve.style.borderBottomColor = Color.clear;
            ve.style.borderLeftColor = Color.clear;
            ve.style.borderRightColor = Color.clear;
        }
        #endregion
    }
}

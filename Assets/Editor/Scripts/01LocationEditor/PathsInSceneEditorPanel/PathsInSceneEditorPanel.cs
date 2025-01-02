using System.Collections.Generic;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class PathsInSceneEditorPanel : VisualElement
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
        private VisualElement PathsInSceneEditorRootPanel { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //弦图数据点容器
        private VisualElement ChordDiagramContainer { get; set; }
        //数据框容器
        private VisualElement DataBoxContainer { get; set; }
        #endregion

        #region 数据
        //路径字典
        public Dictionary<(string, string), ScenePath> PathsInScene { get; set; } = new();
        //弦图数据点字典
        private Dictionary<string, Label> ChordDiagramDataPoints { get; set; } = new();
        //当前指向的数据点
        private Label CurrentPoint { get; set; } = null;
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
        public PathsInSceneEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
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
            using ExecutionTimer timer = new("场景路径编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //获取UI控件
            //基层面板
            PathsInSceneEditorRootPanel = this.Q<VisualElement>("PathsInSceneEditorRootPanel");
            //全名
            FullNameLabel = PathsInSceneEditorRootPanel.Q<Label>("FullNameLabel");
            //弦图容器
            ChordDiagramContainer = PathsInSceneEditorRootPanel.Q<VisualElement>("ChordDiagramContainer");
            //数据框容器
            DataBoxContainer = PathsInSceneEditorRootPanel.Q<VisualElement>("DataBoxContainer");

            //注册事件
            RegisterEvents();
        }
        //刷新面板
        public void PISRefresh()
        {
            //计时
            using ExecutionTimer timer = new("场景路径编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //检测是否有数据被选择
            if (ShowedScene != null)
            {
                //若有，设置全名
                SetFullName();
                //生成路径字典
                PathsInScene.Clear();
                foreach (ScenePath scenePath in ShowedScene.MapData.PathsInScene)
                {
                    (string, string) pathID = GetPathID(scenePath.SceneAID, scenePath.SceneBID);
                    PathsInScene[pathID] = scenePath;
                }
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
            ChordDiagramContainer.RegisterCallback<MouseDownEvent>(evt =>
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
                        ChordDiagramContainer.Add(CurrentDrawingLine);
                        //将起始点提前
                        label.BringToFront();
                    }
                }
            });
            //为连线层创建鼠标移动事件
            ChordDiagramContainer.RegisterCallback<MouseMoveEvent>(evt =>
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
            ChordDiagramContainer.RegisterCallback<MouseUpEvent>(evt =>
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
                        (string, string) lineID = GetPathID(CurrentDrawingLine.userData.ToString(), targetSceneID);
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
                            CreatePath(CurrentDrawingLine.userData.ToString(), targetSceneID);
                        }
                    }
                    //移除当前正在绘制的连线
                    ChordDiagramContainer.Remove(CurrentDrawingLine);
                    CurrentDrawingLine = null;
                }
                else
                {
                    //若不是，则中断绘制
                    IsDrawingLine = false;
                    //移除当前正在绘制的连线
                    if (CurrentDrawingLine != null)
                    {
                        ChordDiagramContainer.Remove(CurrentDrawingLine);
                        CurrentDrawingLine = null;
                    }
                }
                //设置绘制状态为否
                IsDrawingLine = false;
            });

            //每帧更新线条
            EditorApplication.update += UpdateLines;
        }
        #endregion

        #region 数据点增删方法
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
            ChordDiagramContainer.Clear();
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
                //设置当前指向的数据点
                CurrentPoint = dataPoint;
            });
            //注册鼠标离开事件
            dataPoint.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //取消高亮数据点
                UnHighlight(dataPoint);
            });
            //注册右键菜单
            dataPoint.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("设置为出入口", action => SetAsEntrance());
            }));
            //添加到容器中
            ChordDiagramContainer.Add(dataPoint);
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

        #region 弦图连线及对应数据框生成
        //生成弦图连线
        private void GenerateChordDiagramLines()
        {
            //清空当前连线
            foreach (VisualElement line in ChordDiagramLines.Values)
            {
                if (ChordDiagramContainer.Contains(line))
                {
                    ChordDiagramContainer.Remove(line);
                }
            }
            //清空连线列表
            ChordDiagramLines.Clear();
            //清空数据框容器
            DataBoxContainer.Clear();
            //检测当前场景是否为空
            if (ShowedScene == null)
            {
                //若为空，则退出
                return;
            }
            
            //遍历路径数据
            foreach ((string fromSceneID, string toSceneID) in PathsInScene.Keys)
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
                    //暂存数据
                    userData = fromSceneID + "+" + toSceneID,
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
                ChordDiagramContainer.Add(line);
                //添加到连线列表
                ChordDiagramLines[GetPathID(fromSceneID, toSceneID)] = line;
                //生成数据框
                GenerateDataBox(fromSceneID, toSceneID);
                //将数据点提前
                fromDataPoint.BringToFront();
                toDataPoint.BringToFront();
            }
            else
            {
                //若有数据点不存在，考虑数据点之一为父级的可能性
                if (fromSceneID == ShowedScene.ID || toSceneID == ShowedScene.ID)
                {
                    //若是，仅生成数据框
                    GenerateDataBox(fromSceneID, toSceneID, true);
                }
            }
        }
        //生成数据框
        private void GenerateDataBox(string fromSceneID, string toSceneID, bool isEntrance = false)
        {
            //获取路径ID
            (string, string) pathID = GetPathID(fromSceneID, toSceneID);
            //创建容器
            VisualElement dataBox = new()
            {
                //设置样式
                style =
                {
                    //设置外边距
                    marginTop = 3,
                    marginBottom = 3,
                    marginLeft = 2,
                    marginRight = 2,
                    //边框宽度为1
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                }
            };
            //检测是否为出入口
            if (isEntrance)
            {
                //若是，则设置样式
                dataBox.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            }
            //注册鼠标进入事件
            dataBox.RegisterCallback<MouseEnterEvent>(evt =>
            {
                //高亮框
                Highlight(dataBox);
                //尝试获取线条
                if (ChordDiagramLines.TryGetValue(pathID, out VisualElement line))
                {
                    //若存在，则高亮线条
                    Highlight(line);
                }
            });
            //注册鼠标离开事件
            dataBox.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //取消高亮框
                UnHighlight(dataBox);
                //尝试获取线条
                if (ChordDiagramLines.TryGetValue(pathID, out VisualElement line))
                {
                    //若存在，则取消高亮线条
                    UnHighlight(line);
                }
            });
            //创建场景A标签
            Label sceneALabel = new()
            {
                //设置文本
                text = fromSceneID.Split("_").Last(),
                //设置样式
                style =
                {
                    //设置字体大小
                    fontSize = 16,
                    //设置字体颜色
                    color = Color.white,
                    //设置外边距
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //创建场景B标签
            Label sceneBLabel = new()
            {
                //设置文本
                text = toSceneID.Split("_").Last(),
                //设置样式
                style =
                {
                    //设置字体大小
                    fontSize = 16,
                    //设置字体颜色
                    color = Color.white,
                    //设置外边距
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //创建距离设置框
            IntegerField distanceField = new()
            {
                //设置标签
                label = "距离",
                //延迟相应
                isDelayed = true,
                //设置样式
                style =
                {
                    //设置字体大小
                    fontSize = 16,
                    //设置字体颜色
                    color = Color.white,
                    //设置外边距
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //设定距离值
            if (PathsInScene.ContainsKey(pathID))
            {
                //若存在路径数据，则设置距离值
                distanceField.SetValueWithoutNotify(PathsInScene[pathID].Distance);
            }
            //注册距离更改事件
            distanceField.RegisterValueChangedCallback(evt =>
            {
                //检测输入数值
                if (evt.newValue < 0)
                {
                    //若小于零，强制其等于0
                    distanceField.value = 0;
                }
                else
                {
                    //尝试获取路径数据
                    if (PathsInScene.ContainsKey(pathID))
                    {
                        //若存在，则更新距离
                        PathsInScene[pathID].Distance = evt.newValue;
                    }
                }
            });
            //创建删除按钮
            Button deleteButton = new()
            {
                //设置文本
                text = "删除",
                //设置样式
                style =
                {
                    //设置字体大小
                    fontSize = 16,
                    //设置字体颜色
                    color = Color.white,
                    //设置外边距
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = 1,
                    marginRight = 1,
                }
            };
            //注册删除按钮事件
            deleteButton.clicked += () =>
            {
                //删除路径
                RemovePath(fromSceneID, toSceneID);
                //移除数据框
                DataBoxContainer.Remove(dataBox);
                //检测线条是否存在
                if (ChordDiagramLines.TryGetValue(pathID, out VisualElement line))
                {
                    //若存在，则移除线条
                    ChordDiagramContainer.Remove(line);
                }
                //从列表中移除线条
                ChordDiagramLines.Remove(pathID);
            };
            //添加到容器中
            dataBox.Add(sceneALabel);
            dataBox.Add(sceneBLabel);
            dataBox.Add(distanceField);
            dataBox.Add(deleteButton);
            //将数据框添加到容器中
            DataBoxContainer.Add(dataBox);
        }
        //更新连线
        private void UpdateLines()
        {
            //遍历所有连线
            foreach (VisualElement line in ChordDiagramLines.Values)
            {
                //获取存储数据
                if (line.userData is string data)
                {
                    //获取起始点与终点位置
                    string fromSceneID = data.Split("+")[0];
                    string toSceneID = data.Split("+")[1];
                    if (ChordDiagramDataPoints.TryGetValue(fromSceneID, out Label fromDataPoint) && ChordDiagramDataPoints.TryGetValue(toSceneID, out Label toDataPoint))
                    {
                        //获取起始点与终点位置
                        Vector2 fromPosition = fromDataPoint.localBound.center;
                        Vector2 toPosition = toDataPoint.localBound.center;
                        //计算连线角度
                        float angle = Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x);
                        //计算连线长度
                        float length = Vector2.Distance(fromPosition, toPosition);
                        //设置连线
                        line.style.top = fromPosition.y;
                        line.style.left = fromPosition.x;
                        line.style.width = length;
                        line.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Radian)));
                    }
                }
            }
        }
        #endregion

        #region 路径的删除与添加
        //创建场景路径
        private bool CreatePath(string fromSceneID, string toSceneID)
        {
            //获取连线ID
            (string, string) lineID = GetPathID(fromSceneID, toSceneID);
            //尝试路径数据
            if (PathsInScene.ContainsKey(lineID))
            {
                //若存在，则返回false
                return false;
            }
            //若不存在，则创建路径数据
            ScenePath scenePath = new()
            {
                SceneAID = fromSceneID,
                SceneBID = toSceneID,
            };
            //添加到场景数据中
            ShowedScene.MapData.PathsInScene.Add(scenePath);
            //添加到路径数据列表
            PathsInScene[lineID] = scenePath;
            //返回成功
            return true;
        }
        //删除路径
        private bool RemovePath(string fromSceneID, string toSceneID)
        {
            //获取连线ID
            (string, string) lineID = GetPathID(fromSceneID, toSceneID);
            //尝试获取路径数据
            if (PathsInScene.ContainsKey(lineID))
            {
                //从场景数据中删除
                ShowedScene.MapData.PathsInScene.Remove(PathsInScene[lineID]);
                //从路径字典中移除
                PathsInScene.Remove(lineID);
                //返回true
                return true;
            }
            //若不存在，则返回false
            return false;
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
        private (string, string) GetPathID(string str1, string str2)
        {
            return str1.CompareTo(str2) < 0 ? (str1, str2) : (str2, str1);
        }
        //设置为地区出入口，本质为在选中场景与其父级场景之间创建路径
        private void SetAsEntrance()
        {
            //检测是否合法
            if (CurrentPoint != null)
            {
                //合法的状态下，创建路径
                if (CreatePath(ShowedScene.ID, CurrentPoint.userData.ToString()))
                {
                    //创建数据框
                    GenerateDataBox(ShowedScene.ID, CurrentPoint.userData.ToString(), true);
                }
            }
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

using System.Collections.Generic;
using System.IO;
using THLL.LocationSystem;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeEditorPanel : Tab
    {
        #region 基础构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //滚轴容器与节点容器
        public ScrollView ScrollView { get; private set; }
        public VisualElement NodeView { get; private set; }

        //滚轴面板拖放
        public Vector2 ScrollViewDragStart { get; private set; }
        public Vector2 ScrollViewStartPos { get; private set; }

        //ID-地点数据节点缓存
        public Dictionary<int, Node> NodeDicCache { get; private set; }
        //线条数据缓存
        public Dictionary<int, NodeLine> NodeLineCache { get; private set; }
        //ID-地点数据节点元素位置缓存
        public Dictionary<int, (float, float)> NodePosDicCache { get; private set; }

        //当前面板节点与连接
        public List<Node> ShowedNodes { get; private set; }
        public Dictionary<int, NodeLine> ShowedNodeLines { get; private set; }
        public bool NeedToRefresh { get; set; }

        //节点连接功能
        public Node CurrentStartNode { get; private set; }
        public NodeLine CurrentNodeLine { get; private set; }
        #endregion

        //构建函数
        public NodeEditorPanel(MainWindow mainWindow)
        {
            //创建并添加容器
            ScrollView = new()
            {
                style =
                {
                    flexGrow = 1,
                }
            };
            Add(ScrollView);
            NodeView = new()
            {
                style =
                {
                    position = Position.Absolute,
                    width = 5000,
                    height = 5000
                }
            };
            ScrollView.Add(NodeView);

            //初始化缓存
            NodeDicCache = new();
            NodePosDicCache = new();
            NodeLineCache = new();

            //初始化列表
            ShowedNodes = new();
            ShowedNodeLines = new();

            //获取主面板
            MainWindow = mainWindow;

            //初始化
            Init();
        }

        #region 连接编辑面板的初始化与显示
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("连接编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //自身设置为可延展
            style.flexGrow = 1;

            //设定名称
            label = "节点编辑窗口";

            //读取持久化数据
            LoadPersistentData();

            //写入绘制事件
            NodeView.generateVisualContent += DrawGrid;

            //生成节点与连线
            GenerateNodesAndLines();

            //监听事件
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            ScrollView.RegisterCallback<PointerDownEvent>(SVOnPointerDown);
            ScrollView.RegisterCallback<PointerMoveEvent>(SVOnPointerMove);
            ScrollView.RegisterCallback<PointerUpEvent>(SVOnPointerUp);
        }
        //绘制网格纹理
        private void DrawGrid(MeshGenerationContext mgc)
        {
            //网格间隔
            float gridSpacing = 20f;
            //网格颜色
            Color gridColor = ChineseColor.Grey_大理石灰;
            gridColor.a = 0.2f;

            //计算需要绘制的网格线条总数
            int widthDivs = Mathf.CeilToInt(NodeView.worldBound.xMax / gridSpacing);
            int heightDivs = Mathf.CeilToInt(NodeView.worldBound.yMax / gridSpacing);

            //获取画笔
            Painter2D painter = mgc.painter2D;
            painter.lineWidth = 2.0f;
            painter.strokeColor = gridColor;
            painter.BeginPath();

            //绘制垂直网格线
            for (int i = 0; i < widthDivs; i++)
            {
                //绘制线条，从一段到另一端
                painter.MoveTo(new Vector2(gridSpacing * i, 0));
                painter.LineTo(new Vector2(gridSpacing * i, NodeView.worldBound.yMax));
            }
            //水平
            for (int j = 0; j < heightDivs; j++)
            {
                painter.MoveTo(new Vector2(0, gridSpacing * j));
                painter.LineTo(new Vector2(NodeView.worldBound.xMax, gridSpacing * j));
            }

            //结束绘制
            painter.Stroke();
        }
        //刷新逻辑
        public void NRefresh()
        {
            //检测传入是否为空
            if (MainWindow.DataTreeView.ActiveSelection == null)
            {
                //若是，返回
                return;
            }
            //再检测选中节点是否已经在显示中
            if (ShowedNodes.Contains(NodeDicCache[MainWindow.DataTreeView.ActiveSelection.GetAssetHashCode()]))
            {
                //若是，返回
                return;
            }

            //若不是，开始计时
            using ExecutionTimer timer = new("节点编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //刷新
            //首先记录并清除当前显示节点与连接
            foreach (Node node in ShowedNodes)
            {
                NodePosDicCache[node.TargetData.GetAssetHashCode()] = (node.style.left.value.value, node.style.top.value.value);
                NodeView.Remove(node);
            }
            foreach (NodeLine nodeLine in ShowedNodeLines.Values)
            {
                NodeView.Remove(nodeLine);
            }
            ShowedNodes.Clear();
            ShowedNodeLines.Clear();

            //做好显示新系节点的准备
            List<TreeViewItemData<LocUnitData>> peers;
            LocUnitData parentData = MainWindow.DataTreeView.ActiveSelection.ParentData;
            //随后对传入数据进行分析，检测其有无父级
            if (parentData != null)
            {
                //若有父级，则子级直接从父级获取
                peers = MainWindow.DataTreeView.ChildrenDicCache[MainWindow.DataTreeView.ActiveSelection.ParentData.GetAssetHashCode()];
            }
            else
            {
                //若无父级，则子级为顶级
                peers = MainWindow.DataTreeView.RootItemCache;
            }

            //对新系列节点进行生成或显示
            foreach (TreeViewItemData<LocUnitData> item in peers)
            {
                //获取其资源哈希值
                int id = item.data.GetAssetHashCode();
                //读取节点
                Node node = NodeDicCache[id];
                //并显示其连线
                foreach (NodeLine nodeLine in node.NodeLines)
                {
                    //判断是否已有
                    if (!ShowedNodeLines.ContainsKey(nodeLine.ID))
                    {
                        //若没有
                        NodeView.Add(nodeLine);
                        //并加入肯德基豪华午餐
                        ShowedNodeLines[nodeLine.ID] = nodeLine;
                    }
                }
                //顺便给个名字
                node.Label.text = item.data.name;
                //将节点添加至面板
                NodeView.Add(node);
                //尝试更改节点位置
                if (NodePosDicCache.ContainsKey(id))
                {
                    node.style.left = NodePosDicCache[id].Item1;
                    node.style.top = NodePosDicCache[id].Item2;
                }
                //添加到当前节点列表中
                ShowedNodes.Add(node);
            }

            //对新系列线条进行位置更改
            foreach (NodeLine nodeLine1 in ShowedNodeLines.Values)
            {
                nodeLine1.UpdateLine();
            }
        }
        //窗口大小变化事件
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //判断活跃窗口
            if (MainWindow.MultiTabView.activeTab == this)
            {
                //若为自身
                //更改窗口大小
                ScrollView.style.width = evt.newRect.width;
                ScrollView.style.height = evt.newRect.height;
            }
        }
        //重新生成节点与连线
        public void GenerateNodesAndLines()
        {
            //清除数据
            NodeDicCache.Clear();
            NodeLineCache.Clear();
            //记录并清除当前显示节点与连接
            foreach (Node node in ShowedNodes)
            {
                NodeView.Remove(node);
            }
            foreach (NodeLine nodeLine in ShowedNodeLines.Values)
            {
                NodeView.Remove(nodeLine);
            }
            ShowedNodes.Clear();
            ShowedNodeLines.Clear();

            //生成所有节点
            foreach (TreeViewItemData<LocUnitData> item in MainWindow.DataTreeView.ItemDicCache.Values)
            {
                //生成节点
                Node newNode = new(item.data, this);
                //添加到缓存中
                NodeDicCache[item.data.GetAssetHashCode()] = newNode;
            }
            //生成所有连线
            foreach (Node node in NodeDicCache.Values)
            {
                //根据数据中存储的地点连接信息来生成连接
                foreach (LocUnitData locUnitData in node.TargetData.ConnectionKeys)
                {
                    Node otherNode = NodeDicCache[locUnitData.GetAssetHashCode()];
                    //检测父子级关系
                    if (node.TargetData.ParentData == otherNode.TargetData || otherNode.TargetData.ParentData == node.TargetData)
                    {
                        //若是父子级，略过此线条的创建
                        continue;
                    }
                    //生成连线
                    NodeLine line = new(this) { StartNode = node, EndNode = otherNode };
                    //检测是否已经存在
                    if (NodeLineCache.ContainsKey(line.ID))
                    {
                        //若存在，进入下一循环
                        continue;
                    }
                    //若不存在，设定数值
                    line.IntegerField.SetValueWithoutNotify(node.TargetData.ConnectionValues[node.TargetData.ConnectionKeys.IndexOf(locUnitData)]);
                    //显示输入框
                    line.Add(line.IntegerField);
                    //将节点显示提前
                    node.BringToFront();
                    otherNode.BringToFront();
                    //将连线添加到缓存与节点中
                    NodeLineCache[line.ID] = line;
                    node.NodeLines.Add(line);
                    otherNode.NodeLines.Add(line);
                }
            }

            //数据自净，指的是删除缓存中已经被移除的数据
            List<int> missingNodes = new();
            foreach (int id in NodePosDicCache.Keys)
            {
                if (!NodeDicCache.ContainsKey(id))
                {
                    missingNodes.Add(id);
                }
            }
            foreach (int id in missingNodes)
            {
                NodePosDicCache.Remove(id);
            }
        }
        //读取永久性存储文件到缓存
        private void LoadPersistentData()
        {
            //读取文件中数据
            string jsonString = File.ReadAllText(AssetDatabase.GetAssetPath(MainWindow.PersistentDataFile));
            //生成永久性存储实例
            PersistentData persistentData = JsonConvert.DeserializeObject<PersistentData>(jsonString);
            //分配数据
            NodePosDicCache = persistentData.NodePositions;
        }
        #endregion

        #region 节点与面板的移动与节点连接与设定
        //鼠标按下事件
        private void OnPointerDown(PointerDownEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //若左键按下
                if (evt.clickCount == 2)
                {
                    //且为双击
                    if (evt.target is Node nodeA)
                    {
                        //且目标元素为节点，则更改其出入口设定
                        nodeA.AsGateway(!nodeA.IsGateway);
                    }
                }
            }
            else if (evt.button == 1)
            {
                //当鼠标右键按下时
                //检测目标元素
                if (evt.target is Node node)
                {
                    //若目标为节点元素Node，检测当前连接状态
                    if (CurrentStartNode == null)
                    {
                        //若当前无连接在进行，则开始连接
                        StartLine(node);
                    }
                    else
                    {
                        //若当前有连接在进行，检测目标
                        if (node != CurrentStartNode)
                        {
                            //若目标与当前起始节点不同，创建
                            EndLine(node);
                        }
                    }
                }
                //若不是，检查当下是否有连接
                else if (CurrentNodeLine != null)
                {
                    //若是，则清除当前连接
                    NodeView.Remove(CurrentNodeLine);
                    CurrentNodeLine = null;
                    CurrentStartNode = null;
                }
                //阻断事件传递
                evt.StopPropagation();
            }
        }
        //鼠标移动时
        private void OnPointerMove(PointerMoveEvent evt)
        {
            //检测当前是否有在创建连接，若有，更新线条
            CurrentNodeLine?.UpdateLine();
        }
        //开始连接
        private void StartLine(Node startNode)
        {
            //记录起始节点
            CurrentStartNode = startNode;
            //以传入节点为初始节点新建当前连接
            CurrentNodeLine = new(this)
            {
                StartNode = startNode,
            };
            //将线条添加入面板
            NodeView.Add(CurrentNodeLine);
        }
        //结束连接
        private void EndLine(Node endNode)
        {
            //设定当前连线节点终点
            CurrentNodeLine.EndNode = endNode;
            //刷新
            CurrentNodeLine.UpdateLine();
            //节点前置
            CurrentNodeLine.StartNode.BringToFront();
            CurrentNodeLine.EndNode.BringToFront();
            //触发连接完成事件
            CurrentNodeLine.OnLineConnected();
            //清空起始节点
            CurrentStartNode = null;
            //清空当前连线
            CurrentNodeLine = null;
        }
        #endregion

        #region 滚轴面板事件
        //鼠标按下时
        private void SVOnPointerDown(PointerDownEvent evt)
        {
            //检测是否选到了节点
            if (evt.target is Node)
            {
                //若是，返回
                return;
            }
            //记录开始拖动的位置
            ScrollViewDragStart = evt.localPosition;
            ScrollViewStartPos = new Vector2(ScrollView.scrollOffset.x, ScrollView.scrollOffset.y);
            //并捕获鼠标
            ScrollView.CaptureMouse();
        }
        //鼠标移动时
        private void SVOnPointerMove(PointerMoveEvent evt)
        {
            //检测鼠标是否有被捕获
            if (ScrollView.HasMouseCapture())
            {
                //若有，则计算偏移并拖动
                Vector2 dragOffset = (Vector2)evt.localPosition - ScrollViewDragStart;
                ScrollView.scrollOffset = ScrollViewStartPos - dragOffset;
            }
        }
        //鼠标抬起时
        private void SVOnPointerUp(PointerUpEvent evt)
        {
            ScrollView.ReleaseMouse();
        }
        #endregion
    }
}

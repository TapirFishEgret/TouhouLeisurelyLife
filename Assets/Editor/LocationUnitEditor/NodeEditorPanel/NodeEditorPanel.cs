using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeEditorPanel : VisualElement
    {
        #region 基础构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //ID-地点数据节点缓存
        public Dictionary<int, Node> NodeDicCache { get; private set; }

        //节点编辑功能
        public Node CurrentStartNode { get; set; }
        public NodeLine CurrentLine { get; set; }
        #endregion

        //构建函数
        public NodeEditorPanel(MainWindow mainWindow)
        {
            //自身设置为可延展且100%填充
            style.flexShrink = 1;
            style.flexGrow = 1;
            style.width = new Length(100, LengthUnit.Percent);
            style.height = new Length(100, LengthUnit.Percent);

            //获取主面板
            MainWindow = mainWindow;

            //初始化缓存
            NodeDicCache = new();

            //初始化
            Init();
        }

        #region 连接编辑面板的初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("连接编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //写入绘制事件
            generateVisualContent += DrawGrid;

            //监听事件
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
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
            int widthDivs = Mathf.CeilToInt(worldBound.xMax / gridSpacing);
            int heightDivs = Mathf.CeilToInt(worldBound.yMax / gridSpacing);

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
                painter.LineTo(new Vector2(gridSpacing * i, worldBound.yMax));
            }
            //水平
            for (int j = 0; j < heightDivs; j++)
            {
                painter.MoveTo(new Vector2(0, gridSpacing * j));
                painter.LineTo(new Vector2(worldBound.xMax, gridSpacing * j));
            }

            //结束绘制
            painter.Stroke();
        }
        //鼠标按下事件
        private void OnPointerDown(PointerDownEvent evt)
        {
            //检测按键
            if (evt.button == 1)
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
                        //若当前有连接在进行，则结束连接
                        EndLine(node);
                    }
                }
                //若不是，检查当下是否有连接
                else if (CurrentLine != null)
                {
                    //若是，则清除当前连接
                    Remove(CurrentLine);
                    CurrentLine = null;
                    CurrentStartNode = null;
                }
            }
            Debug.Log(evt.target);
        }
        //鼠标移动时
        private void OnPointerMove(PointerMoveEvent evt)
        {
            //检测当前是否有在创建连接，若有，更新线条
            CurrentLine?.UpdateLine();
        }
        //窗口变化事件
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //获取X轴缩放比例
            float scaleX = evt.newRect.width / evt.oldRect.width;
            //获取Y轴缩放比例
            float scaleY = evt.newRect.height / evt.oldRect.height;

            //触发所有节点的更改位置方法
            foreach (Node node in NodeDicCache.Values)
            {
                node.ResetPosition(scaleX, scaleY);
            }
        }
        //开始连接
        private void StartLine(Node startNode)
        {
            //记录起始节点
            CurrentStartNode = startNode;
            //以传入节点为初始节点新建当前连接
            CurrentLine = new(this)
            {
                StartNode = startNode,
            };
            //将线条添加入面板
            Add(CurrentLine);
        }
        //结束连接
        private void EndLine(Node endNode)
        {
            //设定当前连线节点终点
            CurrentLine.EndNode = endNode;
            //刷新
            CurrentLine.UpdateLine();
            //将线条加入双方记录
            CurrentLine.StartNode.Lines.Add(CurrentLine);
            CurrentLine.EndNode.Lines.Add(CurrentLine);
            //节点前置
            CurrentLine.StartNode.BringToFront();
            CurrentLine.EndNode.BringToFront();
            //清空起始节点
            CurrentStartNode = null;
            //清空当前连线
            CurrentLine = null;
        }
        #endregion
    }
}

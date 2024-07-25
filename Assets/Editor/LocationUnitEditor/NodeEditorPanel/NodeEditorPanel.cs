using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeEditorPanel : VisualElement
    {
        #region 基础构成
        //主面板
        private readonly MainWindow _mainWindow;
        public MainWindow MainWindow => _mainWindow;

        //ID-地点数据节点缓存
        private readonly Dictionary<int, Node> _nodeDicCache = new();
        public Dictionary<int, Node> NodeDicCache => _nodeDicCache;

        //连接编辑面板的节点拖拽功能
        private Vector2 _dragStart;
        public Vector2 DragStart { get => _dragStart; private set => _dragStart = value; }
        private bool _isDragging = false;
        public bool IsDragging { get => _isDragging; private set => _isDragging = value; }

        //节点编辑功能
        private Node _currentStartNode;
        public Node CurrentStartNode { get => _currentStartNode; set => _currentStartNode = value; }
        private NodeLine _currentLine;
        public NodeLine CurrentLine { get => _currentLine; set => _currentLine = value; }
        #endregion

        //构建函数
        public NodeEditorPanel(MainWindow mainWindow)
        {
            //设置为可延展且100%填充
            style.position = Position.Absolute;
            style.flexShrink = 1;
            style.flexGrow = 1;
            style.width = new Length(100, LengthUnit.Percent);
            style.height = new Length(100, LengthUnit.Percent);

            //获取主面板
            _mainWindow = mainWindow;

            //初始化
            Init();
        }

        #region 连接编辑面板的初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("连接编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //添加网格背景
            //创建网格
            IMGUIContainer gridContainer = new()
            {
                //设置格式
                style =
                {
                    position = Position.Absolute,
                    width = 2560,
                    height = 1440,
                },
                //写入网格绘制方法
                onGUIHandler = DrawGrid,
            };
            //添加到面板中去
            Add(gridContainer);

            //监听事件
            RegisterCallback<WheelEvent>(OnMouseWheel);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        //绘制网格纹理
        private void DrawGrid()
        {
            //网格间隔
            float gridSpacing = 20f;
            //网格透明度
            float gridOpacity = 0.2f;
            //网格颜色
            Color gridColor = ChineseColor.Grey_大理石灰;

            //计算需要绘制的网格线条总数
            int widthDivs = Mathf.CeilToInt(contentRect.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(contentRect.height / gridSpacing);

            //开始IMGUI绘制
            Handles.BeginGUI();
            //设置绘制颜色
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            //绘制垂直网格线
            for (int i = 0; i < widthDivs; i++)
            {
                //绘制线条，从一段到另一端
                Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, contentRect.height, 0));
            }
            //水平
            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(contentRect.width, gridSpacing * j, 0));
            }

            //重置绘制颜色
            Handles.color = Color.white;
            //结束IMGUI绘制
            Handles.EndGUI();
        }
        //鼠标滚轮事件
        private void OnMouseWheel(WheelEvent evt)
        {
            //处理缩放
            //原缩放比例
            Vector3 scale = transform.scale;
            //缩放调整因子
            float zoomFactor = 1.1f;
            //检测滚轮移动
            if (evt.delta.y > 0)
            {
                //若有移动，调整缩放因子
                zoomFactor = 1 / zoomFactor;
            }
            //调整缩放
            scale *= zoomFactor;
            //应用缩放
            transform.scale = scale;
        }
        //鼠标按下事件
        private void OnMouseDown(MouseDownEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //当鼠标左键按下时
                //记录拖动起始位置
                DragStart = evt.localMousePosition;
                //状态更改为拖放中
                IsDragging = true;
            }
        }
        //鼠标移动时
        private void OnMouseMove(MouseMoveEvent evt)
        {
            //检测拖放状态
            if (IsDragging)
            {
                //若正在拖放
                //获取位置差值
                Vector2 delta = evt.localMousePosition - DragStart;
                //变更位置
                transform.position += (Vector3)delta;
            }
            //检测当前是否有在创建连接，若有，更新线条
            CurrentLine?.UpdateLine();
        }
        //鼠标抬起时
        private void OnMouseUp(MouseUpEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //当左键抬起时
                //取消拖拽状态
                IsDragging = false;
            }
        }
        //鼠标离开时
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            //停止拖拽
            IsDragging = false;
        }
        #endregion
    }
}

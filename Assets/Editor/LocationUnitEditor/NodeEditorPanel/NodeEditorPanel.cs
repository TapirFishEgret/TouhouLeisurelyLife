using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

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
        private bool _isDragging = false;
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
            //新建网格背景
            IMGUIContainer gridContainer = new();
            //将其位置修改为绝对
            gridContainer.style.position = Position.Absolute;
            //设置宽高
            gridContainer.style.width = 2560;
            gridContainer.style.height = 1440;
            //绘制网格
            gridContainer.onGUIHandler = DrawGrid;
            //添加到面板中去
            Add(gridContainer);

            //监听事件
            RegisterCallback<WheelEvent>(OnMouseWheel);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            //_connectionEditorPanel.RegisterCallback<KeyDownEvent>(ConnectionEditor_OnKeyDown);
        }
        //绘制面板网格
        private void DrawGrid()
        {
            //网格间隔
            float gridSpacing = 20f;
            //网格透明度
            float gridOpacity = 0.2f;
            //网格颜色
            Color gridColor = Color.gray;

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
        //鼠标拖放事件
        private void OnMouseDown(MouseDownEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //当鼠标左键按下时
                //记录拖动起始位置
                _dragStart = evt.localMousePosition;
                //状态更改为拖放中
                _isDragging = true;
            }
        }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            //检测拖放状态
            if (_isDragging)
            {
                //若正在拖放
                //获取位置差值
                Vector2 delta = evt.localMousePosition - _dragStart;
                //变更位置
                transform.position += (Vector3)delta;
            }
        }
        private void OnMouseUp(MouseUpEvent evt)
        {
            //检测按键
            if (evt.button == 0)
            {
                //当左键抬起时
                //取消拖拽状态
                _isDragging = false;
            }
        }
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            //停止拖拽
            _isDragging = false;
        }
        //快捷键事件，聚焦功能
        //private void ConnectionEditor_OnKeyDown(KeyDownEvent evt)
        //{
        //    //检测按键
        //    if (evt.keyCode == KeyCode.Home)
        //    {
        //        //当Home键被按下时
        //        //获取当基础面板中心位置
        //        Vector3 centerPosition = new(_rightPanel.contentRect.width / 2, _rightPanel.contentRect.height / 2, 0);
        //        //设置中心位置
        //        _connectionEditorPanel.transform.position = centerPosition - new Vector3(_connectionEditorPanel.contentRect.width / 2, _connectionEditorPanel.contentRect.height / 2, 0);
        //        //更改缩放
        //        _connectionEditorPanel.transform.scale = Vector3.one;
        //    }
        //}
        #endregion
    }
}

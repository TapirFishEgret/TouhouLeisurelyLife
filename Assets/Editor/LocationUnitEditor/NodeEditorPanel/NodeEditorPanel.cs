using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering.LookDev;

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

        //节点连接
        private NodeConnectionLine _currnetNodeLine;
        public NodeConnectionLine CurrentNodeLine { get => _currnetNodeLine; private set => _currnetNodeLine = value; }
        private VisualElement _currentStartElement;
        public VisualElement CurrentStartElement { get => _currentStartElement; private set => _currentStartElement = value; }
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
            RegisterCallback<MouseDownEvent>(OnCreateConnection);
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
                DragStart = evt.localMousePosition;
                //状态更改为拖放中
                IsDragging = true;
            }
        }
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
            //检测是否在连线
            else if (CurrentNodeLine != null)
            {
                //若是，实时更新线条
                CurrentNodeLine.UpdateEndPosition(evt.localMousePosition);
            }
        }
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
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            //停止拖拽
            IsDragging = false;
        }
        //连线
        //响应连线事件
        private void OnCreateConnection(MouseDownEvent evt)
        {
            //检测按键
            if (evt.button == 1)
            {
                //当为右键时，检测当前是否已在生成线条
                if (CurrentNodeLine != null)
                {
                    //若是，检测目标对象是否为另一个节点元素
                    if (evt.target is VisualElement targetElement && targetElement != this && targetElement != CurrentNodeLine)
                    {
                        //若是，确认线条终点
                        CurrentNodeLine.EndElement = targetElement;
                        //生成线条
                        CurrentNodeLine.UpdateEndPosition(targetElement.worldBound.center);
                        //触发连接完成事件
                        OnConnectionComplete(CurrentStartElement, targetElement);
                        //清空当前数据
                        CurrentNodeLine = null;
                    }
                    else
                    {
                        //若不是，添加中断点
                        CurrentNodeLine.UpdateEndPosition(evt.localMousePosition);
                        //清空当前数据
                        CurrentStartElement = null;
                    }
                }
                else
                {
                    //若尚未开始生成线条
                    if (evt.target is VisualElement startElement && startElement != this)
                    {
                        //获取起始位置
                        CurrentStartElement = startElement;
                        //生成线条
                        CurrentNodeLine = new NodeConnectionLine
                        {
                            StartElement = startElement,
                            StartPoint = startElement.worldBound.center,
                            EndPoint = evt.localMousePosition
                        };
                        //添加到面板中
                        Add(CurrentNodeLine);
                    }
                }
                //阻止事件传递
                evt.StopPropagation();
            }
            else if (evt.button == 0 && CurrentNodeLine != null)
            {
                // 左键单击空白部分取消本次创建
                Remove(CurrentNodeLine);
                CurrentNodeLine = null;
                evt.StopPropagation();
            }
        }
        //当链接完成时
        private void OnConnectionComplete(VisualElement startElement, VisualElement endElement)
        {
            //通知一声
            Debug.Log($"'{startElement.name}'和'{endElement.name}'连接完成");
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

using System.Collections.Generic;
using THLL.LocationSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class Node : VisualElement
    {
        #region 基础构成
        //节点面板
        private readonly NodeEditorPanel _nodeEditorPanel;
        public NodeEditorPanel NodeEditorPanel => _nodeEditorPanel;

        //此节点对应树状图物品
        private readonly TreeViewItemData<LocUnitData> _targetItem;
        public TreeViewItemData<LocUnitData> TargetItem => _targetItem;
        //对应数据
        private readonly LocUnitData _targetData;
        public LocUnitData TargetData => _targetData;
        //缓存
        private readonly Dictionary<int, Node> _nodeDicCache;
        public Dictionary<int, Node>  NodeDicCache => _nodeDicCache;

        //拖拽功能
        private Vector2 _dragStart;
        public Vector2 DragStart { get => _dragStart; private set => _dragStart = value; }
        private bool _isDragging = false;
        public bool IsDragging { get => _isDragging; private set => _isDragging = value; }

        //自身线条存储
        private readonly List<NodeLine> _lines = new();
        public List<NodeLine> Lines => _lines;
        #endregion

        //构造函数
        public Node(TreeViewItemData<LocUnitData> targetItem, NodeEditorPanel nodeEditorPanel)
        {
            //赋值
            _targetItem = targetItem;
            _targetData = targetItem.data;
            _nodeEditorPanel = nodeEditorPanel;
            _nodeDicCache = nodeEditorPanel.NodeDicCache;

            //添加自身到缓存中
            NodeDicCache[TargetData.GetAssetHashCode()] = this;

            //初始化
            Init();
        }

        #region 方法
        //初始化
        public void Init()
        {
            //设置节点样式
            style.width = 100;
            style.height = 100;
            style.alignContent = Align.Center;
            style.justifyContent = Justify.Center;
            style.backgroundColor = new StyleColor(ChineseColor.Purple_丁香淡紫);
            style.position = Position.Absolute;

            //添加节点元素
            Label label = new(TargetData.Name)
            {
                style =
                {
                    color = ChineseColor.Brown_凋叶棕,
                    fontSize = 36,
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            Add(label);

            //监听鼠标事件
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        //鼠标按下时
        private void OnMouseDown(MouseDownEvent evt)
        {
            //检测鼠标按键
            if (evt.button == 0)
            {
                //若为左键，记录下当前位置，并启用拖动
                DragStart = evt.localMousePosition;
                IsDragging = true;
                //阻断事件传递
                evt.StopPropagation();
            }
            else if (evt.button == 1)
            {
                //若为右键
                //获取父级
                if (parent is NodeEditorPanel nodeEditorPanel)
                {
                    //检查有无连线在进行
                    if (nodeEditorPanel.CurrentLine == null)
                    {
                        //若无，新增连接
                        StartLine();
                    }
                    else
                    {
                        //若有，结束连接
                        EndLine();

                        //清空当前数据
                        NodeEditorPanel.Remove(NodeEditorPanel.CurrentLine);
                        NodeEditorPanel.CurrentLine = null;
                        NodeEditorPanel.CurrentStartNode = null;
                    }
                    //阻断事件传递
                    evt.StopPropagation();
                }
            }
        }
        //鼠标移动时
        private void OnMouseMove(MouseMoveEvent evt)
        {
            //检测拖放状态
            if (_isDragging)
            {
                //若在拖放状态，获取移动值
                Vector2 delta = evt.localMousePosition - DragStart;
                //对新值进行计算与限制，防止拖出面板外
                float newLeft = Mathf.Clamp(resolvedStyle.left + delta.x, 0, parent.resolvedStyle.width - resolvedStyle.width);
                float newTop = Mathf.Clamp(resolvedStyle.top + delta.y, 0, parent.resolvedStyle.height - resolvedStyle.height);
                //赋值
                style.left = newLeft;
                style.top = newTop;

                //并更新所有线条的位置
                foreach (NodeLine line in Lines)
                {
                    line.UpdateLine();
                }
            }
        }
        //鼠标放开时
        private void OnMouseUp(MouseUpEvent evt)
        {
            //检测鼠标按键
            if (evt.button == 0)
            {
                //若为左键，结束拖放
                IsDragging = false;
            }
        }
        //鼠标离开元素时
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            //当鼠标离开时
            IsDragging = false;
        }
        //开始连接
        public void StartLine()
        {
            //创建连接
            NodeLine line = new(this, null);
            //设定父级面板
            NodeEditorPanel.CurrentStartNode = this;
            NodeEditorPanel.CurrentLine = line;
            //向父级面板添加线条
            NodeEditorPanel.Add(line);
        }
        //结束连接
        public void EndLine()
        {
            //创建连接
            NodeLine line = new(NodeEditorPanel.CurrentStartNode, this);
            //添加到存储中
            Lines.Add(line);
            NodeEditorPanel.CurrentStartNode.Lines.Add(line);
            //向父级面板添加线条，此处线条为固定了的线条
            NodeEditorPanel.Add(line);
            //线条位置更新
            line.UpdateLine();
            //强制刷新
            line.UpdateIntegerFieldPositon();
        }
        #endregion
    }
}

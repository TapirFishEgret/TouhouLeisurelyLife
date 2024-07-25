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
        public NodeEditorPanel NodeEditorPanel { get; private set; }

        //此节点对应树状图物品
        public TreeViewItemData<LocUnitData> TargetItem { get; private set; }
        //对应数据
        public LocUnitData TargetData { get; private set; }
        //缓存
        public Dictionary<int, Node>  NodeDicCache { get; private set; }

        //拖拽功能
        private Vector2 _dragStart;
        public Vector2 DragStart { get => _dragStart; private set => _dragStart = value; }
        private bool _isDragging = false;
        public bool IsDragging { get => _isDragging; private set => _isDragging = value; }

        //自身线条存储
        public List<NodeLine> Lines { get; private set; }
        #endregion

        //构造函数
        public Node(TreeViewItemData<LocUnitData> targetItem, NodeEditorPanel nodeEditorPanel)
        {
            //赋值
            TargetItem = targetItem;
            TargetData = targetItem.data;
            NodeEditorPanel = nodeEditorPanel;
            NodeDicCache = nodeEditorPanel.NodeDicCache;

            //构造
            Lines = new();

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
                //设置样式
                style =
                {
                    color = ChineseColor.Brown_凋叶棕,
                    fontSize = 36,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
                //不响应鼠标事件
                pickingMode = PickingMode.Ignore,
            };
            Add(label);

            //监听鼠标事件
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }
        //鼠标按下时
        private void OnPointerDown(PointerDownEvent evt)
        {
            //检测鼠标按键
            if (evt.button == 0)
            {
                //若为左键，记录下当前位置，并启用拖动
                DragStart = evt.localPosition;
                IsDragging = true;
                //捕获指针，以防止指针移动过快脱出位置导致拖拽停止
                this.CapturePointer(evt.pointerId);
                //阻断事件传递
                evt.StopPropagation();
            }
        }
        //鼠标移动时
        private void OnPointerMove(PointerMoveEvent evt)
        {
            //检测拖放状态
            if (_isDragging)
            {
                //若在拖放状态，获取移动值
                Vector2 delta = (Vector2)evt.localPosition - DragStart;
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
        private void OnPointerUp(PointerUpEvent evt)
        {
            //检测鼠标按键
            if (evt.button == 0)
            {
                //若为左键，结束拖放
                IsDragging = false;
                //释放鼠标指针
                this.ReleasePointer(evt.pointerId);
            }
        }
        //重设自身位置
        public void ResetPosition(float scaleX, float scaleY)
        {
            //设置X轴位置
            style.left = resolvedStyle.left * scaleX;
            //设置Y轴位置
            style.top = resolvedStyle.top * scaleY;
            //刷新线条
            foreach (NodeLine line in Lines)
            {
                line.UpdateLine();
            }
        }
        #endregion
    }
}

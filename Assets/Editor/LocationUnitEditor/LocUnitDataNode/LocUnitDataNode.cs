using System;
using System.Collections.Generic;
using THLL.LocationSystem;
using UnityEngine.UIElements;
using UnityEngine;

namespace THLL.GameEditor
{
    public class LocUnitDataNode : VisualElement
    {
        //自身数据
        //此节点对应树状图物品
        private readonly TreeViewItemData<LocUnitData> _targetItem;
        //对应数据
        private readonly LocUnitData _targetData;
        //缓存
        private readonly Dictionary<int, LocUnitDataNode> _nodeDicCache;

        //拖拽功能
        private Vector2 _dragStart;
        private bool _isDragging = false;

        //构造函数
        public LocUnitDataNode(TreeViewItemData<LocUnitData> targetItem, Dictionary<int, LocUnitDataNode> nodeDicCache)
        {
            //赋值
            _targetItem = targetItem;
            _targetData = targetItem.data;
            _nodeDicCache = nodeDicCache;

            //添加自身到缓存中
            _nodeDicCache[_targetData.GetAssetHashCode()] = this;

            //初始化
            Init();
        }

        //初始化
        public void Init()
        {
            //设置节点样式
            style.width = 100;
            style.height = 100;
            style.alignContent = Align.Center;
            style.backgroundColor = new StyleColor(ChineseColor.Purple_丁香淡紫);
            style.position = Position.Absolute;

            //添加节点元素
            Label label = new(_targetData.Name);
            label.style.color = ChineseColor.Brown_凋叶棕;
            label.style.fontSize = 36;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            Add(label);
            
            //监听鼠标事件
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        //拖动功能
        private void OnMouseDown(MouseDownEvent evt)
        {
            //检测鼠标按键
            if (evt.button == 0)
            {
                //若为左键，记录下当前位置，并启用拖动
                _dragStart = evt.localMousePosition;
                _isDragging = true;
                //阻断事件传递
                evt.StopPropagation();
            }
        }
        private void OnMouseMove(MouseMoveEvent evt)
        {
            //检测拖放状态
            if (_isDragging)
            {
                //若在拖放状态，获取移动值并赋值
                Vector2 delta = evt.localMousePosition - _dragStart;
                style.left = resolvedStyle.left + delta.x;
                style.top = resolvedStyle.top + delta.y;
            }
        }
        private void OnMouseUp(MouseUpEvent evt)
        {
            //检测鼠标按键
            if (evt.button == 0)
            {
                //若为左键，结束拖放
                _isDragging = false;
            }
        }
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            //当鼠标离开时
            _isDragging = false;
        }
    }
}

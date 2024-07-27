using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeLine : VisualElement
    {
        #region 自身构成
        //节点面板
        public NodeEditorPanel NodeEditorPanel { get; private set; }

        //线条ID
        public int ID { get { return StartNode.GetHashCode() ^ EndNode.GetHashCode(); } }

        //起始与结束，点
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }

        //线条颜色
        public Color LineColor { get; private set; }

        //整形输入框
        public IntegerField IntegerField { get; private set; }
        #endregion

        //构造函数
        public NodeLine(NodeEditorPanel nodeEditorPanel)
        {
            //设置节点面板
            NodeEditorPanel = nodeEditorPanel;

            //设置线条颜色为随机颜色
            LineColor = new(Random.value, Random.value, Random.value, 1.0f);

            //设置位置为绝对位置
            style.position = Position.Absolute;

            //创建整形面板
            IntegerField = new IntegerField()
            {
                //设置名称与延迟响应
                label = "通行时间",
                isDelayed = true
            };
            //设置标签大小为可变
            IntegerField.Q<Label>().style.minWidth = new StyleLength(StyleKeyword.Auto);
            //注册输入结束后事件
            IntegerField.RegisterValueChangedCallback(OnIntegerFieldValueChanged);

            //注册生成面板事件以绘制曲线
            generateVisualContent += DrawBezierCurve;
        }

        #region 线条重绘
        //更新线条
        public void UpdateLine()
        {
            //标记面板为脏以进行重绘
            MarkDirtyRepaint();
        }
        //重绘线条
        private void DrawBezierCurve(MeshGenerationContext mgc)
        {
            //起始、终结、点
            //起始，直接获取起始节点的中点
            Vector3 startPos = StartNode.localBound.center;
            //终结，如果有终点那么获取终点，如果没有那么获取鼠标位置
            Vector3 endPos = (EndNode != null) ? EndNode.localBound.center : (Vector3)parent.WorldToLocal(Event.current.mousePosition);

            //更新IntegerField的位置
            IntegerField.style.left = ((startPos.x + endPos.x) / 2) - (IntegerField.resolvedStyle.width / 2);
            IntegerField.style.top = ((startPos.y + endPos.y) / 2) - (IntegerField.resolvedStyle.height / 2);

            //曲线、起始、终结、点
            Vector2 bezierStartPoint = new(startPos.x, startPos.y);
            Vector2 bezierEndPoint = new(endPos.x, endPos.y);

            //贝塞尔曲线控制、起始、终结、点
            Vector2 bezierStartTangent = bezierStartPoint + (Vector2.right * 50);
            Vector2 bezierEndTangent = bezierEndPoint + (Vector2.left * 50);

            // 使用Painter2D绘制贝塞尔曲线
            var painter = mgc.painter2D;
            painter.lineWidth = 2.0f;
            painter.strokeColor = LineColor;

            painter.BeginPath();
            painter.MoveTo(bezierStartPoint);
            painter.BezierCurveTo(bezierStartTangent, bezierEndTangent, bezierEndPoint);
            painter.Stroke();
        }
        #endregion

        #region 连线状态相关
        //当连线成功时
        public void OnLineConnected()
        {
            //检测是否已有连接
            if (NodeEditorPanel.NodeLineCache.ContainsKey(ID))
            {
                //若有，直接返回
                Debug.LogWarning("不要添加重复连接");
                return;
            }
            //若无，添加
            NodeEditorPanel.NodeLineCache[ID] = this;
            //生成为双方生成连接
            StartNode.TargetData.Editor_AddConnection(EndNode.TargetData, 0);
            EndNode.TargetData.Editor_AddConnection(StartNode.TargetData, 0);
            //并放置整形输入框
            Add(IntegerField);
            //将连接加入双方记录
            StartNode.NodeLines.Add(this);
            EndNode.NodeLines.Add(this);
            //并放入节点编辑界面的当前线条下
            NodeEditorPanel.ShowedNodeLines[ID] = this;
        }
        //当输入框数值发生更改时
        private void OnIntegerFieldValueChanged(ChangeEvent<int> evt)
        {
            //检测输入值
            if (evt.newValue < 0)
            {
                //若小于0，取消该链接
                StartNode.NodeLines.Remove(this);
                EndNode.NodeLines.Remove(this);
                NodeEditorPanel.NodeView.Remove(this);
                NodeEditorPanel.ShowedNodeLines.Remove(ID);
                NodeEditorPanel.NodeLineCache.Remove(ID);
                //并删除双方连接字典中的对方
                StartNode.TargetData.Editor_RemoveConnection(EndNode.TargetData);
                EndNode.TargetData.Editor_RemoveConnection(StartNode.TargetData);
                //将移除双方记录
                StartNode.NodeLines.Remove(this);
                EndNode.NodeLines.Remove(this);
            }
            else
            {
                //变更通行时间
                StartNode.TargetData.Editor_SetConnDuration(EndNode.TargetData, evt.newValue);
                EndNode.TargetData.Editor_SetConnDuration(StartNode.TargetData, evt.newValue);
                Debug.Log($"{StartNode.TargetData.Name}和{EndNode.TargetData.Name}喜成连理，共入洞房，耗时{evt.newValue}秒");
            }
        }
        #endregion
    }
}

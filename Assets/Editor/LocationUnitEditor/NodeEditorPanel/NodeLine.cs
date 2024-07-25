using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeLine : VisualElement
    {
        #region 自身构成
        //节点面板
        private readonly NodeEditorPanel _nodeEditorPanel;
        public NodeEditorPanel NodeEditorPanel => _nodeEditorPanel;

        //起始与结束，点
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }

        //线条颜色
        public Color LineColor { get; private set; }

        //数字输入框
        public IntegerField IntegerField { get; private set; }
        #endregion

        //构造函数
        public NodeLine(NodeEditorPanel nodeEditorPanel)
        {
            //设置节点面板
            _nodeEditorPanel = nodeEditorPanel;

            //设置线条颜色为随机颜色
            LineColor = new(Random.value, Random.value, Random.value, 1.0f);

            //设置位置为绝对位置
            style.position = Position.Absolute;

            //创建数字输入框
            IntegerField = new IntegerField();
            //注册输入结束后事件
            IntegerField.RegisterValueChangedCallback(OnIntegerFieldValueChanged);
            //添加到线条上
            Add(IntegerField);

            //注册生成面板事件以绘制曲线
            generateVisualContent += DrawBezierCurve;
        }

        #region 方法
        //当输入框数值发生更改时
        private void OnIntegerFieldValueChanged(ChangeEvent<int> evt)
        {
            //检测输入值
            if (evt.newValue < 0)
            {
                //若小于0，取消该链接
                StartNode.Lines.Remove(this);
                EndNode.Lines.Remove(this);
                NodeEditorPanel.Remove(this);
                //输出信息
                Debug.Log("爷免费啦！");
            }
            else
            {
                //先输出一条信息吧
                Debug.Log($"{StartNode.TargetData.Name}和{EndNode.TargetData.Name}喜成连理，共入洞房，耗时{evt.newValue}秒");
            }
        }
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
            IntegerField.style.left = (startPos.x + endPos.x) / 2 - IntegerField.resolvedStyle.width / 2;
            IntegerField.style.top = (startPos.y + endPos.y) / 2 - IntegerField.resolvedStyle.height / 2;

            //曲线、起始、终结、点
            Vector2 bezierStartPoint = new(startPos.x, startPos.y);
            Vector2 bezierEndPoint = new(endPos.x, endPos.y);

            //贝塞尔曲线控制、起始、终结、点
            Vector2 bezierStartTangent = bezierStartPoint + Vector2.right * 50;
            Vector2 bezierEndTangent = bezierEndPoint + Vector2.left * 50;

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
    }
}

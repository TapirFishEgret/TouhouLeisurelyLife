using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeLine : VisualElement
    {
        #region 自身构成
        //起始与结束，点
        private Node _startNode;
        public Node StartNode { get => _startNode; private set => _startNode = value; }
        private Node _endNode;
        public Node EndNode { get => _endNode; private set => _endNode = value; }

        //线条颜色
        private Color _lineColor;
        public Color LineColor { get => _lineColor; private set => _lineColor = value; }

        //数字输入框
        private IntegerField _integerField;
        public IntegerField IntegerField { get => _integerField; private set => _integerField = value; }
        #endregion

        //构造函数
        public NodeLine(Node startNode, Node endNode)
        {
            //设置起始与结束点
            StartNode = startNode;
            EndNode = endNode;

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

            //注册几何形状更改回调
            generateVisualContent += DrawBezierCurve;
        }

        #region 方法
        //当输入框数值发生更改时
        private void OnIntegerFieldValueChanged(ChangeEvent<int> evt)
        {
            //先输出一条信息吧
            Debug.Log("我连接啦！");
        }
        //更新线条
        public void UpdateLine()
        {
            //更新数字输入框位置
            UpdateIntegerFieldPositon();

            //标记面板为脏以进行重绘
            MarkDirtyRepaint();
        }
        //更新数字输入框位置
        public void UpdateIntegerFieldPositon()
        {
            //起始、终结、点
            //起始，直接获取起始节点的中点
            Vector3 startPos = StartNode.localBound.center;
            //终结，如果有终点那么获取终点，如果没有那么获取鼠标位置
            Vector3 endPos = (EndNode != null) ? EndNode.localBound.center : (Vector3)GetMousePosition();

            //更新IntegerField的位置
            IntegerField.style.left = (startPos.x + endPos.x) / 2 - IntegerField.resolvedStyle.width / 2;
            IntegerField.style.top = (startPos.y + endPos.y) / 2 - IntegerField.resolvedStyle.height / 2;
        }
        //获取鼠标位置
        private Vector2 GetMousePosition()
        {
            //获取鼠标位置
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            //返回相对位置
            return parent.WorldToLocal(mousePos);
        }
        //重绘线条
        private void DrawBezierCurve(MeshGenerationContext mgc)
        {
            //起始、终结、点
            //起始，直接获取起始节点的中点
            Vector3 startPos = StartNode.localBound.center;
            //终结，如果有终点那么获取终点，如果没有那么获取鼠标位置
            Vector3 endPos = (EndNode != null) ? EndNode.localBound.center : (Vector3)GetMousePosition();

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

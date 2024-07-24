using System.Configuration;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class NodeConnectionLine : VisualElement
    {
        //基础信息
        //起始
        private VisualElement _startElement;
        public VisualElement StartElement { get { return _startElement; } set { _startElement = value; } }
        private VisualElement _endElement;
        public VisualElement EndElement { get { return _endElement; } set { _endElement = value; } }
        private Vector2 _startPoint;
        public Vector2 StartPoint { get { return _startPoint; } set { _startPoint = value; } }
        private Vector2 _endPoint;
        public Vector2 EndPoint { get { return _endPoint; } set { _endPoint = value; } }
        private Color _lineColor;
        public Color LineColor { get { return _lineColor; } set { _lineColor = value; } }

        //构造函数
        public NodeConnectionLine()
        {
            //设置默认样式
            style.position = Position.Absolute;
            LineColor = new Color(Random.value, Random.value, Random.value);

            //注册绘制事件
            RegisterCallback<GeometryChangedEvent>(evt => MarkDirtyRepaint());
            generateVisualContent += OnGenerateVisualContent;
        }

        //绘制事件
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            //创建网格绘制线条
            var painter = ctx.painter2D;
            painter.strokeColor = LineColor;
            painter.lineWidth = 2;

            //判断有无起止元素
            if (StartElement != null)
            {
                StartPoint = StartElement.worldBound.center;
            }
            if (EndElement != null)
            {
                EndPoint = EndElement.worldBound.center;
            }

            //开始绘制
            painter.BeginPath();
            painter.MoveTo(StartPoint);
            painter.LineTo(EndPoint);
            painter.Stroke();
        }
        //更新线条
        public void UpdateEndPosition(Vector2 newEndPoint)
        {
            //设定结束点
            EndPoint = newEndPoint;
            //重绘面板
            MarkDirtyRepaint();
        }
    }
}

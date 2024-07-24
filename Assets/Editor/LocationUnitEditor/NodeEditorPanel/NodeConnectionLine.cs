using UnityEditor;
using UnityEngine;
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
        public Color Color { get { return _lineColor; } set { _lineColor = value; } }

        //构造函数
        public NodeConnectionLine()
        {

        }
    }
}

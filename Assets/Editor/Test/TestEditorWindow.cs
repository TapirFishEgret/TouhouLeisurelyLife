using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor
{
    public class TestEditorWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTree;

        private ScrollView _scv;

        [MenuItem("GameEditor/Test")]
        public static void OpenWindow()
        {
            //窗口设置
            TestEditorWindow window = GetWindow<TestEditorWindow>("Test Editor Window");
            window.position = new Rect(100, 100, 1440, 810);
        }

        public void CreateGUI()
        {
            //加载UXML文件
            _visualTree.CloneTree(rootVisualElement);

            _scv = rootVisualElement.Q<ScrollView>("SCV");

            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //若是，则触发更改
            _scv.style.width = evt.newRect.width;
            _scv.style.height = evt.newRect.height;
        }
    }
}

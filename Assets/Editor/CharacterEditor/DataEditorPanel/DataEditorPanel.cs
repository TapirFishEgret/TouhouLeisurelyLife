using UnityEngine.UIElements;

namespace THLL.GameEditor.CharacterEditor
{
    public class DataEditorPanel : Tab
    {
        #region 基础构成
        //主面板
        public MainWindow MainWindow { get; private set; }
        #endregion

        #region 构造及刷新方法
        public DataEditorPanel(VisualTreeAsset visualTree, MainWindow window) 
        {
            //获取面板
            VisualElement panel = visualTree.CloneTree();
            Add(panel);

            //指定主窗口
            MainWindow = window;
        }
        #endregion
    }
}

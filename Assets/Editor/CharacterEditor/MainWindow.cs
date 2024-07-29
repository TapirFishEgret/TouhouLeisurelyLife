using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using THLL.CharacterSystem;
using System.IO;

namespace THLL.GameEditor.CharacterEditor
{
    public class MainWindow : EditorWindow
    {
        #region 基础构成
        //UI文档
        [SerializeField]
        private VisualTreeAsset _windowVisualTree;
        public VisualTreeAsset WindowVisualTree => _windowVisualTree;
        //数据编辑面板UI文档
        [SerializeField]
        private VisualTreeAsset _dataEditorVisualTree;
        public VisualTreeAsset DataEditorVisualTree => _dataEditorVisualTree;
        //永久性存储数据
        [SerializeField]
        private TextAsset _persistentData;
        public TextAsset PersistentData => _persistentData;
        //默认角色头像
        [SerializeField]
        private Sprite _defaultCharacterAvatar;
        public Sprite DefaultCharacterAvatar => _defaultCharacterAvatar;
        //默认角色立绘
        [SerializeField]
        private Sprite _defaultCharacterPortrait;
        public Sprite DefaultCharacterPortrait => _defaultCharacterPortrait;

        //左侧面板
        //树形图窗口
        public DataTreeView DataTreeView { get; private set; }
        //包输入框
        public TextField DefaultPackageField { get; private set; }
        //作者输入框
        public TextField DefaultAuthorField { get; private set; }
        //计时器Debug面板显示开关
        public Toggle TimerDebugLogToggle { get; private set; }
        //右侧面板
        //多标签页窗口
        public TabView MultiTabView { get; private set; }
        //数据编辑窗口
        public VisualElement DataEditorPanel { get; private set; }

        //窗口菜单
        [MenuItem("GameEditor/CharacterDataEditor")]
        public static void OpenWindow()
        {
            MainWindow mainWindow = GetWindow<MainWindow>("Character Editor Window");
            mainWindow.position = new Rect(100, 100, 1440, 810);
        }
        #endregion

        #region UI周期函数
        //窗口生成时
        public void CreateGUI()
        {
            //加载UXML文件
            _windowVisualTree.CloneTree(rootVisualElement);

            //获取UI控件
            DefaultPackageField = rootVisualElement.Q<TextField>("DefaultPackageField");
            DefaultAuthorField = rootVisualElement.Q<TextField>("DefaultAuthorField");
            TimerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            VisualElement dataTreeViewContainer = rootVisualElement.Q<VisualElement>("DataTreeViewContainer");
            MultiTabView = rootVisualElement.Q<TabView>("MultiTabView");

            //生成UI控件
            //左侧面板
            //创建树形图面板并添加
            DataTreeView = new DataTreeView(this);
            dataTreeViewContainer.Add(DataTreeView);
            //右侧面板
            //创建数据编辑面板并添加
            DataEditorPanel = new DataEditorPanel(_dataEditorVisualTree, this);
            MultiTabView.Add(DataEditorPanel);
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //提醒修改可寻址资源包标签
            Debug.LogWarning("窗口已被关闭，请注意修改新增数据的可寻址资源包的Key");
        }
        #endregion
    }
}

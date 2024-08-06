using System.Collections.Generic;
using System.IO;
using System.Linq;
using THLL.LocationSystem;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class MainWindow : EditorWindow
    {
        #region 基础构成
        //自身UXML文件
        [SerializeField]
        private VisualTreeAsset _windowVisualTree;
        //数据编辑面板UXML文件
        [SerializeField]
        private VisualTreeAsset _dataEditorVisualTree;
        //永久性存储文件
        [SerializeField]
        private TextAsset _persistentDataFile;
        public TextAsset PersistentDataFile => _persistentDataFile;
        //默认地点背景图
        [SerializeField]
        private Sprite _defaultLocationBackground;
        public Sprite DefaultLocationBackground => _defaultLocationBackground;

        //UI元素
        //左侧面板
        //树形图
        public DataTreeView DataTreeView { get; private set; }
        //包输入框
        public TextField DefaultPackageField { get; private set; }
        //作者输入框
        public TextField DefaultAuthorField { get; private set; }
        //计时器Debug面板显示开关
        public Toggle TimerDebugLogToggle { get; private set; }
        //右侧面板
        //多标签页面板
        public TabView MultiTabView { get; private set; }
        //数据编辑面板
        public DataEditorPanel DataEditorPanel { get; private set; }
        //连接编辑面板
        public NodeEditorPanel NodeEditorPanel { get; private set; }

        //窗口菜单
        [MenuItem("GameEditor/LocationDataEditor")]
        public static void ShowWindow()
        {
            //窗口设置
            MainWindow window = GetWindow<MainWindow>("Location Unit Editor Window");
            window.position = new Rect(100, 100, 1440, 810);
        }
        #endregion

        #region UI生命周期
        //创建UI
        public void CreateGUI()
        {
            //加载UXML文件
            _windowVisualTree.CloneTree(rootVisualElement);

            //其他控件
            //获取
            DefaultPackageField = rootVisualElement.Q<TextField>("DefaultPackageField");
            DefaultAuthorField = rootVisualElement.Q<TextField>("DefaultAuthorField");
            TimerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            VisualElement dataTreeViewContainer = rootVisualElement.Q<VisualElement>("DataTreeViewContainer");
            MultiTabView = rootVisualElement.Q<TabView>("EditorContainer");

            //设置标签页面容器为可延展
            MultiTabView.contentContainer.style.flexGrow = 1;
            MultiTabView.contentContainer.style.flexShrink = 1;

            //读取持久化数据
            LoadPersistentData();

            //左侧面板
            //创建树形图面板并添加
            DataTreeView = new DataTreeView(this);
            dataTreeViewContainer.Add(DataTreeView);

            //右侧面板
            //创建数据编辑面板并添加
            DataEditorPanel = new DataEditorPanel(_dataEditorVisualTree, this);
            MultiTabView.Add(DataEditorPanel);
            //创建连接面板并添加
            NodeEditorPanel = new NodeEditorPanel(this);
            MultiTabView.Add(NodeEditorPanel);
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //保存持久化数据到磁盘
            SavePersistentData();
            //提醒修改可寻址资源包标签
            Debug.LogWarning("窗口已被关闭，请注意修改新增数据的可寻址资源包的Key。");
        }
        #endregion

        #region 文件存储与读取方法
        //保存缓存到永久性存储文件
        public void SavePersistentData()
        {
            //生成永久性实例
            PersistentData persistentData = new()
            {
                //设置其数值
                DefaultPackage = DefaultPackageField.text,
                DefaultAuthor = DefaultAuthorField.text,
                TimerDebugLogState = TimerDebugLogToggle.value,
                ExpandedState = DataTreeView.ExpandedStateCache.ToList(),
                NodePositions = new()
            };
            //生成节点位置数据
            foreach (Node node in NodeEditorPanel.NodeDicCache.Values)
            {
                //存入
                persistentData.NodePositions[node.TargetData.GetAssetHashCode()] = (node.style.left.value.value, node.style.top.value.value);
            }
            //将永久性存储实例转化为文本
            string jsonString = JsonConvert.SerializeObject(persistentData, Formatting.Indented);
            //写入文件中
            File.WriteAllText(AssetDatabase.GetAssetPath(PersistentDataFile), jsonString);
            //标记为脏
            EditorUtility.SetDirty(PersistentDataFile);
            //保存更改
            AssetDatabase.SaveAssets();
            //刷新数据
            AssetDatabase.Refresh();
        }
        //读取永久性存储文件到缓存
        private void LoadPersistentData()
        {
            //读取文件中数据
            string jsonString = File.ReadAllText(AssetDatabase.GetAssetPath(PersistentDataFile));
            //生成永久性存储实例
            PersistentData persistentData = JsonConvert.DeserializeObject<PersistentData>(jsonString);
            //分配数据
            //属性
            DefaultPackageField.SetValueWithoutNotify(persistentData.DefaultPackage);
            DefaultAuthorField.SetValueWithoutNotify(persistentData.DefaultAuthor);
            TimerDebugLogToggle.SetValueWithoutNotify(persistentData.TimerDebugLogState);
        }
        #endregion
    }
}


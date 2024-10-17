using Newtonsoft.Json;
using System.IO;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
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
        //资源编辑面板UXML文件
        [SerializeField]
        private VisualTreeAsset _assetsEditorVisualTree;
        //地图编辑面板UXML文件
        [SerializeField]
        private VisualTreeAsset _mapEditorVisualTree;
        //背景图容器UXML文件
        public VisualTreeAsset BackgroundAssetContainerVisualTree;
        //永久性存储文件
        [SerializeField]
        private TextAsset _persistentDataFile;
        public TextAsset PersistentDataFile => _persistentDataFile;
        //默认地点背景图
        [SerializeField]
        private Sprite _defaultLocationBackground;
        public Sprite DefaultLocationBackground => _defaultLocationBackground;

        //UI元素
        //树形图面板
        //树形图容纳容器
        public VisualElement DataTreeViewContainer { get; private set; }
        //树形图
        public DataTreeView DataTreeView { get; private set; }
        //计时器Debug面板显示开关
        public Toggle TimerDebugLogToggle { get; private set; }
        //编辑器面板
        //编辑器选择器
        public ToggleButtonGroup EditorSelector { get; private set; }
        //编辑器容器
        public VisualElement EditorContainer { get; private set; }
        //数据编辑面板
        public DataEditorPanel DataEditorPanel { get; private set; }
        //资源编辑面板
        public AssetsEditorPanel AssetsEditorPanel { get; private set; }
        //地图编辑面板
        public MapEditorPanel MapEditorPanel { get; private set; }

        //窗口菜单
        [MenuItem("EditorSystem/SceneEditor")]
        public static void ShowWindow()
        {
            //窗口设置
            MainWindow window = GetWindow<MainWindow>("Scene Editor Window");
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
            TimerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            DataTreeViewContainer = rootVisualElement.Q<VisualElement>("DataTreeViewContainer");
            EditorSelector = rootVisualElement.Q<ToggleButtonGroup>();
            EditorContainer = rootVisualElement.Q<VisualElement>("EditorContainer");

            //读取持久化数据
            LoadPersistentData();

            //树形图面板
            //创建树形图面板并添加
            DataTreeView = new DataTreeView(this);
            DataTreeViewContainer.Add(DataTreeView);
            //编辑面板
            //创建数据编辑面板并添加
            DataEditorPanel = new DataEditorPanel(_dataEditorVisualTree, this);
            EditorContainer.Add(DataEditorPanel);
            //创建资源编辑面板并添加
            AssetsEditorPanel = new AssetsEditorPanel(_assetsEditorVisualTree, this);
            EditorContainer.Add(AssetsEditorPanel);
            //创建地图编辑面板并添加
            MapEditorPanel = new MapEditorPanel(_mapEditorVisualTree, this);
            EditorContainer.Add(MapEditorPanel);

            //设定编辑器选择逻辑
            EditorSelector.RegisterValueChangedCallback(evt =>
            {
                //关闭所有面板
                EditorContainer.Children().ToList().ForEach(visualElement => visualElement.style.display = DisplayStyle.None);
                //根据选择开启面板，0为数据编辑面板，1为资源编辑面板，2为地图编辑面板
                if (evt.newValue[0])
                {
                    DataEditorPanel.style.display = DisplayStyle.Flex;
                }
                if (evt.newValue[1])
                {
                    AssetsEditorPanel.style.display = DisplayStyle.Flex;
                }
                if (evt.newValue[2])
                {
                    MapEditorPanel.style.display = DisplayStyle.Flex;
                }
            });
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //保存持久化数据到磁盘
            SavePersistentData();
            //对数据进行处理，扔到静态类中的同时进行保存
            foreach (var item in DataTreeView.ItemDicCache.Values)
            {
                //向静态类中添加数据
                GameEditor.SceneDataDict[item.data.Data.ID] = item.data.Data;
                //保存数据
                SceneData.SaveToJson(item.data.Data, item.data.Data.JsonFileSavePath);
            }
            //保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
                TimerDebugLogState = TimerDebugLogToggle.value,
                ExpandedState = DataTreeView.ExpandedStateCache.ToList(),
            };
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
            TimerDebugLogToggle.SetValueWithoutNotify(persistentData.TimerDebugLogState);
        }
        #endregion
    }
}


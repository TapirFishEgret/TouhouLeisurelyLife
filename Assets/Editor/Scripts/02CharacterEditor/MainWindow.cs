﻿using Newtonsoft.Json;
using System.IO;
using System.Linq;
using THLL.CharacterSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.CharacterEditor
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
        //资源编辑面板UI文档
        [SerializeField]
        private VisualTreeAsset _assetsEditorVisualTree;
        //Sprite显示模板
        public VisualTreeAsset SpriteVisualElementTemplate;
        //永久性存储数据
        [SerializeField]
        private TextAsset _persistentDataFile;
        public TextAsset PersistentDataFile => _persistentDataFile;
        //默认角色头像
        [SerializeField]
        private Sprite _defaultCharacterAvatar;
        public Sprite DefaultCharacterAvatar => _defaultCharacterAvatar;
        //默认角色立绘
        [SerializeField]
        private Sprite _defaultCharacterPortrait;
        public Sprite DefaultCharacterPortrait => _defaultCharacterPortrait;

        //树形图面板
        //树形图容纳容器
        public VisualElement DataTreeViewContainer { get; private set; }
        //树形图窗口
        public DataTreeView DataTreeView { get; private set; }
        //计时器Debug面板显示开关
        public Toggle TimerDebugLogToggle { get; private set; }
        //编辑器面板
        //编辑器选择器
        public ToggleButtonGroup EditorSelector { get; private set; }
        //编辑器容纳容器
        public VisualElement EditorContainer { get; private set; }
        //数据编辑窗口
        public DataEditorPanel DataEditorPanel { get; private set; }
        //资源编辑面板
        public AssetsEditorPanel AssetsEditorPanel { get; private set; }

        //窗口菜单
        [MenuItem("EditorSystem/CharacterEditor")]
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
            TimerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            DataTreeViewContainer = rootVisualElement.Q<VisualElement>("DataTreeViewContainer");
            EditorSelector = rootVisualElement.Q<ToggleButtonGroup>();
            EditorContainer = rootVisualElement.Q<VisualElement>("EditorContainer");

            //读取永久性存储文件
            LoadPersistentData();

            //生成UI控件
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

            //设定编辑器选择器逻辑
            EditorSelector.RegisterValueChangedCallback(evt =>
            {
                //首先关闭其他面板
                EditorContainer.Children().ToList().ForEach(visualElement => visualElement.style.display = DisplayStyle.None);
                //根据选择开启面板，0为数据编辑面板，1为资源编辑面板
                if (evt.newValue[0])
                {
                    DataEditorPanel.style.display = DisplayStyle.Flex;
                }
                if (evt.newValue[1])
                {
                    AssetsEditorPanel.style.display = DisplayStyle.Flex;
                }
            });
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //保存
            SavePersistentData();
            //对数据进行保存
            foreach (var item in DataTreeView.ItemDicCache.Values)
            {
                if (item.data.Type == DataContainer.ItemDataType.Version)
                {
                    CharacterData.SaveToJson(item.data.Data, item.data.Data.DataPath.Replace("\\", "/"));
                }
            }
            //保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region 数据的保存
        //保存缓存到永久性存储文件
        private void SavePersistentData()
        {
            //生成永久性实例
            PersistentData persistentData = new()
            {
                //设置其数值
                TimerDebugLogState = TimerDebugLogToggle.value,
                ExpandedState = DataTreeView.ExpandedStatePersistentData.ToList(),
            };
            //将永久性存储实例转化为文本
            string jsonString = JsonConvert.SerializeObject(persistentData, Formatting.Indented);
            //写入文件中
            File.WriteAllText(AssetDatabase.GetAssetPath(PersistentDataFile), jsonString);
            //标记为脏
            EditorUtility.SetDirty(PersistentDataFile);

            //保存更改
            AssetDatabase.SaveAssets();
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

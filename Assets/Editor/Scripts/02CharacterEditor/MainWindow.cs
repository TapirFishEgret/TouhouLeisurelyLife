using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
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
        //永久性存储数据
        [SerializeField]
        private TextAsset _persistentDataFile;
        public TextAsset PersistentDataFile => _persistentDataFile;
        //可寻址资源包
        [SerializeField]
        private AddressableAssetGroup _assetGroup;
        public AddressableAssetGroup AssetGroup => _assetGroup;
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
        //多标签页窗口
        public TabView MultiTabView { get; private set; }
        //数据编辑窗口
        public DataEditorPanel DataEditorPanel { get; private set; }

        //窗口菜单
        [MenuItem("EditorSystem/CharacterEditor")]
        public static void OpenWindow()
        {
            MainWindow mainWindow = GetWindow<MainWindow>("CharacterEditor Editor Window");
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
            MultiTabView = rootVisualElement.Q<TabView>("MultiTabView");

            //设置标签页面容器为可延展
            MultiTabView.contentContainer.style.flexGrow = 1;
            MultiTabView.contentContainer.style.flexShrink = 1;

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
            MultiTabView.Add(DataEditorPanel);
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //保存
            SavePersistentData();
            //将数据转移到静态类中
            foreach (var item in DataTreeView.ItemDicCache.Values)
            {
                if (item.data.Type == CharacterSystemDataContainer.ItemDataType.Version)
                {
                    GameEditor.CharacterDataDict[item.data.Data.ID] = item.data.Data;
                }
            }
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

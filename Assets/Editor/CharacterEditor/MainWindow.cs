using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
        //树形图窗口
        public DataTreeView DataTreeView { get; private set; }
        //计时器Debug面板显示开关
        public Toggle TimerDebugLogToggle { get; private set; }
        //编辑器面板
        //多标签页窗口
        public TabView MultiTabView { get; private set; }
        //数据编辑窗口
        public DataEditorPanel DataEditorPanel { get; private set; }
        //可寻址资源包面板
        //包名输入框
        public TextField PackageField { get; private set; }
        //作者输入框
        public TextField AuthorField { get; private set; }
        //描述输入框
        public TextField GroupDescriptionField { get; private set; }
        //获取资源组按钮
        public Button GetAddressableAssetGroupButton { get; private set; }
        //获取的资源组
        public ObjectField AddressableAssetGroupField { get; private set; }

        //数据
        //当前可寻址资源组
        public AddressableAssetGroup CurrentAddressableAssetGroup { get; private set; }

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
            PackageField = rootVisualElement.Q<TextField>("PackageField");
            AuthorField = rootVisualElement.Q<TextField>("AuthorField");
            TimerDebugLogToggle = rootVisualElement.Q<Toggle>("TimerDebugLogToggle");
            VisualElement dataTreeViewContainer = rootVisualElement.Q<VisualElement>("DataTreeViewContainer");
            MultiTabView = rootVisualElement.Q<TabView>("MultiTabView");
            GetAddressableAssetGroupButton = rootVisualElement.Q<Button>("GetAddressableAssetGroupButton");
            AddressableAssetGroupField = rootVisualElement.Q<ObjectField>("AddressableAssetGroupField");
            GroupDescriptionField = rootVisualElement.Q<TextField>("GroupDescriptionField");
            //绑定
            GetAddressableAssetGroupButton.clicked += SetAddressableAssetGroup;

            //设置标签页面容器为可延展
            MultiTabView.contentContainer.style.flexGrow = 1;
            MultiTabView.contentContainer.style.flexShrink = 1;

            //读取永久性存储文件
            LoadPersistentData();

            //生成UI控件
            //树形图面板
            //创建树形图面板并添加
            DataTreeView = new DataTreeView(this);
            dataTreeViewContainer.Add(DataTreeView);
            //编辑面板
            //创建数据编辑面板并添加
            DataEditorPanel = new DataEditorPanel(_dataEditorVisualTree, this);
            MultiTabView.Add(DataEditorPanel);
            //组面板
            SetAddressableAssetGroup();
        }
        //窗口关闭时
        private void OnDestroy()
        {
            //保存
            SavePersistentData();
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
                DefaultPackage = PackageField.text,
                DefaultAuthor = AuthorField.text,
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
            PackageField.SetValueWithoutNotify(persistentData.DefaultPackage);
            AuthorField.SetValueWithoutNotify(persistentData.DefaultAuthor);
            TimerDebugLogToggle.SetValueWithoutNotify(persistentData.TimerDebugLogState);
        }
        #endregion

        #region 可寻址资源包相关
        //设置当前可寻址资源组
        private void SetAddressableAssetGroup()
        {
            //确认输入框中存在内容
            if (string.IsNullOrEmpty(AuthorField.text) || string.IsNullOrEmpty(PackageField.text))
            {
                //若输入框中有空输入，则返回
                Debug.LogWarning("请输入完整信息后获取资源组");
                return;
            }
            //确认开始取得新组之后，首先解绑当前面板数值修改事件
            GroupDescriptionField.UnregisterValueChangedCallback(SetGroupDescription);

            //确认组名
            string groupName = $"Character_{PackageField.text}_{AuthorField.text}";
            //确认构建路径，此处采用可寻址资源包内置路径变量
            string buildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/" + groupName;
            //确认读取路径
            string loadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/" + groupName;
            //获取组
            CurrentAddressableAssetGroup = EditorExtensions.GetAddressableGroup(groupName, buildPath, loadPath);
            //赋值
            AddressableAssetGroupField.SetValueWithoutNotify(CurrentAddressableAssetGroup);

            //结束后再次绑定数值修改事件
            GroupDescriptionField.RegisterValueChangedCallback(SetGroupDescription);
            //并设置值
            GroupDescriptionField.SetValueWithoutNotify(CurrentAddressableAssetGroup.GetSchema<AddressableAssetInfoGroupSchema>().Description);
        }
        //更改组描述
        private void SetGroupDescription(ChangeEvent<string> evt)
        {
            if (CurrentAddressableAssetGroup != null)
            {
                CurrentAddressableAssetGroup.GetSchema<AddressableAssetInfoGroupSchema>().SetDescription(evt.newValue);
            }
        }
        #endregion
    }
}

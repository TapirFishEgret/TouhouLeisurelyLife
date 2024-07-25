using THLL.LocationSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor.LocUnitDataEditor
{
    public class DataEditorPanel : VisualElement
    {
        //UI文档资源
        [SerializeField]
        private VisualTreeAsset _visualTree;

        //主面板
        public MainWindow MainWindow { get; private set; }

        //基础四项
        private TextField _packageField;
        private TextField _authorFiled;
        private ObjectField _parentDataField;
        //全名
        private Label _fullNameLabel;
        //设置控件
        private TextField _nameField;
        private TextField _descriptionField;
        private ObjectField _backgroundField;

        //构建函数
        public DataEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //获取面板
            visualTree.CloneTree(this);

            //设置为可延展
            style.flexGrow = 1;
            style.flexShrink = 1;

            //指定主窗口
            MainWindow = mainWindow;

            //初始化
            Init();
        }

        #region 数据编辑面板的初始化以及数据更新
        //初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //获取UI控件
            //基础项
            _packageField = this.Q<TextField>("PackageField");
            _authorFiled = this.Q<TextField>("AuthorField");
            _parentDataField = this.Q<ObjectField>("ParentDataField");
            //全名
            _fullNameLabel = this.Q<Label>("FullNameLabel");
            //设置控件
            _nameField = this.Q<TextField>("NameField");
            _descriptionField = this.Q<TextField>("DescriptionField");
            _backgroundField = this.Q<ObjectField>("BackgroundField");
        }
        //刷新面板
        public void DRefresh(LocUnitData locUnitData)
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //清除旧的绑定
            Unbind();
            //检测是否有数据被选择
            if (locUnitData != null)
            {
                //若有
                //重新绑定
                Bind(locUnitData);

                //设置数据
                _fullNameLabel.text = string.Join("/", locUnitData.FullName);
                if (_backgroundField.value is Sprite sprite)
                {
                    //设置背景图
                    style.backgroundImage = new StyleBackground(sprite);
                    //设置背景图延展模式为切削
                    style.backgroundPositionX = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                    style.backgroundPositionY = BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition(ScaleMode.ScaleAndCrop);
                    style.backgroundRepeat = BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat(ScaleMode.ScaleAndCrop);
                    style.backgroundSize = BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize(ScaleMode.ScaleAndCrop);
                }
                else
                {
                    style.backgroundImage = null;
                }
            }
        }
        //绑定
        private void Bind(LocUnitData locUnitData)
        {
            //不触发通知的情况下更改数据
            _packageField.SetValueWithoutNotify(locUnitData.Package);
            _authorFiled.SetValueWithoutNotify(locUnitData.Author);
            _parentDataField.SetValueWithoutNotify(locUnitData.ParentData);
            _nameField.SetValueWithoutNotify(locUnitData.Name);
            _descriptionField.SetValueWithoutNotify(locUnitData.Description);
            _backgroundField.SetValueWithoutNotify(locUnitData.Background);

            //检测目标是否需要重新生成全名
            if (MainWindow.DataNeedToReGenerateFullNameCache.Contains(locUnitData))
            {
                //若是，重新生成
                locUnitData.Editor_GenerateFullName();
                //生成结束后移除
                MainWindow.DataNeedToReGenerateFullNameCache.Remove(locUnitData);
            }

            //将控件绑定至新数据上
            _packageField.RegisterValueChangedCallback(OnPackageChanged);
            _authorFiled.RegisterValueChangedCallback(OnAuthorChanged);
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            _descriptionField.RegisterValueChangedCallback(OnDescriptionChanged);
            _backgroundField.RegisterValueChangedCallback(OnBackgroundChanged);
        }
        //清除绑定
        private void Unbind()
        {
            //将控件从旧数据清除绑定
            _packageField.UnregisterValueChangedCallback(OnPackageChanged);
            _authorFiled.UnregisterValueChangedCallback(OnAuthorChanged);
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
            _descriptionField.UnregisterValueChangedCallback(OnDescriptionChanged);
            _backgroundField.UnregisterValueChangedCallback(OnBackgroundChanged);
        }
        //数据处理方法
        private void OnPackageChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveData.Editor_SetPackage(evt.newValue);
        }
        private void OnAuthorChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveData.Editor_SetAuthor(evt.newValue);
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            //更改数据
            MainWindow.DataTreeView.ActiveData.Editor_SetName(evt.newValue);
            //更改显示
            _fullNameLabel.text = string.Join("/", MainWindow.DataTreeView.ActiveData.FullName);
            //检查加上重命名全名标记
            MainWindow.MarkAsNeedToReGenerateFullName(MainWindow.DataTreeView.ActiveData);
        }

        private void OnDescriptionChanged(ChangeEvent<string> evt)
        {
            MainWindow.DataTreeView.ActiveData.Editor_SetDescription(evt.newValue);
        }

        private void OnBackgroundChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is Sprite sprite)
            {
                MainWindow.DataTreeView.ActiveData.Editor_SetBackground(sprite);
                style.backgroundImage = new StyleBackground(sprite);
            }
            else
            {
                MainWindow.DataTreeView.ActiveData.Editor_SetBackground(null);
            }
        }
        #endregion
    }
}

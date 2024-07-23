using System;
using THLL.LocationSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameEditor
{
    public class LocUnitDataEditorDataTab : Tab
    {
        //自身数据
        //UXML文档
        private VisualTreeAsset _visualTree;
        //目标数据物体
        private readonly TreeViewItemData<LocUnitData> _targetItem;
        //目标数据
        private readonly LocUnitData _targetData;

        //UI控件
        //获取控件
        private TextField _packageTextField;
        private TextField _categoryTextField;
        private TextField _authorTextField;
        private ObjectField _backgroundField;
        private Label _fullNameLabel;
        private TextField _nameTextField;
        private TextField _descriptionTextField;
        private ObjectField _parentDataField;
        //背景图片
        private Image _background;

        //构建函数
        public LocUnitDataEditorDataTab(TreeViewItemData<LocUnitData> targetItem, VisualTreeAsset visualTree)
        {
            //设置物体
            _targetItem = targetItem;
            //获取数据
            _targetData = _targetItem.data;
            //设置标签名称
            label = _targetData.name;
            //设置标签为可关闭
            closeable = true;
            //设置文档
            _visualTree = visualTree;

            //初始化UI
            InitUI();
        }

        //初始化UI函数
        private void InitUI()
        {
            //加载UI文档
            VisualElement ui = _visualTree.CloneTree();
            Add(ui);

            //获取控件
            _packageTextField = this.Q<TextField>("PackageTextField");
            _categoryTextField = this.Q<TextField>("CategoryTextField");
            _authorTextField = this.Q<TextField>("AuthorTextField");
            _backgroundField = this.Q<ObjectField>("BackgroundField");
            _fullNameLabel = this.Q<Label>("FullNameLabel");
            _nameTextField = this.Q<TextField>("NameTextField");
            _descriptionTextField = this.Q<TextField>("DescriptionTextField");
            _parentDataField = this.Q<ObjectField>("ParentDataField");
            //动态创建Image元素并添加到UI之中
            //获取容器
            VisualElement backgroundContainer = ui.Q<VisualElement>("BackgroundContainer");
            //创建Image控件
            _background = new()
            {
                name = "Background",
                scaleMode = ScaleMode.ScaleToFit
            };
            //设置格式
            _background.style.flexGrow = 1;
            _background.style.display = DisplayStyle.None;
            //添加
            backgroundContainer.Add(_background);

            //显示数据
            ShowData();

            //绑定UI控件
            BindFields();
        }
        //绑定数据函数
        private void BindFields()
        {
            //设定当当前标签被选中时执行数据显示方法
            selected += (tab) =>
            {
                ShowData();
            };

            //设定当标签页被关闭时进行存储
            closed += (tab) =>
            {
                SaveData();
            };

            //监听控件变化信息并绑定
            _packageTextField.RegisterValueChangedCallback(evt =>
            {
                _targetData.Editor_SetPackage(evt.newValue);
            });
            _authorTextField.RegisterValueChangedCallback(evt =>
            {
                _targetData.Editor_SetAuthor(evt.newValue);
            });
            _backgroundField.RegisterValueChangedCallback(evt =>
            {
                //判断新数据类型
                if (evt.newValue is Sprite sprite)
                {
                    //若为精灵图类型，则更改
                    _targetData.Editor_SetBackground(sprite);
                }
                //同时设置Image控件
                UpdateBackgroundState();
            });
            _nameTextField.RegisterValueChangedCallback(evt =>
            {
                _targetData.Editor_SetName(evt.newValue);
                _fullNameLabel.text = string.Join("/", _targetData.FullName);
                //对子级的全名同样进行重新生成
                foreach (TreeViewItemData<LocUnitData> item in _targetItem.children)
                {
                    item.data.Editor_GenerateFullName();
                }
            });
            _descriptionTextField.RegisterValueChangedCallback(evt =>
            {
                _targetData.Editor_SetDescription(evt.newValue);
            });
        }
        //数据显示函数
        private void ShowData()
        {
            //为防止出错，手动生成一次全名
            _targetData.Editor_GenerateFullName();

            //设定控件的值
            _packageTextField.value = _targetData.Package;
            _categoryTextField.value = _targetData.Category;
            _authorTextField.value = _targetData.Author;
            _backgroundField.value = _targetData.Background;
            _fullNameLabel.text = string.Join("/", _targetData.FullName);
            _nameTextField.value = _targetData.Name;
            _descriptionTextField.value = _targetData.Description;
            _parentDataField.value = _targetData.ParentData;

            //检测是否有背景图并开启显示
            UpdateBackgroundState();

            //并存储一次
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        //背景图更新函数
        private void UpdateBackgroundState()
        {
            //判断新数据类型
            if (_backgroundField.value is Sprite sprite)
            {
                //若为精灵图类型，则更改
                _targetData.Editor_SetBackground(sprite);
                //同时设置Image控件
                _background.sprite = sprite;
                //将其更改为显示
                _background.style.display = DisplayStyle.Flex;
            }
            //若不是，隐藏背景图片
            _background.style.display = DisplayStyle.None;
        }
        //存储函数
        private void SaveData()
        {
            //为防止出错，手动生成一次全名
            _targetData.Editor_GenerateFullName();

            //存储
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

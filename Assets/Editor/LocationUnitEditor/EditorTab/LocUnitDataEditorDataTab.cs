using System.Collections.Generic;
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
        private readonly VisualTreeAsset _visualTree;
        //目标数据物体
        private readonly TreeViewItemData<LocUnitData> _targetItem;
        //目标数据
        private readonly LocUnitData _targetData;
        //面板打开状态缓存
        private readonly Dictionary<int, Tab> _dataEditorTabOpenStateDicCache;

        //UI控件
        private TextField _packageTextField;
        private TextField _categoryTextField;
        private TextField _authorTextField;
        private ObjectField _backgroundField;
        private Label _fullNameLabel;
        private TextField _nameTextField;
        private TextField _descriptionTextField;
        private ObjectField _parentDataField;
        private Button _closeButton;
        private VisualElement _background;
        private Image _backgroundImage;

        //构建函数
        public LocUnitDataEditorDataTab(TreeViewItemData<LocUnitData> targetItem, VisualTreeAsset visualTree, Dictionary<int, Tab> dataEditorTabOpenStateDicCache)
        {
            //设置物体
            _targetItem = targetItem;
            //获取数据
            _targetData = _targetItem.data;
            //设置标签名称
            label = _targetData.name;
            //设置标签为可关闭，额外设置关闭按钮是为了防止标签页过多导致无法选中原生关闭按钮
            closeable = true;
            //设置文档
            _visualTree = visualTree;
            //设置缓存
            _dataEditorTabOpenStateDicCache = dataEditorTabOpenStateDicCache;
            //添加到缓存中
            _dataEditorTabOpenStateDicCache[_targetData.GetAssetHashCode()] = this;

            //设置长宽
            style.width = new Length(100, LengthUnit.Percent);
            style.height = new Length(100, LengthUnit.Percent);

            //初始化UI
            InitUI();
        }

        //初始化UI函数
        private void InitUI()
        {
            //加载UI文档
            _visualTree.CloneTree(this);

            //获取控件
            _packageTextField = this.Q<TextField>("PackageTextField");
            _categoryTextField = this.Q<TextField>("CategoryTextField");
            _authorTextField = this.Q<TextField>("AuthorTextField");
            _backgroundField = this.Q<ObjectField>("BackgroundField");
            _fullNameLabel = this.Q<Label>("FullNameLabel");
            _nameTextField = this.Q<TextField>("NameTextField");
            _descriptionTextField = this.Q<TextField>("DescriptionTextField");
            _parentDataField = this.Q<ObjectField>("ParentDataField");
            _closeButton = this.Q<Button>("CloseButton");

            //图片显示
            _backgroundImage = new Image();
            _background = this.Q<VisualElement>("Background");
            _background.Add(_backgroundImage);

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

            //设定当标签页被关闭时进行存储并从缓存中移除
            closed += (tab) =>
            {
                //关闭时存储数据
                SaveData();
                //并将自身从缓存中移除
                _dataEditorTabOpenStateDicCache.Remove(_targetData.GetAssetHashCode());
            };

            //绑定按钮
            _closeButton.clicked += CloseTab;

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
                //同时设置背景图
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
                //设置背景图
                _backgroundImage.sprite = sprite;
                _backgroundImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                //设置为空
                _backgroundImage.sprite = null;
                _backgroundImage.style.display = DisplayStyle.None;
            }
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
        //关闭函数，无法触发事件
        private void CloseTab()
        {
            //关闭时存储数据
            SaveData();
            //并将自身从缓存中移除
            _dataEditorTabOpenStateDicCache.Remove(_targetData.GetAssetHashCode());
            //从面板移除
            RemoveFromHierarchy();
        }
    }
}

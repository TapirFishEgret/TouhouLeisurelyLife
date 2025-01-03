﻿using THLL.CharacterSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.CharacterEditor
{
    public class DataEditorPanel : VisualElement
    {
        #region 基础构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示数据
        public CharacterData ShowedCharacter { get { return MainWindow.DataTreeView.ActiveSelection.Data; } }

        //基础面板
        private VisualElement DataEditorRootPanel { get; set; }
        //信息显示
        private Label FullInfoLabel { get; set; }
        //数据编辑
        private TextField IDField { get; set; }
        private TextField IDPartField { get; set; }
        private TextField NameField { get; set; }
        private TextField DescriptionField { get; set; }
        private IntegerField SortingOrderField { get; set; }
        private TextField SeriesField { get; set; }
        private TextField GroupField { get; set; }
        private TextField CharaField { get; set; }
        private TextField VersionField { get; set; }
        private ColorField ColorField { get; set; }
        #endregion

        #region 构造及初始化
        //构造函数
        public DataEditorPanel(VisualTreeAsset visualTree, MainWindow window)
        {
            //设置面板为可延展并显示
            style.flexGrow = 1;
            style.display = DisplayStyle.Flex;

            //获取面板
            visualTree.CloneTree(this);

            //指定主窗口
            MainWindow = window;

            //初始化
            Init();
        }
        //初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //获取UI控件
            DataEditorRootPanel = this.Q<VisualElement>("DataEditorRootPanel");
            FullInfoLabel = this.Q<Label>("FullInfoLabel");
            IDField = this.Q<TextField>("IDField");
            IDPartField = this.Q<TextField>("IDPartField");
            NameField = this.Q<TextField>("NameField");
            DescriptionField = this.Q<TextField>("DescriptionField");
            SortingOrderField = this.Q<IntegerField>("SortOrderField");
            SeriesField = this.Q<TextField>("SeriesField");
            GroupField = this.Q<TextField>("GroupField");
            CharaField = this.Q<TextField>("CharaField");
            VersionField = this.Q<TextField>("VersionField");
            ColorField = this.Q<ColorField>("ColorField");
        }
        #endregion

        #region 刷新与绑定与反绑定
        //刷新
        public void DRefresh()
        {
            //刷新前保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //判断当前数据情况
            if (ShowedCharacter != null)
            {
                //若不为空，进行刷新
                //计时
                using ExecutionTimer timer = new("数据编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

                //然后进行绑定
                Bind();
            }
        }
        //绑定
        private void Bind()
        {
            //绑定前以不通知的形式设置显示数据
            IDField.SetValueWithoutNotify(ShowedCharacter.ID);
            IDPartField.SetValueWithoutNotify(ShowedCharacter.IDPart);
            NameField.SetValueWithoutNotify(ShowedCharacter.Name);
            DescriptionField.SetValueWithoutNotify(ShowedCharacter.Description);
            SortingOrderField.SetValueWithoutNotify(ShowedCharacter.SortOrder);
            SeriesField.SetValueWithoutNotify(ShowedCharacter.Series);
            GroupField.SetValueWithoutNotify(ShowedCharacter.Group);
            CharaField.SetValueWithoutNotify(ShowedCharacter.Chara);
            VersionField.SetValueWithoutNotify(ShowedCharacter.Version);
            ColorField.SetValueWithoutNotify(ShowedCharacter.Color);

            //显示全部信息
            SetFullInfo();

            //绑定
            NameField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                    ShowedCharacter.Name = evt.newValue;
            });
            DescriptionField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                    ShowedCharacter.Description = evt.newValue;
            });
            SortingOrderField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                    ShowedCharacter.SortOrder = evt.newValue;
            });
            SeriesField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                {
                    ShowedCharacter.Series = evt.newValue;
                    SetFullInfo();
                }
            });
            GroupField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                {
                    ShowedCharacter.Group = evt.newValue;
                    SetFullInfo();
                }
            });
            CharaField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                {
                    ShowedCharacter.Chara = evt.newValue;
                    SetFullInfo();
                }
            });
            VersionField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                {
                    ShowedCharacter.Version = evt.newValue;
                    SetFullInfo();
                }
            });
            ColorField.RegisterValueChangedCallback(evt =>
            {
                if (ShowedCharacter != null)
                {
                    ShowedCharacter.ColorString = ColorUtility.ToHtmlStringRGB(evt.newValue);
                    FullInfoLabel.style.color = evt.newValue;
                }
            });
        }
        #endregion

        #region 辅助方法
        //生成全部信息
        private void SetFullInfo()
        {
            if (ShowedCharacter == null)
            {
                return;
            }
            //以/分割各字段
            FullInfoLabel.text = string.Join("/", new string[] {
                ShowedCharacter.Series,
                ShowedCharacter.Group,
                ShowedCharacter.Chara,
                ShowedCharacter.Version
            });
            //设置颜色
            FullInfoLabel.style.color = ShowedCharacter.Color;
        }
        #endregion
    }
}

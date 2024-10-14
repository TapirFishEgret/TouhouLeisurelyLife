﻿using System.Collections.Generic;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class DataEditorPanel : Tab
    {
        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示的场景
        public SceneData ShowedScene { get => MainWindow.DataTreeView.ActiveSelection.Data; }

        //基层面板
        private VisualElement DataEditorRootPanel { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //ID
        private TextField IDField { get; set; }
        //IDPart
        private TextField IDPartField { get; set; }
        //名称
        private TextField NameField { get; set; }
        //描述
        private TextField DescriptionField { get; set; }
        //排序位置
        private IntegerField SortOrderField { get; set; }
        //父级ID
        private TextField ParentSceneIDField { get; set; }
        #endregion

        #region 数据编辑面板的初始化以及数据更新
        //构建函数
        public DataEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //获取面板
            VisualElement panel = visualTree.CloneTree();
            Add(panel);

            //指定主窗口
            MainWindow = mainWindow;

            //初始化
            Init();
        }
        //初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //设定名称
            label = "数据编辑面板";

            //获取UI控件
            //基层面板
            DataEditorRootPanel = this.Q<VisualElement>("DataEditorRootPanel");
            //全名
            FullNameLabel = this.Q<Label>("FullNameLabel");
            //ID
            IDField = this.Q<TextField>("IDField");
            //IDPart
            IDPartField = this.Q<TextField>("IDPartField");
            //名称
            NameField = this.Q<TextField>("NameField");
            //排序位置
            SortOrderField = this.Q<IntegerField>("SortOrderField");
            //描述
            DescriptionField = this.Q<TextField>("DescriptionField");
            //父级ID
            ParentSceneIDField = this.Q<TextField>("ParentSceneIDField");

            //绑定UI控件
            Bind();
        }
        //刷新面板
        public void DRefresh()
        {
            //计时
            using ExecutionTimer timer = new("数据编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有
                //设置数据
                //不触发通知的情况下更改数据
                IDField.SetValueWithoutNotify(ShowedScene.ID);
                IDPartField.SetValueWithoutNotify(ShowedScene.IDPart);
                NameField.SetValueWithoutNotify(ShowedScene.Name);
                DescriptionField.SetValueWithoutNotify(ShowedScene.Description);
                SortOrderField.SetValueWithoutNotify(ShowedScene.SortOrder);
                ParentSceneIDField.SetValueWithoutNotify(ShowedScene.ParentSceneID);
                //设置全名显示
                SetFullName();
            }
        }
        //绑定
        private void Bind()
        {
            //将控件绑定至新数据上
            NameField.RegisterValueChangedCallback(evt => ShowedScene.Name = evt.newValue);
            DescriptionField.RegisterValueChangedCallback(evt => ShowedScene.Description = evt.newValue);
            SortOrderField.RegisterValueChangedCallback(evt => ShowedScene.SortOrder = evt.newValue);
            ParentSceneIDField.RegisterValueChangedCallback(evt => ShowedScene.ParentSceneID = evt.newValue);
        }
        #endregion

        #region 辅助方法
        //获取场景的全名
        public void SetFullName()
        {
            //全名列表
            List<string> names = new();
            //将当前名称插入
            names.Insert(0, ShowedScene.Name);
            //获取父级ID
            string parnetID = ShowedScene.ParentSceneID;
            //若有父级
            while (!string.IsNullOrEmpty(parnetID))
            {
                //尝试获取父级数据
                if (MainWindow.DataTreeView.ItemDicCache.ContainsKey(parnetID.GetHashCode()))
                {
                    //获取父级数据
                    SceneData parentData = MainWindow.DataTreeView.ItemDicCache[parnetID.GetHashCode()].data.Data;
                    //将父级名称插入
                    names.Insert(0, parentData.Name);
                    //更新父级ID
                    parnetID = parentData.ParentSceneID;
                }
                else
                {
                    //若无数据，则退出循环
                    break;
                }
            }
            //设置全名显示
            FullNameLabel.text = string.Join("/", names);
        }
        #endregion
    }
}

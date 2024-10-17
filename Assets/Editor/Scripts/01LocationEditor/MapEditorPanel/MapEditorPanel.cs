using System.Collections.Generic;
using System.IO;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class MapEditorPanel : VisualElement
    {
        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示的场景
        public SceneData ShowedScene
        {
            get
            {
                //判断是否有数据被选中
                if (MainWindow.DataTreeView.ActiveSelection == null)
                {
                    return null;
                }
                //获取选中数据
                return MainWindow.DataTreeView.ActiveSelection.Data;
            }
        }

        //基层面板
        private VisualElement MapEditorRootPanel { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //行数整形输入框
        private IntegerField RowCountIntegerField { get; set; }
        //列数整形输入框
        private IntegerField ColCountIntegerField { get; set; }
        //地图容器
        private VisualElement MapContainer { get; set; }
        //创建地图按钮
        private Button CreateMapButton { get; set; }
        //删除地图按钮
        private Button DeleteMapButton { get; set; }
        #endregion

        #region 数据编辑面板的初始化以及数据更新
        //构建函数
        public MapEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
        {
            //设置自身为可扩展并隐藏
            style.flexGrow = 1;
            style.display = DisplayStyle.None;

            //获取面板
            visualTree.CloneTree(this);

            //指定主窗口
            MainWindow = mainWindow;

            //初始化
            Init();
        }
        //初始化
        private void Init()
        {
            //计时
            using ExecutionTimer timer = new("地图编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //获取UI控件
            //基层面板
            MapEditorRootPanel = this.Q<VisualElement>("MapEditorRootPanel");
            //全名
            FullNameLabel = MapEditorRootPanel.Q<Label>("FullNameLabel");
            //行数整形输入框
            RowCountIntegerField = MapEditorRootPanel.Q<IntegerField>("RowCountIntegerField");
            //列数整形输入框
            ColCountIntegerField = MapEditorRootPanel.Q<IntegerField>("ColCountIntegerField");
            //地图容器
            MapContainer = MapEditorRootPanel.Q<VisualElement>("MapContainer");
            //创建地图按钮
            CreateMapButton = MapEditorRootPanel.Q<Button>("CreateMapButton");
            //删除地图按钮
            DeleteMapButton = MapEditorRootPanel.Q<Button>("DeleteMapButton");

            //注册事件
            RegisterEvents();
        }
        //刷新面板
        public void MRefresh()
        {
            //计时
            using ExecutionTimer timer = new("地图编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有
                //设置全名
                SetFullName();
                //获取地图
                ShowNewMap();
                //更新数值
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
            }
        }
        //注册事件
        private void RegisterEvents()
        {
            //注册几何图形改变事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            //注册行数输入框改变事件
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //检测是否有数据被选中
                if (ShowedScene == null)
                {
                    //若没有，返回
                    return;
                }
                //获取新数值
                int newValue = evt.newValue;
                //检测是否合法
                if (newValue < 1)
                {
                    //若不合法，更改为1
                    RowCountIntegerField.value = 1;
                    //并返回以推动进程
                    return;
                }
                //更改数值
                ShowedScene.Map.Rows = newValue;
                //显示获取新地图
                ShowNewMap();
            });
            //注册列数输入框改变事件
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //检测是否有数据被选中
                if (ShowedScene == null)
                {
                    //若没有，返回
                    return;
                }
                //获取新数值
                int newValue = evt.newValue;
                //检测是否合法
                if (newValue < 1)
                {
                    //若不合法，更改为1
                    ColCountIntegerField.value = 1;
                    //并返回以推动进程
                    return;
                }
                //更改数值
                ShowedScene.Map.Cols = newValue;
                //显示新地图
                ShowNewMap();
            });
            //注册创建地图按钮点击事件
            CreateMapButton.clicked += () =>
            {
                //检测是否有数据被选中
                if (ShowedScene == null)
                {
                    //若没有，返回
                    return;
                }
                //若有，则新建地图
                ShowedScene.Map = new Map(5, 9);
                //不触发通知的情况下更改行列显示数值
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
                //显示新地图
                ShowNewMap();
            };
            //注册删除地图按钮点击事件
            DeleteMapButton.clicked += () =>
            {
                //检测是否有数据被选中
                if (ShowedScene == null)
                {
                    //若没有，返回
                    return;
                }
                //若有，则删除地图，表现为新建实例
                ShowedScene.Map = new();
                //不触发通知的情况下更改行列显示数值
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
                //显示新地图
                ShowNewMap();
            };
        }
        //几何图形改变时
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //获取新地图(其实是顺便改变单元格大小)
            ShowNewMap();
        }
        #endregion

        #region 辅助方法
        //获取场景的全名
        private void SetFullName()
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
        //显示新地图
        private void ShowNewMap()
        {
            //检测选中场景是否为空
            if (ShowedScene == null)
            {
                //若是，返回
                return;
            }
            //重新生成地图以适应窗口大小
            MapContainer.Clear();
            MapContainer.Add(ShowedScene.Map.GetMap(MapContainer));
        }
        #endregion
    }
}

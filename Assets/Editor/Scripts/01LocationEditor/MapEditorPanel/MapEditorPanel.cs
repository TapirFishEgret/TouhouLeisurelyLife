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

            //注册事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {
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
                //获取新地图
                GetNewMap();
            });
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {
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
                //获取新地图
                GetNewMap();
            });
            RegisterCallback<MouseDownEvent>(evt => Debug.Log(evt.target.ToString()));
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
                GetNewMap();
                //更新数值
                RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
                ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
            }
        }
        //几何图形改变时
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //获取新地图(其实是顺便改变单元格大小)
            GetNewMap();
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
        //获取新地图
        private void GetNewMap()
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

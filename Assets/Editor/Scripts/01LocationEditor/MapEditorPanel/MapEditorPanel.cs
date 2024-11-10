using System;
using System.Collections.Generic;
using System.Linq;
using THLL.SceneSystem;
using UnityEditor;
using UnityEditor.UIElements;
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
        //列数整形输入框
        private IntegerField ColCountIntegerField { get; set; }
        //行数整形输入框
        private IntegerField RowCountIntegerField { get; set; }
        //笔刷容器
        private VisualElement BrushContainer { get; set; }
        //地图容器
        private VisualElement MapContainer { get; set; }
        //笔刷文字输入框
        private TextField BrushTextField { get; set; }
        //笔刷颜色选择器
        private ColorField BrushColorField { get; set; }
        //创建地图按钮
        private Button CreateMapButton { get; set; }
        //删除地图按钮
        private Button DeleteMapButton { get; set; }
        #endregion

        #region 数据
        //是否在绘画
        private bool IsPainting { get; set; }
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
            //列数整形输入框
            ColCountIntegerField = MapEditorRootPanel.Q<IntegerField>("ColCountIntegerField");
            //行数整形输入框
            RowCountIntegerField = MapEditorRootPanel.Q<IntegerField>("RowCountIntegerField");
            //笔刷容器
            BrushContainer = MapEditorRootPanel.Q<VisualElement>("BrushContainer");
            //地图容器
            MapContainer = MapEditorRootPanel.Q<VisualElement>("MapContainer");
            //笔刷文字输入框
            BrushTextField = MapEditorRootPanel.Q<TextField>("BrushTextField");
            //笔刷颜色选择器
            BrushColorField = MapEditorRootPanel.Q<ColorField>("BrushColorField");
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
            }
        }
        //注册事件
        private void RegisterEvents()
        {
            //注册几何图形改变事件
            RegisterCallback<GeometryChangedEvent>(evt =>
            {

            });

            //注册列数输入框改变事件
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {

            });
            //注册行数输入框改变事件
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {

            });

            //为每个笔刷按钮注册点击事件
            BrushContainer.Query<Button>().ForEach(button =>
            {
                //检测是否为笔刷
                if (button.name.StartsWith("Brush"))
                {
                    //若是，注册点击事件
                    button.clicked += () =>
                    {
                        //设置当前笔刷字体为按钮显示文本
                        BrushTextField.value = button.text;
                        //设置当前笔刷颜色为按钮颜色
                        BrushColorField.value = button.resolvedStyle.color;
                    };
                }
            });
            //为地图容器创建鼠标按下事件
            MapContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                //检测是否有数据被选中
                if (ShowedScene == null)
                {
                    //若没有，返回
                    return;
                }
                //检测按键
                if (evt.button == 0)
                {
                    //若为左键，检测笔刷元素是否被设置
                    if (string.IsNullOrEmpty(BrushTextField.value))
                    {
                        //若没有，返回
                        return;
                    }
                    //设置指针按下标志
                    IsPainting = true;
                }
            });
            //为地图容器创建笔刷移动上色方法
            MapContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //检测是否在绘画
                if (IsPainting)
                {

                }
            });
            //为地图容器创建鼠标抬起事件
            MapContainer.RegisterCallback<MouseUpEvent>(evt =>
            {
                //检测按键
                if (evt.button == 0)
                {
                    //若为左键，取消绘画
                    IsPainting = false;
                }
            });
            //为地图容器创建鼠标离开事件
            MapContainer.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                //结束绘画
                IsPainting = false;
            });

            //注册创建地图按钮点击事件
            CreateMapButton.clicked += () =>
            {

            };
            //注册删除地图按钮点击事件
            DeleteMapButton.clicked += () =>
            {

            };
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
        #endregion
    }
}

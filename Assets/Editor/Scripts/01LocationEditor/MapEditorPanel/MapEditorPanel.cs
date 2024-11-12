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
        //场景ID输入框
        private TextField SceneIDTextField { get; set; }
        //笔刷颜色选择器
        private ColorField BrushColorField { get; set; }
        //子场景列表
        private ListView ChildScenesListView { get; set; }
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
            //场景ID输入框
            SceneIDTextField = MapEditorRootPanel.Q<TextField>("SceneIDTextField");
            //笔刷颜色选择器
            BrushColorField = MapEditorRootPanel.Q<ColorField>("BrushColorField");
            //子场景列表
            ChildScenesListView = MapEditorRootPanel.Q<ListView>("ChildScenesListView");
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
                //若有，设置全名
                SetFullName();
                //设置子级场景列表
                SetChildScenesList();
                //显示场景地图
                ShowSceneMap();
            }
        }
        //注册事件
        private void RegisterEvents()
        {
            //注册列数输入框改变事件
            ColCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //检测地图是否为空
                if (ShowedScene.MapData.IsEmpty)
                {
                    //若为空，则不处理
                    return;
                }
                //检测列数是否有效
                if (evt.newValue < 1)
                {
                    //若无效，则设置为1
                    ColCountIntegerField.value = 1;
                }
                //重创建地图
                ShowedScene.MapData.CreateMap(evt.newValue, RowCountIntegerField.value);
                //显示地图
                ShowSceneMap();
            });
            //注册行数输入框改变事件
            RowCountIntegerField.RegisterValueChangedCallback(evt =>
            {
                //检测地图是否为空
                if (ShowedScene.MapData.IsEmpty)
                {
                    //若为空，则不处理
                    return;
                }
                //检测行数是否有效
                if (evt.newValue < 1)
                {
                    //若无效，则设置为1
                    RowCountIntegerField.value = 1;
                }
                //重创建地图
                ShowedScene.MapData.CreateMap(ColCountIntegerField.value, evt.newValue);
                //显示地图
                ShowSceneMap();
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
                    //进行一次绘制
                    BrushMap(evt.target as VisualElement);
                }
                else if (evt.button == 1)
                {
                    //若为右键，检测场景ID元素是否被设置
                    if (string.IsNullOrEmpty(SceneIDTextField.value))
                    {
                        //若没有，返回
                        return;
                    }
                    //若有设置，进行一次设置
                    SetCellScene(evt.target as VisualElement);
                }
            });
            //为地图容器创建笔刷移动上色方法
            MapContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //检测笔刷元素是否被设置
                if (string.IsNullOrEmpty(BrushTextField.value))
                {
                    //若没有，返回
                    return;
                }
                //移动时绘制
                BrushMap(evt.target as VisualElement);
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

            //对子场景列表进行设置
            ChildScenesListView.makeItem = () =>
            {
                //创建子场景项，以标签形式表示
                Label label = new()
                {
                    //设置标签名称
                    name = "ChildSceneItem",
                    //设置标签样式
                    style =
                    {
                        //设置字体大小
                        fontSize = 16,
                        //设置文本居中
                        unityTextAlign = TextAnchor.MiddleCenter,
                        //换行设置为不换行
                        whiteSpace = WhiteSpace.NoWrap,
                    }
                };
                //添加以标签更改笔刷的选项
                label.RegisterCallback<MouseDownEvent>(evt =>
                {
                    //检测鼠标按钮
                    if (evt.button == 0)
                    {
                        //若为鼠标左键，设置场景ID文本为标签数据
                        SceneIDTextField.value = label.userData.ToString();
                        //设置笔刷颜色为白色
                        BrushColorField.value = Color.white;
                    }
                });
                //返回标签
                return label;
            };
            ChildScenesListView.bindItem = (item, index) =>
            {
                //绑定子场景项，设置文本
                (item as Label).text = ChildScenesListView.itemsSource[index].ToString().Split("_").Last();
                (item as Label).userData = ChildScenesListView.itemsSource[index].ToString();
            };

            //注册创建地图按钮点击事件
            CreateMapButton.clicked += () =>
            {
                //检测是否有地图
                if (ShowedScene.MapData.IsEmpty)
                {
                    //若没有，创建一个5行8列的地图
                    ShowedScene.MapData.CreateMap(8, 5);
                    //显示地图
                    ShowSceneMap();
                }
            };
            //注册删除地图按钮点击事件
            DeleteMapButton.clicked += () =>
            {
                //检测是否有地图
                if (!ShowedScene.MapData.IsEmpty)
                {
                    //若有
                    ShowedScene.MapData.CreateMap(0, 0);
                    //显示地图
                    ShowSceneMap();
                }
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
        //设置子级场景列表
        private void SetChildScenesList()
        {
            //让数据源指定为空
            ChildScenesListView.itemsSource = null;
            //获取子级数据
            var childScenes = MainWindow.DataTreeView.ChildrenDicCache[ShowedScene.ID.GetHashCode()];
            //获取子级ID字符串
            List<string> childSceneIDs = childScenes.Select(item => item.data.Data.ID).ToList();
            //指定数据源
            ChildScenesListView.itemsSource = childSceneIDs;
            //刷新列表
            ChildScenesListView.Rebuild();
        }
        //显示场景地图
        private void ShowSceneMap()
        {
            //首先删除当前地图
            MapContainer.Clear();
            //然后判断有没有地图数据
            if (ShowedScene.MapData.IsEmpty)
            {
                //若没有，则在容器中放一个提示标签
                Label noMapLabel = new()
                {
                    text = "当前场景没有地图数据",
                };
                MapContainer.Add(noMapLabel);
                //设置列数与行数输入框
                ColCountIntegerField.SetValueWithoutNotify(0);
                RowCountIntegerField.SetValueWithoutNotify(0);
            }
            else
            {
                //若有，则显示地图
                MapContainer.Add(ShowedScene.MapData.GetMap());
                //并获取地图行列数
                int colCount = ShowedScene.MapData.Cells.Keys.Max(item => item.Item1) + 1;
                int rowCount = ShowedScene.MapData.Cells.Keys.Max(item => item.Item2) + 1;
                //设置列数与行数输入框
                ColCountIntegerField.SetValueWithoutNotify(colCount);
                RowCountIntegerField.SetValueWithoutNotify(rowCount);
            }
        }
        //粉刷地图
        private void BrushMap(VisualElement visualElement)
        {
            //检测目标是否为Label
            if (visualElement is Label label)
            {
                //若是，检测其userData是否为单元格
                if (label.userData is MapCell cell)
                {
                    //若是，检查是否在绘画状态
                    if (IsPainting)
                    {
                        //若在绘画状态，调用粉刷方法
                        cell.Brush(BrushTextField.value, BrushColorField.value);
                    }
                    else
                    {
                        //若不在绘画状态，仅预览
                        label.text = BrushTextField.value;
                        label.style.color = BrushColorField.value;
                    }
                }
            }
        }
        //设定单元格所代表的场景
        private void SetCellScene(VisualElement visualElement)
        {
            //检测目标是否为Label
            if (visualElement is Label label)
            {
                //若是，检测其userData是否为单元格
                if (label.userData is MapCell cell)
                {
                    //若是，检查当前场景中是否已有该场景
                    if (ShowedScene.MapData.DisplayedScenes.TryGetValue(SceneIDTextField.value, out MapCell oldCell))
                    {
                        //若已有，将其粉刷为空
                        oldCell.Brush("空", Color.clear);
                        //设定单元格为非场景单元格
                        oldCell.IsScene = false;
                    }
                    //设定单元格所代表的场景
                    cell.Brush(SceneIDTextField.value, BrushColorField.value);
                    //将其添加到显示列表中
                    ShowedScene.MapData.DisplayedScenes[SceneIDTextField.value] = cell;
                    //设定单元格为场景单元格
                    cell.IsScene = true;
                }
            }
        }
        #endregion
    }
}

using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private ScrollView MapContainer { get; set; }
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
        //是否在拖动
        private bool IsDragging { get; set; }
        //鼠标最后所在位置
        private Vector2 LastMousePosition { get; set; }
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
            MapContainer = MapEditorRootPanel.Q<ScrollView>("MapContainer");
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
                //获取地图
                ShowNewMap();
            }
        }
        //注册事件
        private void RegisterEvents()
        {
            //注册几何图形改变事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
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
                    //并启动一次笔刷
                    BrushCell(evt.target as VisualElement);
                }
                else if (evt.button == 1)
                {
                    //若为右键，启动拖动
                    IsDragging = true;
                    //记录鼠标位置
                    LastMousePosition = evt.mousePosition;
                }
            });
            //为地图容器创建笔刷移动上色方法
            MapContainer.RegisterCallback<MouseMoveEvent>(evt =>
            {
                //检测是否在绘画
                if (IsPainting)
                {
                    //若是，粉刷单元格
                    BrushCell(evt.target as VisualElement);
                }

                //检测是否在拖动
                if (IsDragging)
                {
                    //若是，获取位置差值
                    Vector2 delta = evt.mousePosition - LastMousePosition;
                    //更新滚轴面板的滚轴位置
                    MapContainer.scrollOffset += -delta;
                    //更新鼠标位置
                    LastMousePosition = evt.mousePosition;
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
                else if (evt.button == 1)
                {
                    //若为右键，取消拖拽
                    IsDragging = false;
                }
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
                ShowedScene.Map = new Map(9, 5);
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
            //重新生成地图
            MapContainer.Clear();
            MapContainer.Add(ShowedScene.Map.GetMap());
            //不触发通知的情况下更改行列显示数值
            ColCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Cols);
            RowCountIntegerField.SetValueWithoutNotify(ShowedScene.Map.Rows);
        }
        //粉刷单元格
        private void BrushCell(VisualElement visualElement)
        {
            //判断传入数据
            if (visualElement is Label label)
            {
                //使用正则表达式尝试获取提取坐标
                Match match = Regex.Match(label.name, @"Cell_\((\d+),(\d+)\)");
                //检测匹配结果
                if (match.Success)
                {
                    //若匹配成功，获取x,y坐标
                    int x = int.Parse(match.Groups[1].Value);
                    int y = int.Parse(match.Groups[2].Value);

                    //在字典中查找单元格
                    if (ShowedScene.Map.Cells.TryGetValue((x, y), out MapCell cell))
                    {
                        //若找到单元格，则设置单元格文字为笔刷文字
                        cell.Text = BrushTextField.value;
                        label.text = BrushTextField.value;
                        //设置单元格颜色为笔刷颜色
                        cell.TextColorString = ColorUtility.ToHtmlStringRGBA(BrushColorField.value);
                        label.style.color = BrushColorField.value;
                        //并调整单元格字体大小
                        label.style.fontSize = new StyleLength(new Length(label.resolvedStyle.width / label.text.Length, LengthUnit.Pixel));
                    }
                }
            }
        }
        #endregion
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using THLL.SceneSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.SceneEditor
{
    public class AssetsEditorPanel : Tab
    {
        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示的场景
        public SceneData ShowedScene { get => MainWindow.DataTreeView.ActiveSelection.Data; }

        //基层面板
        private VisualElement AssetsEditorRootPanel { get; set; }
        //全名
        private Label FullNameLabel { get; set; }
        //添加背景图按钮
        private Button AddBackgroundButton { get; set; }
        //背景图滚轴容器
        private ScrollView AssetsContainerScrollView { get; set; }
        //名称-背景图字典
        private Dictionary<string, VisualElement> NameBackgroundContainerDict { get; set; } = new();
        #endregion

        #region 数据编辑面板的初始化以及数据更新
        //构建函数
        public AssetsEditorPanel(VisualTreeAsset visualTree, MainWindow mainWindow)
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
            using ExecutionTimer timer = new("资源编辑面板初始化", MainWindow.TimerDebugLogToggle.value);

            //设定名称
            label = "资源编辑面板";

            //获取UI控件
            //基层面板
            AssetsEditorRootPanel = this.Q<VisualElement>("AssetsEditorRootPanel");
            //全名
            FullNameLabel = AssetsEditorRootPanel.Q<Label>("FullNameLabel");
            //添加背景图按钮
            AddBackgroundButton = AssetsEditorRootPanel.Q<Button>("AddBackgroundButton");
            //背景图滚轴容器
            AssetsContainerScrollView = AssetsEditorRootPanel.Q<ScrollView>("AssetsContainerScrollView");

            //注册事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            AddBackgroundButton.clicked += AddBackground;
        }
        //刷新面板
        public async Task ARefresh()
        {
            //计时
            using ExecutionTimer timer = new("资源编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //删除所有背景图容器
            AssetsContainerScrollView.Clear();
            NameBackgroundContainerDict.Clear();

            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有
                //设置全名
                SetFullName();
                //加载并显示背景图
                await ShowedScene.LoadBackgroundsAsync(Path.GetDirectoryName(ShowedScene.SavePath), (name, background) => ShowBackground(name, background));
            }
        }
        //几何图形改变时手动容器大小
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //调整背景容器高度
            NameBackgroundContainerDict.Values.ToList().ForEach(backgroundContainer =>
                backgroundContainer.Q<VisualElement>("Container").style.height = backgroundContainer.resolvedStyle.width * 9 / 16);
        }
        #endregion

        #region 背景图资源的增删
        //显示背景图资源
        public void ShowBackground(string name, Sprite background)
        {
            //尝试获取背景图容器
            if (!NameBackgroundContainerDict.TryGetValue(name, out VisualElement backgroundContainer))
            {
                //若无，则创建并设置元素
                backgroundContainer = MainWindow.BackgroundAssetContainerVisualTree.CloneTree();
                //设置样式
                backgroundContainer.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
                //并向容器中添加Image控件
                backgroundContainer.Q<VisualElement>("Container").Add(new Image() { scaleMode = ScaleMode.ScaleAndCrop });
                //添加到容器容器中
                AssetsContainerScrollView.Add(backgroundContainer);
                //添加到字典中
                NameBackgroundContainerDict.Add(name, backgroundContainer);
            }
            //修改元素
            backgroundContainer.Q<Image>().sprite = background;
            //设置名称
            backgroundContainer.Q<Label>("NameLabel").text = name;
            //给按钮绑定移除事件
            backgroundContainer.Q<Button>("RemoveButton").clicked += () => RemoveBackground(name);
        }
        //隐藏背景图资源
        public void HideBackground(string name)
        {
            //尝试获取背景图容器
            if (NameBackgroundContainerDict.TryGetValue(name, out VisualElement backgroundContainer))
            {
                //从容器中移除
                AssetsContainerScrollView.Remove(backgroundContainer);
            }
        }
        //添加背景图资源
        public void AddBackground()
        {
            //显示输入窗口
            TextInputWindow.ShowWindow(async backgroundName =>
            {
                //检查输入的名称是否已存在或为空
                if (string.IsNullOrEmpty(backgroundName) || NameBackgroundContainerDict.ContainsKey(backgroundName))
                {
                    EditorUtility.DisplayDialog("Error", "Background Name is already exists or is empty!", "OK");
                    return;
                }

                //选择目标文件
                string sourceFilePath = EditorUtility.OpenFilePanel("Select Background Image", "", "png,jpg,jpeg,bmp,webp,tiff,tif");
                //判断选择情况
                if (!string.IsNullOrEmpty(sourceFilePath))
                {
                    //若有选中，则首先指定路径
                    string targetFilePath = Path.Combine(Path.GetDirectoryName(ShowedScene.SavePath), "Backgrounds", backgroundName + ".png");
                    //复制文件
                    File.Copy(sourceFilePath, targetFilePath, true);

                    //结束后直接刷新面板
                    await ARefresh();
                }
            },
            "Add New Background",
            "Please Input New Background Name",
            "New Background Name",
            "New Name",
            EditorWindow.focusedWindow
            );
        }
        //移除背景图资源
        public void RemoveBackground(string name)
        {
            //首先，隐藏背景图
            HideBackground(name);
            //然后，获取存放背景图的目录的信息
            DirectoryInfo directory = new(Path.Combine(Path.GetDirectoryName(ShowedScene.SavePath), "Backgrounds"));
            //遍历目录，删除文件
            foreach (FileInfo file in directory.GetFiles())
            {
                if (Path.GetFileNameWithoutExtension(file.Name).Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    //删除meta文件
                    File.Delete(file.FullName + ".meta");
                    //删除文件
                    file.Delete();
                }
            }
            //保存并刷新资源
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

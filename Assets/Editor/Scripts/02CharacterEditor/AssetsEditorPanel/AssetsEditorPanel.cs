using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using THLL.CharacterSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.EditorSystem.CharacterEditor
{
    public class AssetsEditorPanel : Tab
    {
        #region 自身构成
        //主面板
        public MainWindow MainWindow { get; private set; }

        //显示的角色
        public CharacterData ShowedCharacter
        {
            get
            {
                //检测是否有数据被选择
                if (MainWindow.DataTreeView.ActiveSelection == null)
                {
                    return null;
                }
                //若有，则返回角色数据
                return MainWindow.DataTreeView.ActiveSelection.Data;
            }
        }

        //基础
        private VisualElement AssetsEditorRootPanel { get; set; }
        //信息显示
        private Label FullInfoLabel { get; set; }
        //添加头像按钮
        private Button AddAvatarButton { get; set; }
        //头像滚轴容器
        private ScrollView AvatarContainerScrollView { get; set; }
        //名称-头像容器字典
        private Dictionary<string, VisualElement> NameAvatarContainerDict { get; set; } = new();
        //添加立绘按钮
        private Button AddPortraitButton { get; set; }
        //立绘滚轴容器
        private ScrollView PortraitContainerScrollView { get; set; }
        //名称-立绘容器字典
        private Dictionary<string, VisualElement> NamePortraitContainerDict { get; set; } = new();
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

            //设置标签页容器可延展
            style.flexGrow = 1;
            contentContainer.style.flexGrow = 1;

            //设定名称
            label = "资源编辑面板";

            //获取UI控件
            //基层面板
            AssetsEditorRootPanel = this.Q<VisualElement>("AssetsEditorRootPanel");
            //全名
            FullInfoLabel = AssetsEditorRootPanel.Q<Label>("FullInfoLabel");
            //添加头像按钮
            AddAvatarButton = AssetsEditorRootPanel.Q<Button>("AddAvatarButton");
            //头像滚轴容器
            AvatarContainerScrollView = AssetsEditorRootPanel.Q<ScrollView>("AvatarContainerScrollView");
            //添加立绘按钮
            AddPortraitButton = AssetsEditorRootPanel.Q<Button>("AddPortraitButton");
            //立绘滚轴容器
            PortraitContainerScrollView = AssetsEditorRootPanel.Q<ScrollView>("PortraitContainerScrollView");

            //注册事件
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            //添加头像按钮点击事件
            AddAvatarButton.clicked += AddAvatar;
            //添加立绘按钮点击事件
            AddPortraitButton.clicked += AddPortrait;
        }
        //刷新面板
        public async Task ARefresh()
        {
            //计时
            using ExecutionTimer timer = new("资源编辑面板刷新", MainWindow.TimerDebugLogToggle.value);

            //刷新前进行资源的保存
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //删除所有容器及字典数据
            AvatarContainerScrollView.Clear();
            NameAvatarContainerDict.Clear();
            PortraitContainerScrollView.Clear();
            NamePortraitContainerDict.Clear();

            //检测是否有数据被选择
            if (MainWindow.DataTreeView.ActiveSelection != null)
            {
                //若有
                //设置全名
                SetFullInfo();
                //读取头像资源
                await ShowedCharacter.LoadAvatarsAsync(Path.GetDirectoryName(ShowedCharacter.SavePath), (name, avatar) => ShowAvatar(name, avatar));
                //读取立绘资源
                await ShowedCharacter.LoadPortraitsAsync(Path.GetDirectoryName(ShowedCharacter.SavePath), (name, portrait) => ShowPortrait(name, portrait));
            }
        }
        //几何图形改变时手动容器大小
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            //头像保持长宽一致
            NameAvatarContainerDict.Values.ToList().ForEach(avatarContainer =>
                avatarContainer.Q<VisualElement>("Container").style.height = avatarContainer.resolvedStyle.width);
            //立绘不做更改
        }
        #endregion

        #region 头像资源的增删
        //显示头像
        public void ShowAvatar(string name, Sprite avatar)
        {
            //尝试获取头像容器
            if (!NameAvatarContainerDict.TryGetValue(name, out VisualElement avatarContainer))
            {
                //若无，则创建并设置元素
                avatarContainer = MainWindow.SpriteVisualElementTemplate.CloneTree();
                //并向容器中添加Image控件
                avatarContainer.Q<VisualElement>("Container").Add(new Image() { scaleMode = ScaleMode.ScaleToFit, style = { height = 400 } });
                //添加到容器容器中
                AvatarContainerScrollView.Add(avatarContainer);
                //添加到字典中
                NameAvatarContainerDict.Add(name, avatarContainer);
            }
            //修改元素
            avatarContainer.Q<Image>().sprite = avatar;
            //设置名称
            avatarContainer.Q<Label>("NameLabel").text = name;
            //给按钮绑定移除事件
            avatarContainer.Q<Button>("RemoveButton").clicked += () => RemoveAvatar(name);
        }
        //隐藏头像资源
        public void HideAvatar(string name)
        {
            //尝试获取头像容器
            if (NameAvatarContainerDict.TryGetValue(name, out VisualElement avatarContainer))
            {
                //从面板中移除
                AvatarContainerScrollView.Remove(avatarContainer);
            }
        }
        //添加头像资源
        public async void AddAvatar()
        {
            //判断是否有数据被选择
            if (ShowedCharacter == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a character first!", "OK");
                return;
            }

            //若有数据被选中，声明头像名称
            string avatarName;

            //检测头像数量
            if (ShowedCharacter.AvatarsDict.Count == 0)
            {
                //若等于0，则为首个头像，名称固定为0
                avatarName = "0";
            }
            else
            {
                //显示输入窗口
                avatarName = await TextInputWindow.ShowWindowWithResult(
                    "Add New Avatar",
                    "Please Input New Avatar Name",
                    "New Avatar Name",
                    "New Name",
                    EditorWindow.focusedWindow);
                //判断输入结果是否为空或已存在
                if (string.IsNullOrEmpty(avatarName) || NameAvatarContainerDict.ContainsKey(avatarName))
                {
                    EditorUtility.DisplayDialog("Error", "Avatar Name is already exists or is empty!", "OK");
                    return;
                }
            }

            //确认头像名称后，选择目标文件
            string sourceFilePath = EditorUtility.OpenFilePanel("Select Avatar Image", "", "png,jpg,jpeg,bmp,webp,tiff,tif");
            //判断选择情况
            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                //若有选中，则首先指定路径
                string targetFilePath = Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Avatars", avatarName + Path.GetExtension(sourceFilePath));
                //复制文件
                File.Copy(sourceFilePath, targetFilePath, true);

                //结束后直接刷新面板
                await ARefresh();
            }
        }
        //移除头像资源
        public void RemoveAvatar(string name)
        {
            //首先，隐藏头像
            HideAvatar(name);
            //然后，获取存放头像的目录的信息
            DirectoryInfo directory = new(Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Avatars"));
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
            //从字典中移除
            NameAvatarContainerDict.Remove(name);
            //保存并刷新资源
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region 立绘资源的增删
        //显示立绘
        public void ShowPortrait(string name, Sprite portrait)
        {
            //尝试获取立绘容器
            if (!NamePortraitContainerDict.TryGetValue(name, out VisualElement portraitContainer))
            {
                //若无，则创建并设置元素
                portraitContainer = MainWindow.SpriteVisualElementTemplate.CloneTree();
                //并向容器中添加Image控件
                portraitContainer.Q<VisualElement>("Container").Add(new Image() { scaleMode = ScaleMode.ScaleToFit, style = { width = 400 } });
                //添加到容器容器中
                PortraitContainerScrollView.Add(portraitContainer);
                //添加到字典中
                NamePortraitContainerDict.Add(name, portraitContainer);
            }
            //修改元素
            portraitContainer.Q<Image>().sprite = portrait;
            //设置名称
            portraitContainer.Q<Label>("NameLabel").text = name;
            //给按钮绑定移除事件
            portraitContainer.Q<Button>("RemoveButton").clicked += () => RemovePortrait(name);
        }
        //隐藏立绘资源
        public void HidePortrait(string name)
        {
            //尝试获取立绘容器
            if (NamePortraitContainerDict.TryGetValue(name, out VisualElement portraitContainer))
            {
                //从面板中移除
                PortraitContainerScrollView.Remove(portraitContainer);
            }
        }
        //添加立绘资源
        public async void AddPortrait()
        {
            //判断是否有数据被选择
            if (ShowedCharacter == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a character first!", "OK");
                return;
            }

            //若有数据被选中，声明立绘名称
            string portraitName;

            //判断立绘数量
            if (ShowedCharacter.PortraitsDict.Count == 0)
            {
                //若等于0，则为首个立绘，名称固定为0
                portraitName = "0";
            }
            else
            {
                //显示输入窗口
                portraitName = await TextInputWindow.ShowWindowWithResult(
                    "Add New Portrait",
                    "Please Input New Portrait Name",
                    "New Portrait Name",
                    "New Name",
                    EditorWindow.focusedWindow
                    );
                //判断输入结果是否为空或已存在
                if (string.IsNullOrEmpty(portraitName) || NamePortraitContainerDict.ContainsKey(portraitName))
                {
                    EditorUtility.DisplayDialog("Error", "Portrait Name is already exists or is empty!", "OK");
                    return;
                }
            }

            //确认立绘名称后，选择目标文件
            string sourceFilePath = EditorUtility.OpenFilePanel("Select Portrait Image", "", "png,jpg,jpeg,bmp,webp,tiff,tif");
            //判断选择情况
            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                //若有选中，则首先指定路径
                string targetFilePath = Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Portraits", portraitName + Path.GetExtension(sourceFilePath));
                //复制文件
                File.Copy(sourceFilePath, targetFilePath, true);

                //结束后直接刷新面板
                await ARefresh();
            }
        }
        //移除立绘资源
        public void RemovePortrait(string name)
        {
            //首先，隐藏立绘
            HidePortrait(name);
            //然后，获取存放立绘的目录的信息
            DirectoryInfo directory = new(Path.Combine(Path.GetDirectoryName(ShowedCharacter.SavePath), "Portraits"));
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
            //从字典中移除
            NamePortraitContainerDict.Remove(name);
            //保存并刷新资源
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region 辅助方法
        //获取场景的全名
        public void SetFullInfo()
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THLL.BaseSystem;
using THLL.GeographySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.GameUI.GameMainTitleInterface
{
    public class GameMainTitleInterface : Singleton<GameMainTitleInterface>
    {
        #region 数据
        //UI界面
        //UIDocument组件
        public UIDocument UIComponent { get; private set; }
        //GameStartInterface文档
        public VisualTreeAsset gameStartInterfaceVisualTree;
        //GameSavesInterface文档
        public VisualTreeAsset gameSavesInterfaceVisualTree;
        //GameSettingsInterface文档
        public VisualTreeAsset gameSettingsInterface;
        //GameSystemSettingsInterface
        public VisualTreeAsset gameSystemSettingsInterface;
        //GamePlaySettingsInterface
        public VisualTreeAsset gamePlaySettingsInterface;
        //GamePatchesSettingsInterface
        public VisualTreeAsset gamePatchesSettingsInterface;

        //返回功能
        //展示中的面板
        public List<VisualElement> ShowedPanels { get; } = new();

        //UI控件
        //UI容器
        public VisualElement GameInterfaceContainer { get; private set; }
        //主标题界面
        //根界面
        public VisualElement GameMainTitlePanel { get; private set; }
        //LocationLabel
        public Label LocationLabel { get; private set; }
        //GamePlayButtonsPanel
        public VisualElement GamePlayButtonsPanel { get; private set; }
        //NewGameButton
        public Button NewGameButton { get; private set; }
        //LoadGameButton
        public Button LoadGameButton { get; private set; }
        //SettingsButton
        public Button SettingsButton { get; private set; }
        //QuitGameButton
        public Button QuitGameButton { get; private set; }

        //游戏开始界面
        public GameStartInterface GameStartInterface { get; private set; }

        //游戏读取界面
        public GameSavesInterface GameSavesInterface { get; private set; }

        //游戏设定界面
        public GameSettingsInterface GameSettingsInterface { get; private set; }

        //通用
        //当前显示地点
        public Location ShowedLocation { get; private set; }
        //显示地点持续时间
        public float ShowedLocationDuration { get; private set; } = 0;
        //Background
        public VisualElement Background { get; private set; }
        //BackgroundCover
        public VisualElement BackgroundCover { get; private set; }
        //动画方向
        public bool GoToBottom { get; private set; } = true;
        //InterfaceCover
        public VisualElement InterfaceCover { get; private set; }
        //动画方向
        public bool GoToRight { get; private set; } = true;
        #endregion

        #region Unity周期函数
        //Start
        protected override void Start()
        {
            //父类Start
            base.Start();

            //获取组件
            UIComponent = GetComponent<UIDocument>();

            //初始化
            Init();

            //将开始协程绑定至资源加载结束事件中
            GameAssetsMgr.Instance.OnAllResourcesLoaded += () => StartCoroutine(CycleBackground());
        }
        //OnDestroy
        private void OnDestroy()
        {
            //在销毁时，结束所有协程
            StopAllCoroutines();
        }
        #endregion

        #region 初始化与相关方法
        //初始化
        private void Init()
        {
            //容器相关
            GameInterfaceContainer = UIComponent.rootVisualElement.Q<VisualElement>("GameInterfaceContainer");

            //游戏主界面相关
            GameMainTitlePanel = UIComponent.rootVisualElement.Q<VisualElement>("GameMainTitlePanel");
            LocationLabel = GameMainTitlePanel.Q<Label>("LocationLabel");
            GamePlayButtonsPanel = GameMainTitlePanel.Q<VisualElement>("GamePlayButtonsPanel");
            NewGameButton = GamePlayButtonsPanel.Q<Button>("NewGameButton");
            LoadGameButton = GamePlayButtonsPanel.Q<Button>("LoadGameButton");
            SettingsButton = GamePlayButtonsPanel.Q<Button>("SettingsButton");
            QuitGameButton = GamePlayButtonsPanel.Q<Button>("QuitGameButton");

            //新游戏相关
            GameStartInterface = new(this, gameStartInterfaceVisualTree);
            GameInterfaceContainer.Add(GameStartInterface);
            NewGameButton.clicked += () => OpenNewPanel(GameStartInterface);

            //游戏存档相关
            GameSavesInterface = new(this, gameSavesInterfaceVisualTree);
            GameInterfaceContainer.Add(GameSavesInterface);
            LoadGameButton.clicked += () => OpenNewPanel(GameSavesInterface);

            //游戏设置界面相关
            GameSettingsInterface = new(this, gameSettingsInterface);
            GameInterfaceContainer.Add(GameSettingsInterface);
            SettingsButton.clicked += () => OpenNewPanel(GameSettingsInterface);

            //退出游戏相关
            QuitGameButton.clicked += QuitGame;

            //通用
            Background = UIComponent.rootVisualElement.Q<VisualElement>("Background");
            BackgroundCover = UIComponent.rootVisualElement.Q<VisualElement>("BackgroundCover");
            InterfaceCover = UIComponent.rootVisualElement.Q<VisualElement>("InterfaceCover");
            //给所有的返回按钮添加返回功能
            UIComponent.rootVisualElement.Query<Button>("ReturnButton").ForEach(button => button.clicked += ReturnToPreviousPanel);

            //将主面板添加到被打开的面板的列表中
            ShowedPanels.Add(GameMainTitlePanel);
        }
        #endregion

        #region UI方法
        //游戏主界面
        //当退出时
        private void QuitGame()
        {
            //退出游戏
            Application.Quit();
        }

        //通用
        //打开新面板
        public void OpenNewPanel(VisualElement panel)
        {
            //将面板加入列表中
            ShowedPanels.Add(panel);
            //开启最后一个面板
            StartCoroutine(ShowLastPanel());
        }
        //返回上一个面板
        public void ReturnToPreviousPanel()
        {
            //返回时，首先检测当前显示的面板的数量
            if (ShowedPanels.Count > 1)
            {
                //打开索引的最后一个的面板，以返回的形式
                StartCoroutine(ShowLastPanel(true));
            }
        }
        #endregion

        #region 辅助方法
        //循环主界面背景图
        private IEnumerator CycleBackground()
        {
            //保持执行
            while (true)
            {
                //检测持续时间
                if (ShowedLocationDuration > 0)
                {
                    //若大于零，则持续时间自减
                    ShowedLocationDuration -= Time.deltaTime;
                    yield return null;
                }
                else
                {
                    //若小于或等于零，重选地点，重订时间
                    //获取地点列表
                    List<Location> locations = GameLocation.LocationDb.Datas.ToList();
                    //获取随机数
                    int randomNumber = Random.Range(0, locations.Count);
                    //更改地点位置
                    ShowedLocation = locations[randomNumber];

                    //检测动画方向
                    if (GoToBottom)
                    {
                        //若向下
                        //遮盖当前背景图
                        BackgroundCover.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
                        //间隔时间0.5s
                        yield return new WaitForSeconds(0.5f);
                        //更改背景图
                        Background.style.backgroundImage = new StyleBackground(ShowedLocation.Background);
                        //更改地点名称显示
                        LocationLabel.text = ShowedLocation.Name;
                        //显示当前背景图
                        BackgroundCover.style.top = new StyleLength(new Length(100, LengthUnit.Percent));
                        //间隔时间0.5s
                        yield return new WaitForSeconds(0.5f);
                        //更改方向
                        GoToBottom = false;
                    }
                    else
                    {
                        //否则反之
                        //遮盖当前背景图
                        BackgroundCover.style.top = new StyleLength(new Length(0, LengthUnit.Percent));
                        //间隔时间0.5s
                        yield return new WaitForSeconds(0.5f);
                        //更改背景图
                        Background.style.backgroundImage = new StyleBackground(ShowedLocation.Background);
                        //更改地点名称显示
                        LocationLabel.text = ShowedLocation.Name;
                        //显示当前背景图
                        BackgroundCover.style.bottom = new StyleLength(new Length(100, LengthUnit.Percent));
                        //间隔时间0.5s
                        yield return new WaitForSeconds(0.5f);
                        //更改方向
                        GoToBottom = true;
                    }

                    //更改显示时间，测试期间固定为5秒
                    ShowedLocationDuration = 5f;
                }
            }
        }
        //显示列表中最后一个面板
        private IEnumerator ShowLastPanel(bool isReturn = false)
        {
            //播放Cover的动画并显示面板
            if (GoToRight)
            {
                //若动画向右侧延伸，则首先调整右侧到0
                InterfaceCover.style.right = new StyleLength(new Length(0, LengthUnit.Percent));
                //等待0.5s
                yield return new WaitForSeconds(0.5f);

                //关闭当前面板显示
                ShowedPanels.ForEach(panel => panel.style.display = DisplayStyle.None);
                //检测是否为返回
                if (isReturn)
                {
                    //若是，移除最后一个
                    ShowedPanels.Remove(ShowedPanels.Last());
                }
                //设定倒数第一位为显示
                ShowedPanels[^1].style.display = DisplayStyle.Flex;
                //并关闭其可操控性
                ShowedPanels[^1].SetEnabled(false);
                //等待到当前帧结束
                yield return new WaitForEndOfFrame();

                //再将左侧调整为100
                InterfaceCover.style.left = new StyleLength(new Length(100, LengthUnit.Percent));
                //等待0.5s
                yield return new WaitForSeconds(0.5f);

                //开启面板可操控性
                ShowedPanels[^1].SetEnabled(true);

                //更改延伸方向
                GoToRight = false;
            }
            else
            {
                //若不是，则反之
                InterfaceCover.style.left = new StyleLength(new Length(0, LengthUnit.Percent));
                //等待0.5s
                yield return new WaitForSeconds(0.5f);

                //关闭当前面板显示
                ShowedPanels.ForEach(panel => panel.style.display = DisplayStyle.None);
                //检测是否为返回
                if (isReturn)
                {
                    //若是，移除最后一个
                    ShowedPanels.Remove(ShowedPanels.Last());
                }
                //设定倒数第一位为显示
                ShowedPanels[^1].style.display = DisplayStyle.Flex;
                //并关闭其可操控性
                ShowedPanels[^1].SetEnabled(false);
                //等待到当前帧结束
                yield return new WaitForEndOfFrame();

                //再将左侧调整为100
                InterfaceCover.style.right = new StyleLength(new Length(100, LengthUnit.Percent));
                //等待0.5s
                yield return new WaitForSeconds(0.5f);

                //开启面板可操控性
                ShowedPanels[^1].SetEnabled(true);

                //更改延伸方向
                GoToRight = true;
            }
        }
        #endregion
    }
}

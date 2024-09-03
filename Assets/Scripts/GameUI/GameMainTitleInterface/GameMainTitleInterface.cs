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
        #region ����
        //UI����
        //UIDocument���
        public UIDocument UIComponent { get; private set; }
        //GameStartInterface�ĵ�
        public VisualTreeAsset gameStartInterfaceVisualTree;
        //GameSavesInterface�ĵ�
        public VisualTreeAsset gameSavesInterfaceVisualTree;
        //GameSettingsInterface�ĵ�
        public VisualTreeAsset gameSettingsInterface;
        //GameSystemSettingsInterface
        public VisualTreeAsset gameSystemSettingsInterface;
        //GamePlaySettingsInterface
        public VisualTreeAsset gamePlaySettingsInterface;
        //GamePatchesSettingsInterface
        public VisualTreeAsset gamePatchesSettingsInterface;

        //���ع���
        //չʾ�е����
        public List<VisualElement> ShowedPanels { get; } = new();

        //UI�ؼ�
        //UI����
        public VisualElement GameInterfaceContainer { get; private set; }
        //���������
        //������
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

        //��Ϸ��ʼ����
        public GameStartInterface GameStartInterface { get; private set; }

        //��Ϸ��ȡ����
        public GameSavesInterface GameSavesInterface { get; private set; }

        //��Ϸ�趨����
        public GameSettingsInterface GameSettingsInterface { get; private set; }

        //ͨ��
        //��ǰ��ʾ�ص�
        public Location ShowedLocation { get; private set; }
        //��ʾ�ص����ʱ��
        public float ShowedLocationDuration { get; private set; } = 0;
        //Background
        public VisualElement Background { get; private set; }
        //BackgroundCover
        public VisualElement BackgroundCover { get; private set; }
        //��������
        public bool GoToBottom { get; private set; } = true;
        //InterfaceCover
        public VisualElement InterfaceCover { get; private set; }
        //��������
        public bool GoToRight { get; private set; } = true;
        #endregion

        #region Unity���ں���
        //Start
        protected override void Start()
        {
            //����Start
            base.Start();

            //��ȡ���
            UIComponent = GetComponent<UIDocument>();

            //��ʼ��
            Init();

            //����ʼЭ�̰�����Դ���ؽ����¼���
            GameAssetsMgr.Instance.OnAllResourcesLoaded += () => StartCoroutine(CycleBackground());
        }
        //OnDestroy
        private void OnDestroy()
        {
            //������ʱ����������Э��
            StopAllCoroutines();
        }
        #endregion

        #region ��ʼ������ط���
        //��ʼ��
        private void Init()
        {
            //�������
            GameInterfaceContainer = UIComponent.rootVisualElement.Q<VisualElement>("GameInterfaceContainer");

            //��Ϸ���������
            GameMainTitlePanel = UIComponent.rootVisualElement.Q<VisualElement>("GameMainTitlePanel");
            LocationLabel = GameMainTitlePanel.Q<Label>("LocationLabel");
            GamePlayButtonsPanel = GameMainTitlePanel.Q<VisualElement>("GamePlayButtonsPanel");
            NewGameButton = GamePlayButtonsPanel.Q<Button>("NewGameButton");
            LoadGameButton = GamePlayButtonsPanel.Q<Button>("LoadGameButton");
            SettingsButton = GamePlayButtonsPanel.Q<Button>("SettingsButton");
            QuitGameButton = GamePlayButtonsPanel.Q<Button>("QuitGameButton");

            //����Ϸ���
            GameStartInterface = new(this, gameStartInterfaceVisualTree);
            GameInterfaceContainer.Add(GameStartInterface);
            NewGameButton.clicked += () => OpenNewPanel(GameStartInterface);

            //��Ϸ�浵���
            GameSavesInterface = new(this, gameSavesInterfaceVisualTree);
            GameInterfaceContainer.Add(GameSavesInterface);
            LoadGameButton.clicked += () => OpenNewPanel(GameSavesInterface);

            //��Ϸ���ý������
            GameSettingsInterface = new(this, gameSettingsInterface);
            GameInterfaceContainer.Add(GameSettingsInterface);
            SettingsButton.clicked += () => OpenNewPanel(GameSettingsInterface);

            //�˳���Ϸ���
            QuitGameButton.clicked += QuitGame;

            //ͨ��
            Background = UIComponent.rootVisualElement.Q<VisualElement>("Background");
            BackgroundCover = UIComponent.rootVisualElement.Q<VisualElement>("BackgroundCover");
            InterfaceCover = UIComponent.rootVisualElement.Q<VisualElement>("InterfaceCover");
            //�����еķ��ذ�ť��ӷ��ع���
            UIComponent.rootVisualElement.Query<Button>("ReturnButton").ForEach(button => button.clicked += ReturnToPreviousPanel);

            //���������ӵ����򿪵������б���
            ShowedPanels.Add(GameMainTitlePanel);
        }
        #endregion

        #region UI����
        //��Ϸ������
        //���˳�ʱ
        private void QuitGame()
        {
            //�˳���Ϸ
            Application.Quit();
        }

        //ͨ��
        //�������
        public void OpenNewPanel(VisualElement panel)
        {
            //���������б���
            ShowedPanels.Add(panel);
            //�������һ�����
            StartCoroutine(ShowLastPanel());
        }
        //������һ�����
        public void ReturnToPreviousPanel()
        {
            //����ʱ�����ȼ�⵱ǰ��ʾ����������
            if (ShowedPanels.Count > 1)
            {
                //�����������һ������壬�Է��ص���ʽ
                StartCoroutine(ShowLastPanel(true));
            }
        }
        #endregion

        #region ��������
        //ѭ�������汳��ͼ
        private IEnumerator CycleBackground()
        {
            //����ִ��
            while (true)
            {
                //������ʱ��
                if (ShowedLocationDuration > 0)
                {
                    //�������㣬�����ʱ���Լ�
                    ShowedLocationDuration -= Time.deltaTime;
                    yield return null;
                }
                else
                {
                    //��С�ڻ�����㣬��ѡ�ص㣬�ض�ʱ��
                    //��ȡ�ص��б�
                    List<Location> locations = GameLocation.LocationDb.Datas.ToList();
                    //��ȡ�����
                    int randomNumber = Random.Range(0, locations.Count);
                    //���ĵص�λ��
                    ShowedLocation = locations[randomNumber];

                    //��⶯������
                    if (GoToBottom)
                    {
                        //������
                        //�ڸǵ�ǰ����ͼ
                        BackgroundCover.style.bottom = new StyleLength(new Length(0, LengthUnit.Percent));
                        //���ʱ��0.5s
                        yield return new WaitForSeconds(0.5f);
                        //���ı���ͼ
                        Background.style.backgroundImage = new StyleBackground(ShowedLocation.Background);
                        //���ĵص�������ʾ
                        LocationLabel.text = ShowedLocation.Name;
                        //��ʾ��ǰ����ͼ
                        BackgroundCover.style.top = new StyleLength(new Length(100, LengthUnit.Percent));
                        //���ʱ��0.5s
                        yield return new WaitForSeconds(0.5f);
                        //���ķ���
                        GoToBottom = false;
                    }
                    else
                    {
                        //����֮
                        //�ڸǵ�ǰ����ͼ
                        BackgroundCover.style.top = new StyleLength(new Length(0, LengthUnit.Percent));
                        //���ʱ��0.5s
                        yield return new WaitForSeconds(0.5f);
                        //���ı���ͼ
                        Background.style.backgroundImage = new StyleBackground(ShowedLocation.Background);
                        //���ĵص�������ʾ
                        LocationLabel.text = ShowedLocation.Name;
                        //��ʾ��ǰ����ͼ
                        BackgroundCover.style.bottom = new StyleLength(new Length(100, LengthUnit.Percent));
                        //���ʱ��0.5s
                        yield return new WaitForSeconds(0.5f);
                        //���ķ���
                        GoToBottom = true;
                    }

                    //������ʾʱ�䣬�����ڼ�̶�Ϊ5��
                    ShowedLocationDuration = 5f;
                }
            }
        }
        //��ʾ�б������һ�����
        private IEnumerator ShowLastPanel(bool isReturn = false)
        {
            //����Cover�Ķ�������ʾ���
            if (GoToRight)
            {
                //���������Ҳ����죬�����ȵ����Ҳൽ0
                InterfaceCover.style.right = new StyleLength(new Length(0, LengthUnit.Percent));
                //�ȴ�0.5s
                yield return new WaitForSeconds(0.5f);

                //�رյ�ǰ�����ʾ
                ShowedPanels.ForEach(panel => panel.style.display = DisplayStyle.None);
                //����Ƿ�Ϊ����
                if (isReturn)
                {
                    //���ǣ��Ƴ����һ��
                    ShowedPanels.Remove(ShowedPanels.Last());
                }
                //�趨������һλΪ��ʾ
                ShowedPanels[^1].style.display = DisplayStyle.Flex;
                //���ر���ɲٿ���
                ShowedPanels[^1].SetEnabled(false);
                //�ȴ�����ǰ֡����
                yield return new WaitForEndOfFrame();

                //�ٽ�������Ϊ100
                InterfaceCover.style.left = new StyleLength(new Length(100, LengthUnit.Percent));
                //�ȴ�0.5s
                yield return new WaitForSeconds(0.5f);

                //�������ɲٿ���
                ShowedPanels[^1].SetEnabled(true);

                //�������췽��
                GoToRight = false;
            }
            else
            {
                //�����ǣ���֮
                InterfaceCover.style.left = new StyleLength(new Length(0, LengthUnit.Percent));
                //�ȴ�0.5s
                yield return new WaitForSeconds(0.5f);

                //�رյ�ǰ�����ʾ
                ShowedPanels.ForEach(panel => panel.style.display = DisplayStyle.None);
                //����Ƿ�Ϊ����
                if (isReturn)
                {
                    //���ǣ��Ƴ����һ��
                    ShowedPanels.Remove(ShowedPanels.Last());
                }
                //�趨������һλΪ��ʾ
                ShowedPanels[^1].style.display = DisplayStyle.Flex;
                //���ر���ɲٿ���
                ShowedPanels[^1].SetEnabled(false);
                //�ȴ�����ǰ֡����
                yield return new WaitForEndOfFrame();

                //�ٽ�������Ϊ100
                InterfaceCover.style.right = new StyleLength(new Length(100, LengthUnit.Percent));
                //�ȴ�0.5s
                yield return new WaitForSeconds(0.5f);

                //�������ɲٿ���
                ShowedPanels[^1].SetEnabled(true);

                //�������췽��
                GoToRight = true;
            }
        }
        #endregion
    }
}

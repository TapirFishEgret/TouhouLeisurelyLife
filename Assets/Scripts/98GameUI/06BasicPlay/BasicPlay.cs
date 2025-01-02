using System.Collections;
using System.Linq;
using THLL.CharacterSystem;
using THLL.TimeSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BasicPlay : BaseGameInterface
    {
        #region VisualTreeAsset数据
        //历史记录面板
        public VisualTreeAsset VTA_History;
        //角色选择器面板
        public VisualTreeAsset VTA_CharacterSelector;
        //地图面板
        public VisualTreeAsset VTA_Map;
        #endregion

        #region 信息面板
        //信息面板
        public VisualElement InfoPanel { get; private set; }
        //时间信息面板
        public VisualElement TimeInfoPanel { get; private set; }
        //年标签
        public Label YearLabel { get; private set; }
        //月标签
        public Label MonthLabel { get; private set; }
        //日标签
        public Label DayLabel { get; private set; }
        //星期标签
        public Label DayOfWeekLabel { get; private set; }
        //当日时间标签
        public Label DayTimeLabel { get; private set; }
        //地点信息面板
        public VisualElement LocationInfoPanel { get; private set; }
        //地点标签
        public Label LocationLabel { get; private set; }
        #endregion

        #region 角色面板
        //角色面板
        public VisualElement CharacterPanel { get; private set; }
        //角色立绘显示容器
        public VisualElement CharacterPortraitContainer { get; private set; }
        //角色立绘显示
        public Image CharacterPortrait { get; private set; }
        #endregion

        #region 功能面板
        //功能面板
        public VisualElement FunctionPanel { get; private set; }
        //历史记录面板
        public History HistoryPanel { get; private set; }
        //角色选择器面板
        public CharacterSelector CharacterSelector { get; private set; }
        //地图面板
        public Map MapPanel { get; private set; }
        #endregion

        #region 操作面板
        //操作面板
        public VisualElement OperationPanel { get; private set; }
        //角色名称容器
        public VisualElement CharacterNameContainer { get; private set; }
        //角色名称标签
        public Label CharacterNameLabel { get; private set; }
        //交互按钮面板
        public ScrollView InteractionButtonsArea { get; private set; }
        //对话面板
        public VisualElement DialogArea { get; private set; }
        //对话
        public Label DialogLabel { get; private set; }
        #endregion

        #region 初始化与相关方法
        //获取界面元素
        protected override void GetVisualElements()
        {
            //信息面板
            InfoPanel = Document.rootVisualElement.Q<VisualElement>("InfoPanel");
            //时间信息
            TimeInfoPanel = InfoPanel.Q<VisualElement>("TimeInfoPanel");
            YearLabel = TimeInfoPanel.Q<Label>("YearLabel");
            MonthLabel = TimeInfoPanel.Q<Label>("MonthLabel");
            DayLabel = TimeInfoPanel.Q<Label>("DayLabel");
            DayOfWeekLabel = TimeInfoPanel.Q<Label>("DayOfWeekLabel");
            DayTimeLabel = TimeInfoPanel.Q<Label>("DayTimeLabel");
            //地点信息
            LocationInfoPanel = InfoPanel.Q<VisualElement>("LocationInfoPanel");
            LocationLabel = InfoPanel.Q<Label>("LocationLabel");

            //角色面板
            CharacterPanel = Document.rootVisualElement.Q<VisualElement>("CharacterPanel");
            CharacterPortraitContainer = CharacterPanel.Q<VisualElement>("CharacterPortraitContainer");

            //功能面板
            FunctionPanel = Document.rootVisualElement.Q<VisualElement>("FunctionPanel");
            HistoryPanel = new(this, VTA_History, FunctionPanel);
            CharacterSelector = new(this, VTA_CharacterSelector, FunctionPanel);
            MapPanel = new(this, VTA_Map, FunctionPanel);

            //操作面板
            OperationPanel = Document.rootVisualElement.Q<VisualElement>("OperationPanel");
            CharacterNameContainer = OperationPanel.Q<VisualElement>("CharacterNameContainer");
            CharacterNameLabel = OperationPanel.Q<Label>("CharacterNameLabel");
            InteractionButtonsArea = OperationPanel.Q<ScrollView>("InteractionButtonsArea");
            DialogArea = OperationPanel.Q<VisualElement>("DialogArea");
            DialogLabel = OperationPanel.Q<Label>("DialogLabel");
            OperationPanel.Q<Button>("HistoryButton").clicked += () => HistoryPanel.SwitchPanel();
            OperationPanel.Q<Button>("CharacterSelectorButton").clicked += () => CharacterSelector.SwitchPanel();
            OperationPanel.Q<Button>("MapButton").clicked += () => MapPanel.SwitchPanel();
        }
        #endregion

        #region 周期函数
        //Awake
        protected override void Awake()
        {
            //父级Awake方法
            base.Awake();
            //向角色立绘容器中添加角色立绘Image组件
            CharacterPortrait = new()
            {
                name = "CharacterPortrait"
            };
            CharacterPortraitContainer.Add(CharacterPortrait);
        }
        //Update
        private void Update()
        {
            //更新秒数
            if (GameUI.ShowSeconds)
            {
                DayTimeLabel.text = $"{GameTime.Hour:D2}:{GameTime.Minute:D2}:{GameTime.Second:D2}";
            }
            else
            {
                DayTimeLabel.text = $"{GameTime.Hour:D2}:{GameTime.Minute:D2}";
            }
            //更新日期
            DayLabel.text = $"{GameTime.Day:D2}日";
            //更新星期
            DayOfWeekLabel.text = GameTime.DayOfWeekName;
            //更新月份
            if (GameUI.UseMonthName)
            {
                MonthLabel.text = GameTime.MonthName;
            }
            else
            {
                MonthLabel.text = $"{GameTime.Month:D2}月";
            }
            //更新年份
            if (GameUI.UseGensokyoYear)
            {
                YearLabel.text = $"{GameTime.GensokyoYear}之年";
            }
            else
            {
                YearLabel.text = $"第{GameTime.Year:D3}季";
            }
        }
        #endregion

        #region 外部方法
        //切换角色
        public void SwitchCharacter(Character character, string portraitName = "0")
        {
            //首先判断当前显示角色与传入角色是否相同
            if (GameUI.CurrentShowedCharacter == character)
            {
                //若相同，则不进行切换
                return;
            }
            //若不同，首先切换当前显示角色
            GameUI.CurrentShowedCharacter = character;
            //然后判断传入角色是否为空
            if (character == null)
            {
                //若传入角色为空，则清空角色立绘显示
                CharacterPortrait.GradientSwitchImage(null, 0.3f);
            }
            else
            {
                //若不为空，更改角色名称标签
                CharacterNameLabel.GradientDisplayText(character.Name, 0.3f);
                //检测是否要显示角色立绘
                if (GameUI.ShowCharacter == true)
                {
                    //若要显示角色立绘，则进行获取
                    Sprite portrait = character.GetPortrait(portraitName);
                    //随后设置
                    CharacterPortrait.GradientSwitchImage(portrait, 0.3f, () =>
                    {
                        //设置角色立绘的大小
                        if (CharacterPortrait.sprite != null)
                        {
                            //获取显示的角色立绘的原本大小
                            Rect originalSize = CharacterPortrait.sprite.rect;
                            //计算缩放值
                            float scale = CharacterPortrait.contentRect.height / originalSize.height;
                            //设置缩放值
                            CharacterPortrait.style.width = originalSize.width * scale;
                        }
                    });
                }
            }
        }
        //显示对话
        public void ShowDialog(string dialog, Color textColor = default)
        {
            //开启协程
            StartCoroutine(ShowDialogCoroutine(dialog, textColor));
        }
        #endregion

        #region 内部方法
        //显示对话协程
        private IEnumerator ShowDialogCoroutine(string dialog, Color textColor = default)
        {
            //切换到对话显示面板
            yield return SwitchToDialog();
            //显示对话内容
            DialogLabel.ProgressiveDisplayText(dialog, 1f, () => { DialogLabel.style.color = textColor; });
        }
        //切换到对话显示
        private IEnumerator SwitchToDialog()
        {
            //判断当前操作区域
            if (InteractionButtonsArea.resolvedStyle.display == DisplayStyle.Flex)
            {
                //若当前操作区域为交互按钮，则首先隐藏交互区域
                InteractionButtonsArea.style.opacity = 0;
                //等待0.5秒
                yield return new WaitForSeconds(0.3f);

                //切换显示状态
                InteractionButtonsArea.style.display = DisplayStyle.None;
                DialogArea.style.display = DisplayStyle.Flex;

                //显示对话区域
                DialogArea.style.opacity = 1;
                //设置文本为空
                DialogLabel.text = "";
                //等待0.5秒
                yield return new WaitForSeconds(0.3f);
            }
        }
        //切换到交互按钮显示
        private IEnumerator SwitchToInteraction()
        {
            //判断当前操作区域
            if (DialogArea.resolvedStyle.display == DisplayStyle.Flex)
            {
                //若当前操作区域为对话区域，则首先隐藏对话区域
                DialogArea.style.opacity = 0;
                //等待0.5秒
                yield return new WaitForSeconds(0.3f);

                //切换显示状态
                DialogArea.style.display = DisplayStyle.None;
                InteractionButtonsArea.style.display = DisplayStyle.Flex;

                //显示交互区域
                InteractionButtonsArea.style.opacity = 1;
                //等待0.5秒
                yield return new WaitForSeconds(0.3f);
            }
        }
        #endregion
    }
}

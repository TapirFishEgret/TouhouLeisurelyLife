using THLL.TimeSystem;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class BasicPlay : BaseGameInterface
    {
        #region VisualTreeAsset数据
        //历史记录面板
        public VisualTreeAsset VTA_History;
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
        //角色立绘显示
        public VisualElement CharacterPortrait { get; private set; }
        #endregion

        #region 功能面板
        //功能面板
        public VisualElement FunctionPanel { get; private set; }
        //历史记录面板
        public History HistoryPanel { get; private set; }
        #endregion

        #region 操作面板
        //操作面板
        public VisualElement OperationPanel { get; private set; }
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
            CharacterPortrait = CharacterPanel.Q<VisualElement>("CharacterPortrait");

            //功能面板
            FunctionPanel = Document.rootVisualElement.Q<VisualElement>("FunctionPanel");
            HistoryPanel = new(this, VTA_History, FunctionPanel);

            //操作面板
            OperationPanel = Document.rootVisualElement.Q<VisualElement>("OperationPanel");
            CharacterNameLabel = OperationPanel.Q<Label>("CharacterNameLabel");
            InteractionButtonsArea = OperationPanel.Q<ScrollView>("InteractionButtonsArea");
            DialogArea = OperationPanel.Q<VisualElement>("DialogArea");
            DialogLabel = OperationPanel.Q<Label>("DialogLabel");
            OperationPanel.Q<Button>("HistoryButton").clicked += () => HistoryPanel.ShowPanel();
        }
        #endregion

        #region 周期函数
        //Update
        private void Update()
        {
            //更新UI信息
            UpdateTimeInfo();
            //TODO:更新地点信息
        }
        #endregion

        #region UI更新方法
        //更新时间UI内容
        private void UpdateTimeInfo()
        {
            //更新秒数
            if (GameTime.ShowSeconds)
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
            if (GameTime.UseMonthName)
            {
                MonthLabel.text = GameTime.MonthName;
            }
            else
            {
                MonthLabel.text = $"{GameTime.Month:D2}月";
            }
            //更新年份
            if (GameTime.UseGensokyoYear)
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
        //操作区域切换
        public void SwitchOperationArea()
        {
            //判断当前操作区域
            if (InteractionButtonsArea.style.display == DisplayStyle.Flex)
            {
                //若当前操作区域为交互按钮，则切换到对话区域
                InteractionButtonsArea.GradientSwitchElement(DialogArea, 0.5f);
            }
            else
            {
                //若当前操作区域为对话区域，则切换到交互按钮区域
                InteractionButtonsArea.GradientSwitchElement(DialogArea, 0.5f);
            }
        }
        #endregion
    }
}

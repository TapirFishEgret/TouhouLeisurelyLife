namespace THLL.SceneSystem
{
    public class ScenePath
    {
        #region 数据
        //场景A-ID
        public string SceneAID { get; set; } = string.Empty;
        //场景B-ID
        public string SceneBID { get; set; } = string.Empty;
        //距离
        public int Distance { get; set; } = 0;
        #endregion

        #region 方法重载
        public override string ToString()
        {
            return $" {SceneAID} 到 {SceneBID} 的路径，距离 {Distance} 。";
        }
        #endregion
    }
}

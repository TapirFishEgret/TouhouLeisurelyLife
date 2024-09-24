using THLL.BaseSystem;

namespace THLL.CharacterSystem
{
    public class CharacterData : BaseGameData
    {
        #region 数据
        //角色所属作品名
        public string Series { get; set; }
        //角色所属组织名
        public string Group { get; set; }
        //版本名
        public string Version { get; set; }
        #endregion

        #region 构造函数
        //无参
        public CharacterData()
        {

        }
        //有参
        public CharacterData(string id, string name, string description, int sortOrder, string series, string group, string version)
            : base(id, name, description, sortOrder)
        {
            Series = series;
            Group = group;
            Version = version;
        }
        #endregion
    }
}

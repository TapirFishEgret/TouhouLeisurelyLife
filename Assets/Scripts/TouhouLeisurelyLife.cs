using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.UISystem;

namespace THLL
{
    public static class TouhouLeisurelyLife
    {
        #region ����
        //���õĹ�������
        public static HashSet<GameBehaviour> Managers { get; } = new();
        #endregion

        #region ����
        //�������й�����
        public static void EnableAllManager()
        {
            foreach (GameBehaviour manager in Managers)
            {
                manager.enabled = true;
            }
        }
        //���ó���UI����������Դ���ع�������������й�����
        public static void DisableAllManager()
        {
            foreach (GameBehaviour manager in Managers)
            {
                if (!(manager.GetType() == typeof(GameUIManager) || manager.GetType() == typeof(GameAssetsManager)))
                {
                    manager.enabled = false;
                }
            }
        }
        #endregion
    }
}
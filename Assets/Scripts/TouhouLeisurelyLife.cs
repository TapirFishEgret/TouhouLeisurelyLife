using System;
using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.UISystem;

namespace THLL
{
    public static class TouhouLeisurelyLife
    {
        #region ����
        //�汾���ַ���
        public const string Version = "0.0.2_2024.10.14_17:24";
        //���õĹ�������
        public static HashSet<GameBehaviour> Managers { get; } = new();
        #endregion

        #region ����
        //����ͼƬ�ļ���չ��
        public static HashSet<string> ImageExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".png", ".jpeg", ".bmp", ".webp", ".tiff", ".tif" };
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

using THLL.BaseSystem;
using System.Collections.Generic;
using UnityEngine;

namespace THLL.GeographySystem
{
    public class World : BaseGameEntity<WorldData>
    {
        #region ����
        //����ӵ�е���
        public List<Realm> Realms { get; } = new();
        //TODO:���������ɫ
        #endregion

        #region ���켰��ʼ��
        //��ʼ��
        public override void Init()
        {
            //��ʱûʲôҪ����
        }
        #endregion
    }
}

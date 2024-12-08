using System.Collections;
using System.Collections.Generic;
using THLL.BaseSystem;
using THLL.UISystem.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class GameUIManager : Singleton<GameUIManager>
    {
        #region ����
        //����Э�̿�
        private Dictionary<VisualElement, Coroutine> AnimationCoroutinesDict { get; set; } = new();
        #endregion

        #region Unity���ں���
        //Awake
        protected override void Awake()
        {
            //���û���
            base.Awake();

            //UI�������Ƚ����⣬ֱ���趨Ϊ���ð�
            enabled = true;

            //��ȡ���
            GetInterfaces();
        }
        //Start
        private void Start()
        {
            //��ʾ�����
            GameUI.MainTitleInterface.Show(false);
        }
        #endregion

        #region ��ʼ����ط���
        //��ȡ���
        private void GetInterfaces()
        {
            //ϵͳ������
            GameUI.MainTitleInterface = GetComponentInChildren<MainTitle>();
            GameUI.NewGameInterface = GetComponentInChildren<NewGame>();
            GameUI.SaveAndLoadGameInterface = GetComponentInChildren<SaveAndLoadGame>();
            GameUI.GameSettingsInterface = GetComponentInChildren<GameSettings>();
            GameUI.GameSystemSettingsInterface = GetComponentInChildren<GameSystemSettings>();
            GameUI.GameplaySettingsInterface = GetComponentInChildren<GameplaySettings>();
            GameUI.GamePatchesSettingsInterface = GetComponentInChildren<GamePatchesSettings>();
            //�������
            GameUI.BasicPlayInterface = GetComponentInChildren<BasicPlay>();
            //�������
            GameUI.BackgroundLayer = GetComponentInChildren<BackgroundLayer>();
            GameUI.AnimationLayer = GetComponentInChildren<AnimationLayer>();
        }
        #endregion

        #region ����Э�̿���ط���
        //���Э��
        public void AddCoroutine(VisualElement key, IEnumerator routine)
        {
            //����ӦԪ���Ƿ���Э��������
            if (IsRunningCoroutine(key))
            {
                //ֹͣ��ǰЭ��
                StopCoroutine(AnimationCoroutinesDict[key]);
            }
            //����Э��
            Coroutine coroutine = StartCoroutine(routine);
            //��ӵ��ֵ�
            AnimationCoroutinesDict[key] = coroutine;
        }
        //�Ƴ�Э��
        public void RemoveCoroutine(VisualElement key)
        {
            if (AnimationCoroutinesDict.Remove(key, out Coroutine coroutine))
            {
                //ֹͣЭ��
                StopCoroutine(coroutine);
            }
        }
        //���ĳ���Ӿ�Ԫ���Ƿ���Э��������
        public bool IsRunningCoroutine(VisualElement key)
        {
            return AnimationCoroutinesDict.ContainsKey(key);
        }
        //ֹͣ�����ڿ��е�Э��
        public void StopAllCoroutinesInDict()
        {
            foreach (var coroutine in AnimationCoroutinesDict.Values)
            {
                StopCoroutine(coroutine);
            }
            AnimationCoroutinesDict.Clear();
        }
        #endregion
    }
}

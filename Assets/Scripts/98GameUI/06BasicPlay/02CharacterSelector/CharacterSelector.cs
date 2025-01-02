using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THLL.CharacterSystem;
using THLL.PlaySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class CharacterSelector : BaseGamePanel
    {
        #region 构造及初始化及相关方法
        public CharacterSelector
            (BaseGameInterface @interface,
            VisualTreeAsset visualTreeAsset,
            VisualElement parentPanel) 
            : base(@interface, visualTreeAsset, parentPanel) { }
        #endregion

        #region 显示及隐藏
        //显示面板
        protected override void ShowPanel()
        {
            //调用父级
            base.ShowPanel();
            //填充面板
            FillPanel();
        }
        //隐藏面板
        protected override void HidePanel()
        {
            //调用父级
            base.HidePanel();
            //清空面板
            ContainerPanel.Clear();
        }
        #endregion

        #region 内部方法
        //创建角色容器
        private VisualElement CreateCharacterContainer()
        {
            //新建容器
            VisualElement container = new()
            {
                //设置名称
                name = "CharacterContainer",
                //设置样式
                style =
                {
                    //设置外边距
                    marginTop = 5,
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5,
                    //设置内边距
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5,
                }
            };
            //新建图片容器
            Image characterImage = new()
            {
                //设置名称
                name = "Avatar",
                //设置样式
                style =
                {
                    //设置宽高
                    width = 250,
                    height = 250,
                }
            };
            //新建角色名称标签
            Label characterName = new()
            {
                //设置名称
                name = "Name",
                //设置文本
                text = "角色名称",
                //设置样式
                style =
                {
                    //设置字体大小
                    fontSize = 32,
                    //设置字体颜色
                    color = Color.white,
                    //设置字体居中
                    unityTextAlign = TextAnchor.MiddleCenter,
                    //设置外边距
                    marginTop = 2,
                    marginBottom = 2,
                    marginLeft = 2,
                    marginRight = 2,
                    //设置内边距
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                }
            };
            //添加到容器
            container.Add(characterImage);
            container.Add(characterName);
            //返回容器
            return container;
        }
        //设置角色容器
        private void SetCharacterContainer(Character character, VisualElement container)
        {
            //首先设定角色名称
            container.Q<Label>("Name").text = character.Name;
            //然后设定角色颜色
            container.Q<Label>("Name").style.color = character.Color;
            //然后设定角色头像
            container.Q<Image>("Avatar").sprite = character.GetAvatar();
            //接着设定点击事件
            container.RegisterCallback<MouseDownEvent>(evt =>
            {
                //检测按键
                if (evt.button == 0)
                {
                    //若为左键，选择角色
                    GamePlay.SelectCharacter(character);
                }
            });
        }
        //填充面板方法
        private void FillPanel()
        {
            //清空面板
            ContainerPanel.Clear();
            //检测当前场景是否为空
            if (GamePlay.CurrentScene == null)
            {
                //若为空，则不显示角色列表
                return;
            }
            //遍历角色列表，并为每个角色创建容器
            foreach (Character character in GamePlay.CurrentScene.CharactersInScene)
            {
                //创建容器
                VisualElement container = CreateCharacterContainer();
                //设置容器
                SetCharacterContainer(character, container);
                //添加到面板
                ContainerPanel.Add(container);
            }
        }
        #endregion
    }
}

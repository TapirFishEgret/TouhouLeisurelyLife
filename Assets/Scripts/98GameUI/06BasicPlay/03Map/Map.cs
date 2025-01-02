using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THLL.PlaySystem;
using THLL.SceneSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace THLL.UISystem
{
    public class Map : BaseGamePanel
    {
        #region 数据
        //场景选择器
        public ScrollView SceneSelector { get; private set; }
        #endregion

        #region 构造函数与初始化
        //构造函数
        public Map(BaseGameInterface @interface, VisualTreeAsset asset, VisualElement parent) : base(@interface, asset, parent) { }
        //获取元素
        protected override void GetVisualElements()
        {
            //父级
            base.GetVisualElements();
            //获取场景选择器
            SceneSelector = this.Q<ScrollView>("SceneSelector");
        }
        #endregion

        #region 显示及隐藏
        //显示
        protected override void ShowPanel()
        {
            //父级
            base.ShowPanel();
            SetShowScene(GameScene.GetScene("Scene_TouhouProject"));
            //检测是否有场景
            if (GamePlay.CurrentScene == null)
            {
                //若无场景，直接返回
                return;
            }
            //设置展示场景
            SetShowScene(GamePlay.CurrentScene);
        }
        #endregion

        #region 私有方法
        //设置展示场景
        private void SetShowScene(Scene scene)
        {
            //清空地图与场景选择器
            ContainerPanel.Clear();
            SceneSelector.Clear();
            //设置标题
            SetSceneTitle(scene);
            //设置地图
            SetMap(scene);
            //设置场景选择器
            SetSceneSelector(scene);
        }
        //设置场景标题
        private void SetSceneTitle(Scene scene)
        {
            //设置标题
            TitleLabel.text = scene.FullName;
        }
        //设置地图
        private void SetMap(Scene scene)
        {
            //检测场景有无地图
            if (!scene.HasMap)
            {
                //如果没有地图，则生成一个“无地图”Label
                Label noMapLabel = new()
                {
                    //文本
                    text = "当前场景无地图数据",
                    //样式
                    style =
                    {
                        //居中
                        unityTextAlign = TextAnchor.MiddleCenter,
                        //字体大小
                        fontSize = 72,
                        //内外边距设为0
                        paddingLeft = 0,
                        paddingRight = 0,
                        paddingTop = 0,
                        paddingBottom = 0,
                        marginBottom = 0,
                        marginTop = 0,
                        marginLeft = 0,
                        marginRight = 0,
                        //背景黑色
                        backgroundColor = Color.black
                    }
                };
                //添加到容器
                ContainerPanel.Add(noMapLabel);
            }
            else
            {
                //如果有地图，则放入地图
                ContainerPanel.Add(scene.Map);
            }
        }
        //设置场景选择器
        private void SetSceneSelector(Scene scene)
        {
            //对传入场景的子级进行遍历
            foreach (Scene child in scene.ChildScenes)
            {
                //生成容器
                VisualElement container = new()
                {
                    //样式
                    style =
                    {
                        //横排排布
                        flexDirection = FlexDirection.Row,
                        //内外边距为5
                        paddingLeft = 5,
                        paddingRight = 5,
                        paddingTop = 5,
                        paddingBottom = 5,
                        marginBottom = 5,
                        marginTop = 5,
                        marginLeft = 5,
                        marginRight = 5,
                        //边框宽度为1
                        borderRightWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftWidth = 1,
                        borderTopWidth = 1,
                        //边框颜色为透明
                        borderBottomColor = Color.clear,
                        borderTopColor = Color.clear,
                        borderLeftColor = Color.clear,
                        borderRightColor = Color.clear,
                    }
                };
                //生成标签
                Label label = new()
                {
                    //文本
                    text = child.Name,
                    //样式
                    style =
                    {
                        //字体大小
                        fontSize = 24,
                        //外边距设为3
                        marginBottom = 3,
                        marginTop = 3,
                        marginLeft = 3,
                        marginRight = 3,
                        //背景透明
                        backgroundColor = Color.clear,
                        //可延展
                        flexGrow = 1,
                        //宽度设为80%
                        flexBasis = new StyleLength(new Length(60, LengthUnit.Percent))
                    }
                };
                //生成前往按钮
                Button button = new()
                {
                    //文本
                    text = "前往",
                    //样式
                    style =
                    {
                        //字体大小
                        fontSize = 18,
                        //内外边距设为0
                        paddingLeft = 0,
                        paddingRight = 0,
                        paddingTop = 0,
                        paddingBottom = 0,
                        marginBottom = 0,
                        marginTop = 0,
                        marginLeft = 0,
                        marginRight = 0,
                        //背景透明
                        backgroundColor = Color.clear,
                        //宽度设为20%
                        flexBasis = new StyleLength(new Length(20, LengthUnit.Percent))
                    }
                };
                //添加点击事件
                button.clicked += () =>
                {
                    //TODO:暂时先仅实现切换场景，假装自己动了，同时重设地图
                    GameUI.BackgroundLayer.SwitchSceneBackground(child, "0", () => { SetShowScene(child); });
                };
                //生成查看地图按钮
                Button mapButton = new()
                {
                    //文本
                    text = "地图",
                    //样式
                    style =
                    {
                        //字体大小
                        fontSize = 18,
                        //内外边距设为0
                        paddingLeft = 0,
                        paddingRight = 0,
                        paddingTop = 0,
                        paddingBottom = 0,
                        marginBottom = 0,
                        marginTop = 0,
                        marginLeft = 0,
                        marginRight = 0,
                        //背景透明
                        backgroundColor = Color.clear,
                        //宽度设为20%
                        flexBasis = new StyleLength(new Length(20, LengthUnit.Percent))
                    }
                };
                //添加点击事件
                mapButton.clicked += () =>
                {
                    //仅重新设置地图
                    SetShowScene(child);
                };
                //添加到容器
                container.Add(label);
                container.Add(button);
                container.Add(mapButton);
                //添加到选择器
                SceneSelector.Add(container);
            }
        }
        #endregion
    }
}

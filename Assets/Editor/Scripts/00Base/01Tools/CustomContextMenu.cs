using UnityEditor;
using UnityEngine;
using THLL.BaseSystem;

namespace THLL.GameEditor
{
    public class CustomContextMenu
    {
        //静态方法，判断菜单项是否活跃
        [MenuItem("Assets/Create Game Asset", true)]
        private static bool ValidateCreateGameAsset()
        {
            return Selection.activeObject != null;
        }

        //静态方法，构建游戏资源菜单项
        [MenuItem("Assets/Create Game Asset")]
        private static void CreateGameAsset()
        {
            //首先获取选中项
            if (Selection.activeObject == null)
            {
                //若无选中项，报错并返回
                Debug.Log("当前未选中资源！");
                return;
            }
            else
            {
                //若有选中项，获取其资源路径
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                //获取其目录
                string directory = System.IO.Path.GetDirectoryName(path);
                //创建新资源路径
                string assetPath = System.IO.Path.Combine(directory, "GameAsset_", Selection.activeObject.name, ".asset");

                //逐一判断资源类型，并创建相应资源
                if (Selection.activeObject is Sprite sprite)
                {
                    //若选中项为精灵，创建GameSprite资源
                    GameSprite gameSprite = ScriptableObject.CreateInstance<GameSprite>();
                    //赋值
                    gameSprite.asset = sprite;
                    //在硬盘上创建
                    AssetDatabase.CreateAsset(gameSprite, assetPath);
                    //并保存
                    AssetDatabase.SaveAssets();
                    //刷新资源
                    AssetDatabase.Refresh();
                }

                //输出信息
                Debug.Log("游戏资源创建完成");
            }
        }
    }
}

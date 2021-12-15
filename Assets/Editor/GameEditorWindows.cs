using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SGUIHelper = Sirenix.Utilities.Editor.GUIHelper;

public class GameEditorWindows : OdinMenuEditorWindow
{
    [MenuItem("Tools/Game Editor", priority = -100)]
    private static void Open()
    {
        var window = GetWindow<GameEditorWindows>();
        window.position = SGUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree(true)
        {
            {"游戏打包工具", BuildEditor.Instance, EditorIcons.Download},
            {"游戏开发配置", GameConfigs.Instance, EditorIcons.SettingsCog},
            {"资源生成策略", BundleEditorUtil.Instance, EditorIcons.Folder},
            {"音效设置工具", AudioClipEditor.Instance, EditorIcons.Bell},
            {"游戏配置工具", null, EditorIcons.File},
        };
        //tree.DefaultMenuStyle.IconSize = 28.00f;
        tree.Config.DrawSearchToolbar = true;

        //游戏配置工具
        tree.AddObjectAtPath("游戏配置工具/策划配置编译", new ConfigEditor()).AddIcon(EditorIcons.File);
        tree.AddObjectAtPath("游戏配置工具/界面路径映射", UIPathMapping.Instance()).AddIcon(EditorIcons.Tree);

        return tree;
    }

    protected override void OnBeginDrawEditors()
    {
        if (this.MenuTree == null || this.MenuTree.Selection == null)
            return;

        var selected = this.MenuTree.Selection.FirstOrDefault();
        var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

        // Draws a toolbar with the name of the currently selected menu item.
        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            if (selected != null)
            {
                EditorGUILayout.LabelField(selected.Name);
                if (selected.Value != null && selected.Value is Object)
                {
                    SirenixEditorFields.UnityObjectField(selected.Value as Object, selected.Value.GetType(), false);
                }
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }


    [MenuItem("Tools/场景切换/开始游戏", priority = -100)]
    private static void OpenGameStart()
    {
        EditorSceneManager.OpenScene("Assets/Basic/Scenes/GameStart.unity");
    }

    [MenuItem("Tools/场景切换/盘古", priority = -100)]
    private static void OpenPanGuEditor()
    {
        EditorSceneManager.OpenScene("Assets/Basic/Scenes/Game_PanGu.unity");
    }

    [MenuItem("Tools/场景切换/女娲", priority = -100)]
    private static void OpenNvWaEditor()
    {
        EditorSceneManager.OpenScene("Assets/Basic/Scenes/Game_NvWa.unity");
    }

    [MenuItem("Tools/场景切换/神农", priority = -100)]
    private static void OpenShenNonEditor()
    {
        EditorSceneManager.OpenScene("Assets/Basic/Scenes/Game_ShenNong.unity");
    }

    [MenuItem("Tools/场景切换/大禹", priority = -100)]
    private static void OpenDaYuEditor()
    {
        EditorSceneManager.OpenScene("Assets/Basic/Scenes/Game_DaYu.unity");
    }
}

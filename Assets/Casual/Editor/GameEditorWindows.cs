﻿using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Linq;
using UnityEditor;

public class GameEditorWindows : OdinMenuEditorWindow
{
    [MenuItem("Tools/Game Editor", priority = -100)]
    private static void Open()
    {
        var window = GetWindow<GameEditorWindows>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true)
        {
            {"游戏开发配置",GameConfigs.Instance, EditorIcons.SettingsCog},
            {"资源生成策略",BundleEditorUtil.Instance, EditorIcons.Folder},
        };
        //tree.DefaultMenuStyle.IconSize = 28.00f;
        tree.Config.DrawSearchToolbar = true;

        //游戏配置工具
        tree.AddObjectAtPath("策划配置编译", new ConfigEditor()).AddIcon(EditorIcons.File);

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
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}

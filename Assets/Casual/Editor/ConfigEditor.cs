using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class ConfigEditor
{
    public ConfigEditor()
    {
        Refresh();
    }

    [Serializable]
    public class Config
    {
        [HideLabel(),HorizontalGroup]
        public string Name;
        [HideLabel(),HorizontalGroup]
        public bool Complier;
    }

    [LabelText("配置列表")]
    public List<Config> ConfigList = new List<Config>();

    [Button("开始编译", 32), ButtonGroup(""), PropertyOrder(1)]
    public void Compiler()
    {
        string directory = Application.dataPath + "/../配置编译/";
        string fileName = directory + "files.txt";

        StringBuilder sb = new StringBuilder();
        foreach (var fileConfig in ConfigList)
        {
            if (fileConfig.Complier)
            {
                sb.AppendLine("xls/" + fileConfig.Name);
            }
        }
        File.WriteAllText(fileName, sb.ToString());

        if(string.IsNullOrEmpty(File.ReadAllText(fileName)))
        {
            Debug.LogError("没有文件参与编译！");
            return;
        }
        EditorUtils.RunBat(directory + "编译配置文件.bat", "", directory);
    }

    [Button("刷新", 32), ButtonGroup(""), PropertyOrder(2)]
    public void Refresh()
    {
        ConfigList.Clear();
        foreach (string filePath in Directory.GetFiles(Application.dataPath + "/../配置编译/xls"))
        {
            string fileName = Path.GetFileName(filePath);

            ConfigList.Add(new Config()
            {
                Name = fileName,
                Complier = false,
            });
        }
        ConfigList.Add(new Config()
        {
            Name = "*.*",
            Complier = false,
        });
    }
}

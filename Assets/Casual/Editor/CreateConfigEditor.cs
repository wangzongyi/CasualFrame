using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace UConfig
{
    public class CreateConfigEditor : EditorWindow
    {
        static CreateConfigEditor configWindow;

        [MenuItem("Tools/CreateConfig")]
        static void CreatConfig()
        {
            if (null == configWindow)
            {
                configWindow = GetWindow<CreateConfigEditor>(typeof(CreateConfigEditor));
                configWindow.position = new Rect(Screen.resolutions[0].width, Screen.resolutions[0].height - 400, 520f, 400f);
            }
        }

        static string file_floder = string.Empty;

        static string class_Name = string.Empty;

        static string config_Name = string.Empty;

        static string path;

        private void OnGUI()
        {
            //垂直布局
            GUILayout.BeginVertical();

            Title();

            fileSclect(80f);

            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 26;
            GUI.backgroundColor = new Color(128.0f / 255, 1, 128.0f / 255, 1);
            if (GUI.Button(new Rect(configWindow.position.width * 0.5f - 75, configWindow.position.height * 0.8f, 150, 50), "Create", style))
            {
                CreateAsset();
            }


            GUILayout.EndVertical();

            configWindow.Repaint();
        }
        /// <summary>
        /// 标题
        /// </summary>
        public void Title()
        {
            GUILayout.Space(10);
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 36;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.contentColor = Color.yellow;
            GUILayout.Label("UConfig", style);
            GUI.contentColor = Color.white;
            GUILayout.Space(10);

            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("LHY V 1.0", style);
        }

        /// <summary>
        /// 文件路径选择器
        /// </summary>
        private void fileSclect(float beginY)
        {

            GUILayout.BeginHorizontal();
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 18;
            GUI.Label(new Rect(20, beginY + 20f, 120f, 30f), "Config类名:", style);
            style = new GUIStyle(GUI.skin.textField);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 15;
            style.fontStyle = FontStyle.Normal;
            class_Name = GUI.TextField(new Rect(configWindow.position.width * 0.3f, beginY + 20f, 260f, 30f), class_Name, style);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 18;
            GUI.Label(new Rect(20, beginY + 80f, 120f, 30f), "打包路径:", style);

            style = new GUIStyle(GUI.skin.textField);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 15;
            style.fontStyle = FontStyle.Normal;
            file_floder = GUI.TextField(new Rect(configWindow.position.width * 0.3f, beginY + 80f, 260f, 30f), file_floder, style);

            style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 15;
            style.fontStyle = FontStyle.Bold;

            if (GUI.Button(new Rect(configWindow.position.width * 0.82f, beginY + 80f, 80, 30f), "选择路径", style))
            {
                file_floder = EditorUtility.OpenFolderPanel("选择文件夹", file_floder, "");
                PlayerPrefs.SetString("file_floder", file_floder);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 18;
            GUI.Label(new Rect(20, beginY + 140f, 120f, 30f), "配置文件名称:", style);

            style = new GUIStyle(GUI.skin.textField);
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 15;
            style.fontStyle = FontStyle.Normal;
            config_Name = GUI.TextField(new Rect(configWindow.position.width * 0.3f, beginY + 140f, 260f, 30f), config_Name, style);
            GUILayout.EndHorizontal();
        }

        static void CreateAsset()
        {
            if (class_Name.Length == 0 || config_Name.Length == 0 || file_floder.Length == 0)
            {
                Debug.LogError(string.Format("[UConfig]: 创建失败 , 信息不完整!"));
                return;
            }
            ScriptableObject config = ScriptableObject.CreateInstance(class_Name);

            if (config == null)
            {
                Debug.LogError(string.Format("[UConfig]: 创建失败 , 类名无法识别! --> {0}", class_Name));
                return;
            }
            // 自定义资源保存路径
            string path = file_floder;
            //如果项目总不包含该路径，创建一个
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            config_Name = config_Name.Replace(".asset", "");
            path = string.Format("{0}/{1}.asset", file_floder, config_Name);
            string defFilePath = "Assets/" + config_Name + ".asset";
            // 生成自定义资源到指定路径
            AssetDatabase.CreateAsset(config, defFilePath);
            File.Move(Application.dataPath + string.Format("/{0}.asset", config_Name), path);
            AssetDatabase.Refresh();
            Debug.Log(string.Format("<color=yellow>[UConfig]: 创建成功 ! --> {0}</color>", path));
            configWindow.Close();
        }
    }
}



using UnityEngine;

[DisallowMultipleComponent]
public class Game : MonoSingleton<Game>
{
    protected override void OnAwake()
    {
        RegisterEvents();
    }

    private void Start()
    {
        LoadConfigs();
        GameBegin();
    }

    private void RegisterEvents()
    {
    }

    /// <summary>
    /// 加载初始配置表
    /// </summary>
    private void LoadConfigs()
    {
        LanguageConfigManager.Instance().LoadConfig();
    }

    /// <summary>
    /// 游戏开始
    /// </summary>
    void GameBegin()
    {
        Debug.LogFormat("游戏开始：{0}", TimeManager.Instance().ServerDateTime);
        Casual.SceneManager.LoadSceneAsync("Welcome", () =>
        {
            UIManager.Instance().Open<UILogin>();
        });
    }
}

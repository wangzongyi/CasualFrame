using Proto;

public class LanguageConfigManager : BaseConfigManager<LanguageConfigManager, int, LanguageConfig>
{
    public override void Deserialize(byte[] bytes)
    {
        MapField = LanguageConfigMap.Parser.ParseFrom(bytes).Items;
    }
}

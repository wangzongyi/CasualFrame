using Proto;

public class GoodsInfoMapManager : BaseConfigManager<GoodsInfoMapManager, uint, GoodsInfo>
{
    public override void Deserialize(byte[] bytes)
    {
        MapField = GoodsInfoMap.Parser.ParseFrom(bytes).Items;
    }
}

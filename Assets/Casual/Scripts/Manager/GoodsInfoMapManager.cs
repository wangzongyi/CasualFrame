using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;

public class GoodsInfoMapManager : BaseConfigManager<GoodsInfoMapManager, uint, GoodsInfo>
{
    public GoodsInfoMapManager()
    {
        assetName = "ClientProto/goodsinfo";
    }
}

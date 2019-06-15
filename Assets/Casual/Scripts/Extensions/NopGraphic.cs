using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NopGraphic : Graphic
{
    public override void SetAllDirty() { }
    public override void SetVerticesDirty() { }
}

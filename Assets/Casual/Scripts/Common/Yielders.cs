using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yielders
{
    public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    private static readonly int MaxYielderCount = 30;

    private static Dictionary<float, WaitForSeconds> waitYielders;

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (waitYielders == null)
            waitYielders = new Dictionary<float, WaitForSeconds>();

        WaitForSeconds yield = null;

        if (!waitYielders.TryGetValue(seconds, out yield))
        {
            yield = new WaitForSeconds(seconds);
            if (waitYielders.Count <= MaxYielderCount)
                waitYielders[seconds] = yield;
        }

        return yield;
    }

    public static void ClearYielders()
    {
        if (waitYielders == null)
            return;

        waitYielders.Clear();
    }
}

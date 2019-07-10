using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : UIBase
{
    [SerializeField]
    Button btnExit;

    protected override void InitComponent()
    {
        AddClick(btnExit, OnClickExit);
    }

    private void OnClickExit()
    {
        Close();
    }
}

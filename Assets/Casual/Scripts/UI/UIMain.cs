﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : UIBase
{
    [SerializeField]
    Button btnExit, btnSetting;

    protected override void InitComponent()
    {
        AddClick(btnExit, OnClickExit);
        AddClick(btnSetting, OnClickNext);
    }

    private void OnClickExit()
    {
        Close();
    }

    private void OnClickNext()
    {
        UIManager.Instance().Open<UISetting>();
    }
}

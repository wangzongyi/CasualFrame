using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : UIBase
{
    [SerializeField]
    Button btnLogin;

    protected override void InitComponent()
    {
        AddClick(btnLogin, OnClickLogin);
    }

    private void OnClickLogin()
    {
        UIManager.Instance().Open<UIMain>();
    }
}

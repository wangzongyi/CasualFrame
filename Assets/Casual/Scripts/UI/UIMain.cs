using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : UIBase
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickBack()
    {
        UIManager.Instance().Close<UIMain>();
    }

    public void OnClickMsgBox()
    {
        UIManager.Instance().Open<UIMsgBox>("UIMsgBox");
    }

    public void OnClickShowMsgBox()
    {
        UIManager.Instance().Show<UIMsgBox>();
    }

    public void OnClickHideMsgBox()
    {
        UIManager.Instance().Hide<UIMsgBox>();
    }
}

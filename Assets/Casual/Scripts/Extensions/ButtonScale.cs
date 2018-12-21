using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// Simple example script of how a button can be scaled visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("DoTween/Interaction/Button Scale")]
public class ButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public Transform TweenTarget;
    public Vector3 Hovered = Vector3.one;
    public Vector3 Pressed = new Vector3(0.95f, 0.95f, 0.95f);
    public float Duration = 0.2f;

    Vector3 mScale;
    bool mPointerDown = false;

    void Awake()
    {
        if (TweenTarget == null) TweenTarget = transform;
        mScale = TweenTarget.localScale;
    }

    void Start()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enabled && !mPointerDown)
        {
            transform.DOScale(Vector3.Scale(mScale, Hovered), Duration);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (enabled && !mPointerDown)
        {
            transform.DOScale(mScale, Duration);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (enabled)
        {
            mPointerDown = true;
            transform.DOScale(Vector3.Scale(mScale, Pressed), Duration);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (enabled)
        {
            mPointerDown = false;
            transform.DOScale(mScale, Duration);
        }
    }
}

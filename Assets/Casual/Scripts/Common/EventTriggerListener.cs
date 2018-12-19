using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class EventTriggerListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void Delegate(GameObject go, PointerEventData eventData);
    public VoidDelegate OnClick;
    public VoidDelegate OnDown;
    public VoidDelegate OnEnter;
    public VoidDelegate OnExit;
    public VoidDelegate OnUp;
    public Delegate onBeginDrag;
    public Delegate onDrag;
    public Delegate onEndDrag;
    public VoidDelegate onSelect;
    public VoidDelegate OnUpdateSelect;

    public Action<GameObject, float> onLongPress;

    [Tooltip("How long must pointer be down on this object to trigger a long press")]
    public float durationThreshold = 0.35f;

    private bool isLongPress = false;
    private bool isPointerDown = false;
    private float timePressStarted;

    private void Update()
    {
        if (isPointerDown)
        {
            float timePass = Time.time - timePressStarted - durationThreshold;
            if(timePass > 0)
            {
                isLongPress = true;
                if (onLongPress != null) onLongPress(gameObject, timePass);
            }
        }
    }

    void OnApplicationFocus(bool focus)
    {
        if(!focus)
        {
            isPointerDown = false;
        }
    }

    public static EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>() ?? go.AddComponent<EventTriggerListener>();
        return listener;
    }

    internal void OnPointerDown(object onPointerDown)
    {
        throw new NotImplementedException();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClick != null && !isLongPress) OnClick(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isLongPress = false;
        isPointerDown = true;
        timePressStarted = Time.time;
        if (OnDown != null) OnDown(gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (OnUp != null) OnUp(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OnEnter != null) OnEnter(gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (OnExit != null) OnExit(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null) onBeginDrag(gameObject, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null) onDrag(gameObject, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (onEndDrag != null) onEndDrag(gameObject, eventData);
    }
    //public  void OnSelect(BaseEventData eventData)
    //{
    //    if (onSelect != null) onSelect(gameObject);
    //}
    //public  void OnUpdateSelected(BaseEventData eventData)
    //{
    //    if (OnUpdateSelect != null) OnUpdateSelect(gameObject);
    //}
}


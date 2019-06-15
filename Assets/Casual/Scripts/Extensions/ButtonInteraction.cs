using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;

/// <summary>
/// Simple example script of how a button can be scaled visibly when the mouse hovers over it or it gets pressed.
/// </summary>

[AddComponentMenu("DoTween/Interaction/Button Interaction"), RequireComponent(typeof(Button))]
public class ButtonInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
#if UNITY_EDITOR
    private string[] ClipNames = new string[] { "None", "Click"};
#endif
    private Button button;
    [ValueDropdown("ClipNames")]
    public string ClickAudio = "Click";
    [SerializeField]
    public Vector3 Hovered = Vector3.one;
    public Vector3 Pressed = new Vector3(0.95f, 0.95f, 0.95f);
    public float Duration = 0.2f;

    private Vector3 mScale;
    private bool mPointerDown = false;

    void Awake()
    {
        button = GetComponent<Button>();
        mScale = button.transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.enabled && button.interactable && !mPointerDown)
        {
            transform.DOScale(Vector3.Scale(mScale, Hovered), Duration);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (button.enabled && button.interactable && !mPointerDown)
        {
            transform.DOScale(mScale, Duration);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.enabled && button.interactable)
        {
            mPointerDown = true;
            transform.DOScale(Vector3.Scale(mScale, Pressed), Duration);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button.enabled && button.interactable)
        {
            mPointerDown = false;
            transform.DOScale(mScale, Duration);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button.enabled && button.interactable && !string.IsNullOrEmpty(ClickAudio) && ClickAudio != "None")
        {
            SoundManager.Instance().PlayOneShot(ClickAudio);
        }
    }
}

using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public static class DOTweenExtensions
{
    public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchoredMove(this RectTransform transform, Vector2 target, float duration)
    {
        return DOTween.To(() => transform.anchoredPosition, (x) => transform.anchoredPosition = x, target, duration);
    }
}

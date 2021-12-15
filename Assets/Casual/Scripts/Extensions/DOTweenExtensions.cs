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

    public static Tweener DOAnchoredOffsetY(this RectTransform transform, float offset, float duration)
    {
        return transform.DOAnchorPosY(transform.anchoredPosition.y + offset, duration);
    }

    public static Tweener DOAnchoredOffsetX(this RectTransform transform, float offset, float duration)
    {
        return transform.DOAnchorPosX(transform.anchoredPosition.x + offset, duration);
    }

    public static Vector3 SnapToGrid(this Grid grid, Vector3 worldPosition)
    {
        return grid.GetCellCenterWorld(grid.WorldToCell(worldPosition));
    }
}

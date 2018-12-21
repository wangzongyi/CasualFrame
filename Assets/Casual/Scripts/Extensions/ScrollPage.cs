using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ScrollPage : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public enum Movement
    {
        Horizontal,
        Vertical,
    }

    ScrollRect scrollRect;
    public Movement moveMent = Movement.Horizontal;

    //页面：0，1，2，3  索引从0开始
    //每页占的比列：0/3=0  1/3=0.333  2/3=0.6666 3/3=1
    //float[] pages = { 0f, 0.333f, 0.6666f, 1f };
    List<float> pages = new List<float>();
    [HideInInspector]
    public int currentPageIndex = 0;
    //滑动速度
    public float smooting = 4;

    //滑动的起始坐标
    float targetNormalizedPosition = 0;

    /// <summary>
    /// 用于返回一个页码，-1说明page的数据为0
    /// </summary>
    public System.Action<int, int> OnPageChanged;

    bool isDrag;

    // Use this for initialization
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        Reset();
    }

    void FixedUpdate()
    {
        UpdatePages();
        if (scrollRect && !isDrag && pages.Count > 1)
        {
            if (moveMent == Movement.Horizontal)
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetNormalizedPosition, Time.deltaTime * smooting);
            else
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetNormalizedPosition, Time.deltaTime * smooting);
        }
    }

    public void Reset()
    {
        currentPageIndex = 0;
        targetNormalizedPosition = 0f;
        scrollRect.horizontalNormalizedPosition = scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        if (!scrollRect || pages.Count <= 0)
            return;

        float deltaTime = Time.unscaledDeltaTime;
        float v0 = 0f;
        float distance = 0f;
        float normallizedPosition = 0f;
        if (moveMent == Movement.Horizontal)
        {
            v0 = scrollRect.velocity.x;
            distance = scrollRect.content.rect.width;
            normallizedPosition = scrollRect.horizontalNormalizedPosition;
        }
        else
        {
            v0 = scrollRect.velocity.y;
            distance = scrollRect.content.rect.height;
            normallizedPosition = scrollRect.verticalNormalizedPosition;
        }

        int index = 0;
        //假设离第一位最近
        float offset = Mathf.Abs(pages[index] - normallizedPosition);
        for (int i = 1; i < pages.Count; i++)
        {
            float temp = Mathf.Abs(pages[i] - normallizedPosition);
            if (temp < offset)
            {
                index = i;

                //保存当前的偏移量
                offset = temp;
            }
        }
        if (index == currentPageIndex && pages.Count > 1)
        {
            /*float av = -Mathf.Pow(scrollRect.decelerationRate, deltaTime) * v0;
            float moveTime = -v0 / av;
            float movePosition = v0 * moveTime + 0.5f * av * moveTime * moveTime;
            float moveNormalizedPosition = -movePosition / distance;*/
            float movePosition = v0 * (1 / (1 - Mathf.Pow(scrollRect.decelerationRate, deltaTime))) * deltaTime;
            float moveNormalizedPosition = -movePosition / distance ;

            float offsetNormalizedPosition = Mathf.Abs(normallizedPosition + moveNormalizedPosition - pages[index]);
            if (offsetNormalizedPosition > pages[1] * 0.5f)
            {
                index += v0 < 0 ? 1 : -1;
                index = Mathf.Clamp(index, 0, pages.Count - 1);
            }
        }

        if (index != currentPageIndex)
        {
            currentPageIndex = index;
            if (OnPageChanged != null)
                OnPageChanged(pages.Count, currentPageIndex + 1);
        }

        targetNormalizedPosition = pages[index];

        scrollRect.StopMovement();
    }

    public void UpdatePages()
    {
        // 获取子对象的数量
        int count = scrollRect.content.childCount;
        int temp = 0;
        for (int i = 0; i < count; i++)
        {
            if (scrollRect.content.GetChild(i).gameObject.activeSelf)
            {
                temp++;
            }
        }
        count = temp;

        if (pages.Count != count)
        {
            if (count != 0)
            {
                pages.Clear();
                for (int i = 0; i < count; i++)
                {
                    float page = 0;
                    if (count != 1)
                        page = i / ((float)(count - 1));
                    pages.Add(page);
                }
            }
        }
    }

    public int MoveNext()
    {
        if (!scrollRect)
            return -1;

        currentPageIndex = Mathf.Clamp(++currentPageIndex, 0, pages.Count - 1);
        targetNormalizedPosition = pages[currentPageIndex];

        OnPageChanged(pages.Count, currentPageIndex + 1);
        return currentPageIndex;
    }

    public int MovePrevious()
    {
        if (!scrollRect)
            return -1;

        currentPageIndex = Mathf.Clamp(--currentPageIndex, 0, pages.Count - 1);
        targetNormalizedPosition = pages[currentPageIndex];
        OnPageChanged(pages.Count, currentPageIndex + 1);
        return currentPageIndex;
    }

    public int LocatePage(int page)
    {
        UpdatePages();
        currentPageIndex = Mathf.Clamp(page, 0, pages.Count - 1); ;
        targetNormalizedPosition = pages[currentPageIndex];

        StartCoroutine(DoLocatePage(targetNormalizedPosition));

        return currentPageIndex;
    }
    IEnumerator DoLocatePage(float targetNormalizedPosition)
    {
        yield return Yielders.WaitForEndOfFrame;

        if (moveMent == Movement.Horizontal)
            scrollRect.horizontalNormalizedPosition = targetNormalizedPosition;
        else
            scrollRect.verticalNormalizedPosition = targetNormalizedPosition;
    }
}

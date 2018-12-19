using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Tiny Image", 12)]
    public class TinyImage : Image
    {
        public enum MirrorType
        {
            None,
            Horizontal,
            Vertical,
            Cross,
        }

        [SerializeField]
        private MirrorType m_Mirror = MirrorType.None;

        public override void SetNativeSize()
        {
            if (overrideSprite != null)
            {
                float w = overrideSprite.rect.width / pixelsPerUnit * (m_Mirror == MirrorType.None || m_Mirror == MirrorType.Vertical ? 1f : 2f);
                float h = overrideSprite.rect.height / pixelsPerUnit * (m_Mirror == MirrorType.Cross || m_Mirror == MirrorType.Vertical ? 2f : 1f);
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
                SetAllDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (m_Mirror == MirrorType.None || type != Type.Simple)
            {
                base.OnPopulateMesh(toFill);
            }
            else
            {
                GenerateSimpleSprite(toFill, preserveAspect);
            }
        }
        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        /// 用于绘图的图像尺寸。X=左，Y=底，Z=右，W=顶。
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var padding = overrideSprite == null ? Vector4.zero : Sprites.DataUtility.GetPadding(overrideSprite);
            var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);

            Rect r = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                    padding.x / spriteW,
                    padding.y / spriteH,
                    (spriteW - padding.z) / spriteW,
                    (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                var spriteRatio = size.x / size.y;
                var rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio)
                {
                    var oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * rectTransform.pivot.y;
                }
                else
                {
                    var oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * rectTransform.pivot.x;
                }
            }

            v = new Vector4(
                    r.x + r.width * v.x,
                    r.y + r.height * v.y,
                    r.x + r.width * v.z,
                    r.y + r.height * v.w
                    );

            return v;
        }

        /// <summary>
        /// Generate vertices for a simple Image.
        /// </summary>
        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Vector4 v = GetDrawingDimensions(lPreserveAspect);
            var uv = (overrideSprite != null) ? Sprites.DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

            var color32 = color;
            vh.Clear();

            if (m_Mirror == MirrorType.Horizontal)
            {
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));//0
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));//1
                vh.AddVert(new Vector3((v.x + v.z) * 0.5f, v.w), color32, new Vector2(uv.z, uv.w));//2
                vh.AddVert(new Vector3((v.x + v.z) * 0.5f, v.y), color32, new Vector2(uv.z, uv.y));//3
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.x, uv.w));//4
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.x, uv.y));//5

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.AddTriangle(3, 2, 4);
                vh.AddTriangle(4, 5, 3);
            }
            if (m_Mirror == MirrorType.Vertical)
            {
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));//0
                vh.AddVert(new Vector3(v.x, (v.y + v.w) * 0.5f), color32, new Vector2(uv.x, uv.w));//1
                vh.AddVert(new Vector3(v.z, (v.y + v.w) * 0.5f), color32, new Vector2(uv.z, uv.w));//2
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));//3
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.y));//6
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.y));//7

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.AddTriangle(1, 4, 5);
                vh.AddTriangle(5, 2, 1);
            }
            else if (m_Mirror == MirrorType.Cross)
            {
                vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));//0
                vh.AddVert(new Vector3(v.x, (v.y + v.w) * 0.5f), color32, new Vector2(uv.x, uv.w));//1
                vh.AddVert(new Vector3((v.x + v.z) * 0.5f, (v.y + v.w) * 0.5f), color32, new Vector2(uv.z, uv.w));//2
                vh.AddVert(new Vector3((v.x + v.z) * 0.5f, v.y), color32, new Vector2(uv.z, uv.y));//3
                vh.AddVert(new Vector3(v.z, (v.y + v.w) * 0.5f), color32, new Vector2(uv.x, uv.w));//4
                vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.x, uv.y));//5
                vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.y));//6
                vh.AddVert(new Vector3((v.x + v.z) * 0.5f, v.w), color32, new Vector2(uv.z, uv.y));//7
                vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.x, uv.y));//8

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.AddTriangle(3, 2, 4);
                vh.AddTriangle(4, 5, 3);

                vh.AddTriangle(1, 6, 7);
                vh.AddTriangle(7, 2, 1);
                vh.AddTriangle(2, 7, 8);
                vh.AddTriangle(8, 4, 2);
            }
        }
        /*
        /// <summary>
        /// Generate vertices for a 9-sliced Image.
        /// </summary>

        static readonly Vector2[] s_VertScratch = new Vector2[4];
        static readonly Vector2[] s_UVScratch = new Vector2[4];

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, false);
                return;
            }

            Vector4 outer, inner, padding, border;

            if (overrideSprite != null)
            {
                outer = Sprites.DataUtility.GetOuterUV(overrideSprite);
                inner = Sprites.DataUtility.GetInnerUV(overrideSprite);
                padding = Sprites.DataUtility.GetPadding(overrideSprite);
                border = overrideSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            toFill.Clear();

            for (int x = 0; x < 3; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y)
                {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;


                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
                }
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness)
                // may be slightly larger than the original rect.
                // Adjust the border to match the adjustedRect to avoid
                // small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }*/
    }
}


using UnityEngine;

namespace Quadrop.Presentation
{
    // 패턴: Object Pool — 블록 셀을 매번 생성·파괴하면 GC 스파이크가 생겨서, 미리 만든 렌더러를 켜고 끈다
    public sealed class SpriteCellPool
    {
        private readonly SpriteRenderer[] _renderers;

        public int Count => _renderers.Length;

        public SpriteCellPool(Transform parent, Sprite sprite, int count, int sortingOrder, string name)
        {
            _renderers = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                GameObject cell = new GameObject(name);
                cell.transform.SetParent(parent, false);
                SpriteRenderer spriteRenderer = cell.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingOrder = sortingOrder;
                cell.SetActive(false);
                _renderers[i] = spriteRenderer;
            }
        }

        public void Show(int index, Vector3 localPosition, Color color)
        {
            SpriteRenderer spriteRenderer = _renderers[index];
            spriteRenderer.transform.localPosition = localPosition;
            spriteRenderer.color = color;
            if (!spriteRenderer.gameObject.activeSelf)
            {
                spriteRenderer.gameObject.SetActive(true);
            }
        }

        public void SetScale(int index, Vector3 localScale)
        {
            _renderers[index].transform.localScale = localScale;
        }

        public void Hide(int index)
        {
            GameObject cell = _renderers[index].gameObject;
            if (cell.activeSelf)
            {
                cell.SetActive(false);
            }
        }

        public void HideFrom(int startIndex)
        {
            for (int i = startIndex; i < _renderers.Length; i++)
            {
                Hide(i);
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace Presentation.Map.GunLine
{
    public class GunLineView : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float flashSpeed = 5f;
        [SerializeField] private float lengthRate = 1;
        [SerializeField] private Material material;
        [SerializeField] private float textureScrollSpeed = 1f;
        
        private Coroutine _flashCoroutine;
        private Color _originalStartColor;
        private Color _originalEndColor;

        private void Awake()
        {
            _originalStartColor = lineRenderer.startColor;
            _originalEndColor = lineRenderer.endColor;
            
            if (material != null)
            {
                lineRenderer.material = material;
            }
            lineRenderer.textureMode = LineTextureMode.Tile;
        }

        public void Refresh(Vector3 position0, Vector3  position1)
        {
            Vector3 center = (position0 + position1) / 2f;
            Vector3 extend = (position1 - position0) / 2f * lengthRate;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, (center - extend) + (Vector3)offset);
            lineRenderer.SetPosition(1, (center + extend) + (Vector3)offset);
            
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        public void Remove()
        {
            lineRenderer.positionCount = 0;
            
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }
            lineRenderer.startColor = _originalStartColor;
            lineRenderer.endColor = _originalEndColor;
        }

        private IEnumerator FlashRoutine()
        {
            while (true)
            {
                float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;
                
                Color newStart = _originalStartColor;
                newStart.a = _originalStartColor.a * alpha;
                Color newEnd = _originalEndColor;
                newEnd.a = _originalEndColor.a * alpha;
                
                lineRenderer.startColor = newStart;
                lineRenderer.endColor = newEnd;
                
                if (lineRenderer.material != null)
                {
                    Vector2 currentOffset = lineRenderer.material.mainTextureOffset;
                    currentOffset.x += textureScrollSpeed * Time.deltaTime;
                    lineRenderer.material.mainTextureOffset = currentOffset;
                }
                
                yield return null;
            }
        }
    }
}
using System.Collections;
using Systems.Damage;
using Systems.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class DamageText : MonoBehaviour
    {
        private DamageExecutingContext _context;
        private ICoordinateConverter _coordinateConverter;
        [SerializeField] private Text damageText;
        [SerializeField] private Text defenseDamageText;
        [SerializeField] private float xOffset;
        [SerializeField] private float yOffset;
        [SerializeField] private float floatSpeed = 50f;
        [SerializeField] private float fadeDuration = 1.0f;

        public void Init(DamageExecutingContext context, ICoordinateConverter coordinateConverter)
        {
            _context = context;
            _coordinateConverter = coordinateConverter;

            if (context.isMiss)
            {
                damageText.text = "Miss";
                defenseDamageText.enabled = false;
            }
            else
            {
                damageText.text = context.Damage.ToString();
                defenseDamageText.enabled = context.DefenceDamage > 0;
                defenseDamageText.text = context.DefenceDamage.ToString();
            }

            Vector3 worldPosition = _coordinateConverter.CellToWorld(_context.Defender.position) + new Vector3(xOffset, yOffset, 0);
            transform.position = Camera.main.WorldToScreenPoint(worldPosition);
            Play();
        }

        private void Play()
        {
            StartCoroutine(AnimateProcess());
        }

        private IEnumerator AnimateProcess()
        {
            float elapsed = 0f;
            Color originalColor = damageText.color;
            Color defenseOriginalColor = defenseDamageText.color;
            Vector3 startPos = transform.position;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / fadeDuration;

                transform.position = startPos + Vector3.up * (floatSpeed * elapsed);

                float alpha = Mathf.Lerp(1f, 0f, normalizedTime);
                damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                defenseDamageText.color = new Color(defenseOriginalColor.r, defenseOriginalColor.g, defenseOriginalColor.b, alpha);
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}
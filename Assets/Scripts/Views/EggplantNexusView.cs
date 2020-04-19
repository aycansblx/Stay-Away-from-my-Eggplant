using System.Collections;
using UnityEngine;
using Utils;

namespace Views
{
    public class EggplantNexusView : MonoBehaviour
    {
        const VectorUtils.EasingType _EASE_ = VectorUtils.EasingType.EaseOutSine;

        [SerializeField] Transform _body;
        [SerializeField] Transform _healthBarBase;
        [SerializeField] Transform _healthBarLiquid;

        Coroutine _breathingRoutine;
        Coroutine _healthBarUpdateRoutine;

        public void SetBodyVisibility(bool value)
        {
            if (value)
            {
                _breathingRoutine = StartCoroutine(BreathingRoutine());
            }
            else
            {
                _body.localScale = Vector3.one;
                StopCoroutine(_breathingRoutine);
            }
            _body.gameObject.SetActive(value);
        }

        public void SetHealthBarVisibility(bool value)
        {
            _healthBarBase.gameObject.SetActive(value);
        }

        public void ModifyHealthBar(float value, float max)
        {
            value = Mathf.Clamp(value, 0f, max) / max;
            if (_healthBarUpdateRoutine != null)
            {
                StopCoroutine(_healthBarUpdateRoutine);
            }
            _healthBarUpdateRoutine = StartCoroutine(HealthBarUpdateRoutine(value));
        }

        IEnumerator HealthBarUpdateRoutine(float value)
        {
            Vector3 startingScale = _healthBarLiquid.localScale;
            Vector3 endingScale = new Vector3(value, 1f, 1f);

            float timer = 0f, duration = 0.5f;

            while ((timer += Time.deltaTime) < duration)
            {
                _healthBarLiquid.localScale = VectorUtils.Lerp(startingScale, endingScale, timer, duration, _EASE_);
                yield return null;
            }

            _healthBarUpdateRoutine = null;
        }

        IEnumerator BreathingRoutine()
        {
            bool direction = true;
            float timer = 0f, duration = 1f;

            while (true)
            {
                if (direction)
                {
                    _body.transform.localScale = VectorUtils.Lerp(Vector3.one, 1.2f * Vector3.one, timer, duration, _EASE_);
                }
                else
                {
                    _body.transform.localScale = VectorUtils.Lerp(1.2f * Vector3.one, Vector3.one, timer, duration, _EASE_);
                }
                if ((timer += Time.deltaTime) > duration)
                {
                    direction = !direction;
                    timer = 0f;
                }
                yield return null;
            }
        }
    }
}

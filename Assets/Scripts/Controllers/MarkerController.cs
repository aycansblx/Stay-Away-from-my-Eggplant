using System.Collections;
using Managers;
using Models;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class MarkerController : MonoBehaviour
    {
        float _speed;

        BoxCollider2D _targetBox;

        CircleCollider2D _collider;
        SpriteRenderer _spriteRenderer;

        Vector3 _source;
        Vector3 _target;

        public MarkerType Type { get; private set; }
        public Transform Target { get; private set; }

        public delegate void MarkerControllerEvent(MarkerType Type, Transform target);
        public static MarkerControllerEvent OnMarkerControllerEvent;

        void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            _collider.enabled = true;
        }

        void Update()
        {
            if (Vector3.Distance(transform.localPosition, _target) > _speed * Time.deltaTime)
            {
                transform.localPosition += (_target - _source).normalized * _speed * Time.deltaTime;
            }
            else
            {
                Vector3 ext = _targetBox.bounds.extents;

                _source = transform.localPosition = _target;
                _target = new Vector3(Random.Range(-ext.x, ext.x), Random.Range(-ext.y, ext.y), 0f);
            }
        }

        public void Initialize(BoxCollider2D targetBox, float speed)
        {
            _spriteRenderer.color = Color.white;

            _speed = speed;

            _targetBox = targetBox;

            Vector3 ext = targetBox.bounds.extents;

            _source = new Vector3(Random.Range(-ext.x, ext.x), Random.Range(-ext.y, ext.y), 0f);
            _target = new Vector3(Random.Range(-ext.x, ext.x), Random.Range(-ext.y, ext.y), 0f);

            transform.localPosition = _source;

            Type = targetBox.GetComponent<EggplantNexusController>() ? MarkerType.Nexus : MarkerType.Enemy;

            Target = targetBox.transform;
        }

        public void Clicked(UnitModel unitModel)
        {
            _collider.enabled = false;

            if (Type == MarkerType.Nexus)
            {
                Target.GetComponent<EggplantNexusController>().ModifyCurrentHealth(unitModel.Health);
            }
            else
            {
                Target.GetComponent<EnemyController>().ModifyHealth(-unitModel.Damage);
            }

            OnMarkerControllerEvent?.Invoke(Type, Target);

            StartCoroutine(DestroyRoutine());
        }

        IEnumerator DestroyRoutine()
        {
            float timer = 0f, duration = 0.5f;

            Color startingColor = _spriteRenderer.color;
            Color endingColor = startingColor;
            endingColor.a = 0f;

            while ((timer += Time.deltaTime) < duration)
            {
                transform.localScale = VectorUtils.Lerp(Vector3.one, 1.3f * Vector3.one, timer, duration, VectorUtils.EasingType.EaseOutSine);
                _spriteRenderer.color = Color.Lerp(startingColor, endingColor, timer / duration);
                yield return null;
            }

            _spriteRenderer.color = startingColor;
            transform.localScale = Vector3.one;

            PoolingManager.Instance.Add(gameObject);
        }

        public enum MarkerType { Nexus, Enemy }
    }
}

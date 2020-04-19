using System.Collections;
using Controllers;
using Managers;
using Models;
using UnityEngine;
using Utils;

namespace Views
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] Sprite[] _sprites;
        [SerializeField] Transform _swath;

        int _spriteIndex;

        float _walkRoutineTimer = float.MaxValue;

        SpriteRenderer _spriteRenderer;

        Coroutine _walkRoutine;
        Coroutine _attackRoutine;

        void Awake() { _spriteRenderer = GetComponent<SpriteRenderer>(); }

        void Start()
        {
            _spriteRenderer.sprite = _sprites[0];

            if (_swath != null)
            {
                _swath.gameObject.SetActive(true);
            }
        }

        void ResetRoutine(Coroutine routine)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
            routine = null;
        }

        public void Turn(bool left) { transform.eulerAngles = new Vector3(0f, left ? 180f : 0f, 0f); }

        public void Walk(float speed)
        {
            ResetRoutine(_walkRoutine);
            _walkRoutine = StartCoroutine(WalkRoutine(speed));
        }

        public void Stop()
        {
            ResetRoutine(_walkRoutine);

            _spriteIndex = 0;
            _walkRoutineTimer = float.MaxValue;
            _spriteRenderer.sprite = _sprites[_spriteIndex];

            if (_swath != null)
            {
                _swath.localPosition = new Vector3(0.185f, -0.195f, 0f);
                _swath.localEulerAngles = Vector3.zero;
            }
        }

        IEnumerator WalkRoutine(float speed)
        {
            while (true)
            {
                if ((_walkRoutineTimer += Time.deltaTime) > Mathf.Clamp(0.025f, 0.2f, 0.5f / speed))
                {
                    _spriteIndex = (_spriteIndex + 1) % _sprites.Length;
                    _spriteRenderer.sprite = _sprites[_spriteIndex];

                    if (_swath != null)
                    {
                        if (_spriteIndex == 1)
                        {
                            _swath.localPosition = new Vector3(0.185f, -0.07f, 0f);
                            _swath.localEulerAngles = 13f * Vector3.forward;
                        }
                        else if (_spriteIndex == 3)
                        {
                            _swath.localPosition = new Vector3(0.197f, -0.166f, 0f);
                            _swath.localEulerAngles = 23f * Vector3.back;
                        }
                        else
                        {
                            _swath.localPosition = new Vector3(0.185f, -0.195f, 0f);
                            _swath.localEulerAngles = Vector3.zero;
                        }
                    }

                    _walkRoutineTimer = 0f;
                }

                yield return null;
            }
        }

        public void UseSwath(MarkerController marker, UnitModel model)
        {
            InputManager.Instance.SetActionControlsActivity(false);
            _attackRoutine = StartCoroutine(SwathRoutine(marker, model));
        }

        IEnumerator SwathRoutine(MarkerController marker, UnitModel model)
        {
            _spriteRenderer.sprite = _sprites[1];

            Vector3 firstPosition = _swath.localPosition;
            Vector3 secondPosition = new Vector3(0.475f, 0.1f, 0f);

            Vector3 firstEuler = _swath.localEulerAngles;
            Vector3 secondEuler = new Vector3(0f, 0f, 15f);

            float timer = 0f;

            marker.Clicked(model);

            while ((timer += Time.deltaTime) < model.AttackSpeed)
            {
                _swath.localPosition = VectorUtils.Lerp(firstPosition, secondPosition, timer, model.AttackSpeed, VectorUtils.EasingType.EaseOutSine);
                _swath.localEulerAngles = VectorUtils.Lerp(firstEuler, secondEuler, timer, model.AttackSpeed, VectorUtils.EasingType.EaseOutSine);
                yield return null;
            }

            timer = 0f;

            while ((timer += Time.deltaTime) < model.AttackSpeed)
            {
                _swath.localPosition = VectorUtils.Lerp(secondPosition, firstPosition, timer, model.AttackSpeed, VectorUtils.EasingType.EaseOutSine);
                _swath.localEulerAngles = VectorUtils.Lerp(secondEuler, firstEuler, timer, model.AttackSpeed, VectorUtils.EasingType.EaseOutSine);
                yield return null;
            }

            _attackRoutine = null;
            _spriteRenderer.sprite = _sprites[0];
            InputManager.Instance.SetActionControlsActivity(true);
        }

        public void Attack(float attackSpeed)
        {
            _attackRoutine = StartCoroutine(AttackRoutine(attackSpeed));
        }

        IEnumerator AttackRoutine(float attackSpeed)
        {
            _spriteRenderer.sprite = _sprites[1];

            float timer = 0f;

            while ((timer += Time.deltaTime) < attackSpeed / 3f)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            _spriteRenderer.sprite = _sprites[0];

            yield return new WaitForSeconds(attackSpeed * 0.75f);

            _attackRoutine = null;
        }

        public void InterruptEnemy()
        {
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
            }

            if (_walkRoutine != null)
            {
                StopCoroutine(_walkRoutine);
            }
        }
    }
}

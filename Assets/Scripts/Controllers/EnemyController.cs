using System.Collections;
using Managers;
using Models;
using UnityEngine;
using Views;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        const float _RANGE_ = 1.5f;

        [SerializeField] GameObject _bloodEffect;
        [SerializeField] AudioClip _oopfhs;
        [SerializeField] AudioClip _runaway;

        [SerializeField] float _initialSpeed;
        [SerializeField] float _initialDamage;
        [SerializeField] float _initialAttackSpeed;
        [SerializeField] float _initialHealth;

        EggplantNexusController _eggplant;

        AudioSource _audioSource;

        BoxCollider2D _collider;

        UnitView _view;
        UnitModel _model;

        bool _moving = false;

        Coroutine _stunRoutine;

        public bool RunningAway { get; private set; }

        public bool CanMove { get; private set; } = true;

        public delegate void EnemyEvent(EnemyController enemy, EventType type);
        public static EnemyEvent OnEnemyEvent;

        void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _audioSource = GetComponent<AudioSource>();
            _view = GetComponent<UnitView>();
            _model = new UnitModel(_initialSpeed, _initialDamage, _initialAttackSpeed, _initialHealth);

            _eggplant = FindObjectOfType<EggplantNexusController>();
        }

        void OnEnable()
        {
            if (_collider.enabled == false)
            {
                CanMove = true;
                RunningAway = false;
                GetComponent<BoxCollider2D>().enabled = true;
                _model = new UnitModel(_initialSpeed, _initialDamage, _initialAttackSpeed, _initialHealth);
            }
        }

        void Update()
        {
            if (CanMove)
            {
                if (Vector3.Distance(_eggplant.transform.position, transform.position) < _RANGE_)
                {
                    _moving = false;
                    _view.Stop();
                    CanMove = false;
                    _view.Attack(_model.AttackSpeed);
                    _eggplant.ModifyCurrentHealth(-_model.Damage);
                    StartCoroutine(ReleaseAI());
                }
                else
                {
                    if (_moving  == false)
                    {
                        _view.Walk(_model.Speed);
                        _moving = true;
                    }
                    transform.position += (_eggplant.transform.position - transform.position).normalized * Time.deltaTime * _model.Speed;
                }
            }
        }

        IEnumerator ReleaseAI()
        {
            yield return new WaitForSeconds(_model.AttackSpeed * 2f);
            CanMove = true;
        }

        public float DirectMove(Vector3 position)
        {
            _view.Turn(position.x < transform.position.x);

            _view.Walk(_model.Speed);

            StartCoroutine(DirectMoveRoutine(position));

            _moving = true;

            return Vector3.Distance(position, transform.position) / _model.Speed;
        }

        IEnumerator DirectMoveRoutine(Vector3 position)
        {
            Vector3 direction = position - transform.position;

            while (Vector3.Distance(position, transform.position) > _model.Speed * Time.deltaTime)
            {
                transform.position += direction.normalized * _model.Speed * Time.deltaTime;
                yield return null;
            }

            transform.position = position;
            _view.Stop();
            _moving = false;
        }

        public void ModifyHealth(float amount)
        {
            if (RunningAway)
            {
                return;
            }

            _model.ModifyHealth(amount);

            if (amount < 0f)
            {
                Instantiate(_bloodEffect, transform.position, Quaternion.identity, transform.parent);
                AudioManager.Instance.PlayAudio(_audioSource, _oopfhs, false);
                _view.Stop();
                _view.InterruptEnemy();
                _moving = false;
                CanMove = false;
                if (_stunRoutine != null)
                {
                    StopCoroutine(_stunRoutine);
                }
                _stunRoutine = StartCoroutine(StunRoutine());
            }

            if (_model.Health <= 0f)
            {
                _collider.enabled = false;
                OnEnemyEvent?.Invoke(this, EventType.GET_HIT_AND_GO);
                CanMove = false;
                RunAway();
            }
            else
            {
                OnEnemyEvent?.Invoke(this, EventType.GET_HIT);
            }
        }

        IEnumerator StunRoutine()
        {
            yield return new WaitForSeconds(1f);
            _stunRoutine = null;
            SetCanMove(true);
        }

        public void RunAway()
        {
            RunningAway = true;
            Vector3 direction = transform.position - _eggplant.transform.position;

            StartCoroutine(RunAwayRoutine(direction));

            _view.Turn(direction.x < 0f);
            _view.Walk(_model.Speed * 3f);
        }

        IEnumerator RunAwayRoutine(Vector3 direction)
        {
            AudioManager.Instance.PlayAudio(_audioSource, _runaway, false);
            while (Mathf.Abs(transform.position.x) < 10f)
            {
                transform.position += direction.normalized * _model.Speed * 3f * Time.deltaTime;
                yield return null;
            }
            CanMove = true;
            PoolingManager.Instance.Add(gameObject);
        }

        public void SetCanMove(bool value)
        {
            if (!RunningAway) { CanMove = value; }
        }

        public void TurnFace(bool value)
        {
            _view.Turn(value);
        }

        public void ApplyCoefs(float speed, float damage, float attackSpeed)
        {
            _model.ModifySpeed(speed);
            _model.ModifyDamage(damage);
            _model.ModifyAttackSpeed(attackSpeed);
        }

        public enum EventType { HIT, GET_HIT, GET_HIT_AND_GO }
    }
}

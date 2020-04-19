using Views;
using Models;
using Managers;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        const float _ENEMY_RANGE_ = 1.25f;
        const float _EGGPLANT_RANGE_ = 2.25f;

        [SerializeField] EggplantNexusController _eggplant;

        [SerializeField] private float _initialSpeed;
        [SerializeField] private float _initialDamage;
        [SerializeField] private float _initialHeal;
        [SerializeField] private float _initialAttackSpeed;

        UnitView _view;
        UnitModel _model;

        bool _isWalking;

        bool _controllable;

        bool _checkNexusMarkers;
        bool _checkEnemyMarkers;

        int _moving;
        int _layerMask;

        Vector3 _direction;

        readonly Dictionary<Transform, MarkerController> _markers = new Dictionary<Transform, MarkerController>();

        public List<EnemyController> EnemyList { get; } = new List<EnemyController>();

        void Awake()
        {
            _view = GetComponent<UnitView>();
            _model = new UnitModel(_initialSpeed, _initialDamage, _initialAttackSpeed, _initialHeal);
        }

        void OnEnable()
        {
            InputManager.Instance.OnActionEvent += OnActionEvent;
            InputManager.Instance.OnDirectionEvent += OnDirectionEvent;
            PoolingManager.Instance.OnPoolingManagerEvent += OnPoolingManagerEvent;
        }

        void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnActionEvent -= OnActionEvent;
                InputManager.Instance.OnDirectionEvent -= OnDirectionEvent;
            }
            if (PoolingManager.Instance != null)
            {
                PoolingManager.Instance.OnPoolingManagerEvent -= OnPoolingManagerEvent;
            }
        }

        void Start()
        {
            _direction = Vector3.zero;
            _layerMask = LayerMask.GetMask("Marker");
        }

        void OnDirectionEvent(InputManager.EventType type)
        {
            if (!_controllable)
            {
                return;
            }

            switch (type)
            {
                case InputManager.EventType.DirectionControlsDisabled:
                    _moving = 0;
                    break;
                case InputManager.EventType.RightStarted:
                    _moving += 1;
                    break;
                case InputManager.EventType.DownStarted:
                    _moving += 2;
                    break;
                case InputManager.EventType.LeftStarted:
                    _moving += 4;
                    break;
                case InputManager.EventType.UpStarted:
                    _moving += 8;
                    break;
                case InputManager.EventType.RightEnded:
                    if (_moving % 2 == 1)
                        _moving -= 1;
                    break;
                case InputManager.EventType.DownEnded:
                    if ((_moving / 2) % 2 == 1)
                        _moving -= 2;
                    break;
                case InputManager.EventType.LeftEnded:
                    if ((_moving / 4) % 2 == 1)
                        _moving -= 4;
                    break;
                case InputManager.EventType.UpEnded:
                    if ((_moving / 8) % 2 == 1)
                        _moving -= 8;
                    break;
            }

            _direction = Vector3.zero;

            if (_moving == 1 || _moving == 3 || _moving == 9)
            {
                _direction.x = 1f;
            }
            else if (_moving == 4 || _moving == 6 || _moving == 12)
            {
                _direction.x = -1f;
            }

            if (_moving == 2 || _moving == 3 || _moving == 6)
            {
                _direction.y = -1f;
            }
            else if (_moving == 8 || _moving == 9 || _moving == 12)
            {
                _direction.y = 1f;
            }

            if (_direction.magnitude == 0f && _isWalking)
            {
                _isWalking = false;
                _view.Stop();
            }
            else
            {
                if (_direction.x != 0f)
                {
                    _view.Turn(_direction.x == -1f);
                }
                if (!_isWalking)
                {
                    _isWalking = true;
                    _view.Walk(_model.Speed);
                }
            }
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (!_controllable)
            {
                return;
            }

            if (type == InputManager.EventType.ActionControlsDisabled || type == InputManager.EventType.ActionControlsEnabled)
            {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(InputManager.Instance.ActionPosition, Vector2.zero, 10f, _layerMask);

            if (hit.collider != null)
            {
                SetControllability(false);
                _view.UseSwath(hit.transform.GetComponent<MarkerController>(), _model);
                StartCoroutine(ReleaseControls());
            }
        }

        void OnPoolingManagerEvent(GameObject gameObject, PoolingManager.EventType type)
        {
            MarkerController markerController = gameObject.GetComponent<MarkerController>();

            if (markerController != null)
            {
                if (type == PoolingManager.EventType.DESTROYED || type == PoolingManager.EventType.DISABLED)
                {
                    _markers.Remove(markerController.Target);
                }
            }

            EnemyController enemyController = gameObject.GetComponent<EnemyController>();

            if (enemyController != null)
            {
                if (type == PoolingManager.EventType.INSTANTIATED || type == PoolingManager.EventType.ENABLED)
                {
                    EnemyList.Add(enemyController);
                }

                if (type == PoolingManager.EventType.DESTROYED || type == PoolingManager.EventType.DISABLED)
                {
                    EnemyList.Remove(enemyController);
                }
            }
        }

        IEnumerator ReleaseControls()
        {
            yield return new WaitForSeconds(2f * _model.AttackSpeed + 0.1f);
            SetControllability(true);
        }

        void Update()
        {
            if (_direction.magnitude != 0f)
            {
                Vector3 pos = transform.position + _direction.normalized * _model.Speed * Time.deltaTime;

                if (pos.x < 7 && pos.x > -7 && pos.y < 4.5f && pos.y > -4.5f)
                {
                    transform.position = pos;
                }
            }
            if (_checkNexusMarkers)
            {
                CheckNexusMarkers();
            }
            if (_checkEnemyMarkers)
            {
                CheckEnemyMarkers();
            }
        }

        public void SetControllability(bool value)
        {
            _moving = 0;
            _direction = Vector2.zero;
            _controllable = value;
        }

        public void SetMarkerChecks(bool nexus, bool enemy)
        {
            _checkNexusMarkers = nexus;
            _checkEnemyMarkers = enemy;
        }

        public float DirectMove(Vector3 position)
        {
            _view.Turn(position.x < transform.position.x);

            _view.Walk(_model.Speed);

            StartCoroutine(DirectMoveRoutine(position));

            return Vector3.Distance(position, transform.position) / _model.Speed;
        }

        IEnumerator DirectMoveRoutine(Vector3 position)
        {
            Vector3 direction = position - transform.position;

            while(Vector3.Distance(position, transform.position) > _model.Speed * Time.deltaTime)
            {
                transform.position += direction.normalized * _model.Speed * Time.deltaTime;
                yield return null;
            }

            transform.position = position;
            _view.Stop();
        }

        public void CheckNexusMarkers()
        {
            if (Vector3.Distance(_eggplant.transform.position, transform.position) < _EGGPLANT_RANGE_)
            {
                if (!_markers.ContainsKey(_eggplant.transform))
                {
                    GameObject marker = PoolingManager.Instance.CreateMarker(Vector3.zero, Quaternion.identity, _eggplant.transform);
                    _markers.Add(_eggplant.transform, marker.GetComponent<MarkerController>());
                    _markers[_eggplant.transform].Initialize(_eggplant.GetComponent<BoxCollider2D>(), 1.5f);
                }
            }
            else if (_markers.ContainsKey(_eggplant.transform))
            {
                PoolingManager.Instance.Add(_markers[_eggplant.transform].gameObject);
            }
        }

        public void  CheckEnemyMarkers()
        {
            for (int i = EnemyList.Count - 1; i >= 0; i--)
            {
                if (EnemyList[i].RunningAway)
                {
                    continue;
                }
                if (Vector3.Distance(EnemyList[i].transform.position, transform.position) < _ENEMY_RANGE_)
                {
                    if (!_markers.ContainsKey(EnemyList[i].transform))
                    {
                        GameObject marker = PoolingManager.Instance.CreateMarker(Vector3.zero, Quaternion.identity, EnemyList[i].transform);
                        _markers.Add(EnemyList[i].transform, marker.GetComponent<MarkerController>());
                        _markers[EnemyList[i].transform].Initialize(EnemyList[i].GetComponent<BoxCollider2D>(), 1.5f);
                    }
                }
                else if (_markers.ContainsKey(EnemyList[i].transform))
                {
                    PoolingManager.Instance.Add(_markers[EnemyList[i].transform].gameObject);
                }
            }
        }

        public void BoostSpeed(float multiplier)
        {
            _model.ModifySpeed(multiplier);
            _view.Stop();
            _isWalking = false;
        }

        public void ModifyDamage(float  damage)
        {
            _model.ModifyDamage(damage);

        }
    }
}

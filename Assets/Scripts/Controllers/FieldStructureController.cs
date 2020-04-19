using System.Collections;
using Managers;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class FieldStructureController : MonoBehaviour
    {
        const float _TOWER_RANGE_ = 3f;
        const float _TOWER_SPEED_ = 3f;

        const float _SHED_RANGE_ = 3f;

        const float _DOG_RANGE_ = 1f;
        const float _DOG_SPEED_ = 2f;

        [SerializeField] AudioClip _clip;
        [SerializeField] Type _type;
        [SerializeField] Sprite[] _dogSprites;

        SpriteRenderer _spriteRenderer;

        AudioSource _audioSource;

        BoxCollider2D _collider;

        bool _boosted = false;
        bool _deployed;

        float _timer;

        Transform _target;

        PlayerController _player;

        Coroutine _dogWalkingRoutine;
        Coroutine _dogAttackRoutine;

        int _index;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();
            _collider = GetComponent<BoxCollider2D>();

            _player = FindObjectOfType<PlayerController>();
        }

        void Start()
        {
            _collider.enabled = false;
            _deployed = false;
            _spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        }

        void Update()
        {
            if (!_deployed)
            {
                transform.position = (Vector2)InputManager.Instance.ActionPosition;
                return;
            }

            _timer += Time.deltaTime;

            if (_type == Type.Tower)
            {
                if (_target != null)
                {
                    if (Vector3.Distance(_target.position, transform.position) < _TOWER_RANGE_)
                    {
                        if (_timer > _TOWER_SPEED_)
                        {
                            StartCoroutine(ArrowRoutine(_target));
                            _timer = 0f;
                        }
                    }
                    else
                    {
                        _target = null;
                    }
                }
                else
                {
                    float min = float.MaxValue;
                    foreach (EnemyController enemy in _player.EnemyList)
                    {
                        float distance = Vector3.Distance(enemy.transform.position, transform.position);
                        if (distance < _TOWER_RANGE_ && min > distance)
                        {
                            min = distance;
                            _target = enemy.transform;
                        }
                    }
                }
            }

            if (_type == Type.Shed)
            {
                if (_boosted)
                {
                    if (Vector3.Distance(_player.transform.position, transform.position) > _SHED_RANGE_)
                    {
                        _player.BoostSpeed(1f/2f);
                        _boosted = false;
                    }
                }
                else
                {
                    if (Vector3.Distance(_player.transform.position, transform.position) < _SHED_RANGE_) 
                    {
                        _player.BoostSpeed(2f);
                        _boosted = true;
                    }
                }
            }

            if (_type == Type.Dog)
            {
                if (_dogAttackRoutine != null)
                {
                    return;
                }

                if (_target != null)
                {
                    if (Vector3.Distance(_target.position, transform.position) < _DOG_RANGE_)
                    {
                        if (_timer > _DOG_SPEED_)
                        {
                            if (_dogWalkingRoutine != null)
                            {
                                StopCoroutine(_dogWalkingRoutine);
                            }
                            _dogWalkingRoutine = null;
                            _dogAttackRoutine = StartCoroutine(DogAttackRoutine());
                            _timer = 0f;
                        }
                    }
                    else
                    {
                        _target = null;
                    }
                }
                else
                {
                    float min = float.MaxValue;
                    foreach (EnemyController enemy in _player.EnemyList)
                    {
                        float distance = Vector3.Distance(enemy.transform.position, transform.position);
                        if (distance < _DOG_RANGE_ && min > distance)
                        {
                            min = distance;
                            _target = enemy.transform;
                        }
                    }
                    if (_target == null && _dogWalkingRoutine == null)
                    {
                        _dogWalkingRoutine = StartCoroutine(DogWalkingRoutine());
                    }
                }
            }
        }

        IEnumerator DogAttackRoutine()
        {
            Vector2 diff = _target.position - transform.position;
            if (diff.x < 0)
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
            }

            Vector3 pos = transform.position;

            float timer = 0f;
            float duration = Vector3.Distance(_target.position, transform.position) / 7f;

            AudioManager.Instance.PlayAudio(_audioSource, _clip, false);

            while ((timer += Time.deltaTime) < duration)
            {
                if (!_target.gameObject.activeInHierarchy)
                {
                    _dogAttackRoutine = null;
                    yield break;
                }
                transform.position = VectorUtils.Lerp(pos, _target.position, timer, duration, VectorUtils.EasingType.EaseInQuad);
                yield return null;
            }
            _target.GetComponent<EnemyController>().ModifyHealth(-30f);

            timer = 0f;

            while ((timer += Time.deltaTime) < duration)
            {
                transform.position = VectorUtils.Lerp(_target.position, pos, timer, duration, VectorUtils.EasingType.EaseInQuad);
                yield return null;
            }

            _dogAttackRoutine = null;
        }

        IEnumerator DogWalkingRoutine()
        {
            Vector3 position = Vector2.zero;

            position.x = Random.Range(-7f, 7f);
            position.y = Random.Range(-5f, 5f);

            if  (position.x < transform.position.x)
            {
                transform.eulerAngles = new Vector3(0f, 180f, 0f);
            }
            else
            {
                transform.eulerAngles = new Vector3(0f, 0f, 0f);
            }

            float timer = 0f;

            while(Vector3.Distance(transform.position, position) > _DOG_SPEED_ * Time.deltaTime)
            {
                transform.position += _DOG_SPEED_ * Time.deltaTime * (position - transform.position).normalized;
                yield return null;
                if ((timer+=Time.deltaTime) >  0.1f)
                {
                    timer = 0f;
                    _index = (_index + 1) % _dogSprites.Length;
                    _spriteRenderer.sprite = _dogSprites[_index];
                }
            }
            _dogWalkingRoutine = null;
        }

        IEnumerator ArrowRoutine(Transform enemy)
        {
            AudioManager.Instance.PlayAudio(_audioSource, _clip, false);
            Vector2 diff = enemy.position - transform.position;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            GameObject arrow = PoolingManager.Instance.CreateArrow(transform.position, Quaternion.Euler(0f, 0f, angle), transform.parent);

            float timer = 0f;
            float duration = Vector3.Distance(enemy.position, transform.position) / 8f;

            while ((timer += Time.deltaTime) < duration)
            {
                if (enemy == null)
                {
                    break;
                }
                arrow.transform.position = VectorUtils.Lerp(transform.position, enemy.position, timer, duration, VectorUtils.EasingType.EaseInQuad);
                yield return null;
            }
            enemy.GetComponent<EnemyController>().ModifyHealth(-20f);
            PoolingManager.Instance.Add(arrow);
        }

        public void Deploy()
        {
            if (_type != Type.Dog)
                _collider.enabled = true;
            _deployed = true;
            _spriteRenderer.color = Color.white;
            transform.position = (Vector2)InputManager.Instance.ActionPosition;
        }

        public enum Type { Tower, Shed, Dog, Cat }
    }
}

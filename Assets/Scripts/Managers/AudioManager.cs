using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Managers
{
    public class AudioManager : MonoBehaviour
    {
        const ScalarUtils.EasingType _EASE_ = ScalarUtils.EasingType.EaseOutSine;

        static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                return _instance ? _instance : _instance = FindObjectOfType<AudioManager>();
            }
        }

        [SerializeField] AudioSource _musicSource;
        [SerializeField] [Range(0f, 2f)] float _musicVolume;

        [SerializeField] AudioSource _mainEffectSource;
        [SerializeField] [Range(0f, 2f)] float _mainEffectVolume;

        [SerializeField] [Range(0f, 2f)] float _otherEffectsVolume;

        readonly List<AudioSource> _otherEffectSources = new List<AudioSource>();

        Coroutine _changeMusicVolumeRoutine;
        Coroutine _changeMainEffectVolumeRoutine;
        Coroutine _changeOtherEffectsVolumeRoutine;

        public delegate void AudioManagerEvent(EventType type);
        public AudioManagerEvent OnAudioManagerEvent;

        void Awake()
        {
            _musicSource.volume = _musicVolume;
            _mainEffectSource.volume = _mainEffectVolume;

            foreach(AudioSource source in FindObjectsOfType<AudioSource>())
            {
                if (source != _musicSource && source !=  _mainEffectSource)
                {
                    _otherEffectSources.Add(source);
                    source.volume = _otherEffectsVolume;
                }
            }
        }

        void OnEnable()
        {
            PoolingManager.Instance.OnPoolingManagerEvent += OnPoolingManagerEvent;
        }

        void OnDisable()
        {
            if (PoolingManager.Instance != null)
            {
                PoolingManager.Instance.OnPoolingManagerEvent -= OnPoolingManagerEvent;
            }
        }

        void OnPoolingManagerEvent(GameObject gameObject, PoolingManager.EventType type)
        {
            if (type == PoolingManager.EventType.INSTANTIATED)
            {
                AudioSource audioSource = gameObject.GetComponent<AudioSource>();

                if (audioSource != null)
                {
                    _otherEffectSources.Add(audioSource);
                }
            }

            if (type == PoolingManager.EventType.DESTROYED)
            {
                _otherEffectSources.Remove(gameObject.GetComponent<AudioSource>());
            }
        }

        void ResetCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        public void PlayAudio(AudioSource source, AudioClip clip, bool loop)
        {
            source.loop = loop;
            if (clip != null)
            {
                source.clip = clip;
            }
            source.Play();
        }

        public void StopAudio(AudioSource source) { source.Stop(); }

        public void ChangeMusicVolume(float value, float speed)
        {
            ResetCoroutine(_changeMusicVolumeRoutine);
            _changeMusicVolumeRoutine = StartCoroutine(ChangeMusicVolumeRoutine(value, speed));
            OnAudioManagerEvent?.Invoke(EventType.MUSIC_VOLUME_CHANGE_STARTED);
        }

        public void ChangeMainEffectVolume(float value, float speed)
        {
            ResetCoroutine(_changeMainEffectVolumeRoutine);
            _changeMainEffectVolumeRoutine = StartCoroutine(ChangeMainEffectVolumeRoutine(value, speed));
            OnAudioManagerEvent?.Invoke(EventType.MAIN_EFFECT_VOLUME_CHANGE_STARTED);
        }

        public void ChangeOtherEffectsVolume(float value, float speed)
        {
            ResetCoroutine(_changeOtherEffectsVolumeRoutine);
            _changeOtherEffectsVolumeRoutine = StartCoroutine(ChangeOtherEffectsVolumeRoutine(value, speed));
            OnAudioManagerEvent?.Invoke(EventType.OTHER_EFFECTS_VOLUME_CHANGE_STARTED);
        }

        IEnumerator ChangeMusicVolumeRoutine(float value, float speed)
        {
            float timer = 0f, starting = _musicVolume;

            while (_musicSource.volume != value)
            {
                _musicVolume = _musicSource.volume = ScalarUtils.Lerp(starting, value, timer, speed, _EASE_);
                yield return null;
                timer += Time.deltaTime / speed;
            }

            OnAudioManagerEvent?.Invoke(EventType.MUSIC_VOLUME_CHANGE_ENDED);
            _changeMusicVolumeRoutine = null;
        }

        IEnumerator ChangeMainEffectVolumeRoutine(float value, float speed)
        {
            float timer = 0f, starting = _mainEffectVolume;

            while (_mainEffectSource.volume != value)
            {
                _mainEffectVolume = _mainEffectSource.volume = ScalarUtils.Lerp(starting, value, timer, speed, _EASE_);
                yield return null;
                timer += Time.deltaTime / speed;
            }

            OnAudioManagerEvent?.Invoke(EventType.MAIN_EFFECT_VOLUME_CHANGE_ENDED);
            _changeMainEffectVolumeRoutine = null;
        }

        IEnumerator ChangeOtherEffectsVolumeRoutine(float value, float speed)
        {
            float timer = 0f, starting = _otherEffectsVolume;

            while (_otherEffectsVolume != value)
            {
                _otherEffectsVolume = ScalarUtils.Lerp(starting, value, timer, speed, _EASE_);
                foreach (AudioSource source in _otherEffectSources)
                {
                    source.volume = _otherEffectsVolume;
                }
                yield return null;
                timer += Time.deltaTime / speed;
            }

            OnAudioManagerEvent?.Invoke(EventType.OTHER_EFFECTS_VOLUME_CHANGE_ENDED);
            _changeOtherEffectsVolumeRoutine = null;
        }

        public enum EventType
        {
            MUSIC_VOLUME_CHANGE_STARTED,
            MAIN_EFFECT_VOLUME_CHANGE_STARTED,
            OTHER_EFFECTS_VOLUME_CHANGE_STARTED,
            MUSIC_VOLUME_CHANGE_ENDED,
            MAIN_EFFECT_VOLUME_CHANGE_ENDED,
            OTHER_EFFECTS_VOLUME_CHANGE_ENDED,
        }
    }
}

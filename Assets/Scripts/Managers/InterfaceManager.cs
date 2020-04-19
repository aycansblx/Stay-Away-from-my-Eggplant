using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers
{
    public class InterfaceManager : MonoBehaviour
    {
        const VectorUtils.EasingType _EASE_ = VectorUtils.EasingType.EaseOutQuad;

        static InterfaceManager _instance;

        public static InterfaceManager Instance
        {
            get
            {
                return _instance ? _instance : _instance = FindObjectOfType<InterfaceManager>();
            }
        }

        [SerializeField] AudioClip _interfaceClick;

        [SerializeField] AudioClip _dialogue;

        [SerializeField] RectTransform[] _interfaceViewsList;

        [SerializeField] TMP_Text _lootHeader;
        [SerializeField] Button _firstButton;
        [SerializeField] Button _secondButton;
        [SerializeField] TMP_Text _firstButtonTitle;
        [SerializeField] TMP_Text _firstButtonDescription;
        [SerializeField] TMP_Text _secondButtonTitle;
        [SerializeField] TMP_Text _secondButtonDescription;

        [SerializeField] TMP_Text _levelStart;

        public Button FirstLootButton { get { return _firstButton; } }
        public Button SecondLootButton { get { return _secondButton; } }

        Dictionary<string, RectTransform> _interfaceViewsTable;

        Dictionary<RectTransform, Coroutine> _interfaceCoroutines;

        AudioSource _audioSource;

        Vector2 _size;

        public delegate void InterfaceManagerEvent(string viewName, EventType type);
        public InterfaceManagerEvent OnInterfaceManagerEvent;

        void Awake()
        {
            _interfaceViewsTable = new Dictionary<string, RectTransform>();
            _interfaceCoroutines = new Dictionary<RectTransform, Coroutine>();

            foreach (RectTransform rt in _interfaceViewsList)
            {
                _interfaceViewsTable.Add(rt.name, rt);
                _interfaceCoroutines.Add(rt, null);
            }

            _size = GetComponentInParent<CanvasScaler>().referenceResolution;

            _audioSource = GetComponent<AudioSource>();
        }

        void ResetCoroutine(RectTransform uiView)
        {
            if (_interfaceCoroutines[uiView] != null)
            {
                StopCoroutine(_interfaceCoroutines[uiView]);
            }
            _interfaceCoroutines[uiView] = null;
        }

        public void ShowViews(params string[] openingViews)
        {
            foreach (string openingView in openingViews)
            {
                if (!_interfaceViewsTable.ContainsKey(openingView))
                {
                    Debug.LogError("No such an interface view :(");
                }
                else
                {
                    _interfaceViewsTable[openingView].gameObject.SetActive(true);
                    OnInterfaceManagerEvent?.Invoke(openingView, EventType.SHOW);
                }
            }
        }

        public void CloseViews(params string[] closingViews)
        {
            foreach (string closingView in closingViews)
            {
                if (!_interfaceViewsTable.ContainsKey(closingView))
                {
                    Debug.LogError("No such an interface view :(");
                }
                else
                {
                    _interfaceViewsTable[closingView].gameObject.SetActive(false);
                    OnInterfaceManagerEvent?.Invoke(closingView, EventType.HIDE);
                }
            }
        }

        public void CloseAllViews()
        {
            string[] keys = new string[_interfaceViewsList.Length];
            for (int i = 0; i < _interfaceViewsList.Length; i++)
            {
                keys[i] = _interfaceViewsList[i].name;
                OnInterfaceManagerEvent?.Invoke(keys[i], EventType.HIDE);
            }
            CloseViews(keys);
        }

        public void Slide(string view, SlideDirection direction, float duration)
        {
            RectTransform uiView = _interfaceViewsTable[view];

            Vector2 startingPosition = uiView.anchoredPosition;
            Vector2 endingPosition = GetPosition(direction);

            ResetCoroutine(uiView);

            _interfaceCoroutines[uiView] = StartCoroutine(SlideRoutine(uiView, startingPosition, endingPosition, duration));
        }

        IEnumerator SlideRoutine(RectTransform uiView, Vector2 start, Vector2 end, float duration)
        {
            OnInterfaceManagerEvent?.Invoke(uiView.name, EventType.SLIDE_STARTED);

            float timer = 0f;
            while ((timer += Time.deltaTime) < duration)
            {
                uiView.anchoredPosition = VectorUtils.Lerp(start, end, timer, duration, _EASE_);
                yield return null;
            }
            _interfaceCoroutines[uiView] = null;

            OnInterfaceManagerEvent?.Invoke(uiView.name, EventType.SLIDE_ENDED);
        }

        public Vector2 GetPosition(SlideDirection direction)
        {
            switch (direction)
            {
                case SlideDirection.RIGHT:
                    return new Vector2(_size.x, 0f);
                case SlideDirection.DOWN:
                    return new Vector2(0f, -_size.y);
                case SlideDirection.LEFT:
                    return new Vector2(-_size.x, 0f);
                case SlideDirection.UP:
                    return new Vector2(0f, _size.y);
            }
            return Vector2.zero;
        }

        public void TypeDialogue(string view, float duration)
        {
            RectTransform uiView = _interfaceViewsTable[view];
            ShowViews(view);

            ResetCoroutine(uiView);

            AudioManager.Instance.PlayAudio(_audioSource, _dialogue, true);

            TMP_Text component = uiView.GetComponent<TMP_Text>();
            _interfaceCoroutines[uiView] = StartCoroutine(TypeDialogueCoroutine(uiView, component, component.text, duration));
        }

        IEnumerator TypeDialogueCoroutine(RectTransform uiView, TMP_Text component, string text, float duration)
        {
            component.text = "";
            for (int i = 0; i < text.Length; i++)
            {
                component.text += text[i];
                yield return new WaitForSeconds(duration / text.Length);
            }
            _interfaceCoroutines[uiView] = null;
            AudioManager.Instance.StopAudio(_audioSource);
        }

        public void Blink(string view, float interval)
        {
            RectTransform uiView = _interfaceViewsTable[view];

            TMP_Text component = uiView.GetComponent<TMP_Text>();

            ResetCoroutine(uiView);

            _interfaceCoroutines[uiView] = StartCoroutine(BlinkRoutine(component, interval));
        }

        public void StopBlinking(string view)
        {
            if (_interfaceCoroutines[_interfaceViewsTable[view]] != null)
            {
                StopCoroutine(_interfaceCoroutines[_interfaceViewsTable[view]]);
            }

            TMP_Text text = _interfaceViewsTable[view].GetComponent<TMP_Text>();

            Color color = text.color;

            _interfaceViewsTable[view].GetComponent<TMP_Text>().color = new Color(color.r, color.g, color.b, 1f);

            CloseViews(view);
        }

        IEnumerator BlinkRoutine(TMP_Text component, float interval)
        {
            bool direction = false;

            float timer = 0f;

            Color startingColor = component.color;
            Color endingColor = component.color;

            endingColor.a = 0f;

            while (true)
            {
                if (!direction)
                {
                    component.color = Color.Lerp(startingColor, endingColor, timer / interval);
                }
                else
                {
                    component.color = Color.Lerp(endingColor, startingColor, timer / interval);
                }
                if ((timer += Time.deltaTime) > interval)
                {
                    direction = !direction;
                    timer = 0f;
                }
                yield return null;
            }
        }

        public void DecorateLootWindow(int cLevel, int mLevel, string b1Header, string b1Description, string b2Header, string b2description)
        {
            _lootHeader.text = "Level " + cLevel + " of " + mLevel + " CLEARED!";
            _firstButtonTitle.text = b1Header;
            _secondButtonTitle.text = b2Header;
            _firstButtonDescription.text = b1Description;
            _secondButtonDescription.text = b2description;
        }

        public void DecorateLevelStart(int cLevel)
        {
            _levelStart.text = "Wave " + cLevel + " of 7 - Ready!";
        }

        public void PlayClick()
        {
            AudioManager.Instance.PlayAudio(_audioSource, _interfaceClick, false);
        }

        public enum EventType { SHOW, HIDE, SLIDE_STARTED, SLIDE_ENDED }

        public enum SlideDirection { RIGHT, DOWN, LEFT, UP, CENTER }
    }
}

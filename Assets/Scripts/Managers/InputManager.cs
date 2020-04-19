using UnityEngine;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        static InputManager _instance;

        public static InputManager Instance
        {
            get
            {
                return _instance ? _instance : _instance = FindObjectOfType<InputManager>();
            }
        }

        bool _actionControlsEnabled;
        bool _directionControlsEnabled;

        Vector3 _lastActionPosition;

        AudioSource _audioSource;

        public Vector3 ActionPosition { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }

        public delegate void InputManagerEvent(EventType type);
        public InputManagerEvent OnDirectionEvent;
        public InputManagerEvent OnActionEvent;

        void Awake()
        {
            _actionControlsEnabled = true;
            _directionControlsEnabled = true;

            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (_actionControlsEnabled)
            {
                CheckActionControls();
            }
            if (_directionControlsEnabled)
            {
                CheckDirectionControls();
            }
        }

        void CheckActionControls()
        {
            if  (Input.GetMouseButtonDown(0))
            {
                OnActionEvent?.Invoke(EventType.ActionStarted);
                _lastActionPosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                OnActionEvent?.Invoke(EventType.ActionEnded);
            }
            if (Input.GetMouseButton(0) && !_lastActionPosition.Equals(Input.mousePosition))
            {
                OnActionEvent?.Invoke(EventType.ActionMoved);
                _lastActionPosition = Input.mousePosition;
            }
        }

        void CheckDirectionControls()
        {
            if (Input.GetKeyDown(KeyCode.W)) { OnDirectionEvent.Invoke(EventType.UpStarted); }
            if (Input.GetKeyUp(KeyCode.W)) { OnDirectionEvent.Invoke(EventType.UpEnded); }

            if (Input.GetKeyDown(KeyCode.A)) { OnDirectionEvent.Invoke(EventType.LeftStarted); }
            if (Input.GetKeyUp(KeyCode.A)) { OnDirectionEvent.Invoke(EventType.LeftEnded); }

            if (Input.GetKeyDown(KeyCode.S)) { OnDirectionEvent.Invoke(EventType.DownStarted); }
            if (Input.GetKeyUp(KeyCode.S)) { OnDirectionEvent.Invoke(EventType.DownEnded); }

            if (Input.GetKeyDown(KeyCode.D)) { OnDirectionEvent.Invoke(EventType.RightStarted); }
            if (Input.GetKeyUp(KeyCode.D)) { OnDirectionEvent.Invoke(EventType.RightEnded); }
        }

        public void SetActionControlsActivity(bool value)
        {
            _actionControlsEnabled = value;
            OnActionEvent?.Invoke(value ? EventType.ActionControlsEnabled : EventType.ActionControlsDisabled);
        }

        public void SetDirectionControlsActivity(bool value)
        {
            _directionControlsEnabled = value;
            OnDirectionEvent?.Invoke(value ? EventType.DirectionControlsEnabled : EventType.DirectionControlsDisabled);
        }

        public enum EventType
        {
            ActionStarted, ActionMoved, ActionEnded,
            RightStarted, RightEnded, LeftStarted, LeftEnded,
            UpStarted, UpEnded, DownStarted, DownEnded,
            ActionControlsEnabled, ActionControlsDisabled,
            DirectionControlsEnabled, DirectionControlsDisabled,
        }
    }
}

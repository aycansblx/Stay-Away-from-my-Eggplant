using UnityEngine;

namespace Managers
{
    public class CursorManager : MonoBehaviour
    {
        static CursorManager _instance;

        public static CursorManager Instance
        {
            get
            {
                return _instance ? _instance : _instance = FindObjectOfType<CursorManager>();
            }
        }

        [SerializeField] private Texture2D _failCursor;
        [SerializeField] private Texture2D _successCursor;

        bool _cursor;

        int _layerMask;

        public bool CursorCheck { get; set; } = true;

        void Start()
        {
            _layerMask = LayerMask.GetMask("Marker");
            SetFailCursor();
        }

        void Update()
        {
            CheckCursor();
        }

        void CheckCursor()
        {
            if (!CursorCheck)
            {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(InputManager.Instance.ActionPosition, Vector2.zero, 10f, _layerMask);
            if (hit.collider != null && !_cursor)
            {
                SetSuccessCursor();
            }
            if (hit.collider == null && _cursor)
            {
                SetFailCursor();
            }
        }

        public void SetFailCursor()
        {
            _cursor = false;
            Vector2 cursorHotspot = new Vector2(_failCursor.width / 2, _failCursor.height / 2);
            Cursor.SetCursor(_failCursor, cursorHotspot, CursorMode.Auto);
        }

        public void SetSuccessCursor()
        {
            Vector2 cursorHotspot = new Vector2(_successCursor.width / 2, _successCursor.height / 2);
            Cursor.SetCursor(_successCursor, cursorHotspot, CursorMode.Auto);
            _cursor = true;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class PoolingManager : MonoBehaviour
    {
        static PoolingManager _instance;

        public static PoolingManager Instance
        {
            get
            {
                return _instance ? _instance : _instance = FindObjectOfType<PoolingManager>();
            }
        }

        [SerializeField] GameObject _markerPrefab;
        [SerializeField] GameObject _enemyPrefab;
        [SerializeField] GameObject _arrowPrefab;

        [SerializeField] GameObject[] _fieldStructures;

        readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();

        public delegate void PoolingManagerEvent(GameObject gameObject, EventType type);
        public PoolingManagerEvent OnPoolingManagerEvent;

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!_pool.ContainsKey(prefab.name) || _pool[prefab.name].Count == 0)
            {
                GameObject instantiated = Instantiate(prefab, position, rotation, parent);

                instantiated.name = prefab.name;

                OnPoolingManagerEvent?.Invoke(instantiated, EventType.INSTANTIATED);

                return instantiated;
            }
            else
            {
                Transform enabled = _pool[prefab.name].Dequeue().transform;

                enabled.gameObject.SetActive(true);

                enabled.position = position;
                enabled.rotation = rotation;

                enabled.SetParent(parent);

                OnPoolingManagerEvent?.Invoke(enabled.gameObject, EventType.ENABLED);

                return enabled.gameObject;
            }
        }

        public void Add(GameObject toBeAdded, bool forceDestroy = false)
        {
            if (forceDestroy)
            {
                OnPoolingManagerEvent?.Invoke(toBeAdded, EventType.DESTROYED);
                Destroy(toBeAdded);
            }
            else
            {
                OnPoolingManagerEvent?.Invoke(toBeAdded, EventType.DISABLED);

                if (!_pool.ContainsKey(toBeAdded.name))
                {
                    _pool.Add(toBeAdded.name, new Queue<GameObject>());
                }

                toBeAdded.transform.SetParent(transform);
                toBeAdded.SetActive(false);
                _pool[toBeAdded.name].Enqueue(toBeAdded);
            }
        }

        public GameObject CreateMarker(Vector3 position, Quaternion rotation, Transform parent)
        {
            return Get(_markerPrefab, position, rotation, parent);
        }

        public GameObject CreateEnemy(Vector3 position, Quaternion rotation, Transform parent)
        {
            return Get(_enemyPrefab, position, rotation, parent);
        }

        public GameObject CreateArrow(Vector3 position, Quaternion rotation, Transform parent)
        {
            return Get(_arrowPrefab, position, rotation, parent);
        }

        public GameObject CreateFieldStructure(int index, Vector2 position, Quaternion rotation, Transform parent)
        {
            return Get(_fieldStructures[index - 1], position, rotation, parent);
        }

        public enum EventType { INSTANTIATED, ENABLED, DISABLED, DESTROYED }
    }
}

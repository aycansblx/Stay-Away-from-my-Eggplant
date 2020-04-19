using System.Collections;
using Controllers;
using Managers.States;
using UnityEngine;

namespace Managers
{
    public class StateManager : MonoBehaviour
    {
        static StateManager _instance;

        public static StateManager Instance
        {
            get
            {
                return _instance ? _instance : _instance = FindObjectOfType<StateManager>();
            }
        }

        public State CurrentState { get; private set; }

        Coroutine _delayedStateChangeRoutine;

        public delegate void StateChangeEvent(State previous, State next);
        public StateChangeEvent OnStateChangeEvent;

        void Start()
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            EggplantNexusController eggplantNexusController = FindObjectOfType<EggplantNexusController>();
            CurrentState = new InitialState(playerController, eggplantNexusController);
            CurrentState.Enter();
            OnStateChangeEvent?.Invoke(null, CurrentState);
        }

        void Update()
        {
            CurrentState.Update();
        }

        public void ChangeState(State next)
        {
            CurrentState.Exit();
            OnStateChangeEvent?.Invoke(CurrentState, next);
            CurrentState = next;
            CurrentState.Enter();
        }

        public void ChangeState(State next, float delay)
        {
            if (_delayedStateChangeRoutine != null)
            {
                StopCoroutine(_delayedStateChangeRoutine);
            }
            _delayedStateChangeRoutine = StartCoroutine(DelayedStateChange(next, delay));
        }

        IEnumerator DelayedStateChange(State next, float delay)
        {
            yield return new WaitForSeconds(delay);
            ChangeState(next);
            _delayedStateChangeRoutine = null;
        }
    }
}
using System.Collections;
using Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers.States
{
    public abstract class State
    {
        protected static float EnemySpeed = 1f;
        protected static float EnemyDamage = 1f;
        protected static float AttackSpeed = 1f;

        protected PlayerController _player;

        protected EggplantNexusController _eggplant;

        public State(PlayerController player, EggplantNexusController eggplant)
        {
            _player = player;
            _eggplant = eggplant;
        }

        public abstract void Enter();
        public virtual void Update() { }
        public abstract void Exit();
    }

    public class InitialState : State
    {
        public InitialState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant)
        {
            EnemySpeed = 1f;
            EnemyDamage = 1f;
            AttackSpeed = 1f;
    }

        public override void Enter()
        {
            _eggplant.SetVisibility(true, false);

            _player.transform.position = 10f * Vector3.left;
            _player.SetControllability(false);

            InputManager.Instance.SetActionControlsActivity(false);
            InputManager.Instance.OnActionEvent += OnActionEvent;

            PoolingManager.Instance.StartCoroutine(InitialStateEnterRoutine());
        }

        public override void Exit()
        {
            InterfaceManager.Instance.CloseViews("Introduction View");
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (type == InputManager.EventType.ActionStarted)
            {
                InterfaceManager.Instance.PlayClick();
                InputManager.Instance.OnActionEvent -= OnActionEvent;
                PoolingManager.Instance.StartCoroutine(InitialStateExitRoutine());
            }
        }

        IEnumerator InitialStateEnterRoutine()
        {
            yield return new WaitForSeconds(6f);

            InterfaceManager.Instance.ShowViews("Introduction View");
            InterfaceManager.Instance.Slide("Introduction View", InterfaceManager.SlideDirection.CENTER, 0.5f);

            yield return new WaitForSeconds(0.5f);

            InterfaceManager.Instance.ShowViews("Instruction View");

            InputManager.Instance.SetActionControlsActivity(true);
        }

        IEnumerator InitialStateExitRoutine()
        {
            InterfaceManager.Instance.Slide("Introduction View", InterfaceManager.SlideDirection.UP, 1f);

            yield return new WaitForSeconds(0.25f);

            InterfaceManager.Instance.CloseViews("Instruction View");

            StateManager.Instance.ChangeState(new InitialMovementState(_player, _eggplant));
        }
    }

    public class InitialMovementState : State
    {
        public InitialMovementState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            float duration = _player.DirectMove(5.5f * Vector3.left);
            StateManager.Instance.ChangeState(new InitialDialogueState(_player, _eggplant), duration + 0.5f);
        }

        public override void Exit()
        {
            AudioManager.Instance.ChangeMusicVolume(0.2f, 1f);
        }
    }

    public class InitialDialogueState : State
    {
        int _index = 1;

        public InitialDialogueState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            InputManager.Instance.OnActionEvent += OnActionEvent;
            Process();
        }

        public override void Exit()
        {
            InputManager.Instance.OnActionEvent -= OnActionEvent;
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (type == InputManager.EventType.ActionStarted)
            {
                InterfaceManager.Instance.CloseViews("Dialog # " + _index);

                if (++_index > 3)
                {
                    StateManager.Instance.ChangeState(new InitialStepState(_player, _eggplant));
                }
                else
                {
                    Process();
                }
            }
        }

        void Process()
        {
            InputManager.Instance.SetActionControlsActivity(false);
            InterfaceManager.Instance.TypeDialogue("Dialog # " + _index, 1f);
            PoolingManager.Instance.StartCoroutine(ProcessRoutine());
        }

        IEnumerator ProcessRoutine()
        {
            yield return new WaitForSeconds(1f);
            InputManager.Instance.SetActionControlsActivity(true);
        }
    }

    public class InitialStepState : State
    {
        public InitialStepState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            InterfaceManager.Instance.ShowViews("Tutorial Instruction # I");
            InterfaceManager.Instance.Blink("Tutorial Instruction # I", 1f);

            InputManager.Instance.SetDirectionControlsActivity(true);
            InputManager.Instance.SetActionControlsActivity(false);

            _player.SetControllability(true);
            _player.SetMarkerChecks(false, false);
        }

        public override void Update()
        {
            if (Vector3.Distance(_player.transform.position, _eggplant.transform.position) < 2.25f)
            {
                StateManager.Instance.ChangeState(new SecondStepState(_player, _eggplant));
            }
        }

        public override void Exit()
        {
            InterfaceManager.Instance.StopBlinking("Tutorial Instruction # I");
        }
    }

    public class SecondStepState : State
    {
        public SecondStepState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            _player.SetMarkerChecks(true, false);
            _eggplant.SetVisibility(true, true);

            InputManager.Instance.SetDirectionControlsActivity(false);
            InputManager.Instance.SetActionControlsActivity(true);

            InterfaceManager.Instance.ShowViews("Tutorial Instruction # II");
            InterfaceManager.Instance.Blink("Tutorial Instruction # II", 1f);

            MarkerController.OnMarkerControllerEvent += OnMarkerControllerEvent;
        }

        public override void Exit()
        {
            InterfaceManager.Instance.StopBlinking("Tutorial Instruction # II");
            MarkerController.OnMarkerControllerEvent -= OnMarkerControllerEvent;
        }

        void OnMarkerControllerEvent(MarkerController.MarkerType type, Transform target)
        {
            _player.SetControllability(false);
            _player.SetMarkerChecks(false, false);
            StateManager.Instance.ChangeState(new SecondMovementState(_player, _eggplant), 0.5f);
        }
    }

    public class SecondMovementState : State
    {
        EnemyController _enemycontroller;

        public SecondMovementState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            GameObject enemy = PoolingManager.Instance.CreateEnemy(9f * Vector3.right, Quaternion.identity, null);
            _enemycontroller = enemy.GetComponent<EnemyController>();
            float duration = _enemycontroller.DirectMove(5.5f * Vector3.right);
            _enemycontroller.SetCanMove(false);
            StateManager.Instance.ChangeState(new SecondDialogueState(_player, _eggplant, _enemycontroller), duration + 1f);
        }

        public override void Exit()
        {

        }
    }

    public class SecondDialogueState : State
    {
        int _index = 4;

        readonly EnemyController _enemy;

        public SecondDialogueState(PlayerController player, EggplantNexusController eggplant, EnemyController enemy) : base(player, eggplant) { _enemy = enemy; }

        public override void Enter()
        {
            InputManager.Instance.OnActionEvent += OnActionEvent;
            Process();
        }

        public override void Exit()
        {
            InputManager.Instance.OnActionEvent -= OnActionEvent;
            _enemy.SetCanMove(true);
            _player.SetControllability(true);
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (type == InputManager.EventType.ActionStarted)
            {
                InterfaceManager.Instance.CloseViews("Dialog # " + _index);

                if (++_index > 5)
                {
                    StateManager.Instance.ChangeState(new ThirdStepState(_player, _eggplant));
                }
                else
                {
                    Process();
                }
            }
        }

        void Process()
        {
            InputManager.Instance.SetActionControlsActivity(false);
            InterfaceManager.Instance.TypeDialogue("Dialog # " + _index, 1f);
            PoolingManager.Instance.StartCoroutine(ProcessRoutine());
        }

        IEnumerator ProcessRoutine()
        {
            yield return new WaitForSeconds(1f);
            InputManager.Instance.SetActionControlsActivity(true);
        }
    }

    public class ThirdStepState : State
    {
        public ThirdStepState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            InterfaceManager.Instance.ShowViews("Tutorial Instruction # III");
            InterfaceManager.Instance.Blink("Tutorial Instruction # III", 1f);

            InputManager.Instance.SetDirectionControlsActivity(true);
            InputManager.Instance.SetActionControlsActivity(true);

            _player.SetMarkerChecks(false, true);

            EnemyController.OnEnemyEvent += OnEnemyEvent;
        }

        public override void Exit()
        {
            InterfaceManager.Instance.StopBlinking("Tutorial Instruction # III");
            EnemyController.OnEnemyEvent -= OnEnemyEvent;
        }

        void OnEnemyEvent(EnemyController enemy, EnemyController.EventType type)
        {
            if (type == EnemyController.EventType.GET_HIT_AND_GO)
            {
                StateManager.Instance.ChangeState(new LootState(_player, _eggplant, 1), 1.5f);
            }
        }
    }

    public class LootState : State
    {
        string[] Button1Headers = { "Tower", "Cat", "Manure", "Faster Neighbors", "Cat", "Shed"};
        string[] Button1Descriptions = {
            "Shoots arrow to enemies in a range.",
            "It is just cute!",
            "Maximum eggplant health increases +50%.",
            "Enemy speed increases +50%.",
            "You know that one cat isn't enough :3",
            "Doubles your speed when you're near.",
        };

        string[] Button2Headers = { "Shed", "Dog",  "Cheeseburger", "Stronger Neighbors", "Dog", "Tower"};
        string[] Button2Descriptions = {
            "Doubles your speed when you're near.",
            "Randomly walks and attacks nearby enemies.",
            "Your damage increases 50%.",
            "Enemies deal +50% damage.",
            "Randomly walks and attacks nearby enemies.",
            "Shoots arrow to enemies in a range.",
        };

        int _level;

        public LootState(PlayerController player, EggplantNexusController eggplant, int level) : base(player, eggplant) { _level = level; }

        public override void Enter()
        {
            InputManager.Instance.SetActionControlsActivity(false);
            InputManager.Instance.SetDirectionControlsActivity(false);

            InterfaceManager.Instance.ShowViews("Loot");
            InterfaceManager.Instance.Slide("Loot", InterfaceManager.SlideDirection.CENTER, 1f);
            InterfaceManager.Instance.DecorateLootWindow(_level, 7, Button1Headers[_level - 1], Button1Descriptions[_level - 1], Button2Headers[_level - 1], Button2Descriptions[_level - 1]);

            InterfaceManager.Instance.FirstLootButton.onClick.AddListener(Button1);
            InterfaceManager.Instance.SecondLootButton.onClick.AddListener(Button2);
        }

        public override void Exit()
        {
            InterfaceManager.Instance.Slide("Loot", InterfaceManager.SlideDirection.DOWN, 1f);

            InterfaceManager.Instance.FirstLootButton.onClick.RemoveAllListeners();
            InterfaceManager.Instance.SecondLootButton.onClick.RemoveAllListeners();
        }

        void Button1()
        {
            switch (_level)
            {
                case 1:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 1, _level));
                    break;
                case 2:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 4, _level));
                    break;
                case 3:
                    _eggplant.ModifyMaximumHealth(50f);
                    StateManager.Instance.ChangeState(new PlayState(_player, _eggplant, _level));
                    break;
                case 4:
                    EnemySpeed = 1.5f;
                    StateManager.Instance.ChangeState(new PlayState(_player, _eggplant, _level));
                    break;
                case 5:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 4, _level));
                    break;
                case 6:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 2, _level));
                    break;
            }

            InterfaceManager.Instance.PlayClick();
        }

        void Button2()
        {
            switch (_level)
            {
                case 1:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 2, _level));
                    break;
                case 2:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 3, _level));
                    break;
                case 3:
                    _player.ModifyDamage(20f);
                    StateManager.Instance.ChangeState(new PlayState(_player, _eggplant, _level));
                    break;
                case 4:
                    EnemyDamage = 1.5f;
                    StateManager.Instance.ChangeState(new PlayState(_player, _eggplant, _level));
                    break;
                case 5:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 3, _level));
                    break;
                case 6:
                    StateManager.Instance.ChangeState(new DeployState(_player, _eggplant, 1, _level));
                    break;
            }

            InterfaceManager.Instance.PlayClick();
        }
    }

    public class DeployState : State
    {
        int _level;

        int _choice;

        bool _cursor;

        FieldStructureController _structure;

        public DeployState(PlayerController player, EggplantNexusController eggplant, int choice, int level) : base(player, eggplant) { _level = level; _choice = choice; _cursor = true; }

        public override void Enter()
        {
            CursorManager.Instance.CursorCheck = false;
            CursorManager.Instance.SetSuccessCursor();
            InterfaceManager.Instance.CloseViews("Loot");
            InputManager.Instance.OnActionEvent += OnActionEvent;
            InputManager.Instance.SetActionControlsActivity(true);
            _structure = PoolingManager.Instance.CreateFieldStructure(_choice, InputManager.Instance.ActionPosition, Quaternion.identity, _player.transform.parent).GetComponent<FieldStructureController>();
        }

        public override void Update()
        {
            RaycastHit2D hit = Physics2D.Raycast(InputManager.Instance.ActionPosition, Vector2.zero, 10f);
            if (hit.collider != null && _cursor)
            {
                _cursor = false;
                CursorManager.Instance.SetFailCursor();
            }
            if (hit.collider == null && !_cursor)
            {
                _cursor = true;
                CursorManager.Instance.SetSuccessCursor();
            }
        }

        public override void Exit()
        {
            InputManager.Instance.SetActionControlsActivity(true);
            InputManager.Instance.SetDirectionControlsActivity(true);
            CursorManager.Instance.CursorCheck = true;
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (type == InputManager.EventType.ActionStarted)
            {
                _structure.Deploy();
                StateManager.Instance.ChangeState(new PlayState(_player, _eggplant, _level));
                InputManager.Instance.OnActionEvent -= OnActionEvent;
            }
        }
    }

    public class PlayState : State
    {
        int _level;

        int _count;
        int _dead;
        int _capacity;

        float _duration;
        float _timer = -2f;

        public PlayState(PlayerController player, EggplantNexusController eggplant, int level) : base(player, eggplant) { _level = level; }

        public override void Enter()
        {
            _player.SetMarkerChecks(true, true);
            _player.SetControllability(true);
            InputManager.Instance.SetActionControlsActivity(true);
            InputManager.Instance.SetDirectionControlsActivity(true);

            _level++;
            switch (_level)
            {
                case 2:
                    _capacity = 3;
                    _duration = 10f;
                    break;
                case 3:
                    _capacity = 5;
                    _duration = 15f;
                    break;
                case 4:
                    _capacity = 6;
                    _duration = 18f;
                    break;
                case 5:
                    _capacity = 7;
                    _duration = 20f;
                    break;
                case 6:
                    _capacity = 8;
                    _duration = 21f;
                    break;
                case 7:
                    _capacity = 10;
                    _duration = 25f;
                    break;
            }

            EnemyController.OnEnemyEvent += OnEnemyEvent;

            InterfaceManager.Instance.DecorateLevelStart(_level);
            InterfaceManager.Instance.ShowViews("Wave Start");

            PoolingManager.Instance.StartCoroutine(HideViewRoutine());
        }

        IEnumerator HideViewRoutine()
        {
            yield return new WaitForSeconds(3f);
            InterfaceManager.Instance.CloseViews("Wave Start");
        }

        public override void Update()
        {
            if ((_timer += Time.deltaTime) > _duration / _capacity && _count < _capacity)
            {
                _timer = 0f;
                EnemyController enemy = PoolingManager.Instance.CreateEnemy(GetAPosition(), Quaternion.identity, _player.transform.parent).GetComponent<EnemyController>();
                enemy.TurnFace(enemy.transform.position.x > 0f);
                enemy.ApplyCoefs(EnemySpeed, EnemyDamage, AttackSpeed);

                _count++;
            }
        }

        public override void Exit()
        {
            EnemyController.OnEnemyEvent -= OnEnemyEvent;
        }

        void OnEnemyEvent(EnemyController enemyController, EnemyController.EventType type)
        {
            if (type == EnemyController.EventType.GET_HIT_AND_GO)
            {
                _dead++;
                if (_capacity == _dead)
                {
                    if (_level < 7)
                        StateManager.Instance.ChangeState(new LootState(_player, _eggplant, _level), 1.5f);
                    else
                        StateManager.Instance.ChangeState(new VictoryState(_player, _eggplant), 1.5f);

                }
            }
        }

        Vector2 GetAPosition()
        {
            Vector2 position = Vector2.zero;
            while (true)
            {
                position.x = Random.Range(-10f, -7f);
                position.x *= Random.value > 0.5f ? -1f : 1f;

                position.y = Random.Range(-7f, -5.5f);
                position.y *= Random.value > 0.5f ? -1f : 1f;

                RaycastHit2D hit = Physics2D.Raycast(position, (-1f * position).normalized, 20f);
                if (hit.collider != null && hit.collider.name == "Eggplant Nexus")
                {
                    break;
                }
            }
            return position;
        }
    }

    public class VictoryState : State
    {
        public VictoryState(PlayerController player, EggplantNexusController eggplant) : base(player, eggplant) { }

        public override void Enter()
        {
            _player.SetMarkerChecks(false, false)
;            _player.SetControllability(false);
            InterfaceManager.Instance.CloseAllViews();
            InterfaceManager.Instance.ShowViews("Victory Screen");
            PoolingManager.Instance.StartCoroutine(DelayedRegister());
        }

        public override void Exit()
        {

        }

        IEnumerator DelayedRegister()
        {
            yield return new WaitForSeconds(1f);
            InputManager.Instance.OnActionEvent += OnActionEvent;
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (type == InputManager.EventType.ActionStarted)
            {
                InterfaceManager.Instance.PlayClick();
                InputManager.Instance.OnActionEvent -= OnActionEvent;
                SceneManager.LoadScene(0);
            }
        }
    }

    public class DefeatState : State
    {
        public DefeatState() : base(null, null) { }

        public override void Enter()
        {
            PlayerController player = Object.FindObjectOfType<PlayerController>();
            player.SetControllability(false);
            player.SetMarkerChecks(false, false);
            InterfaceManager.Instance.CloseAllViews();
            InterfaceManager.Instance.ShowViews("Defeat Screen");
            PoolingManager.Instance.StartCoroutine(DelayedRegister());
        }

        public override void Exit()
        {

        }

        IEnumerator DelayedRegister()
        {
            yield return new WaitForSeconds(1f);
            InputManager.Instance.OnActionEvent += OnActionEvent;
        }

        void OnActionEvent(InputManager.EventType type)
        {
            if (type == InputManager.EventType.ActionStarted)
            {
                InterfaceManager.Instance.PlayClick();
                InputManager.Instance.OnActionEvent -= OnActionEvent;
                SceneManager.LoadScene(0);
            }
        }
    }
}

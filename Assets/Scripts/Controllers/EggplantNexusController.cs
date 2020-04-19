using Managers;
using Managers.States;
using Models;
using UnityEngine;
using Views;

namespace Controllers
{
    public class EggplantNexusController : MonoBehaviour
    {
        [SerializeField] AudioClip _nexusHeal;
        [SerializeField] AudioClip _nexusHurt;

        [SerializeField] float _initialHealth;
        [SerializeField] float _initialMaximumHealth;

        [SerializeField] GameObject _healEffect;
        [SerializeField] GameObject _hurtEffect;

        EggplantNexusView _view;

        EggplantNexusModel _model;

        AudioSource _audioSource;

        void Awake()
        {
            _view = GetComponent<EggplantNexusView>();
            _model = new EggplantNexusModel(_initialHealth, _initialMaximumHealth);
            _audioSource = GetComponent<AudioSource>();
        }

        public void SetVisibility(bool bodyVisibility, bool healthBarVisibility)
        {
            _view.ModifyHealthBar(_model.CurrentHealth, _model.MaximumHealth);
            _view.SetBodyVisibility(bodyVisibility);
            _view.SetHealthBarVisibility(healthBarVisibility);
        }

        public void ModifyCurrentHealth(float modification)
        {
            _model.ModifyCurrentHealth(modification);
            if (modification > 0f)
            {
                Instantiate(_healEffect, transform.position, Quaternion.identity, transform.parent);
                AudioManager.Instance.PlayAudio(_audioSource, _nexusHeal, false);
            }

            if (modification < 0f)
            {
                Instantiate(_hurtEffect, transform.position, Quaternion.identity, transform.parent);
                AudioManager.Instance.PlayAudio(_audioSource, _nexusHurt, false);
            }

            if (_model.CurrentHealth <= 0f)
            {
                StateManager.Instance.ChangeState(new DefeatState());
            }
            _view.ModifyHealthBar(_model.CurrentHealth, _model.MaximumHealth);
        }

        public void ModifyMaximumHealth(float modification)
        {
            _model.ModifyMaximumHealth(modification);
            _view.ModifyHealthBar(_model.CurrentHealth, _model.MaximumHealth);
        }
    }
}
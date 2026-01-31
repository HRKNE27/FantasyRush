using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCharacter : MonoBehaviour
{
    [SerializeField] public SpawnSpellOnFrame _spellManager;
    private StateMachine _spellStateMachine;
    private PlayerMovement _playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        _spellStateMachine = GetComponent<StateMachine>();
        _playerMovement = _spellStateMachine._playerMovement;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Need to check that the button was released before casting in addition to the timer being over threshold
        if (InputManager._qTimer >= 5.0f && _spellStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            _spellManager.CastSpell(SpellType.Fireball);
            _spellStateMachine.SetNextState(new FirstSpell());
            InputManager.ResetTimer(KeyPressed.Q);
        }

        if (InputManager._wTimer >= 2.0f && _spellStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            _spellManager.CastSpell(SpellType.Waterball);
            _spellStateMachine.SetNextState(new SecondSpell());
            InputManager.ResetTimer(KeyPressed.W);
        }

        if (InputManager._eTimer >= 2.0f && _spellStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            _spellManager.CastSpell(SpellType.Windblast);
            _spellStateMachine.SetNextState(new ThirdSpell());
            InputManager.ResetTimer(KeyPressed.E);
        }

        if (InputManager._rTimer >= 2.0f && _spellStateMachine.CurrentState.GetType() == typeof(IdleCombatState))
        {
            _spellManager.CastSpell(SpellType.LightningBolt);
            _spellStateMachine.SetNextState(new FirstSpell());
            InputManager.ResetTimer(KeyPressed.R);
        }
    }
}

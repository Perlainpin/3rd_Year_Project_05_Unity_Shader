using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackSword : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private InputActionReference _attackSwordAction;
    [SerializeField] private Animator _animator;

    private void OnEnable()
    {
        _attackSwordAction.action.Enable();
    }

    private void OnDisable()
    {
        _attackSwordAction.action.Disable();
    }

    private void Update()
    {
        if (_attackSwordAction.action.WasPressedThisFrame())
        {
            TriggerAttack();
        }
    }

    private void TriggerAttack()
    {
        _animator.SetBool("attackSword", true);
        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.1f);
        _animator.SetBool("attackSword", false);
    }
}

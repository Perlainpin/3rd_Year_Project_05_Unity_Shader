using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMagicAttack : MonoBehaviour
{
    [Header("Magic Attack Settings")]
    [SerializeField] private InputActionReference _magicAttackAction;
    [SerializeField] private Animator _animator;

    private void OnEnable()
    {
        _magicAttackAction.action.Enable();
    }

    private void OnDisable()
    {
        _magicAttackAction.action.Disable();
    }

    private void Update()
    {
        if (_magicAttackAction.action.WasPressedThisFrame() && _animator.GetBool("bookmenu"))
        {
            TriggerMagicAttack();
        }
    }

    private void TriggerMagicAttack()
    {
        _animator.SetBool("magic_attack", true);
        StartCoroutine(ResetMagicAttack());
    }

    private IEnumerator ResetMagicAttack()
    {
        yield return new WaitForSeconds(0.1f);
        _animator.SetBool("magic_attack", false);
    }
}

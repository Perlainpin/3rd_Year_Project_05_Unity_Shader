using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private InputActionReference _shieldAction; // Référence à l'action d'entrée pour le bouclier
    [SerializeField] private Animator _animator; // Référence à l'Animator

    private void OnEnable()
    {
        _shieldAction.action.Enable();
    }

    private void OnDisable()
    {
        _shieldAction.action.Disable();
    }

    private void Update()
    {
        // Vérifie si le clic droit (ou une action configurée pour le bouclier) est en cours d'activation
        bool isShieldActive = _shieldAction.action.IsPressed();

        // Met à jour le paramètre "shield" dans l'Animator
        _animator.SetBool("shield", isShieldActive);
    }
}

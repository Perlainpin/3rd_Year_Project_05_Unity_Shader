using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBookMenu : MonoBehaviour
{
    [Header("Book Menu Settings")]
    [SerializeField] private InputActionReference _toggleBookMenuAction;
    [SerializeField] private Animator _animator;

    [Header("UI Elements")]
    [SerializeField] private GameObject menuOpenObject;
    [SerializeField] private GameObject menuClosedObject;

    [Header("Avatar and Materials")]
    [SerializeField] private Renderer[] avatarRenderers; // Tableau de Renderer pour l'avatar entier
    [SerializeField] private Material[] spellMaterials; // Tableau pour les 4 matériaux (0 à 4)

    private bool isBookMenuActive = false;

    private void OnEnable()
    {
        _toggleBookMenuAction.action.Enable();
    }

    private void OnDisable()
    {
        _toggleBookMenuAction.action.Disable();
    }

    private void Update()
    {
        if (_toggleBookMenuAction.action.WasPressedThisFrame())
        {
            ToggleBookMenu();
        }

        if (Keyboard.current[Key.Digit0].wasPressedThisFrame)
        {
            SetSpell(0);
        }
        else if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            SetSpell(1);
        }
        else if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
        {
            SetSpell(2);
        }
        else if (Keyboard.current[Key.Digit3].wasPressedThisFrame)
        {
            SetSpell(3);
        }
        else if (Keyboard.current[Key.Digit4].wasPressedThisFrame)
        {
            SetSpell(4);
        }
    }

    private void ToggleBookMenu()
    {
        isBookMenuActive = !isBookMenuActive;

        _animator.SetBool("bookmenu", isBookMenuActive);

        if (isBookMenuActive)
        {
            if (menuOpenObject != null)
            {
                menuOpenObject.SetActive(true);
            }

            if (menuClosedObject != null)
            {
                menuClosedObject.SetActive(false);
            }
        }
        else
        {
            if (menuOpenObject != null)
            {
                menuOpenObject.SetActive(false);
            }

            if (menuClosedObject != null)
            {
                menuClosedObject.SetActive(true);
            }
        }
    }

    private void SetSpell(int spellValue)
    {
        if (_animator != null)
        {
            _animator.SetInteger("Spells", spellValue);
        }

        // Si l'avatar et les matériaux sont définis
        if (avatarRenderers != null && spellMaterials != null && spellMaterials.Length > spellValue)
        {
            // Parcourir tous les rendus (Renderers) de l'avatar pour changer le matériau
            foreach (Renderer renderer in avatarRenderers)
            {
                if (renderer != null)
                {
                    // Appliquer le matériau correspondant à la valeur de "Spells"
                    renderer.material = spellMaterials[spellValue];
                }
            }
        }
    }
}

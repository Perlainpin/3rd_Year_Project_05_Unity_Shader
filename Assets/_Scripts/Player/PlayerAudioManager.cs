using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip attackAudioClip;
    [SerializeField] private AudioClip shieldAudioClip;
    [SerializeField] private AudioClip bookAudioClip;
    [SerializeField] private AudioClip walkingAudioClip;
    [SerializeField] private AudioClip runningAudioClip;
    [SerializeField] private AudioClip jumpSoundEffect; // Son pour le saut
    [SerializeField] private AudioClip magicAttackAudioClip; // Son pour l'attaque magique

    [Header("Audio Sources")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioSource shieldAudioSource;
    [SerializeField] private AudioSource bookAudioSource;
    [SerializeField] private AudioSource walkingAudioSource;
    [SerializeField] private AudioSource runningAudioSource;
    [SerializeField] private AudioSource jumpAudioSource; // AudioSource pour le saut
    [SerializeField] private AudioSource magicAttackAudioSource; // AudioSource pour l'attaque magique

    [Header("Animator")]
    [SerializeField] private Animator _animator;

    private bool wasShieldActive = false;
    private bool wasBookMenuActive = false;
    private bool wasAttackActive = false;
    private bool wasWalking = false;
    private bool wasSprinting = false;
    private bool wasJumping = false; // État précédent du saut
    private bool wasMagicAttackActive = false; // État précédent de l'attaque magique

    private void Update()
    {
        HandleAttackAudio();
        HandleShieldAudio();
        HandleBookAudio();
        HandleWalkingAudio();
        HandleRunningAudio();
        HandleJumpAudio();
        HandleMagicAttackAudio();
    }

    private void HandleAttackAudio()
    {
        bool isAttackActive = _animator.GetBool("attackSword");

        if (isAttackActive && !wasAttackActive && !_animator.GetBool("bookmenu") && _animator.GetBool("GotSword"))
        {
            PlayAudio(attackAudioSource, attackAudioClip);
        }

        wasAttackActive = isAttackActive;
    }

    private void HandleShieldAudio()
    {
        bool isShieldActive = _animator.GetBool("shield");

        if (isShieldActive && !wasShieldActive)
        {
            PlayAudio(shieldAudioSource, shieldAudioClip);
        }

        wasShieldActive = isShieldActive;
    }

    private void HandleBookAudio()
    {
        bool isBookMenuActive = _animator.GetBool("bookmenu");

        if (isBookMenuActive && !wasBookMenuActive)
        {
            PlayAudio(bookAudioSource, bookAudioClip);
        }

        wasBookMenuActive = isBookMenuActive;
    }

    private void HandleWalkingAudio()
    {
        bool isWalking = _animator.GetBool("isWalking");

        if (isWalking && !wasWalking)
        {
            PlayLoopingAudio(walkingAudioSource, walkingAudioClip);
        }
        else if (!isWalking && wasWalking)
        {
            StopAudio(walkingAudioSource);
        }

        wasWalking = isWalking;
    }

    private void HandleRunningAudio()
    {
        bool isSprinting = _animator.GetBool("isSprinting");

        if (isSprinting && !wasSprinting)
        {
            PlayLoopingAudio(runningAudioSource, runningAudioClip);
        }
        else if (!isSprinting && wasSprinting)
        {
            StopAudio(runningAudioSource);
        }

        wasSprinting = isSprinting;
    }

    private void HandleJumpAudio()
    {
        bool isJumping = _animator.GetBool("isJumping");

        if (isJumping && !wasJumping)
        {
            PlayAudio(jumpAudioSource, jumpSoundEffect);
        }

        wasJumping = isJumping;
    }

    private void HandleMagicAttackAudio()
    {
        bool isMagicAttackActive = _animator.GetBool("magic_attack");

        if (isMagicAttackActive && !wasMagicAttackActive)
        {
            PlayAudio(magicAttackAudioSource, magicAttackAudioClip);
        }

        wasMagicAttackActive = isMagicAttackActive;
    }

    private void PlayAudio(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null)
            return;

        source.clip = clip;
        source.Play();
    }

    private void PlayLoopingAudio(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null || source.isPlaying)
            return;

        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    private void StopAudio(AudioSource source)
    {
        if (source == null || !source.isPlaying)
            return;

        source.Stop();
    }
}

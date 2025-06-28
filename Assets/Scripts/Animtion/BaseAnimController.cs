using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class BaseAnimController : MonoBehaviour
{
    protected Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void PauseAnim()
    {
        _anim.speed = 0;
    }

    public void RestartAnim()
    {
        _anim.speed = 1;
    }
}

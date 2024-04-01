using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Helpers;
public class Player : CharacterBase
{
    private Animator    animator;
    private Rigidbody   rigbody;
    private float       jumpForce = 30f;
    private bool        isJumpingAnimation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigbody = GetComponent<Rigidbody>();
    }
    public void Start()
    {
        animator.Play("Dribble");
    }

    public void OnShoot()
    {
        rigbody.AddForce(jumpForce * Vector3.up, ForceMode.Force);
        animator.Play("Jump");
        isJumpingAnimation = true;
    }

    private void Update()
    {
        //transform.LookAt(HOOP_POSITION, Vector3.up);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnShoot();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        return;
        if(isJumpingAnimation && collision.gameObject.CompareTag(StringTag(GameTag.Terrain)))
        {
            animator.Play("Dribble");
        }
    }
}

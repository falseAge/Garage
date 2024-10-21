using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private DoorTrigger _doorTrigger;

    private void Start()
    {
        _animator.StopPlayback();
        _doorTrigger.OnTrigger.AddListener(DoorOpener);
    }

    public void DoorOpener()
    {
        _animator.Play("Door");
        Debug.Log("Beeeeeep");
    }
}

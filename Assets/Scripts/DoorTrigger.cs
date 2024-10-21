using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    public UnityEvent OnTrigger;
    
    private void OnTriggerEnter(Collider obj)
    {
        if (obj.gameObject == _player)
        {
            OnTrigger?.Invoke();
        }
    }
}

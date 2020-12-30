using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// JimJam Utilities
/// Self Destruct
///
/// Features:
/// |-> Remove gameobject after set amount of time after enabled
/// |-> Remove gameobject after current playing animation finishes
///
/// Helpful resources:
/// |-> Find out if animator is playing an animation - https://answers.unity.com/questions/362629/how-can-i-check-if-an-animation-is-being-played-or.html
/// </summary>
public class JJU_SelfDestruct : MonoBehaviour
{
    private enum DestructTypes
    {
        InTime,
        AfterAnimation
    }

    [SerializeField] private DestructTypes destructType;
    [SerializeField] private float timer;
    private Animator _anim;
    private void Start()
    {
        StartCoroutine(TimedDestruct());
    }

    IEnumerator TimedDestruct()
    {
        // Determine which type the component is using then execute the appropriate behavior
        switch (destructType)
        {
            // Just wait for set amount of time and commit sudoku
            case DestructTypes.InTime:
                yield return new WaitForSeconds(timer);        
                break;
            
            // Wait until the currently playing animation finishes
            case DestructTypes.AfterAnimation:
                
                // Verify that there is a valid animator on the object
                _anim = GetComponent<Animator>();
                if(!_anim)
                    throw new Exception(this.gameObject.name + " does not have an animator for its self destruct!");
                // If so wait until it finishes its current animation
                while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                    yield return null;
                break;
        }
        // Remove the object
        Destroy(this.gameObject);
    }
}

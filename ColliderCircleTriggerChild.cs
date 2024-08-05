using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCircleTriggerChild : MonoBehaviour
{
    private CircleCollider2D c_circleCollider;

    public delegate void OnTriggerEnter(Collider2D collision);
    public event OnTriggerEnter EventOnTriggerEnter;
    public delegate void OnTriggerExit(Collider2D collision);
    public event OnTriggerExit EventOnTriggerExit;

    private void Awake()
    {
        c_circleCollider = GetComponent<CircleCollider2D>();
    }

    public void Initialize(float _radius, bool _startActive)
    {
        c_circleCollider.radius = _radius;
        c_circleCollider.enabled = _startActive;
    }

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Set radius of CircleCollider2D
    /// </summary>
    public void SetRadius(float _radius)
    {
        c_circleCollider.radius = _radius;
    }

    /// <summary>
    /// Either enable or disable CircleCollider2D
    /// </summary>
    public void SetEnableTrigger(bool _enable)
    {
        c_circleCollider.enabled = _enable;
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    private void OnTriggerEnter2D(Collider2D collision)
    {
        EventOnTriggerEnter?.Invoke(collision); // Send signal that something entered trigger
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EventOnTriggerExit?.Invoke(collision); // Send signal that something exited trigger
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deathzone : MonoBehaviour
{

    /// <summary>
    /// Collider is set to only include "Ball" layer and exclude everything else
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ManagerBall.Instance.OnBallEnteredDeathzone(collision);
    }
}

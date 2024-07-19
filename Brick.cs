using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// When Brick collides with Ball, destroy Brick
    /// </summary>
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // Did Paddle hit a Ball?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            // Update score upon destroying brick
            _ball.OnBrickHit();

            // Destroy Brick
            Destroy(gameObject);
        }
    }
}

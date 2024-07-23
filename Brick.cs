using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] private ManagerPowerup.PowerupType powerupType = ManagerPowerup.PowerupType.Anything;
    private int brickBaseScore = 10;

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Return the type of Powerup that this Brick can drop
    /// </summary>
    public ManagerPowerup.PowerupType GetDroppedPowerupType()
    {
        return powerupType;
    }

    /// <summary>
    /// Return the score that destroying this Brick gives
    /// </summary>
    public int GetScore()
    {
        return brickBaseScore;
    }

    /// <summary>
    /// Destroy this Brick, spawn a ParticleSystem
    /// </summary>
    public void DestroyBrick()
    {
        Destroy(gameObject);
    }

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// When Brick collides with Ball, destroy Brick
    /// </summary>
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // Did Ball hit Brick?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            // Destroy Brick
            ManagerBrick.Instance.OnBallHitBrick(_ball, this);
        }
    }
}

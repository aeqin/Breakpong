using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpore : Ball
{
    protected override void Awake()
    {
        base.Awake();

        c_trailRenderer.enabled = false; // No need for trail since BallSpore has a ParticleSystem attached
    }

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/
    #region Protected Methods
    /// <summary>
    /// When BallSpore collides with anything, play hit particles, then destroy self
    /// </summary>
    protected override void OnCollisionDo(Collision2D collision)
    {
        OnCollisionSpawnParticles(collision.GetContact(0).point); // Spawn hit particles

        Destroy(gameObject); // Destroy BallSpore on first collision with anything
    }

    protected override void UpdateBallColor() { } // Do nothing
    #endregion
}

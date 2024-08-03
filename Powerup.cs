using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    private Rigidbody2D c_rb;
    private SpriteRenderer c_spriteRenderer;

    // Movement variables
    private float powerUpMoveSpeed = 175.0f;
    private int powerUpXDir = -1; // Either -1 or 1

    // Type variables
    private ManagerPowerup.PowerupType powerUpType;

    // On Death variables
    [SerializeField] private ParticleSystem pf_onDeathParticles;

    private void Awake()
    {
        c_rb = GetComponent<Rigidbody2D>();
        c_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(ManagerPowerup.PowerupType _spawnType)
    {
        powerUpType = _spawnType;
        powerUpXDir = Utilities.FlipACoin() ? -1 : 1; // 50% of Powerup going left, 50% going right

        UpdateSprite();
    }

    private void FixedUpdate()
    {
        c_rb.velocity = powerUpMoveSpeed * new Vector2(powerUpXDir, 0);

        // Destroy Powerup if offscreen
        if (Utilities.IsPosOffScreen(transform.position))
        {
            ManagerPowerup.Instance.OnPowerupOffScreen(this);
        }
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// When Powerup is destroyed, play particles
    /// </summary>
    private void OnDeathSpawnParticles(Vector2 _spawnPos)
    {
        // Instantiate death particles
        ParticleSystem _ps = Instantiate(pf_onDeathParticles, _spawnPos, Quaternion.identity);
    }

    /// <summary>
    /// Depending on type of this Powerup, change its display sprite
    /// </summary>
    private void UpdateSprite()
    {
        Sprite _spr_powerup = ManagerPowerup.Instance.GetSpriteFromPowerupType(powerUpType);
        c_spriteRenderer.sprite = _spr_powerup;
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// Returns type of Powerup this is
    /// </summary>
    public ManagerPowerup.PowerupType GetPowerupType()
    {
        return powerUpType;
    }
    
    /// <summary>
    /// Destroy this Powerup, spawn a ParticleSystem
    /// </summary>
    public void DestroyPowerup()
    {
        OnDeathSpawnParticles(transform.position);
        Destroy(gameObject);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    #region On Event Methods
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Did Powerup hit Paddle?
        if (collision.gameObject.TryGetComponent<Paddle>(out Paddle _paddle))
        {
            ManagerPowerup.Instance.OnPowerupHitPaddle(this, _paddle);
        }
    }
    #endregion

}

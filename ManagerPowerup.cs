using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPowerup : MonoBehaviour
{
    #region Singleton
    public static ManagerPowerup Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }
    #endregion

    /// <summary>
    /// Enum of every type of Powerup in the game
    /// </summary>
    public enum PowerupType
    {
        None,
        Anything,

        PaddleMagnet,
        PaddleMagnetOnce,
        PaddleSlam,

        BallSplit,
    }

    [SerializeField] private Powerup pf_powerup;
    private List<Powerup> list_powerups = new List<Powerup>();

    // Sprites of each Powerup
    [SerializeField] private Sprite spr_magnet;
    [SerializeField] private Sprite spr_slam;
    [SerializeField] private Sprite spr_ballSplit;

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Get the type of Powerup to spawn, depending on level and drop weights
    /// </summary>
    private ManagerPowerup.PowerupType GetNextDroppedPowerUpType()
    {
        // TODO ManagerLevel.Instance.GetNextDroppedPowerupType();

        return PowerupType.PaddleMagnet;
    }
    
    /// <summary>
    /// Create a Powerup at a particular position, and add Powerup to list of Powerup
    /// </summary>
    private Powerup CreatePowerupAt(ManagerPowerup.PowerupType _powerupType, Vector2 _spawnPos)
    {
        Powerup _powerup = Instantiate(pf_powerup, _spawnPos, Quaternion.identity);
        list_powerups.Add(_powerup);

        if (_powerupType == ManagerPowerup.PowerupType.Anything)
        {
            _powerup.Initialize(GetNextDroppedPowerUpType()); // Type of spawned Powerup, depending on current level drop rates
        }
        else
        {
            _powerup.Initialize(_powerupType);
        }
        

        return _powerup;
    }

    /// <summary>
    /// Remove Powerup from list, and ask itself to Destroy itself
    /// </summary>
    private void RemovePowerup(Powerup _powerupToDestroy)
    {
        list_powerups.RemoveAll(_pwr => _pwr == _powerupToDestroy);

        _powerupToDestroy.DestroyPowerup();
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    #region Public Methods
    /// <summary>
    /// When a Paddle picks up a Powerup, handle it
    /// </summary>
    /// <param name="_powerUp">Type of Powerup</param>
    /// <param name="_pickerUpper">The Paddle that picked up the Powerup</param>
    public Sprite GetSpriteFromPowerupType(PowerupType _powerUp)
    {
        switch (_powerUp)
        {
            case PowerupType.PaddleMagnet:
                return spr_magnet;

            case PowerupType.PaddleSlam:
                return spr_slam;

            case PowerupType.BallSplit:
                return spr_ballSplit;

            default:
                Debug.LogError("Add case to ManagerPowerup:GetSpriteFromPowerupType() for Powerup." + _powerUp.ToString());
                break;
        }

        return spr_magnet;
    }

    /// <summary>
    /// When a Paddle picks up a Powerup, handle it
    /// </summary>
    /// <param name="_powerUp">Type of Powerup</param>
    /// <param name="_pickerUpper">The Paddle that picked up the Powerup</param>
    public void HandlePowerupPickup(PowerupType _powerUp, Paddle _pickerUpper)
    {
        switch (_powerUp)
        {
            case PowerupType.BallSplit:
                ManagerBall.Instance.OnPowerupPickup(_powerUp);
                break;

            default:
                _pickerUpper.AssignActionFromPowerup(_powerUp);
                break;
        }
    }

    /// <summary>
    /// Spawn a Powerup at given position
    /// </summary>
    /// <param name="_spawnPos">World position of Powerup</param>
    public void SpawnPowerupAt(Vector2 _spawnPos)
    {
        CreatePowerupAt(ManagerPowerup.PowerupType.Anything, _spawnPos);
    }

    /// <summary>
    /// Spawn a specific Powerup at given position
    /// </summary>
    /// <param name="_powerupType">Type of Powerup</param>
    /// <param name="_spawnPos">World position of Powerup</param>
    public void SpawnSpecificPowerupAt(ManagerPowerup.PowerupType _powerupType, Vector2 _spawnPos)
    {
        CreatePowerupAt(_powerupType, _spawnPos);
    }

    /// <summary>
    /// Ask ManagerPowerup to handle event when Powerup leaves screen
    /// </summary>
    public void OnPowerupHitPaddle(Powerup _powerup, Paddle _paddle)
    {
        HandlePowerupPickup(_powerup.GetPowerupType(), _paddle);
        RemovePowerup(_powerup);
    }

    /// <summary>
    /// Ask ManagerPowerup to handle event when Powerup leaves screen
    /// </summary>
    public void OnPowerupOffScreen(Powerup _powerup)
    {
        RemovePowerup(_powerup);
    }
    #endregion
}

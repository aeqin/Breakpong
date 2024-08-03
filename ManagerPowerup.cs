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
        PaddleGhostPaddle,
        PaddleGrowPaddle,
        PaddleShrinkPaddle,
        PaddleLaser,

        BallSplit,
        BallSpore,
    }

    [SerializeField] private Powerup pf_powerup;
    private List<Powerup> list_powerups = new List<Powerup>();

    // Sprites of each Powerup
    [SerializeField] private Sprite spr_magnet;
    [SerializeField] private Sprite spr_slam;
    [SerializeField] private Sprite spr_ghostPaddle;
    [SerializeField] private Sprite spr_growPaddle;
    [SerializeField] private Sprite spr_shrinkPaddle;
    [SerializeField] private Sprite spr_laser;
    [SerializeField] private Sprite spr_ballSplit;
    [SerializeField] private Sprite spr_ballSpore;

    // PowerupDropEngine
    private PowerupDropEngine currPowerupDropEngine;
    // Class used to store & calculate Powerup droprates
    public class PowerupDropEngine
    {
        private Dictionary<PowerupType, int> dict_pwrType_weight;
        int totalWeight = 0;

        public PowerupDropEngine(Dictionary<PowerupType, int> _dict_pwrType_weight)
        {
            dict_pwrType_weight = new Dictionary<PowerupType, int>(_dict_pwrType_weight); // Shallow copy

            // After weight assignment for each PowerupType, add up all the total weights
            foreach ((PowerupType _pwrType, int _weight) in dict_pwrType_weight)
            {
                totalWeight += _weight;
            }
        }

        /// <summary>
        /// Returns the droprate of given PowerupType
        /// </summary>
        public float GetDroprateFor(PowerupType _pwrType)
        {
            if (!dict_pwrType_weight.ContainsKey(_pwrType))
                return 0;

            return (float)dict_pwrType_weight[_pwrType] / totalWeight;
        }

        /// <summary>
        /// Change the weight of the given PowerupType
        /// </summary>
        public void AdjustWeightFor(PowerupType _pwrType, int _newWeight)
        {
            totalWeight -= dict_pwrType_weight[_pwrType]; // Subtract old weight from total weight
            dict_pwrType_weight[_pwrType] = _newWeight; // Assign new weight
            totalWeight += dict_pwrType_weight[_pwrType]; // Add new weight to total weight
        }

        /// <summary>
        /// Returns the next PowerupType to be randomly selected, based on its weight
        /// </summary>
        public PowerupType Next()
        {
            float _randRoll = Mathf.Clamp(Random.Range(0f, 1.0f) * totalWeight, 1, totalWeight); // Lay out a road (Clamp 1 as minimum so 0 weights NEVER reach the end of the road)

            foreach ((PowerupType _pwrType, int _weight) in dict_pwrType_weight)
            {
                _randRoll -= _weight; // Travel the road by weight of each PowerupType (larger weights travel more distance, better chance of reaching end of road)
                if (_randRoll < 0f) // Reaching the end of the road, return the PowerupType
                {
                    return _pwrType;
                }
            }

            return PowerupType.None;
        }

        /// <summary>
        /// Debug.Log out all the Powerups, and their % chance to appear
        /// </summary>
        public void DebugPowerupDroprates()
        {
            string _dropRateString = "-------------------\n" +
                                     "Powerup Droprates:\n" +
                                     "-----------------------------\n";
            foreach (PowerupType _pwrType in dict_pwrType_weight.Keys)
            {
                _dropRateString += _pwrType.ToString() + " | " + GetDroprateFor(_pwrType) * 100 + "%\n";
            }
            _dropRateString += "-------------------";

            Debug.Log(_dropRateString);
        }
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    /// <summary>
    /// Create a Powerup at a particular position, and add Powerup to list of Powerup. Will return null when no Powerup is created
    /// </summary>
    private Powerup CreatePowerupAt(ManagerPowerup.PowerupType _requestedPowerupType, Vector2 _spawnPos)
    {
        if (_requestedPowerupType == ManagerPowerup.PowerupType.None)
        {
            return null;
        }

        PowerupType _powerupType; // Type of created Powerup
        if (_requestedPowerupType == ManagerPowerup.PowerupType.Anything)
        {
            _powerupType = currPowerupDropEngine.Next(); // Can be anything, so randomly roll a Powerup allowed for this level
            if (_powerupType == ManagerPowerup.PowerupType.None)
            {
                return null; // Randomly rolled a "None" Powerup
            }
        }
        else
        {
            _powerupType = _requestedPowerupType; // Set Powerup type as requested
        }

        Powerup _powerup = Instantiate(pf_powerup, _spawnPos, Quaternion.identity);
        list_powerups.Add(_powerup);
        _powerup.Initialize(_powerupType);

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

            case PowerupType.PaddleGhostPaddle:
                return spr_ghostPaddle;

            case PowerupType.PaddleGrowPaddle:
                return spr_growPaddle;

            case PowerupType.PaddleShrinkPaddle:
                return spr_shrinkPaddle;

            case PowerupType.PaddleLaser:
                return spr_laser;

            case PowerupType.BallSplit:
                return spr_ballSplit;

            case PowerupType.BallSpore:
                return spr_ballSpore;

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
            // Powerup affects Balls
            case PowerupType.BallSplit:
            case PowerupType.BallSpore:
                ManagerBall.Instance.OnPowerupPickup(_powerUp);
                break;

            // Powerup affects Paddles
            default:
                _pickerUpper.AssignActionFromPowerup(_powerUp);
                break;
        }
    }

    /// <summary>
    /// When Laser hits Powerup, destroy Powerup
    /// </summary>
    public void OnLaserHitPowerup(Laser _laser, Powerup _powerup)
    {
        RemovePowerup(_powerup);
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

    /// <summary>
    /// Reset PowerupDropEngine for the current level
    /// </summary>
    public void ResetPowerupDropEngine(Dictionary<PowerupType, int> _dict_pwrType_weight)
    {
        currPowerupDropEngine = new PowerupDropEngine(_dict_pwrType_weight); // Assign new weighted drop table for current level

        currPowerupDropEngine.DebugPowerupDroprates();
    }
    #endregion
}

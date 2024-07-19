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
    /// Enum of every Powerup in the game
    /// </summary>
    public enum PowerupType
    {
        None,

        PaddleMagnet,
        PaddleMagnetOnce,
        PaddleSlam,

        BallSplit,
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/

    /// <summary>
    /// When a Paddle picks up a Powerup, handle it
    /// </summary>
    /// <param name="_powerUp">Type of Powerup</param>
    /// <param name="_pickerUpper">The Paddle that picked up the Powerup</param>
    public void HandlePowerupPickup(PowerupType _powerUp, Paddle _pickerUpper)
    {
        switch (_powerUp)
        {
            case PowerupType.PaddleMagnet:
                _pickerUpper.AssignActionFromPowerup(_powerUp);
                break;
            case PowerupType.PaddleMagnetOnce:
                _pickerUpper.AssignActionFromPowerup(_powerUp);
                break;
            case PowerupType.PaddleSlam:
                _pickerUpper.AssignActionFromPowerup(_powerUp);
                break;

            case PowerupType.BallSplit:
                ManagerBall.Instance.OnPowerupPickup(_powerUp);
                break;

            default:
                Debug.LogError("Add case to ManagerPowerup:HandlePowerup() for Powerup." + _powerUp.ToString());
                break;
        }
    }

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
}

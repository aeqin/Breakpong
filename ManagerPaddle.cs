using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerPaddle : MonoBehaviour
{
    #region Singleton
    public static ManagerPaddle Instance { get; private set; }

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

    [SerializeField] private PaddleLeft PaddleLeft;
    [SerializeField] private PaddleRight PaddleRight;

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/

    /*********************************************************************************************************************************************************************************
     * Protected Methods
     *********************************************************************************************************************************************************************************/

    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    public Paddle GetPaddleLeft() { return PaddleLeft; }
    public Paddle GetPaddleRight() { return PaddleRight; }

    /// <summary>
    /// Disable both Paddles (on Game Over or Win)
    /// </summary>
    public void DisablePaddles()
    {
        PaddleLeft.ResetPaddleAndActions();
        PaddleRight.ResetPaddleAndActions();

        // Re-enable
        PaddleLeft.gameObject.SetActive(false);
        PaddleRight.gameObject.SetActive(false);
    }

    /// <summary>
    /// Reset both PaddleActions of each Paddle
    /// </summary>
    public void ResetPaddles()
    {
        PaddleLeft.ResetPaddleAndActions();
        PaddleRight.ResetPaddleAndActions();

        // Re-enable
        PaddleLeft.gameObject.SetActive(true);
        PaddleRight.gameObject.SetActive(true);
    }

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
}

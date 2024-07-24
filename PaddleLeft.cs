using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleLeft : Paddle
{
    // PaddleAction Variables
    private PaddleAction currPaddleLeftActionOne = null;
    private PaddleAction currPaddleLeftActionTwo = null;

    private Vector2 paddleLeftInputVector;

    private new void Awake()
    {
        base.Awake();
        dirToCenter = 1; // Paddle faces right to center
    }

    private void OnEnable()
    {
        inputControls.Enable();
        inputControls.Paddle.PaddleLeftMovement.performed += OnMovePaddleLeftPerformed;
        inputControls.Paddle.PaddleLeftMovement.canceled += OnMovePaddleLeftCancelled;

        inputControls.Paddle.PaddleLeftActionOne.started += OnPaddleLeftActionOneStarted;
        inputControls.Paddle.PaddleLeftActionOne.canceled += OnPaddleLeftActionOneCanceled;
        inputControls.Paddle.PaddleLeftActionTwo.started += OnPaddleLeftActionTwoStarted;
        inputControls.Paddle.PaddleLeftActionTwo.canceled += OnPaddleLeftActionTwoCanceled;
    }

    private void OnDisable()
    {
        inputControls.Disable();
        inputControls.Paddle.PaddleLeftMovement.performed -= OnMovePaddleLeftPerformed;
        inputControls.Paddle.PaddleLeftMovement.canceled -= OnMovePaddleLeftCancelled;

        inputControls.Paddle.PaddleLeftActionOne.started -= OnPaddleLeftActionOneStarted;
        inputControls.Paddle.PaddleLeftActionTwo.started -= OnPaddleLeftActionTwoStarted;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    private void OnMovePaddleLeftPerformed(InputAction.CallbackContext callbackContext)
    {
        paddleLeftInputVector = callbackContext.ReadValue<Vector2>();
    }
    private void OnMovePaddleLeftCancelled(InputAction.CallbackContext callbackContext)
    {
        paddleLeftInputVector = Vector2.zero;
    }

    private void OnPaddleLeftActionOneStarted(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionStart(currPaddleLeftActionOne);
    }
    private void OnPaddleLeftActionOneCanceled(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionCancel(currPaddleLeftActionOne);
    }
    private void OnPaddleLeftActionTwoStarted(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionStart(currPaddleLeftActionTwo);
    }
    private void OnPaddleLeftActionTwoCanceled(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionCancel(currPaddleLeftActionTwo);
    }
    #endregion

    /*********************************************************************************************************************************************************************************
     * Override parent Methods
     *********************************************************************************************************************************************************************************/
    #region Override Methods
    /// <summary>
    /// Move the Paddle in the direction captured by the input vector
    /// </summary>
    protected override void MovePaddle()
    {
        c_rb.velocity = paddleLeftInputVector * paddleMoveSpeed;
    }

    /// <summary>
    /// Get the current PaddleActionOne of this particular Paddle
    /// </summary>
    protected override PaddleAction GetPaddleActionOne()
    {
        return currPaddleLeftActionOne;
    }

    /// <summary>
    /// Get the current PaddleActionTwo of this particular Paddle
    /// </summary>
    protected override PaddleAction GetPaddleActionTwo()
    {
        return currPaddleLeftActionTwo;
    }

    /// <summary>
    /// Set the current PaddleActionOne of this particular Paddle
    /// </summary>
    protected override void SetPaddleActionOne(PaddleAction _PA)
    {
        currPaddleLeftActionOne = _PA;
    }

    /// <summary>
    /// Set the current PaddleActionTwo of this particular Paddle
    /// </summary>
    protected override void SetPaddleActionTwo(PaddleAction _PA)
    {
        currPaddleLeftActionTwo = _PA;
    }
    #endregion
}

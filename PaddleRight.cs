using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleRight : Paddle
{
    // PaddleAction Variables
    private PaddleAction currPaddleRightActionOne = null;
    private PaddleAction currPaddleRightActionTwo = null;

    private Vector2 paddleRightInputVector;

    private new void Awake()
    {
        base.Awake();
        dirToCenter = -1; // Paddle faces left to center
    }

    private void OnEnable()
    {
        inputControls.Enable();
        inputControls.Paddle.PaddleRightMovement.performed += OnMovePaddleRightPerformed;
        inputControls.Paddle.PaddleRightMovement.canceled += OnMovePaddleRightCancelled;

        inputControls.Paddle.PaddleRightActionOne.started += OnPaddleRightActionOneStarted;
        inputControls.Paddle.PaddleRightActionOne.canceled += OnPaddleRightActionOneCanceled;
        inputControls.Paddle.PaddleRightActionTwo.started += OnPaddleRightActionTwoStarted;
        inputControls.Paddle.PaddleRightActionTwo.canceled += OnPaddleRightActionTwoCanceled;
    }

    private void OnDisable()
    {
        inputControls.Disable();
        inputControls.Paddle.PaddleRightMovement.performed -= OnMovePaddleRightPerformed;
        inputControls.Paddle.PaddleRightMovement.canceled -= OnMovePaddleRightCancelled;

        inputControls.Paddle.PaddleRightActionOne.started -= OnPaddleRightActionOneStarted;
        inputControls.Paddle.PaddleRightActionTwo.started -= OnPaddleRightActionTwoStarted;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    private void OnMovePaddleRightPerformed(InputAction.CallbackContext callbackContext)
    {
        paddleRightInputVector = callbackContext.ReadValue<Vector2>();
    }
    private void OnMovePaddleRightCancelled(InputAction.CallbackContext callbackContext)
    {
        paddleRightInputVector = Vector2.zero;
    }

    private void OnPaddleRightActionOneStarted(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionStart(currPaddleRightActionOne);
    }
    private void OnPaddleRightActionOneCanceled(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionCancel(currPaddleRightActionOne);
    }
    private void OnPaddleRightActionTwoStarted(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionStart(currPaddleRightActionTwo);
    }
    private void OnPaddleRightActionTwoCanceled(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionCancel(currPaddleRightActionTwo);
    }

    /*********************************************************************************************************************************************************************************
     * Override parent Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Move the Paddle in the direction captured by the input vector
    /// </summary>
    protected override void MovePaddle()
    {
        c_rb.velocity = paddleRightInputVector * paddleMoveSpeed;
    }

    /// <summary>
    /// Get the current PaddleActionOne of this particular Paddle
    /// </summary>
    protected override PaddleAction GetPaddleActionOne()
    {
        return currPaddleRightActionOne;
    }

    /// <summary>
    /// Get the current PaddleActionTwo of this particular Paddle
    /// </summary>
    protected override PaddleAction GetPaddleActionTwo()
    {
        return currPaddleRightActionTwo;
    }

    /// <summary>
    /// Set the current PaddleActionOne of this particular Paddle
    /// </summary>
    protected override void SetPaddleActionOne(PaddleAction _PA)
    {
        currPaddleRightActionOne = _PA;
    }

    /// <summary>
    /// Set the current PaddleActionTwo of this particular Paddle
    /// </summary>
    protected override void SetPaddleActionTwo(PaddleAction _PA)
    {
        currPaddleRightActionTwo = _PA;
    }
}

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
        inputControls.Paddle.PaddleRightMovement.canceled += OnMovePaddleRightReleased;

        inputControls.Paddle.PaddleRightActionOne.started += OnPaddleRightActionOnePressed;
        inputControls.Paddle.PaddleRightActionOne.canceled += OnPaddleRightActionOneReleased;
        inputControls.Paddle.PaddleRightActionTwo.started += OnPaddleRightActionTwoPressed;
        inputControls.Paddle.PaddleRightActionTwo.canceled += OnPaddleRightActionTwoReleased;
    }

    private void OnDisable()
    {
        inputControls.Disable();
        inputControls.Paddle.PaddleRightMovement.performed -= OnMovePaddleRightPerformed;
        inputControls.Paddle.PaddleRightMovement.canceled -= OnMovePaddleRightReleased;

        inputControls.Paddle.PaddleRightActionOne.started -= OnPaddleRightActionOnePressed;
        inputControls.Paddle.PaddleRightActionTwo.started -= OnPaddleRightActionTwoPressed;
    }

    /*********************************************************************************************************************************************************************************
     * Private Methods
     *********************************************************************************************************************************************************************************/
    #region Private Methods
    private void OnMovePaddleRightPerformed(InputAction.CallbackContext callbackContext)
    {
        paddleRightInputVector = callbackContext.ReadValue<Vector2>();
    }
    private void OnMovePaddleRightReleased(InputAction.CallbackContext callbackContext)
    {
        paddleRightInputVector = Vector2.zero;
    }

    private void OnPaddleRightActionOnePressed(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionPress(currPaddleRightActionOne);
    }
    private void OnPaddleRightActionOneReleased(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionRelease(currPaddleRightActionOne);
    }
    private void OnPaddleRightActionTwoPressed(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionPress(currPaddleRightActionTwo);
    }
    private void OnPaddleRightActionTwoReleased(InputAction.CallbackContext callbackContext)
    {
        OnPaddleActionRelease(currPaddleRightActionTwo);
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
    #endregion
}

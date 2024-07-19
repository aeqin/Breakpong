using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleActionIcon : MonoBehaviour
{
    private Dictionary<Ball, bool> dict_Ball_inOrNot = new Dictionary<Ball, bool>();

    // Display variables
    [SerializeField] Sprite spr_unpressed;
    [SerializeField] Sprite spr_pressed;
    private SpriteRenderer c_spriteRenderer;
    private Color lowAlpha = new Color(1f, 1f, 1f, 0.2f);
    private Color normalAlpha = new Color(1f, 1f, 1f, 0.8f);

    protected void Awake()
    {
        c_spriteRenderer = GetComponent<SpriteRenderer>();
        SetPressedSpriteAs(false);
        c_spriteRenderer.color = normalAlpha;
    }


    /*********************************************************************************************************************************************************************************
     * Public Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// Display either "is pressed" sprite or "unpressed" sprite
    /// </summary>
    /// <param name="_isPressed"></param>
    public void SetPressedSpriteAs(bool _isPressed)
    {
        if (_isPressed)
        {
            c_spriteRenderer.sprite = spr_pressed;
        }
        else
        {
            c_spriteRenderer.sprite = spr_unpressed;
        }
    }

    /// <summary>
    /// Display new Sprite
    /// </summary>
    /// <param name="_spr"></param>
    public void SetSpriteAs(Sprite _spr)
    {
        c_spriteRenderer.sprite = _spr;
    }

    /*********************************************************************************************************************************************************************************
     * On Event Methods
     *********************************************************************************************************************************************************************************/
    /// <summary>
    /// When Ball enters space of PaddleActionIcon, lower alpha of the icon
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Did Ball enter PaddleActionIcon space?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            dict_Ball_inOrNot[_ball] = true;
            c_spriteRenderer.color = lowAlpha; // Lower alpha
        }
    }

    /// <summary>
    /// When ALL Balls exit space of PaddleActionIcon, return alpha of the icon
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Did Ball exit PaddleActionIcon space?
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            dict_Ball_inOrNot.Remove(_ball);
            if (dict_Ball_inOrNot.Count == 0)
            {
                c_spriteRenderer.color = normalAlpha; // Restore alpha
            }
        }
    }
}

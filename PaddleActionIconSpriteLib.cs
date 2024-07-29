using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleActionIconSpriteLib : MonoBehaviour
{
    // Sprites for PaddleActionIcons
    public enum PaddleActionIconSpr
    {
        EMPTY,
        MAGNET, MAGNETOFF,
        SLAM,
        GHOSTPADDLE,
    };
    [SerializeField] public Sprite spr_Empty;
    [SerializeField] public Sprite spr_Magnet;
    [SerializeField] public Sprite spr_MagnetOFF;
    [SerializeField] public Sprite spr_Slam;
    [SerializeField] public Sprite spr_GhostPaddle;
}

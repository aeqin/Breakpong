using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to hold common sprites/prefabs shared between left and right Paddle
public class PaddleSharedLib : MonoBehaviour
{
    [SerializeField] public Sprite spr_Empty;
    [SerializeField] public Sprite spr_Magnet;
    [SerializeField] public Sprite spr_MagnetOFF;
    [SerializeField] public Sprite spr_Slam;
    [SerializeField] public Sprite spr_GhostPaddle;
    [SerializeField] public Sprite spr_GrowPaddle;
    [SerializeField] public Sprite spr_ShrinkPaddle;
    [SerializeField] public Sprite spr_Laser;

    [SerializeField] public LineRendererRadial pf_RadialLineRenderer; // Used to display duration/presses left around each icon

    [SerializeField] public PaddleGhost pf_PaddleGhost; // Prefab of spawned PaddleGhost

    [SerializeField] public Laser pf_Laser; // Prefab of laser
    [SerializeField] public LineRendererDotted pf_LineRendererDotted; // Used to display Laser path
}

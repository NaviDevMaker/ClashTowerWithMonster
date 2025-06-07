using UnityEngine;

public static class Layers
{
    public static readonly LayerMask groundLayer = LayerMask.GetMask("Ground");
    public static readonly LayerMask buildingLayer = LayerMask.GetMask("Building");
}

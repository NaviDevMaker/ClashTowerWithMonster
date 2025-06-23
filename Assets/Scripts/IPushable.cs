using System;
using UnityEngine;

public interface IPushable
{
    float rangeX { get; }
    float rangeZ{get;}
    float prioritizedRange { get; }

    bool isKnockBacked_Monster { get; set; }
    bool isKnockBacked_Spell { get; set; }
    MoveType moveType { get; }
}
/// <summary>
/// Monster,�܂���Player�̏ꍇ��walk��fly�����w��Aspell�̏ꍇ��Spell
/// </summary>
public enum MoveType
{
   Walk,
   Fly,
   Spell,
}


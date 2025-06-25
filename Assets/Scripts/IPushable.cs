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
/// Monster,‚Ü‚½‚ÍPlayer‚Ìê‡‚Íwalk‚©fly‚©‚ğw’èAspell‚Ìê‡‚ÍSpell
/// </summary>
[Flags]
public enum MoveType
{
   Walk = 1 << 0,
   Fly = 1 << 1,
   Spell = 1 << 2,
}


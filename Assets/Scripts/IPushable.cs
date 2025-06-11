using UnityEngine;

public interface IPushable
{
    float rangeX { get; }
    float rangeZ{get;}
    float prioritizedRange { get; }

    bool isKnockBacked { get; set; }
    MoveType moveType { get; }

}
/// <summary>
/// Monster,‚Ü‚½‚ÍPlayer‚Ìê‡‚Íwalk‚©fly‚©‚ğw’èAspell‚Ìê‡‚ÍSpell
/// </summary>
public enum MoveType
{
   Walk,
   Fly,
   Spell,
}


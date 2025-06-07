using UnityEngine;

public interface IPushable
{
    Transform pushbleTransform { get; }
    float radiusX { get; }
    float radiusZ{get;}
    float prioritizedRadius { get; }

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


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
/// Monster,�܂���Player�̏ꍇ��walk��fly�����w��Aspell�̏ꍇ��Spell
/// </summary>
public enum MoveType
{
   Walk,
   Fly,
   Spell,
}


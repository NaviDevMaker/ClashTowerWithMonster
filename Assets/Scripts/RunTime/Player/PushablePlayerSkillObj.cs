using UnityEngine;

public interface ISkills
{
    Transform skillTra { get; }
    float timerOffsetY { get; }
}

public class PushablePlayerSkillObj : MonoBehaviour, IPushable,ISkills,ISide
{
    public float rangeX { get; protected set;}

    public float rangeZ { get; protected set; }

    public float prioritizedRange { get; protected set;}

    public bool isKnockBacked_Unit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool isKnockBacked_Spell { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public MoveType moveType => throw new System.NotImplementedException();

    public UnitScale UnitScale => throw new System.NotImplementedException();

    public Transform skillTra { get; set; }

    public float timerOffsetY { get; set; }

    public int ownerID { get; private set; }

    public void Initialize(Transform skillTra,float timerOffsetY,int ownerID)
    {
        SetRange();
        this.ownerID = ownerID;
        this.skillTra = skillTra;
        this.timerOffsetY = timerOffsetY;
    }
    void SetRange()
    {    
        IColliderRangeProvider colliderRangeProvider = null;

        if(TryGetComponent<BoxCollider>(out var boxCollider))
        {
            colliderRangeProvider = new BoxColliderrangeProvider { boxCollider = boxCollider };
            rangeX = colliderRangeProvider.GetRangeX();
            rangeZ = colliderRangeProvider.GetRangeZ();
            prioritizedRange = colliderRangeProvider.GetPriorizedRange();
        }
        else if(TryGetComponent<SphereCollider>(out var sphereCollider))
        {
            colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
            var scaleAmount = sphereCollider.gameObject.transform.localScale.magnitude;
            rangeX = colliderRangeProvider.GetRangeX() * scaleAmount;
            rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount;
            prioritizedRange = colliderRangeProvider.GetPriorizedRange() * scaleAmount;
        }
        else return;
    }
}


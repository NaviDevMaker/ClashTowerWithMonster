using UnityEngine;

public class PrefabBase : MonoBehaviour
{
    public int sortOrder;//����̐��l��cardData�̋K��̂�ɍ��킹��
    public CardType cardType { get; set;}
    public Vector3 colliderSize { get; protected set; }
    public virtual void Initialize() { }
}

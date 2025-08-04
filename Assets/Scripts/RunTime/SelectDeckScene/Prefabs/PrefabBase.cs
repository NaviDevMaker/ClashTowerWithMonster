using UnityEngine;

public class PrefabBase : MonoBehaviour
{
    public int sortOrder;//‚±‚ê‚Ì”’l‚ğcardData‚Ì‹K’è‚Ì‚â‚Â‚É‡‚í‚¹‚é
    public CardType cardType { get; set;}
    public Vector3 colliderSize { get; protected set; }
    public virtual void Initialize() { }
}

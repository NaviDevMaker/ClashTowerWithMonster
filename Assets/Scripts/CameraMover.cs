using Game.Players.Sword;
using UnityEditor.Rendering;
using UnityEngine;
using Game.Players;

public class CameraMover : MonoBehaviour
{
    [SerializeField] PlayerControllerBase<SwordPlayerController> player;

    Vector3 currentPos = Vector3.zero;
    Vector3 previousPos = Vector3.zero;
    private void Start()
    {
        SetPlayerCenter();
        previousPos = player.transform.position;
    }
    private void LateUpdate()
    {
        SetCameraPosition();
    }

    void SetCameraPosition()
    {
        currentPos = player.transform.position;
        var moveAmount = currentPos - previousPos;
        transform.position += moveAmount;
        previousPos = currentPos;
    }
    void SetPlayerCenter()
    {
        if(Physics.Raycast(transform.position,transform.forward, out RaycastHit hit,Mathf.Infinity,Layers.groundLayer))
        {
            var hitPos = hit.point;
            var pos = new Vector3(hitPos.x, player.transform.position.y, hitPos.z);
            player.transform.position = pos;
        }
    }
}

using UnityEngine;

namespace Game.Monsters.StingRay
{
    public class StingRaysSpawnController : MultiSpawnMonster<StingRayController>
    {
        protected override Vector3[] GetPositions(Vector3 pos)
        {
            var offsetX = 2f;
            var positions = new Vector3[]
            {
                pos + Vector3.right * offsetX,
                pos + Vector3.left * offsetX,
            };
            return positions;
        }
    }
}


using UnityEngine;

namespace Game.Monsters.Skeleton
{
    public class SkeletonSpawnController :MultiSpawnMonster<SkeletonController>
    {
        protected override Vector3[] GetPositions(Vector3 pos)
        {
            var offset = 2.5f;
            var positions = new Vector3[]
            {
                pos + Vector3.forward * offset,
                pos + Vector3.right * offset + Vector3.back * offset,
                pos + Vector3.left * offset + Vector3.back * offset
            };
            return positions;
        }
    }
}


using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.RatAssassin
{
    public class RatAssasinSpawnController : MultiSpawnMonster<RatAssassinController>
    {
        protected override void Start()
        {
            base.Start();
        }
        protected override Vector3[] GetPositions(Vector3 pos)
        {
            var offset = 1.5f;
            var positions = new Vector3[]
            {
                pos + Vector3.forward * offset,
                pos + (Vector3.right + Vector3.back) * offset,
                pos + (Vector3.left + Vector3.back) * offset,
            };
            return positions;
        }
    }
}

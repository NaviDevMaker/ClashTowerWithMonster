using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using UnityEngine;
using UnityEngine.Events;


namespace Game.Monsters.Bat
{
    public class BatsSpawnController : MultiSpawnMonster<BatController>
    {
        // Start is called once before the firstColor execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            Debug.Log(ownerID);
        }

        protected override Vector3[] GetPositions(Vector3 pos)
        {
            var offset = 2.0f;
            var positions = new Vector3[]
            {
                pos + Vector3.forward * offset,
                pos + Vector3.back * offset,
                pos + Vector3.right * offset,
                pos + Vector3.left * offset
            };
            return positions;
        }
    }
}


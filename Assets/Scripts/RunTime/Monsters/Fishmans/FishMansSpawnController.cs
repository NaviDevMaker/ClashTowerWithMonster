using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

namespace Game.Monsters.Fishman
{
    public class FishMansSpawnController : MultiSpawnMonster<FishmanController>
    {
        // Start is called once before the firstColor execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
        }

        protected override Vector3[] GetPositions(Vector3 pos)
        {
            var positions = new Vector3[]
            {
                pos,
                pos + Vector3.forward + Vector3.right,
                pos + Vector3.back + Vector3.right,
                pos + Vector3.forward + Vector3.left,
                pos + Vector3.back + Vector3.left
           };

            return positions;
        }
    }
}







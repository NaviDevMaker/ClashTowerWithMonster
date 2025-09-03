using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.Fishman
{
    public class FishMansSpawnController : MultiSpawnMonster
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
        }
        public async override void SpawnMonsters(Vector3 pos, Quaternion rot, UnityAction<IMonster, bool> alphaChange)
        {
            var positions = new Vector3[]
            {
                 pos,
                 pos + Vector3.forward + Vector3.right,
                 pos + Vector3.back + Vector3.right,
                 pos + Vector3.forward + Vector3.left,
                 pos + Vector3.back + Vector3.left
            };

            var prefab = multiSpawnMonsterData.MonsterPrefab;
            var delayTime = multiSpawnMonsterData.EachSpawnDelayTime;
            for (int i = 0; i < multiSpawnMonsterData.SpawnCount; i++)
            {
                var spawnPos = positions[i];
                var monster = Instantiate(prefab, spawnPos, rot);
                var monsterInterFace = monster.GetComponent<IMonster>();
                alphaChange(monsterInterFace, true);

                //Ç±Ç±ïÅí Ç…Ç¢ÇÁÇÒÇ©Ç‡ÅAPhotonÇæÇ∆CreateÇ»ÇÒÇøÇ·ÇÁÇ≈InstaciateÇíNÇ™ÇµÇΩÇ©ÇÃèÓïÒéùÇ¡ÇƒÇÈÇÁÇµÇ¢Ç©ÇÁ
                var cmp = monster.GetComponent<FishmanController>();
                if (cmp != null)
                {
                    cmp.ownerID = ownerID;
                    cmp.isSummoned = true;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            if (this != null) Destroy(this.gameObject);
        }
    }
}







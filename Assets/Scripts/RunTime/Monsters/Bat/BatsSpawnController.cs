using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using UnityEngine;
using UnityEngine.Events;


namespace Game.Monsters.Bat
{
    public class BatsSpawnController : MultiSpawnMonster
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            Debug.Log(ownerID);
        }

        public override async void SpawnMonsters(Vector3 pos, Quaternion rot, UnityAction<IMonster,bool> alphaChange)
        {
            var offset = 2.0f;
            var positions = new Vector3[]
            {
                pos + Vector3.forward * offset,
                pos + Vector3.back * offset,
                pos + Vector3.right * offset,
                pos + Vector3.left * offset
            };

            var prefab = multiSpawnMonsterData.MonsterPrefab;
            var delayTime = multiSpawnMonsterData.EachSpawnDelayTime;
            for (int i = 0; i < multiSpawnMonsterData.SpawnCount; i++)
            {
                var spawnPos = positions[i];
                var monster = Instantiate(prefab, spawnPos, rot);
                var monsterInterFace = monster.GetComponent<IMonster>();
                alphaChange(monsterInterFace,true);

                //Ç±Ç±ïÅí Ç…Ç¢ÇÁÇÒÇ©Ç‡ÅAPhotonÇæÇ∆CreateÇ»ÇÒÇøÇ·ÇÁÇ≈InstaciateÇíNÇ™ÇµÇΩÇ©ÇÃèÓïÒéùÇ¡ÇƒÇÈÇÁÇµÇ¢Ç©ÇÁ
                var cmp = monster.GetComponent<BatController>();
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


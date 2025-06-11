using Cysharp.Threading.Tasks;
using System;
using UnityEngine;


namespace Game.Spells.LaneFire
{
    public class LaneFire : SpellBase
    {
        protected override async UniTaskVoid Spell()
        {
            Debug.Log("ファイア発動！！！！！！！！！");
            addForceToUnit.KeepDistance(moveType);
            spellDamageHelper.DamageToUnit();
            particle.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
            particle.Stop();
            //Destroy(gameObject);
        }
    }
}



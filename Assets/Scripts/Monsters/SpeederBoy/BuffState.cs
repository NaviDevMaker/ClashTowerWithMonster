using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Game.Monsters.SpeederBoy
{
    public class BuffState :BuffStateBase<SpeederBoyController>
    {
        public BuffState(SpeederBoyController controller): base(controller) { }
        public override void OnEnter()
        {
            if (radius == 0f) radius = 8f;
            buffType = BuffType.Speed;
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
    }
}



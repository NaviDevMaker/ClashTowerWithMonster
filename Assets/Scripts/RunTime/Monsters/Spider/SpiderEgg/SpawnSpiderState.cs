using UnityEngine;
using static Game.Monsters.Spider.SpiderController;


namespace Game.Monsters.SpiderEgg
{
    public class SpawnSpiderState : StateMachineBase<SpiderEggController>
    {
        public SpawnSpiderState(SpiderEggController controller) : base(controller) { }

        public override void OnEnter()
        {
            nextState = controller.DeathState;
            SpawnSpider();
            controller.ChangeState(nextState);
        }
        public override void OnExit() { }
        public override void OnUpdate() { }
        void SpawnSpider()
        {
            var originalScale = controller.spiderPrefab.transform.lossyScale;
            var pos = controller.transform.position;
            var rot = controller.spiderPrefab.transform.rotation;
            var spider = UnityEngine.Object.Instantiate(controller.spiderPrefab, pos, rot);
            spider.ownerID = controller.ownerID;
        }
    }
}



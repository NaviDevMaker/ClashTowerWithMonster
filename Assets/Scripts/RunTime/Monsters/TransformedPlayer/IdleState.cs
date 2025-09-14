using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
namespace Game.Monsters.TransformedPlayer
{
    public class IdleState : IdleStateBase<TransformedPlayer>
    {
        public IdleState(TransformedPlayer controller) : base(controller) { }


        public override void OnEnter()
        {
            OnEnterProcess().Forget();
        }
        public override void OnUpdate()
        {
           base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask OnEnterProcess()
        {
            nextState = controller.ChaseState;
            var fadeDuration =  3.0f;
            var tasks = Enumerable.Empty<UniTask>().ToList();
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    FadeProcessHelper.ChangeToTranparent(mat);
                    var color = mat.color;
                    color.a = 0f;
                    mat.color = color;
                    Debug.Log(mat.color);
                    var task = FadeProcessHelper.FadeInColor(fadeDuration,mat,controller.GetCancellationTokenOnDestroy());
                    tasks.Add(task);
                }
            }
            await UniTask.WhenAll(tasks);
            if (controller == null) return;
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    FadeProcessHelper.ChangeToOpaque(mat);
                }
            }
            isEndSummon = true;
        }
    }
}
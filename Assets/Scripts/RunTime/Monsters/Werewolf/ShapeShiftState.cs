using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class ShapeShiftState : StateMachineBase<WerewolfController>
    {
        public ShapeShiftState(WerewolfController controller) : base(controller)
        {
            SetTransformedPlayerPrefab();
        }

        TransformedPlayer.TransformedPlayer transformedPlayer;
        public override void OnEnter()
        {
            var animPar = controller.MonsterAnimPar;
            controller.animator.SetBool(animPar.Attack_Hash,true);
            controller.animator.SetBool(animPar.Chase_Hash, true);
            controller.statusCondition.NonTarget.isActive = true;
            controller.animator.Play("Idle");
            DisappaerWerewolf();
        } 
        public override void OnUpdate() { }
        public override void OnExit()
        {
            controller.statusCondition.NonTarget.isActive = false;
        }
        async void SetTransformedPlayerPrefab()
        {
            var transformedPlayerObj = await SetFieldFromAssets.SetField<GameObject>("");
            if (transformedPlayerObj == null) return;
            transformedPlayer = transformedPlayerObj.GetComponent<TransformedPlayer.TransformedPlayer>();
        }

        async void DisappaerWerewolf()
        {
            var fadeDuration = 3.0f;
            var tasks = Enumerable.Empty<UniTask>().ToList();
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    FadeProcessHelper.ChangeToTranparent(mat);
                    var color = mat.color;
                    color.a = 1.0f;
                    mat.color = color;
                    Debug.Log(mat.color);
                    var task = FadeProcessHelper.FadeOutColor(fadeDuration, mat, controller.GetCancellationTokenOnDestroy());
                    tasks.Add(task);
                }
            }
            await UniTask.WhenAll(tasks);
            controller.gameObject.SetActive(false);
        }
    }
}



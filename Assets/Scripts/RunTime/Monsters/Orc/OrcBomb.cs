using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Monsters.Orc;
using System;
using UnityEngine;

public class OrcBomb : MonoBehaviour,IRangeAttack
{
    float elapsedTime = 0f;
    float maxDegree = 30f;
    float baseFrequency = 2f;
    float acccelation = 0.3f;
    float angle = 0f;


    public GameObject rangeAttackObj { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    private void Update()
    {
       PendulumAction();
    }
    public async void StartBombCount(OrcController attacker)
    {
        try
        {
            this.SummonMoveAction(offsetY:4.0f);
            Debug.Log("”š’e‚ð’u‚«‚Ü‚·");
            var mat = GetComponent<MeshRenderer>().material;
            var startIntencity = -10f;
            var finalIntencity = 10f;

            var baseColor = new Color(191f, 74f, 74f); //
            var endColor = baseColor * finalIntencity;
            var duration = 4.0f;
            var colorTask = DOTween.To(
                () => startIntencity,
                 currentIntencity => mat.SetColor("_EmissionColor", baseColor * currentIntencity),
                 finalIntencity,
                 duration
                 ).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
            await colorTask;
            Debug.Log("”š”j‚µ‚Ü‚·");
            var color = mat.color;
            color.a = 0f;
            mat.color = color;
            var damage = attacker.bombInfo.bombDamage;
            EffectManager.Instance.expsionEffect.GenerateExplosionEffect(transform.position);
            var currentTargets = this.GetUnitInSpecificRangeItem(attacker).Invoke();
            if (currentTargets.Count == 0) return;
            currentTargets.ForEach(target =>
            {
                if (target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
                {
                    unitDamagable.Damage(damage);
                }
            });
        }
        catch (OperationCanceledException) { }
        finally { if (this != null) Destroy(gameObject); }
    }
    void PendulumAction()
    {
        elapsedTime += Time.deltaTime;
        var currentFrequency = baseFrequency + acccelation * elapsedTime;
        var angularVelocity = currentFrequency * 2 * Mathf.PI;
        angle += angularVelocity * Time.deltaTime;
        var rotZ = maxDegree * Mathf.Sin(angle);
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }

    public void SetHitJudgementObject(){ throw new NotImplementedException(); }
}

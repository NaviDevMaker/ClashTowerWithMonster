using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public interface ISelectableSpell
{ 
    SpellStatus _spellStatus { get;}
    UnityAction<LineRenderer> OnSelectedDeckFirst { get; set; }
    void SpellInvoke(CancellationTokenSource cls);
    void SpellRangeDraw();
}

public class SelectableSpell : PrefabBase, ISelectableSpell
{
    [SerializeField] SpellStatus spellStatus;
    [SerializeField] SpellBase spellBase;
    SpellBase spellPrefab;
    public SpellStatus _spellStatus  => spellStatus;
    public LineRenderer lineRenderer { get;private set;}
    public UnityAction<LineRenderer> OnSelectedDeckFirst { get; set; } = null;
    class SpellInfo
    {
        public Vector3 pos { get; set; }
        public Quaternion rot { get; set; }
        public float rangeX { get; set; }
        public float rangeZ { get; set; }
    }
    SpellInfo spellInfo;
    public override async void Initialize()
    {
        base.Initialize();
        //gameObject.SetActive(false);
        var pos = transform.position;
        var rot = spellBase.transform.rotation;
        spellPrefab = Instantiate(spellBase,pos,rot);
        await UniTask.DelayFrame(100);
        spellPrefab.gameObject.SetActive(false);

        Debug.Log($"{spellPrefab.rangeX},{spellPrefab.rangeZ}");
        spellInfo = new SpellInfo
        {
            pos = pos,
            rot = rot,
            rangeX = spellPrefab.rangeX,
            rangeZ = spellPrefab.rangeZ
        };

        offsetZ = spellPrefab.rangeZ;
        SetUpLineRenderer();
        SpellRangeDraw();
        LitLineRenderer();
        if (OnSelectedDeckFirst != null) OnSelectedDeckFirst.Invoke(lineRenderer);
    }
    public async void SpellInvoke(CancellationTokenSource spellCls)
    {
        //ここのclsはscrollClsとuseButtonのcls
        lineRenderer.enabled = false;
        var pos = spellInfo.pos;    
        var rot = spellInfo.rot;
        //カード押されたとき、スクロールされたとき、
        var spell = Instantiate(spellPrefab, pos, rot);
        try
        {
            spell.gameObject.SetActive(true);
            await UniTask.DelayFrame(100,cancellationToken:spellCls.Token);
            var duration = spell.spellDuration + 5.0f;
            spell.SpellInvoke();

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: spellCls.Token);
        }            
        catch (OperationCanceledException) { }
        finally
        {
           if(spell != null) Destroy(spell.gameObject);
        }
    }
    public void SpellRangeDraw()
    {
        var center = transform.position;
        var rangeX = spellInfo.rangeX;
        var rangeZ = spellInfo.rangeZ;
        var offsetY = 0.25f;
        Debug.Log($"ｘは{rangeX}、ｚは{rangeZ}");
        if (lineRenderer != null)
        {
            lineRenderer.DrawRange(center,rangeX,rangeZ,offsetY);
        }
    }
    async void SetUpLineRenderer()
    {
        GameObject prefab = null;
        try
        {
            prefab = await SetFieldFromAssets.SetField<GameObject>("Prefabs/LineRenderer");
        }
        catch (Exception) {throw;}
        var lineRendererObj = Instantiate(prefab);
        lineRenderer = lineRendererObj.GetComponent<LineRenderer>();
        lineRendererObj.transform.SetParent(transform);
        lineRenderer.SetUpLineRenderer();
    }

    void LitLineRenderer()
    {
        Func<float, float, Material, UniTask> waitAction = async (radAdjust, maxIntencity, material) =>
        {
            var baseColor = material.GetColor("_EmissionColor");
            while (!this.GetCancellationTokenOnDestroy().IsCancellationRequested)
            {
                var amount = (Mathf.Cos(Time.time * radAdjust) * 0.5f + 0.5f) * maxIntencity;
                material.SetColor("_EmissionColor", baseColor * amount);
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        };
        lineRenderer.LitLineRendererMaterial(waitAction);
    }
}

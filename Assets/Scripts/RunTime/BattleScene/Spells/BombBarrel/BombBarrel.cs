using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BombBarrel : SpellBase
{
    List<GameObject> chunks = new List<GameObject>();
    Transform barrelMainbody = null;
    protected override async void Initialize()
    {
        _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/BombBarrel");
        base.Initialize();
        pushEffectUnit = PushEffectUnit.AllUnit;
        particle = transform.GetChild(0).GetComponent<ParticleSystem>();
        addForceToUnit = new AddForceToUnit<SpellBase>(this, _SpellStatus.PushAmount, _SpellStatus.PerPushDurationAndStunTime,pushEffectUnit);
    }
    protected override async UniTaskVoid Spell()
    {
        TimerSetter.Instance.StartSpellTimer(spellDuration, this);//Ç±ÇÍÇ†Ç∆Ç≈è¡ÇµÇƒÇÀ
        await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
        addForceToUnit.KeepDistance(moveType);
        spellEffectHelper.EffectToUnit();
        await ExplosionBarrel();
        DestroyAll();
    }
    async UniTask ExplosionBarrel()
    {
        if (particle == null) return;
        particle.Play();
        barrelMainbody = transform.GetChild(1);
        var scale = barrelMainbody.transform.localScale;
        foreach (Transform child in barrelMainbody)
        {
            var barrelMesh = child.GetComponent<MeshFilter>().mesh;
            var barrelMaterial = child.GetComponent<MeshRenderer>().material;
            var verticles = barrelMesh.vertices;
            var triangles = barrelMesh.triangles;
            var trianglePos = child.transform.position;
            int step = 6 * 3;
            chunks.AddRange(this.GetDivisionMesh<BombBarrel>(
            
                step,
                triangles,
                verticles,
                scale,
                trianglePos,
                barrelMaterial

            ));
            //for (int i = 0; i < triangles.Length; i += step)
            //{
            //    List<Vector3> verts = new();
            //    List<int> tris = new();
            //    Dictionary<int, int> vertMap = new(); // oldIndex -> newIndex

            //    for (int t = i; t < i + step && t + 2 < triangles.Length; t += 3)
            //    {
            //        for (int j = 0; j < 3; j++)
            //        {
            //            int oldIndex = triangles[t + j];
            //            if (!vertMap.ContainsKey(oldIndex))
            //            {
            //                vertMap[oldIndex] = verts.Count;
            //                verts.Add(verticles[oldIndex]);
            //            }
            //            tris.Add(vertMap[oldIndex]);
            //        }
            //    }
            //    var triangleMesh = new Mesh();
            //    triangleMesh.vertices = verts.ToArray();
            //    triangleMesh.triangles = tris.ToArray();

            //    var triangleObj = new GameObject("Chunk");
            //    var size = barrelMainbody.transform.localScale;
            //    triangleObj.transform.position = child.transform.position;
            //    triangleObj.transform.localScale = size;
            //    triangleObj.AddComponent<MeshFilter>().mesh = triangleMesh;
            //    triangleObj.AddComponent<MeshRenderer>().material = barrelMaterial;
            //    triangleObj.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //    triangleObj.AddComponent<BoxCollider>();
            //    var rb = triangleObj.AddComponent<Rigidbody>();
            //    rb.isKinematic = true;
            //    Debug.Log("ÉÅÉbÉVÉÖÇ™äÆê¨ÇµÇ‹ÇµÇΩ");
            //    chunks.Add(triangleObj);
            //}
            child.gameObject.SetActive(false);
        }
        
        await BarrelScattering();
    }

    async UniTask BarrelScattering()
    {
        //MaxílÇä‹Ç‹ÇπÇΩÇ¢Ç©ÇÁfloat    

        //var min = -5.0f;
        //var max = 5.0f;
        //var fadeOutTime = 4.0f;
        //var tasks = new List<UniTask>();
        //chunks.ForEach(chunk =>
        //{
        //    var rb = chunk.GetComponent<Rigidbody>();
        //    var material = chunk.GetComponent<MeshRenderer>().material;
        //    if(material.name.StartsWith("Barrel"))
        //    {
        //        if (material.HasProperty("_Surface")) FadeOutHelper.ChangeToTranparent(material);
        //    }
        //    rb.isKinematic = false;
        //    var forceVector = new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(0, max), UnityEngine.Random.Range(min, max));
        //    rb.AddForce(forceVector,ForceMode.Impulse);
        //    rb.AddTorque(forceVector, ForceMode.Impulse);
        //    var task = FadeOutHelper.FadeOutColor(fadeOutTime,this.GetCancellationTokenOnDestroy(),material);
        //    tasks.Add(task);   
        //});
        var name = "Barrel";
        var min = -5.0f;
        var max = 5.0f;
        //await UniTask.WhenAll(tasks);
        await this.Scattering<BombBarrel>(chunks, name,min,max);
    }
    protected override void SetDuration()
    {
        spellDuration = 3f;
    }
    protected override void DestroyAll()
    {
        Destroy(gameObject);
    }
}

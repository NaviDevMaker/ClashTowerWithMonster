using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;


public interface ISelectableMonster
{
    SkinnedMeshRenderer _bodyMesh { get;}

    bool _isFlying { get; }
    CancellationTokenSource expectedCls { get; set; }

    void Depetrification(CancellationTokenSource cls, Func<bool> isSettedOriginalPos);
    void Repetrification();
}

public class SelectableMonster : PrefabBase, ISelectableMonster
{
    public List<Renderer> myMeshRenderers { get; set; } = new List<Renderer>();
    List<Material> originalMaterials = new List<Material>();
    List<Material> copiedMaterials = new List<Material>();
    List<GameObject> chunks = new List<GameObject>();
    List<UniTask> depetrificationTasks = new List<UniTask>();
    public Material stoneMaterial { get; set; } = null;
    public MonsterAnimatorPar monsterAnimatorPar { get; set; }
    Animator animator;

    [SerializeField] bool isFlying;
    [SerializeField] SkinnedMeshRenderer bodyMesh;
    [SerializeField] MonsterStatusData monsterStatusData;
    bool isOneMesh = false;
    bool isPetrification = false;
    public CancellationTokenSource expectedCls { get; set;} = null;
    public SkinnedMeshRenderer _bodyMesh => bodyMesh;
    public bool _isFlying => isFlying;

    public override void Initialize()
    {
        var col = GetComponent<BoxCollider>();
        colliderSize = col.bounds.size;
        animator = GetComponent<Animator>();
        animator.Play("Idle");
        animator.speed = 0f;
        myMeshRenderers = GetComponentsInChildren<Renderer>().ToList();
        myMeshRenderers.ForEach(renderer => originalMaterials.Add(renderer.material));

        if(isFlying)
        {
            var offsetY = 3f;
            var newPos = transform.position;
            newPos.y += offsetY;
            transform.position = newPos;
        }
        SetStoneMaterial();
    }
    void SetStoneMaterial()
    {
        Debug.Log("石化します");
        if (myMeshRenderers.Count == 1)
        {
            SetStoneMaterial_OneMesh();
            isOneMesh = true;
            return;
        }

        myMeshRenderers.ForEach(mesh =>
        {
            var newMats = new Material[mesh.materials.Length];
            var obj = mesh.gameObject;

            for (int i = 0; i < mesh.materials.Length; i++)
            {
                var copiedMaterial = new Material(stoneMaterial);
                //copiedMaterial.renderQueue = 3010;
                var mainTexture = mesh.materials[i].GetTexture("_BaseMap");
                if(copiedMaterial.HasProperty("_MainTex"))
                {
                    copiedMaterial.SetTexture("_MainTex", mainTexture);
                }
                copiedMaterial.SetFloat("_Alpha", 1.0f);
                newMats[i] = copiedMaterial;   
                copiedMaterials.Add(copiedMaterial);
            }
            mesh.materials = newMats;
        });
    }
    void SetStoneMaterial_OneMesh()
    {
        Mesh mesh = null;
        isPetrification = true;
        var targetRenderer = myMeshRenderers[0];
        if (targetRenderer is SkinnedMeshRenderer skinned)
        {
            mesh = new Mesh();//Meshに上書きしていくからインスタンスは必要らしい
            skinned.BakeMesh(mesh);
        }
        else if (targetRenderer is MeshRenderer meshRenderer) mesh = meshRenderer.GetComponent<MeshFilter>().mesh;

        var main = targetRenderer.material.GetTexture("_BaseMap");
        var copiedMaterial = new Material(stoneMaterial);
        copiedMaterial.SetTexture("_MainTex", main);
        copiedMaterial.SetFloat("_Alpha", 1.0f);
        var verticles = mesh.vertices;
        var triangles = mesh.triangles;
        var uvs = mesh.uv;
        var step = 512 * 3;
        var scale = Vector3.one;
        var pos = gameObject.transform.position;
        chunks.AddRange(this.GetDivisionMesh<SelectableMonster>(      

            step,
            triangles,
            verticles,
            uvs,
            scale,
            pos,
            copiedMaterial
        ));

        var parentName = $"{gameObject.name}Chunks";
        var parentObj = new GameObject(parentName);
    
        chunks.ForEach(chunk =>
        {
            chunk.transform.SetParent(parentObj.transform);
            var chunkMesh = chunk.GetComponent<MeshFilter>().mesh;
            var vertices = chunkMesh.vertices;         

            var newUvs = new Vector2[vertices.Length];
            var scale = 0.1f;
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                newUvs[i] = new Vector2(vert.x * scale, vert.z * scale);
            }
            chunkMesh.uv = newUvs;
            chunk.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        });
        gameObject.SetActive(false);
    }

    public async void Depetrification(CancellationTokenSource cls,Func<bool> isSettedOriginalPos)
    {
        isPetrification = false;
        depetrificationTasks.Clear();
        depetrificationTasks.TrimExcess();
        var max = 1.0f;
        var min = 0.25f;
        var duration = 2.0f;
        var time = 0f;

        Func<float,float> lerpValue =((lerp) => Mathf.Clamp01(Mathf.Lerp(max, min, lerp)));
        Func<Material, UniTask> depetrificationAction = (async (m) =>
        {
            try
            {
                while (time < duration && !cls.IsCancellationRequested && !isSettedOriginalPos())
                {
                    time += Time.deltaTime;
                    var lerp = time / duration;
                    var progress = lerpValue(lerp);
                    m.SetFloat("_PetrifyProgress", progress);
                    if(isOneMesh)
                    {
                        var alpha = lerpValue(lerp);
                        m.SetFloat("_Alpha", alpha);
                    }
                    await UniTask.Yield(cancellationToken: cls.Token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        });

        if(isOneMesh)
        {
            gameObject.SetActive(true);
            chunks.ForEach(chunk =>
            {
                var meshRenderer = chunk.GetComponent<MeshRenderer>();
                var material = meshRenderer.material;
                material.renderQueue = 3010;
                depetrificationTasks.Add(depetrificationAction(material));
            });
            await UniTask.WhenAll(depetrificationTasks);
            chunks.ForEach(chunk =>
            {
                var meshRenderer = chunk.GetComponent<MeshRenderer>();
                var material = meshRenderer.material;
                material.renderQueue = 3000;
            });
            var originalMaterial = originalMaterials[0];
            SetEmission(originalMaterial,cls);
            //gameObject.SetActive(true);
        }
        else
        {
            myMeshRenderers.ForEach(r =>
            {
                var m = r.material;
                depetrificationTasks.Add(depetrificationAction(m));
            });
            await UniTask.WhenAll(depetrificationTasks);

            try
            {
                if (cls.IsCancellationRequested) return;
            }
            catch (ObjectDisposedException) { return; }
            for (int i = 0; i < myMeshRenderers.Count;i++)
            {
                var originalMaterial = originalMaterials[i];
                var r = myMeshRenderers[i];
                r.material = originalMaterial;
                SetEmission(originalMaterial,cls);
            }
        }
    }
    public void Repetrification()
    {
        isPetrification = true;
        animator.Play("Idle");
        animator.speed = 0f;
        if(isOneMesh)
        {
            gameObject.SetActive(false);
            chunks.ForEach(chunk =>
            {
                chunk.gameObject.SetActive(true);
                var r = chunk.GetComponent<MeshRenderer>();
                var m = r.material;
                m.SetFloat("_PetrifyProgress", 1.0f);
                m.SetFloat("_Alpha", 1.0f);
            });           
        }
        else
        {
            for(int i = 0;i < myMeshRenderers.Count;i++)
            {
                var r = myMeshRenderers[i];
                var copiedMaterial  = copiedMaterials[i];
                r.material = copiedMaterial;
                var m = r.material;
                m.SetFloat("_PetrifyProgress", 1.0f);
                m.SetFloat("_Alpha", 1.0f);
            }
        }
    }

    async void SetEmission(Material originalMaterial,CancellationTokenSource cls)
    {
        var color = new Color(191,191,0);
        originalMaterial.EnableKeyword("_EMISSION");
        originalMaterial.SetTexture("_EmissionMap", null);
        var max = 16;
        var inactiveValue = max / 2;
        var start = 1f;
        originalMaterial.SetColor("_EmissionColor", color * start);
        var duration = 1.0f;
        var time = 0f;
        var isSummoned = false;
        Func<float,float,UniTask> intencitySet = (async (startValue,endValue) =>
        {
            try
            {
                while (time < duration && !cls.IsCancellationRequested && gameObject.activeSelf)
                {
                    time += Time.deltaTime;
                    var lerp = time / duration;
                    var intencity = Mathf.Lerp(startValue, endValue, lerp);
                    originalMaterial.SetColor("_EmissionColor", color * intencity);
                    if(intencity >= inactiveValue && !isSummoned && gameObject.activeSelf)
                    {
                        if (isOneMesh) chunks.ForEach(chunk => chunk.SetActive(false));
                        var myPos = transform.position;
                        var pos = PositionGetter.GetFlatPos(myPos);
                        StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(pos, cardType));
                        isSummoned = true;
                    }
                    await UniTask.Yield(cancellationToken: cls.Token);
                }
                originalMaterial.SetColor("_EmissionColor", color * endValue);
            }
            catch (OperationCanceledException)
            {
                originalMaterial.SetColor("_EmissionColor", color * start);
                originalMaterial.DisableKeyword("_EMISSION");
                return;
            }
        });

        await intencitySet(start,max);
        var endValue = -10f;
        await intencitySet(max, endValue);
        originalMaterial.DisableKeyword("_EMISSION");
        
        if(expectedCls != null && expectedCls == cls && !isPetrification) animator.speed = 1.0f;
    }
}

using Cysharp.Threading.Tasks.Triggers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SelectableMonster : MonoBehaviour
{
    public List<Renderer> myMeshRenderers { get; set; } = new List<Renderer>();
    List<Material> originalMaterial = new List<Material>();
    List<GameObject> chunks = new List<GameObject>();
    Material stoneMaterial = null;
    Animator animator;
    public void Initialize(Material stoneMaterial)
    {
        animator = GetComponent<Animator>();
        animator.Play("Idle");
        animator.speed = 0f;
        myMeshRenderers = GetComponentsInChildren<Renderer>().ToList();
        myMeshRenderers.ForEach(renderer => originalMaterial.Add(renderer.material));
        this.stoneMaterial = stoneMaterial;
        SetStoneMaterial();
    }
    void SetStoneMaterial()
    {
        if (myMeshRenderers.Count == 1)
        {
            SetStoneMaterial_OneMesh();
            return;
        }

        myMeshRenderers.ForEach(mesh =>
        {
            var newMats = new Material[mesh.materials.Length];
            var obj = mesh.gameObject;

            for (int i = 0; i < mesh.materials.Length; i++)
            {
                var copiedMaterial = new Material(stoneMaterial);
                newMats[i] = copiedMaterial;   
            }
            mesh.materials = newMats;
        });
    }
    void SetStoneMaterial_OneMesh()
    {
        Mesh mesh = null;
        var targetRenderer = myMeshRenderers[0];
        if (targetRenderer is SkinnedMeshRenderer skinned)
        {
            mesh = new Mesh();
            skinned.BakeMesh(mesh);
        }
        else if (targetRenderer is MeshRenderer meshRenderer) mesh = meshRenderer.GetComponent<MeshFilter>().mesh;
   
        var verticles = mesh.vertices;
        var triangles = mesh.triangles;
        var uvs = mesh.uv;
        var step = 512 * 3;
        var scale = gameObject.transform.localScale;
        var pos = gameObject.transform.position;
        chunks.AddRange(this.GetDivisionMesh<SelectableMonster>(      

            step,
            triangles,
            verticles,
            uvs,
            scale,
            pos,
            stoneMaterial
        ));

        var parentName = $"{gameObject.name}Chunks";
        var parentObj = new GameObject(parentName);
    
        chunks.ForEach(chunk =>
        {
            chunk.transform.SetParent(parentObj.transform);
            //var meshRenderer = chunk.GetComponent<MeshRenderer>();  
            //var material = meshRenderer.material;
            //material.SetFloat("_UVScale", 0.05f);
            var chunkMesh = chunk.GetComponent<MeshFilter>().mesh;
            var vertices = chunkMesh.vertices;
            //var triangles = chunkMesh.triangles;
            //var newUvs = new Vector2[]
            //{
            //   Vector2.zero,
            //   new Vector2(1,0),
            //   new Vector2(0,1),
            //};

            var newUvs = new Vector2[vertices.Length];
            var scale = 0.1f;
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = vertices[i];
                  //Debug.Log($"UV[{i}] = {worldPos.x * scale}, {worldPos.z * scale}");
                newUvs[i] = new Vector2(vert.x * scale, vert.z * scale);//uvs.Length > i ? uvs[i]  : Vector2.zero; // ←XZベース & スケーリング
            }
            chunkMesh.uv = newUvs;
            //Debug.Log($"uv length: {chunkMesh.uv.Length}, vertices length: {mesh.vertices.Length}");
            chunk.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        });
        gameObject.SetActive(false);
    }
}

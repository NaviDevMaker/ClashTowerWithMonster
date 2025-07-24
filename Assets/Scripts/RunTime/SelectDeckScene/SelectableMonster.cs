using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectableMonster : MonoBehaviour
{
    public List<MeshRenderer> myMeshRenderers { get; set; } = new List<MeshRenderer>();
    List<Material> originalMaterial = new List<Material>();
    public Material stoneMaterial { get; set; } = null;
    public void Initialize()
    {
        myMeshRenderers = GetComponentsInChildren<MeshRenderer>().ToList();
        myMeshRenderers.ForEach(renderer => originalMaterial.Add(renderer.material));
    }
    void SetStoneMaterial()
    {
        if (myMeshRenderers.Count  > 1) return;

    }
}

using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public static class GetObjectFromChildren
{
    public static GameObject GetObject(this GameObject parentObj,string name)
    {
        var parentTransform = parentObj.transform;
        foreach (Transform child in parentTransform)
        {
            Debug.Log(child.name);
            if (child.name.StartsWith(name)) return child.gameObject;
            var found = GetObject(child.gameObject,name);
            if (found != null) return found;
        }
        return null;
    }
}

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class SortExtention
{
    public static T[] GetSortedArrayByDistance_Sphere<T>(GameObject originObj, float radius) where T : UnitBase
    {
        Collider[] sortedArray;
        Collider[] hits = Physics.OverlapSphere(originObj.transform.position, radius);
        if (hits.Length > 0)
        {
           sortedArray = hits.OrderBy(hit => Vector3.Distance(originObj.transform.position, hit.transform.position)).ToArray();
           var filterdArray = sortedArray
                .Select(hit => hit.GetComponent<T>())
                .Where(cmp => cmp != null && originObj.gameObject != cmp.gameObject).ToArray();
           return filterdArray;
        } 

        return default;             
    }

    public static Collider[] GetSpecificColliderInRange<T>(UnitBase originObj,float radius)
    {

        Collider[] sortedArray;
        Collider[] hits = Physics.OverlapSphere(originObj.transform.position, radius);
        if (hits.Length > 0)
        {
            sortedArray = hits.OrderBy(hit => Vector3.Distance(originObj.transform.position, hit.transform.position)).ToArray();
            var filterdArray = sortedArray
                 .Where(hit => hit.GetComponent<T>() != null && originObj.gameObject != hit.gameObject).ToArray();
            return filterdArray;
        }

        return default;
    }
    public static RaycastHit[] GetSortedArrayByDistance_Ray<T>(UnitBase originObj,Vector3 rayDirection, float rayDistance)
    {

        RaycastHit[] sortedArray;
        RaycastHit[] hits = Physics.RaycastAll(originObj.transform.position,rayDirection,rayDistance);
        if (hits.Length > 0)
        {
            sortedArray = hits.OrderBy(hit => Vector3.Distance(originObj.transform.position, hit.transform.position)).ToArray();
            var filterdArray = sortedArray
                .Where(hit =>hit.collider.GetComponent<T>() != null).ToArray();
            return filterdArray;
        }

        return default;
    }
}

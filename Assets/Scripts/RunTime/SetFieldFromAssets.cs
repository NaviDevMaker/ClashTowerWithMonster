using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

//asset‚©‚çƒf[ƒ^‚ğæ‚è‚Şˆ—
public static class SetFieldFromAssets
{
   public static async UniTask<T> SetField<T>(string address)
   {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
        await handle.ToUniTask();
        if (handle.Status == AsyncOperationStatus.Succeeded) return handle.Result;
        else return (T)default;

   }
}

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

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

   public static async UniTask<IList<T>> SetFieldByLabel<T>(string labelName)
   {
      AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(labelName);
      await handle.ToUniTask();
      if(handle.Status == AsyncOperationStatus.Succeeded) return handle.Result;
      else return (IList<T>)default;
   }
}

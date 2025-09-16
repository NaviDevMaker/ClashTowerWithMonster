using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class TransformedMethod
{
    public static Sequence GetTransformSequence<T>(this T controller,float shapeShiftDuration) where T : MonoBehaviour
    {
        var originalScale = controller.transform.lossyScale;
        var targetScale = new Vector3(originalScale.x, originalScale.y * 4f, originalScale.z);

        var scaleSet = new Vector3TweenSetup(targetScale, shapeShiftDuration / 2);
        var scaleToOriginalSet = new Vector3TweenSetup(originalScale, shapeShiftDuration / 2);
        var scaleTween = controller.gameObject.Scaler(scaleSet);
        var originalScaleTween = controller.gameObject.Scaler(scaleToOriginalSet);
        var rotTween = controller.transform.DORotate(new Vector3(0f, -720f, 0f)
                                 , shapeShiftDuration, RotateMode.FastBeyond360);
        var sequence = DOTween.Sequence();
        sequence.Append(rotTween)
             .Join(scaleTween)
            .Append(originalScaleTween);
        return sequence;
    }

    public static async void ShapeEffectAction<T>(this T controller) where T : MonoBehaviour
    {
        var pos = controller.transform.position;
        var effectObj = PoolObjectPreserver.TransformerEffectGetter();
        if (effectObj == null)
        {
            var shapeObjPrefab = await SetFieldFromAssets.SetField<GameObject>("Effects/ShapeShiftSpellEffect");
            effectObj = UnityEngine.Object.Instantiate(shapeObjPrefab);
            PoolObjectPreserver.transformerEffectList.Add(effectObj);
        }
        effectObj.transform.position = pos;
        effectObj.transform.rotation = Quaternion.identity;
        var effect = effectObj.GetComponent<ParticleSystem>();
        effect.Play();
        await RelatedToParticleProcessHelper.WaitUntilParticleDisappear(effect);
        effectObj.SetActive(false);
    }

}

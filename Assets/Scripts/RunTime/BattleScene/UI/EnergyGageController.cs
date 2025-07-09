using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnergyGageController : MonoBehaviour
{
    RawImage energyLiquidImage;
    [SerializeField] Text energyCountText;

    float energyChargeTime_1 = 2.8f;
    float energyChargeTIme_2 = 1.4f;
    float energyChargeTIme_3 = 0.9f;

    float energyTimer = 0f;

    float maxWidth = 0f;
    int maxEnergy = 10;
    public int currentEnergy { get; private set; } = 0;

    public UnityAction ShakeUIAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        energyLiquidImage = GetComponent<RawImage>();
        maxWidth = energyLiquidImage.rectTransform.rect.width;
        SetFirstEnergy();
    }
    // Update is called once per frame
    void Update()
    {
        ChargeEnergy();
    }

    void SetFirstEnergy()
    {
        var energyRect = energyLiquidImage.rectTransform;
        var firstEnergy = 6;
        currentEnergy = firstEnergy;
        energyCountText.text = currentEnergy.ToString();
        RenewChargeImageVisual();
    }
    void ChargeEnergy()
    {
        if (currentEnergy == maxEnergy) return;
        energyTimer += Time.deltaTime;
        if(energyTimer >= energyChargeTime_1)
        {
            Debug.Log(energyTimer);
            energyTimer -= energyChargeTime_1;
            currentEnergy++;
            energyCountText.text = currentEnergy.ToString();
            var tween = UIFuctions.ShakeUI(energyCountText);
        }
        RenewChargeImageVisual();
    }

    void RenewChargeImageVisual()
    {
        var currentFill = (float)currentEnergy / maxEnergy;
        var plusFill = (energyTimer / energyChargeTime_1) / maxEnergy;
        var targetFill = currentFill + plusFill;
        var targetWidth = maxWidth * targetFill;
        var rect = energyLiquidImage.rectTransform;
        rect.sizeDelta = new Vector2(targetWidth,rect.rect.height);
    }
    public void ReduceCardEnergy(Card card) // bool
    {
        //if (card.CardData.Energy > currentEnergy) return false;
        currentEnergy -= card.CardData.Energy;
        energyCountText.text = currentEnergy.ToString();
        var tween = UIFuctions.ShakeUI(energyCountText);

        //return true;
    } 
    public void ReduceSkillEnergy(int energy)
    {
        currentEnergy -= energy;
        energyCountText.text = currentEnergy.ToString();
        var tween = UIFuctions.ShakeUI(energyCountText);
        ShakeUIAction?.Invoke();
    }
}


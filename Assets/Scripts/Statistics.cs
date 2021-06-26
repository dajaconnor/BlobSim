using Assets.Enums;
using Assets.Models;
using UnityEngine;

public class Statistics : MonoBehaviour
{

    internal int numBlobs;
    public int recordBlobCount;
    public int numBlobsEaten;
    public int recordBlobsEaten;
    public int numFruitEaten;
    public int recordFruitEaten;
    public int recordChildren;

    public AttributeStatistic jogModifier = new AttributeStatistic();
    public AttributeStatistic size = new AttributeStatistic();
    public AttributeStatistic runModifier = new AttributeStatistic();
    public AttributeStatistic randomRotation = new AttributeStatistic();
    public AttributeStatistic aggression = new AttributeStatistic();
    public AttributeStatistic fearOfPredator = new AttributeStatistic();
    public AttributeStatistic wantForPrey = new AttributeStatistic();
    public AttributeStatistic incubationTicks = new AttributeStatistic();
    public AttributeStatistic reproductionLimit = new AttributeStatistic();
    public AttributeStatistic speed = new AttributeStatistic();
    public AttributeStatistic armor = new AttributeStatistic();
    public AttributeStatistic melee = new AttributeStatistic();
    public AttributeStatistic carnivorous = new AttributeStatistic();
    public AttributeStatistic sexualMaturity = new AttributeStatistic();
    public AttributeStatistic useMemory = new AttributeStatistic();
    public AttributeStatistic rotationSpeed = new AttributeStatistic();


    internal int totalFemales {
        get;
        set;
    }

    public float percentFemale { get { return (float)totalFemales / numBlobs; } }
    public float averageChildrenPerFemale { get { return (float)totalChildren / totalFemales; } }

    private int totalChildren;

    // Use this for initialization
    void Start()
    {
        totalFemales = 1;
        numBlobs = 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateAverages(BlobBehavior blob, StatType statType)
    {
        UpdateAverage(size, blob.size, statType);
        UpdateAverage(jogModifier, blob.jogModifier, statType);
        UpdateAverage(runModifier, blob.runModifier, statType);
        UpdateAverage(randomRotation, blob.randomRotation, statType);
        UpdateAverage(aggression, blob.aggression, statType);
        UpdateAverage(fearOfPredator, blob.fearOfPredator, statType);
        UpdateAverage(wantForPrey, blob.wantForPrey, statType);
        UpdateAverage(incubationTicks, blob.incubationTicks, statType);
        UpdateAverage(reproductionLimit, blob.reproductionLimit, statType);
        UpdateAverage(rotationSpeed, blob.rotationSpeed, statType);
        UpdateAverage(useMemory, blob.useMemoryPercent, statType);
        UpdateAverage(sexualMaturity, blob.sexualMaturity, statType);
        UpdateAverage(carnivorous, blob.carnivorous, statType);
        UpdateAverage(melee, blob.melee, statType);
        UpdateAverage(armor, blob.armor, statType);
        UpdateAverage(speed, blob.speed, statType);

        if (blob.gender.Equals(GenderType.Female) && statType == StatType.Death) totalChildren -= blob.children; //averageChildrenPerFemale = CalculateChildrenAverage(averageChildrenPerFemale, blob.children, statType);

        numFruitEaten -= blob.fruitEaten;
        numBlobsEaten -= blob.blobsEaten;

        if (statType.Equals(StatType.Birth))
        {
            if (blob.generation > 0) totalChildren++;
            numBlobs++;
            if (blob.gender.Equals(GenderType.Female)) totalFemales++;
            if (recordBlobCount < numBlobs) recordBlobCount = numBlobs;
        }
        else {
            numBlobs--;
            if (blob.gender.Equals(GenderType.Female)) totalFemales--;
        }
    }

    private void UpdateAverage(AttributeStatistic currentStatistic, float addition, StatType stateType)
    {
        var sign = stateType.Equals(StatType.Birth) ? 1 : -1;

        currentStatistic.Average = (currentStatistic.Average * numBlobs + addition * sign) / (numBlobs + sign);
    }

    private float CalculateNewAverage(float currentAverage, float addition, StatType stateType)
    {
        var sign = stateType.Equals(StatType.Birth) ? 1 : -1;

        return (currentAverage * numBlobs + addition * sign) / (numBlobs + sign);
    }

    // this happens after the new blob is instantiated
    public void UpdateSurvivalStatistics(BlobBehavior blob)
    {
        if (recordChildren < blob.children) recordChildren = blob.children;
    }

    public void BothGenderRecords(BlobBehavior blob)
    {
        // First time reproducers get to contribute to records
        if (blob.children == 1)
        {
            incubationTicks.UpdateMinMax(blob.incubationTicks);
            jogModifier.UpdateMinMax(blob.jogModifier);
            reproductionLimit.UpdateMinMax(blob.reproductionLimit);
            runModifier.UpdateMinMax(blob.runModifier);
            size.UpdateMinMax(blob.size);
            aggression.UpdateMinMax(blob.aggression);
            armor.UpdateMinMax(blob.armor);
            melee.UpdateMinMax(blob.melee);
            carnivorous.UpdateMinMax(blob.carnivorous);
            randomRotation.UpdateMinMax(blob.randomRotation);
            speed.UpdateMinMax(blob.speed);
            sexualMaturity.UpdateMinMax(blob.sexualMaturity);
            useMemory.UpdateMinMax(blob.useMemoryPercent);
            wantForPrey.UpdateMinMax(blob.wantForPrey);
            fearOfPredator.UpdateMinMax(blob.fearOfPredator);
            rotationSpeed.UpdateMinMax(blob.rotationSpeed);
        }
    }
}

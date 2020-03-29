using Assets.Enums;
using UnityEngine;

public class Statistics : MonoBehaviour
{

    internal int numBlobs;
    public int recordBlobCount;
    public int numBlobsEaten;
    public int recordBlobsEaten;
    public int numFruitEaten;
    public int recordFruitEaten;
    public float averageSize;
    public float averageJogModifier;
    public float recordHighSize;
    public float recordLowSize;
    public float recordHighJogModifier;
    public float recordLowJogModifier;
    public float averageRunModifier;
    public float recordHighRunModifier;
    public float recordLowRunModifier;
    public float averageRandomRotation;
    public float averageAggression;
    public float averageFearOfPredator;
    public float averageWantOfPrey;
    public float averageIncubationTicks;
    public int recordHighIncubationTicks;
    public int recordLowIncubationTicks;
    public float averageReproductionLimit;
    public int recordHighReproductionLimit;
    public int recordLowReproductionLimit;
    public int recordChildren;
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

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateAverages(BlobBehavior blob, StatType statType)
    {
        averageSize = CalculateNewAverage(averageSize, blob.size, statType);

        averageJogModifier = CalculateNewAverage(averageJogModifier, blob.jogModifier, statType);

        averageRunModifier = CalculateNewAverage(averageRunModifier, blob.runModifier, statType);

        averageRandomRotation = CalculateNewAverage(averageRandomRotation, blob.randomRotation, statType);

        averageAggression = CalculateNewAverage(averageAggression, blob.aggression, statType);

        averageFearOfPredator = CalculateNewAverage(averageFearOfPredator, blob.fearOfPredator, statType);

        averageWantOfPrey = CalculateNewAverage(averageWantOfPrey, blob.wantForPrey, statType);

        averageIncubationTicks = CalculateNewAverage(averageIncubationTicks, blob.incubationTicks, statType);

        averageReproductionLimit = CalculateNewAverage(averageReproductionLimit, blob.reproductionLimit, statType);

        //percentFemale = CalculateNewAverage(percentFemale, (int)blob.gender, statType);

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

    private float CalculateNewAverage(float currentAverage, float addition, StatType stateType)
    {
        var sign = stateType.Equals(StatType.Birth) ? 1 : -1;

        return (currentAverage * numBlobs + addition * sign) / (numBlobs + sign);
    }

    // this happens after the new blob is instantiated
    public void UpdateSurvivalStatistics(BlobBehavior blob)
    {
        BothGenderRecords(blob);

        if (recordChildren < blob.children) recordChildren = blob.children;
    }

    public void BothGenderRecords(BlobBehavior blob)
    {
        // First time reproducers get to contribute to records
        if (blob.children == 1)
        {
            if (recordHighIncubationTicks < blob.incubationTicks) recordHighIncubationTicks = blob.incubationTicks;
            if (recordHighJogModifier < blob.jogModifier) recordHighJogModifier = blob.jogModifier;
            if (recordHighReproductionLimit < blob.reproductionLimit) recordHighReproductionLimit = blob.reproductionLimit;
            if (recordHighRunModifier < blob.runModifier) recordHighRunModifier = blob.runModifier;
            if (recordHighSize < blob.size) recordHighSize = blob.size;
            if (recordLowIncubationTicks > blob.incubationTicks || recordLowIncubationTicks == 0) recordLowIncubationTicks = blob.incubationTicks;
            if (recordLowJogModifier > blob.jogModifier || recordLowJogModifier == 0) recordLowJogModifier = blob.jogModifier;
            if (recordLowReproductionLimit > blob.reproductionLimit || recordLowReproductionLimit == 0) recordLowReproductionLimit = blob.reproductionLimit;
            if (recordLowRunModifier > blob.runModifier || recordLowRunModifier == 0) recordLowRunModifier = blob.runModifier;
            if (recordLowSize > blob.size) recordLowSize = blob.size;
        }
    }
}

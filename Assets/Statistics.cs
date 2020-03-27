using UnityEngine;

public class Statistics : MonoBehaviour
{

    public int numBlobs;
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
    public float averageChildren;
    public int recordChildren;
    public float percentFemale;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateCurrentStatisticsForBirth(BlobBehavior blob)
    {
        UpdateAveragesForBeingBorn(blob);

        numBlobs++;
        if (recordBlobCount < numBlobs) recordBlobCount = numBlobs;
    }

    public void UpdateAveragesForBeingBorn(BlobBehavior blob)
    {
        averageSize = (averageSize * numBlobs + blob.size) / (numBlobs + 1);
        averageJogModifier = (averageJogModifier * numBlobs + blob.jogModifier) / (numBlobs + 1);
        averageRunModifier = (averageRunModifier * numBlobs + blob.runModifier) / (numBlobs + 1);
        averageRandomRotation = (averageRandomRotation * numBlobs + blob.randomRotation) / (numBlobs + 1);
        averageAggression = (averageAggression * numBlobs + blob.aggression) / (numBlobs + 1);
        averageFearOfPredator = (averageFearOfPredator * numBlobs + blob.fearOfPredator) / (numBlobs + 1);
        averageWantOfPrey = (averageWantOfPrey * numBlobs + blob.wantForPrey) / (numBlobs + 1);
        averageIncubationTicks = (averageIncubationTicks * numBlobs + blob.incubationTicks) / (numBlobs + 1);
        averageReproductionLimit = (averageReproductionLimit * numBlobs + blob.reproductionLimit) / (numBlobs + 1);
        percentFemale = (percentFemale * numBlobs + (int)blob.gender) / (numBlobs + 1);
    }

    public void UpdateCurrentStatisticsForDeath(BlobBehavior blob)
    {
        UpdateAveragesForDeath(blob);

        numBlobs--;
    }

    public void UpdateAveragesForDeath(BlobBehavior blob)
    {
        averageSize = (averageSize * numBlobs - blob.size) / (numBlobs - 1);
        averageJogModifier = (averageJogModifier * numBlobs - blob.jogModifier) / (numBlobs - 1);
        averageRunModifier = (averageRunModifier * numBlobs - blob.runModifier) / (numBlobs - 1);
        averageRandomRotation = (averageRandomRotation * numBlobs - blob.randomRotation) / (numBlobs - 1);
        averageAggression = (averageAggression * numBlobs - blob.aggression) / (numBlobs - 1);
        averageFearOfPredator = (averageFearOfPredator * numBlobs - blob.fearOfPredator) / (numBlobs - 1);
        averageWantOfPrey = (averageWantOfPrey * numBlobs - blob.wantForPrey) / (numBlobs - 1);
        averageIncubationTicks = (averageIncubationTicks * numBlobs - blob.incubationTicks) / (numBlobs - 1);
        averageReproductionLimit = (averageReproductionLimit * numBlobs - blob.reproductionLimit) / (numBlobs - 1);
        percentFemale = (percentFemale * numBlobs - (int)blob.gender) / (numBlobs - 1);
        averageChildren = (averageChildren * numBlobs - blob.children) / (numBlobs - 1);
        numFruitEaten -= blob.fruitEaten;
        numBlobsEaten -= blob.blobsEaten;
    }

    // this happens after the new blob is instantiated
    public void UpdateSurvivalStatistics(BlobBehavior blob)
    {
        BothGenderSurvivalStats(blob);

        if (recordChildren < blob.children) recordChildren = blob.children;

        var previousTotalChildrenHad = averageChildren * (numBlobs - 1);
        averageChildren = (previousTotalChildrenHad + 1) / (numBlobs);
    }

    public void BothGenderSurvivalStats(BlobBehavior blob)
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

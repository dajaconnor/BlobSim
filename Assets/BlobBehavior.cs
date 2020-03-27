using Assets.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlobBehavior : MonoBehaviour
{
    public GameObject ground;
    public PerceptionBehavior perception;
    public GameObject blobPrefab;

    float geneticDrift = 0.1f;

    // Things that evolve
    internal float size = 1;
    internal float perceptionDepth = 3;
    internal float perceptionWidth = 2;
    internal float perceptionShift = 1 / 3;
    internal float speed = 4;
    internal float jogModifier = 2;
    internal float runModifier = 3;
    internal float randomRotation = 5;
    internal float fearOfPredator = 5;
    internal float wantForPrey = 5;
    internal int incubationTicks = 100;
    internal int reproductionLimit = 100000;
    internal float useMemoryPercent = 0.25f;
    internal int generation = 0;
    internal float childGenderRatio = 0.5f;
    public GenderType gender = GenderType.Male;

    // Dependent on size
    float rotationSpeed = 10;

    int incubatedEnergy = 0;
    bool die = false;
    internal float currentSpeed = 4;

    internal int energy = 100000;


    int energyPerFruit = 30000;


    GameObject target;
    BlobBehavior predator;
    BlobBehavior prey;
    BlobBehavior parent;
    public BlobBehavior partner;

    int currentIncubation = 0;
    float predationLimit = 2f;
    Vector3 randomTarget;
    private static float yConstant = 0.47f;

    // stats
    internal int fruitEaten = 0;
    internal int energyFromFruit = 0;
    internal int blobsEaten = 0;
    internal int energyFromBlobs = 0;
    internal int children = 0;
    internal long ticksLived = 0;
    private bool updatedForBirth = false;

    private int latestPlace = 0;
    Vector2[] places = new Vector2[6];

    void Start()
    {
        fruitEaten = 0;
        blobsEaten = 0;
        energyFromFruit = 0;
        energyFromBlobs = 0;
        children = 0;
        ticksLived = 0;


        if (!updatedForBirth)
        {
            updatedForBirth = true;
            UpdateCurrentStatisticsForBirth();
        }
    }

    // Update is called once per frame
    void Update()
    {
        ticksLived++;

        if (ShouldDie())
        {
            UpdateCurrentStatisticsForDeath();
            Destroy(this.gameObject);
            Destroy(this);
            return;
        }

        FixHeightBug();

        if (ShouldReproduce())
        {
            MakeNewBlob();
            return;
        }

        CleanupPredatorAndPrey();
        CheckForPredatorAndPrey();

        if (predator != null)
        {
            RunAway();
            return;
        }

        if (prey != null)
        {
            ChasePrey();
            return;
        }

        if (target == null)
        {
            target = perception.LatestFruit;

            if (target == null)
            {
                RandomWalk();
                return;
            }
            else
            {
                currentSpeed = speed * jogModifier;
            }
        }

        HeadToTarget(target.transform.position);
    }

    void OnMouseDown()
    {
        Camera.main.GetComponent<CameraBehavior>().target = this;
    }

    private bool ShouldReproduce()
    {
        return gender.Equals(GenderType.Female) && energy > reproductionLimit || currentIncubation > 0 && partner != null;
    }

    private void FixHeightBug()
    {
        transform.position = new Vector3(transform.position.x, yConstant, transform.position.z);
    }

    private bool ShouldDie()
    {
        return energy < 0 || die || (parent == null && ticksLived > 200000);
    }

    private void ChasePrey()
    {
        HeadToTarget(prey.transform.position);
    }

    private void RunAway()
    {
        if (!LeaveEdge())
        {
            var targetPosition = predator.transform.position;
            targetPosition.y = yConstant;
            var lookRotation = Quaternion.LookRotation(-targetPosition);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * Random.Range(0.1f, 1.2f));
        }

        MoveForward();
    }

    private void CheckForPredatorAndPrey()
    {
        energy -= (int)(perceptionWidth * perceptionDepth) / 2;

        if (perception.LatestBlob != null)
        {
            var newBlob = perception.LatestBlob;

            if (parent != null && (newBlob.Equals(parent) || newBlob.parent.Equals(this))) return;

            if (newBlob.size > size * predationLimit)
            {
                predator = perception.LatestBlob;
                currentSpeed = speed * runModifier;
                target = null;
            }
            else if (newBlob.size < size / predationLimit)
            {
                prey = perception.LatestBlob;
                currentSpeed = speed * runModifier;
                target = null;
            }
            else if (newBlob.gender != gender)
            {
                partner = perception.LatestBlob;
            }
        }
    }

    private void CleanupPredatorAndPrey()
    {
        if (prey != null && Vector3.Distance(prey.transform.position, transform.position) > wantForPrey)
        {
            prey = null;

            if (predator == null)
            {
                currentSpeed = speed;
            }
        }
        if (predator != null && Vector3.Distance(predator.transform.position, transform.position) > fearOfPredator)
        {
            predator = null;

            if (prey == null)
            {
                currentSpeed = speed;
            }
        }
    }

    private void MakeNewBlob()
    {
        if (partner == null)
        {
            currentIncubation = 0;
            incubatedEnergy = 0;
        }

        currentIncubation++;
        energy -= 1000 * (int)size;
        incubatedEnergy += (int)(500 * size);

        if (currentIncubation >= incubationTicks && partner != null)
        {
            GameObject gameObject = Instantiate(this.gameObject, transform.position, transform.rotation);

            float newSize = SetNewBlobSize(gameObject);
            float speedModifier = newSize * newSize;

            BlobBehavior newBlob = gameObject.GetComponent<BlobBehavior>();

            var vectorScale = perception.gameObject.transform.localScale;

            SetPerceptionFields(newBlob);
            newBlob.randomRotation = MeOrMate().randomRotation * GetDrift();

            newBlob.transform.position = new Vector3(transform.position.x, yConstant, transform.position.z);

            newBlob.speed = MeOrMate().speed * GetDrift();
            newBlob.jogModifier = MeOrMate().jogModifier * GetDrift();
            newBlob.runModifier = MeOrMate().runModifier * GetDrift();
            newBlob.fearOfPredator = MeOrMate().fearOfPredator * GetDrift();
            newBlob.wantForPrey = MeOrMate().wantForPrey * GetDrift();
            newBlob.incubationTicks = (int)(incubationTicks * GetDrift());
            newBlob.rotationSpeed /= speedModifier;
            newBlob.ground = ground;
            newBlob.blobPrefab = blobPrefab;
            newBlob.size = newSize;
            newBlob.energy = incubatedEnergy;
            newBlob.reproductionLimit = (int)(reproductionLimit * GetDrift());
            newBlob.places = (Vector2[])places.Clone();
            newBlob.latestPlace = latestPlace;
            newBlob.generation = generation + 1;
            newBlob.useMemoryPercent = MeOrMate().useMemoryPercent * GetDrift();
            //newBlob.geneticDrift = MeOrMate().geneticDrift * GetDrift();
            
            newBlob.gender = RandomGender();
            newBlob.name = newBlob.gender.ToString() + "Blob";

            newBlob.Start();

            energy /= 3;

            currentIncubation = 0;
            children++;
            partner.children++;

            var stats = ground.GetComponent<Statistics>();
            BothGenderSurvivalStats(stats, partner);

            UpdateSurvivalStatistics();
        }
    }

    private GenderType RandomGender()
    {
        if (Random.value < childGenderRatio) return GenderType.Female;
        else return GenderType.Male;
    }

    private BlobBehavior MeOrMate()
    {
        if (Random.value < 0.5f) return this;
        else return partner;
    }

    private void UpdateCurrentStatisticsForBirth()
    {
        var stats = ground.GetComponent<Statistics>();

        UpdateAveragesForBeingBorn(stats);

        stats.numBlobs++;
        if (stats.recordBlobCount < stats.numBlobs) stats.recordBlobCount = stats.numBlobs;
    }

    private void UpdateAveragesForBeingBorn(Statistics stats)
    {
        stats.averageSize = (stats.averageSize * stats.numBlobs + size) / (stats.numBlobs + 1);
        stats.averageJogModifier = (stats.averageJogModifier * stats.numBlobs + jogModifier) / (stats.numBlobs + 1);
        stats.averageRunModifier = (stats.averageRunModifier * stats.numBlobs + runModifier) / (stats.numBlobs + 1);
        stats.averageRandomRotation = (stats.averageRandomRotation * stats.numBlobs + randomRotation) / (stats.numBlobs + 1);
        stats.averageFearOfPredator = (stats.averageFearOfPredator * stats.numBlobs + fearOfPredator) / (stats.numBlobs + 1);
        stats.averageWantOfPrey = (stats.averageWantOfPrey * stats.numBlobs + wantForPrey) / (stats.numBlobs + 1);
        stats.averageIncubationTicks = (stats.averageIncubationTicks * stats.numBlobs + incubationTicks) / (stats.numBlobs + 1);
        stats.averageReproductionLimit = (stats.averageReproductionLimit * stats.numBlobs + reproductionLimit) / (stats.numBlobs + 1);
        stats.percentFemale = (stats.percentFemale * stats.numBlobs + (int)gender) / (stats.numBlobs + 1);
    }

    private void UpdateCurrentStatisticsForDeath()
    {
        var stats = ground.GetComponent<Statistics>();

        UpdateAveragesForDeath(stats);

        stats.numBlobs--;
    }

    private void UpdateAveragesForDeath(Statistics stats)
    {
        stats.averageSize = (stats.averageSize * stats.numBlobs - size) / (stats.numBlobs - 1);
        stats.averageJogModifier = (stats.averageJogModifier * stats.numBlobs - jogModifier) / (stats.numBlobs - 1);
        stats.averageRunModifier = (stats.averageRunModifier * stats.numBlobs - runModifier) / (stats.numBlobs - 1);
        stats.averageRandomRotation = (stats.averageRandomRotation * stats.numBlobs - randomRotation) / (stats.numBlobs - 1);
        stats.averageFearOfPredator = (stats.averageFearOfPredator * stats.numBlobs - fearOfPredator) / (stats.numBlobs - 1);
        stats.averageWantOfPrey = (stats.averageWantOfPrey * stats.numBlobs - wantForPrey) / (stats.numBlobs - 1);
        stats.averageIncubationTicks = (stats.averageIncubationTicks * stats.numBlobs - incubationTicks) / (stats.numBlobs - 1);
        stats.averageReproductionLimit = (stats.averageReproductionLimit * stats.numBlobs - reproductionLimit) / (stats.numBlobs - 1);
        stats.percentFemale = (stats.percentFemale * stats.numBlobs - (int)gender) / (stats.numBlobs - 1);
        stats.averageChildren = (stats.averageChildren * stats.numBlobs - children) / (stats.numBlobs - 1);
        stats.numFruitEaten -= fruitEaten;
        stats.numBlobsEaten -= blobsEaten;
    }

    // this happens after the new blob is instantiated
    private void UpdateSurvivalStatistics()
    {
        var stats = ground.GetComponent<Statistics>();

        BothGenderSurvivalStats(stats, this);

        if (stats.recordChildren < children) stats.recordChildren = children;

        var previousTotalChildrenHad = stats.averageChildren * (stats.numBlobs - 1);
        stats.averageChildren = (previousTotalChildrenHad + 1) / (stats.numBlobs);
    }

    private static void BothGenderSurvivalStats(Statistics stats, BlobBehavior blob)
    {
        // First time reproducers get to contribute to records
        if (blob.children == 1)
        {
            if (stats.recordHighIncubationTicks < blob.incubationTicks) stats.recordHighIncubationTicks = blob.incubationTicks;
            if (stats.recordHighJogModifier < blob.jogModifier) stats.recordHighJogModifier = blob.jogModifier;
            if (stats.recordHighReproductionLimit < blob.reproductionLimit) stats.recordHighReproductionLimit = blob.reproductionLimit;
            if (stats.recordHighRunModifier < blob.runModifier) stats.recordHighRunModifier = blob.runModifier;
            if (stats.recordHighSize < blob.size) stats.recordHighSize = blob.size;
            if (stats.recordLowIncubationTicks > blob.incubationTicks || stats.recordLowIncubationTicks == 0) stats.recordLowIncubationTicks = blob.incubationTicks;
            if (stats.recordLowJogModifier > blob.jogModifier || stats.recordLowJogModifier == 0) stats.recordLowJogModifier = blob.jogModifier;
            if (stats.recordLowReproductionLimit > blob.reproductionLimit || stats.recordLowReproductionLimit == 0) stats.recordLowReproductionLimit = blob.reproductionLimit;
            if (stats.recordLowRunModifier > blob.runModifier || stats.recordLowRunModifier == 0) stats.recordLowRunModifier = blob.runModifier;
            if (stats.recordLowSize > blob.size) stats.recordLowSize = blob.size;
        }
    }

    private void SetPerceptionFields(BlobBehavior newBlob)
    {
        var perceptionTransform = newBlob.perception.gameObject.transform;

        newBlob.perceptionDepth = MeOrMate().perceptionDepth * GetDrift();
        newBlob.perceptionWidth = MeOrMate().perceptionWidth * GetDrift();

        // to keep from stagnating at 0
        newBlob.perceptionShift = MeOrMate().perceptionShift + GetDrift() + 0.01f;

        perceptionTransform.localScale = new Vector3(newBlob.perceptionWidth, newBlob.perceptionWidth, newBlob.perceptionDepth);

        perceptionTransform.localPosition = new Vector3(0, 0, perceptionTransform.localScale.z * perceptionShift);
    }

    private float SetNewBlobSize(GameObject gameObject)
    {
        var sizeModifier = GetDrift();
        gameObject.transform.localScale = gameObject.transform.localScale * sizeModifier;
        return size * sizeModifier;
    }

    private float GetDrift()
    {
        return Random.Range(1 - geneticDrift, 1 + geneticDrift);
    }

    private void HeadToTarget(Vector3 targetLocation)
    {
        var targetPosition = targetLocation;
        targetPosition.y = yConstant;
        var direction = (targetPosition - transform.position).normalized;
        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        MoveForward();
    }

    private void RandomWalk()
    {
        if (!LeaveEdge())
        {
            if (GoToRememberedPlace()) return;

            if (Random.Range(0, 1) < 0.1)
            {

                Vector2 random2DPoint = Random.insideUnitCircle;
                Vector3 direction = new Vector3(random2DPoint.x, 0, random2DPoint.y).normalized;

                var lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed / randomRotation);

            }
        }

        MoveForward();
    }

    private bool GoToRememberedPlace()
    {
        if (Random.Range(0f, 1f) < useMemoryPercent)
        {
            var place = GetRandomPlace();

            if (place == default(Vector2)) return false;

            HeadToTarget(new Vector3(place.x, 0, place.y));
            return true;
        }

        return false;
    }

    private Vector2 GetRandomPlace()
    {
        var i = Random.Range(0, places.Length);
        return places[i];
    }

    private bool LeaveEdge()
    {
        if (new Vector2(transform.position.x, transform.position.z).magnitude > ground.gameObject.transform.localScale.x * 0.48)
        {
            Vector3 direction = new Vector3(0, 0, 0).normalized;

            transform.LookAt(direction);

            if (predator != null || prey != null)
            {
                if (Random.value > 0.5f) transform.Rotate(0f, 85f, 0f);
                else transform.Rotate(0f, -85f, 0f);
            }

            return true;
        }

        return false;
    }

    private void MoveForward()
    {
        var moveAmount = new Vector3(0, 0, 1).normalized * currentSpeed * Time.deltaTime;

        energy -= (int)(size * size * size * currentSpeed * currentSpeed);

        transform.Translate(moveAmount, Space.Self);
    }

    void FixedUpdate()
    {
        //myRigidBody.position += velocity * Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider triggerCollider)
    {
        var stats = ground.GetComponent<Statistics>();

        if (triggerCollider.gameObject.name.StartsWith("Fruit"))
        {
            Destroy(triggerCollider.gameObject);
            target = null;
            energy += energyPerFruit;
            energyFromFruit += energyPerFruit;
            currentSpeed = speed;
            fruitEaten++;
            stats.numFruitEaten++;

            if (stats.recordFruitEaten < fruitEaten) stats.recordFruitEaten = fruitEaten;

            RememberThisPlace();
        }

        if (triggerCollider.gameObject.name.StartsWith("Percepticon"))
        {
            var targetBlob = triggerCollider.gameObject.GetComponentInParent<BlobBehavior>();

            if (targetBlob == null || targetBlob.size < size / predationLimit)
            {
                var deltaEnergy = (int)(energyPerFruit * targetBlob.size);

                target = null;
                energy += deltaEnergy;
                energyFromBlobs += deltaEnergy;
                currentSpeed = speed;
                targetBlob.die = true;
                blobsEaten++;
                stats.numBlobsEaten++;

                if (stats.recordBlobsEaten < blobsEaten) stats.recordBlobsEaten = blobsEaten;

                RememberThisPlace();
            }
        }
    }

    private void RememberThisPlace()
    {
        latestPlace++;

        places[WrapLatestPlace()] = new Vector2(transform.position.x, transform.position.z);
    }

    private int WrapLatestPlace()
    {
        if (latestPlace >= places.Length) latestPlace = 0;

        return latestPlace;
    }
}

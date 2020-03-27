using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlobBehavior : MonoBehaviour
{
    public GameObject ground;
    public PerceptionBehavior perception;
    public GameObject blobPrefab;
    public Material material;

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
    internal float aggression = 10f;
    public GenderType gender = GenderType.Male;

    // Dependent on size
    internal float rotationSpeed = 10;

    internal int incubatedEnergy = 0;
    bool die = false;
    internal float currentSpeed = 4;

    internal int energy = 100000;


    int energyPerFruit = 30000;


    GameObject target;
    BlobBehavior predator;
    BlobBehavior prey;
    BlobBehavior parent;
    BlobBehavior rival;
    public BlobBehavior partner;

    internal int currentIncubation = 0;
    float predationLimit = 2f;
    Vector3 randomTarget;


    // stats
    internal int fruitEaten = 0;
    internal int energyFromFruit = 0;
    internal int blobsEaten = 0;
    internal int energyFromBlobs = 0;
    internal int children = 0;
    internal long ticksLived = 0;
    private bool updatedForBirth = false;

    internal int latestPlace = 0;
    internal Vector2[] places = new Vector2[6];
    internal Statistics stats;

    internal void Start()
    {
        fruitEaten = 0;
        blobsEaten = 0;
        energyFromFruit = 0;
        energyFromBlobs = 0;
        children = 0;
        ticksLived = 0;
        stats = ground.GetComponent<Statistics>();

        if (generation < 2) partner = this;

        if (!updatedForBirth)
        {
            updatedForBirth = true;
            stats.UpdateCurrentStatisticsForBirth(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ticksLived++;

        if (ShouldDie())
        {
            stats.UpdateCurrentStatisticsForDeath(this);
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

        if (rival != null)
        {
            FightRival();
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

    private void FightRival()
    {
        var targetPosition = rival.transform.position;
        //targetPosition.y = ReproductionUtil.yConstant;
        var direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * aggression);

        MoveForward();
    }

    void OnMouseDown()
    {
        Camera.main.GetComponent<CameraBehavior>().target = this;
    }

    private bool ShouldReproduce()
    {
        return gender.Equals(GenderType.Female) && (energy > reproductionLimit || currentIncubation > 0) && partner != null;
    }

    private void FixHeightBug()
    {
        transform.position = new Vector3(transform.position.x, ReproductionUtil.yConstant, transform.position.z);
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
            targetPosition.y = -transform.position.y;
            var lookRotation = Quaternion.LookRotation(-targetPosition);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * Random.Range(0.1f, 1.2f));
        }

        MoveForward();
    }

    private void CheckForPredatorAndPrey()
    {
        energy -= (int)(perceptionWidth * perceptionDepth);

        if (perception.LatestBlob != null)
        {
            var newBlob = perception.LatestBlob;

            if (parent != null && (newBlob.Equals(parent) || newBlob.parent.Equals(this))) return;

            if (newBlob.size > size * predationLimit)
            {
                predator = newBlob;
                currentSpeed = speed * runModifier;
                target = null;
            }
            else if (newBlob.size < size / predationLimit)
            {
                prey = newBlob;
                currentSpeed = speed * runModifier;
                target = null;
            }
            else if (IsGoodPartner(newBlob))
            {
                if (partner != null) partner.partner = null;
                partner = newBlob;
                newBlob.partner = this;
            }
            else if (IsRival(newBlob))
            {
                rival = newBlob;
                newBlob.rival = this;
                currentSpeed = speed * runModifier;
            }
        }
    }

    private bool IsRival(BlobBehavior newBlob)
    {
        // must both be male, and need to steal their partner
        return (gender == GenderType.Male && newBlob.gender == GenderType.Male && partner == null & newBlob.partner != null && rival == null && newBlob.rival == null);
    }

    private bool IsGoodPartner(BlobBehavior blob)
    {
        // girls pick boys
        if (gender == GenderType.Male || blob.gender == GenderType.Female) return false;
        if (partner == null || partner == this) return true;

        if (partner.energy == 0) return true;
        if (blob.energy == 0) return false;

        return partner.energy / partner.ticksLived < blob.energy / blob.ticksLived;
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
            return;
        }

        currentIncubation++;
        energy -= 1000 * (int)size;
        incubatedEnergy += (int)(500 * size);

        if (currentIncubation >= incubationTicks && partner != null)
        {
            GameObject newGameObject = Instantiate(gameObject, transform.position, transform.rotation);
            ReproductionUtil.ReproduceBlob(this, newGameObject);
        }
    }

    private void HeadToTarget(Vector3 targetLocation)
    {
        var targetPosition = targetLocation;
        targetPosition.y = ReproductionUtil.yConstant;
        var direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
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
            LookAt(new Vector2(0, 0));

            if (predator != null || prey != null)
            {
                if (Random.value > 0.5f) transform.Rotate(0f, 85f, 0f);
                else transform.Rotate(0f, -85f, 0f);
            }

            return true;
        }

        return false;
    }

    private void LookAt(Vector2 targetCoords)
    {
        var targetPosition = new Vector3(targetCoords.x, transform.position.y, targetCoords.y);
        transform.LookAt(targetPosition);

        //var targetPosition = new Vector3(targetCoords.x, 0, targetCoords.y);
        //var lookPos = targetPosition - transform.position;
        //lookPos.y = 0;
        //var rotation = Quaternion.LookRotation(lookPos);
        //transform.rotation = rotation;
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
                return;
            }

            if (targetBlob.Equals(rival))
            {
                int hurtRivalAmount = (int)(size * size * size * currentSpeed * currentSpeed * aggression * 1000);
                int hurtSelfAmount = hurtRivalAmount / 2;
                int hurtFromRival = (int)(rival.size * rival.size * rival.size * rival.currentSpeed * rival.currentSpeed * rival.aggression * 1000);
                int hurtRivalSelf = hurtFromRival / 2;

                energy -= (hurtSelfAmount + hurtFromRival);
                rival.energy -= (hurtRivalAmount + hurtRivalSelf);

                LookAt(new Vector2(-rival.transform.position.x, -rival.transform.position.z));

                rival.LookAt(new Vector2(-transform.position.x, -transform.position.z));

                if (hurtRivalAmount > hurtFromRival && rival.partner != null && partner == null)
                {
                    rival.partner.partner = this;
                    partner = rival.partner;
                    rival.partner = null;
                }

                else if (hurtRivalAmount < hurtFromRival && rival.partner == null && partner != null)
                {
                    partner.partner = rival;
                    rival.partner = partner;
                    partner = null;
                }

                // make them flee each other after the encounter
                predator = rival;
                rival.predator = this;
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

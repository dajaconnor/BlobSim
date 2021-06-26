using Assets;
using Assets.Enums;
using Assets.Utils;
using BlobSimulation.Utils;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlobBehavior : MonoBehaviour
{
    private static readonly bool REALLY_PICKY_MATE_SELECTION = true;

    public MapGenerator ground;
    public PerceptionBehavior perception;
    public GameObject blobPrefab;
    public GameObject fruitPrefab;
    public GameObject treePrefab;

    // Things that evolve
    internal float size = 1;
    internal float perceptionDepth = 3;
    internal float perceptionWidth = 2;
    internal float perceptionShift = 1 / 3;
    internal float speed = 4;
    internal float jogModifier = 2;
    internal float runModifier = 3;
    internal float jogRotationModifier = 1.25f;
    internal float runRotationModifier = 1.5f;
    internal float randomRotation = 5;
    internal float fearOfPredator = 5;
    internal float wantForPrey = 5;
    internal int incubationTicks = 100;
    internal int reproductionLimit = 100000;
    internal float useMemoryPercent = 0.10f;
    internal int generation = 0;
    internal float childGenderRatio = 0.5f;
    internal float aggression = 10f;
    internal int sexualMaturity = 1000;
    internal int reserveEnergy = 100000;
    internal float carnivorous = 0.5f;
    internal TreeGenes seedInPoop;


    internal BlobStatusType status = BlobStatusType.Wandering;
    public GenderType gender = GenderType.Male;

    // Dependent on size
    internal float rotationSpeed = 10;
    internal float currentRotationSpeed = 5;

    internal int incubatedEnergy = 0;
    bool die = false;
    internal float currentSpeed = 4;

    internal int energy = 100000;


    int energyPerFruit = 40000;
    int energyPerTreeFruit = 10000;


    GameObject food;
    BlobBehavior predator;
    BlobBehavior prey;
    internal BlobBehavior parent;
    BlobBehavior rival;
    public BlobBehavior partner;

    internal int currentIncubation = 0;
    float predationLimit = 1.3f;
    float mateLimit = 1.1f;


    // stats
    internal int fruitEaten = 0;
    internal int energyFromFruit = 0;
    internal int blobsEaten = 0;
    internal int energyFromBlobs = 0;
    internal int children = 0;
    internal long ticksLived = 0;

    internal int latestPlace = 0;
    internal Vector2[] places = new Vector2[6];
    internal Statistics stats;

    private Color normalColor = new Color(0.125f, 0.5f, 1);
    private Color angryColor = new Color(1, 0, 0);
    private Color incubationColor = new Color(0, 0, 1);
    private Color scaredColor = new Color(1, 1, 0);
    private Color hungryColor = new Color(0.09375f, 0.3984375f, 0);
    private Color femaleColor = new Color(1, 0.08f, 1);

    internal static float heightAdjust = 0.47f;

    internal void Start()
    {
        stats = ground.GetComponent<Statistics>();
        ground = FindObjectOfType<MapGenerator>();

        if (generation == 0)
        {
            fruitEaten = 0;
            blobsEaten = 0;
            energyFromFruit = 0;
            energyFromBlobs = 0;
            children = 0;
            ticksLived = 0;
            perceptionShift = 0.5f;
            energy = 300000000;
            gender = GenderType.Female;
            childGenderRatio = 0.5f;
            rotationSpeed = 10;
            carnivorous = 0.5f;
        }

        if (generation < 2) partner = this;

        
    }

    // Update is called once per frame
    void Update()
    {
        ticksLived++;

        if (ShouldDie())
        {
            Die();
            return;
        }

        UpdateTargetReferences();
        SetStatus();
        Act();

        FixHeightBug();
    }

    private void Act()
    {
        if (ShouldReproduce())
        {
            MakeNewBlob();
        }

        switch (status)
        {
            case BlobStatusType.Providing:

                SlowToTarget(partner.transform.position);

                HeadToTarget(partner.transform.position);
                break;
            case BlobStatusType.Fleeing:
                RunAway();
                break;
            case BlobStatusType.Fighting:
                FightRival();
                break;
            case BlobStatusType.Chasing:
                HeadToTarget(prey.transform.position);
                break;
            case BlobStatusType.Foraging:

                SlowToTarget(food.transform.position);

                HeadToTarget(food.transform.position);

                break;
            case BlobStatusType.Wandering:
            default:
                RandomPoop();
                RandomWalk();
                break;
        }
        
        MoveForward();
    }

    private void RandomPoop()
    {
        if (seedInPoop == null) return;

        if (Random.value < seedInPoop.fiber)
        {
            if (LocationUtil.IsInShade(transform.position))
            {
                return;
            }

            GameObject newGameObject = Instantiate(treePrefab, transform.position, transform.rotation);
            ReproductionUtil.GerminateTree(seedInPoop, transform.position, newGameObject, fruitPrefab, ground);
            seedInPoop = null;
        }
    }

    private float SlowToTarget(Vector3 targetPosition)
    {
        var distanceToTarget = Vector3.Distance(targetPosition, transform.position);

        if (distanceToTarget < 3 * ground.scale)
        {
            currentSpeed *= distanceToTarget / (3 * ground.scale);
        }

        return distanceToTarget;
    }

    private void Die()
    {
        stats.UpdateAverages(this, StatType.Death);
        if (!die)
        {
            FruitSpawner.MakeFruit(transform.position, fruitPrefab, ground);
        }
        Destroy(this.gameObject);
        Destroy(this);
    }

    private void SetStatus()
    {

        if (ShouldProvide())
        {
            SetSpeedAndColor(BlobSpeedType.Running, incubationColor, BlobStatusType.Providing);
            return;
        }
        
        if (predator != null)
        {
            if (Vector3.Distance(predator.transform.position, transform.position) > fearOfPredator) predator = null;

            else
            {
                SetSpeedAndColor(BlobSpeedType.Running, scaredColor, BlobStatusType.Fleeing);
                return;
            }
        }
        if (rival != null)
        {
            // collision resets rival
            SetSpeedAndColor(BlobSpeedType.Running, angryColor, BlobStatusType.Fighting);
            return;
        }
        if (prey != null)
        {
            if (Vector3.Distance(prey.transform.position, transform.position) > wantForPrey) prey = null;

            else
            {
                SetSpeedAndColor(BlobSpeedType.Running, hungryColor, BlobStatusType.Chasing);
                return;
            }
        }
        if (food != null)
        {
            SetSpeedAndColor(BlobSpeedType.Jogging, hungryColor, BlobStatusType.Foraging);
            return;
        }

        SetSpeedAndColor(BlobSpeedType.Walking, normalColor, BlobStatusType.Wandering);
    }

    private bool ShouldProvide()
    {
        return gender.Equals(GenderType.Male) 
            && energy > reserveEnergy * 2 
            && partner != null 
            && partner.partner != null
            && partner != this
            && partner.partner.Equals(this) && 
            !partner.ShouldReproduce();
    }

    private void FightRival()
    {
        if (Vector3.Dot(rival.transform.position - transform.position, transform.forward) < 0.1 || Random.value < 0.1) HeadToTarget(-rival.transform.position);
        HeadToTarget(rival.transform.position);
    }

    void OnMouseDown()
    {
        Camera.main.GetComponent<CameraBehavior>().target = this;
    }

    private bool ShouldReproduce()
    {
        return (ticksLived > sexualMaturity && (energy > reproductionLimit || currentIncubation > 0) && partner != null) || generation == 0;
    }

    private void FixHeightBug()
    {
        transform.position = new Vector3(transform.position.x, LocationUtil.GetHeight(transform.position, ground) + heightAdjust, transform.position.z);
    }

    private bool ShouldDie()
    {
        return energy < 0 || die;
    }

    private void RunAway()
    {
        var targetPosition = predator.transform.position;
        targetPosition += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        transform.rotation = LookAwayFrom(targetPosition); //Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private Quaternion LookAwayFrom(Vector3 objectToShun)
    {

        return Quaternion.LookRotation(transform.position - objectToShun);
    }

    private void UpdateTargetReferences()
    {
        energy -= (int)(perceptionWidth * perceptionDepth);

        if (food == null) food = perception.LatestFruit;

        if (perception.LatestBlob != null)
        {
            var newBlob = perception.LatestBlob;
            perception.LatestBlob = null;

            if (parent != null && (newBlob.Equals(parent) || (newBlob.parent != null && newBlob.parent.Equals(this)))) return;

            if (newBlob.size > size * predationLimit)
            {
                predator = newBlob;
                food = null;
            }
            else if (newBlob.size < size / predationLimit)
            {
                prey = newBlob;
                food = null;
            }
            else if (IsGoodPartner(newBlob, false))
            {
                if (partner != null) partner.partner = null;
                partner = newBlob;
                newBlob.partner = this;
            }
            else if (IsRival(newBlob))
            {
                rival = newBlob;
                newBlob.rival = this;
                LookAt(rival.transform.position);
                rival.LookAt(transform.position);
            }
        }
    }

    private bool IsRival(BlobBehavior newBlob)
    {
        // must both be male, and need to steal their partner
        return (gender == GenderType.Male && newBlob.gender == GenderType.Male && partner == null & newBlob.partner != null && rival == null && newBlob.rival == null && IsGoodPartner(newBlob.partner, true));
    }

    private bool IsGoodPartner(BlobBehavior potentialPartner, bool checkingRivalry)
    {
        // no sense checking stuff if they're alraedy your partner
        // ... and don't pick yourself.  that's ridiculous.
        if (potentialPartner == partner || potentialPartner == this) return false;

        // girls pick boys unless it's a rivalry
        if ((gender == GenderType.Male || potentialPartner.gender == GenderType.Female) && !checkingRivalry) return false;

        // check size
        if (potentialPartner.size / size > mateLimit || size / potentialPartner.size > mateLimit) return false;

        // both parters should be sexually mature
        if (potentialPartner.ticksLived < potentialPartner.sexualMaturity || ticksLived < sexualMaturity) return false;

        if (REALLY_PICKY_MATE_SELECTION && !IsPickyMateMatch(potentialPartner)) {
            return false;
        }

        // they're a good match, so assuming we don't already have a partner
        // or this is gen 0 and your partner is yourself (you wierdo...)
        if (partner == null || partner == this) return true;

        return partner.energy / partner.ticksLived < potentialPartner.energy / potentialPartner.ticksLived;
    }

    private bool IsPickyMateMatch(BlobBehavior potentialPartner)
    {
        if (potentialPartner.speed / speed > mateLimit || speed / potentialPartner.speed > mateLimit) return false;
        if (potentialPartner.jogModifier / jogModifier > mateLimit || jogModifier / potentialPartner.jogModifier > mateLimit) return false;
        if (potentialPartner.runModifier / runModifier > mateLimit || runModifier / potentialPartner.runModifier > mateLimit) return false;
        if (potentialPartner.sexualMaturity / sexualMaturity > mateLimit || sexualMaturity / potentialPartner.sexualMaturity > mateLimit) return false;
        if (potentialPartner.wantForPrey / wantForPrey > mateLimit || wantForPrey / potentialPartner.wantForPrey > mateLimit) return false;
        if (potentialPartner.aggression / aggression > mateLimit || aggression / potentialPartner.aggression > mateLimit) return false;
        if (potentialPartner.carnivorous / carnivorous > mateLimit || carnivorous / potentialPartner.carnivorous > mateLimit) return false;
        if (potentialPartner.fearOfPredator / fearOfPredator > mateLimit || fearOfPredator / potentialPartner.fearOfPredator > mateLimit) return false;
        return true;
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
        incubatedEnergy += (int)(1000 * size);

        if (currentIncubation >= incubationTicks && partner != null)
        {
            GameObject newGameObject = Instantiate(gameObject, transform.position, transform.rotation);
            ReproductionUtil.ReproduceBlob(this, newGameObject, ground);
        }
    }

    private void HeadToTarget(Vector3 targetLocation)
    {
        var targetPosition = targetLocation;
        targetPosition.y = 0;
        var direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.Equals(Vector3.zero)) return;

        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * currentRotationSpeed);
        energy -= (int)(size * size * size * currentRotationSpeed * currentRotationSpeed);
    }

    private void RandomWalk()
    {
        if (GoToRememberedPlace()) return;

        if (Random.Range(0f, 1f) < 0.1)
        {
            Vector2 random2DPoint = Random.insideUnitCircle;
            Vector3 direction = new Vector3(random2DPoint.x, 0, random2DPoint.y).normalized;

            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * currentRotationSpeed / randomRotation);

        }
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

    private void LookAt(Vector2 targetCoords)
    {
        var targetPosition = new Vector3(targetCoords.x, 0, targetCoords.y);
        transform.LookAt(targetPosition);
    }

    private void MoveForward()
    {
        var scaleAdjustedSpeed = currentSpeed * ground.scale;

        var moveAmount = new Vector3(0, 0, 1).normalized * scaleAdjustedSpeed * Time.deltaTime;

        energy -= (int)(size * size * size * currentSpeed * currentSpeed);

        transform.Translate(moveAmount, Space.Self);
    }

    void FixedUpdate()
    {
        //myRigidBody.position += velocity * Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider triggerCollider)
    {
        if (triggerCollider.gameObject.name.StartsWith("Fruit"))
        {
            var fruit = triggerCollider.gameObject.GetComponent<FruitBehavior>();
            
            if (fruit.genes == null)
            {
                energy += energyPerFruit;
                energyFromFruit += energyPerFruit;
            } else
            {
                seedInPoop = fruit.genes;

                var energyGain = (int) (energyPerTreeFruit * (1 - carnivorous) * 2);

                energy += energyGain;
                energyFromFruit += energyGain;
            }

            Destroy(fruit.gameObject);
            Destroy(fruit);
            food = null;
            
            fruitEaten++;
            stats.numFruitEaten++;

            if (stats.recordFruitEaten < fruitEaten) stats.recordFruitEaten = fruitEaten;

            RememberThisPlace();
        }

        else if (triggerCollider.gameObject.name.StartsWith("Percepticon"))
        {
            var targetBlob = triggerCollider.gameObject.GetComponentInParent<BlobBehavior>();

            if (targetBlob == null || targetBlob.size < size / predationLimit)
            {
                var deltaEnergy = (int)(energyPerFruit * targetBlob.size * carnivorous * 2);

                food = null;
                energy += deltaEnergy;
                energyFromBlobs += deltaEnergy;
                targetBlob.die = true;
                blobsEaten++;
                stats.numBlobsEaten++;

                if (stats.recordBlobsEaten < blobsEaten) stats.recordBlobsEaten = blobsEaten;

                RememberThisPlace();
                return;
            }

            if (targetBlob.Equals(rival))
            {
                int hurtRivalAmount = (int)(size * size * size * currentSpeed * currentSpeed * aggression * 100);
                int hurtSelfAmount = hurtRivalAmount / 2;
                int hurtFromRival = (int)(rival.size * rival.size * rival.size * rival.currentSpeed * rival.currentSpeed * rival.aggression * 100);
                int hurtRivalSelf = hurtFromRival / 2;
                int totalSelfHurt = hurtSelfAmount + hurtFromRival;
                int totalRivalHurt = hurtRivalAmount + hurtRivalSelf;

                if (totalSelfHurt > energy && totalRivalHurt > rival.energy)
                {
                    if (energy > rival.energy)
                    {
                        energy -= rival.energy;
                        rival.energy = 0;
                    }
                    else
                    {
                        rival.energy -= energy;
                        energy = 0;
                    }
                }
                else
                {
                    energy -= totalSelfHurt;
                    rival.energy -= totalRivalHurt;
                }
                

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

                if (rival.energy <= 0)
                {
                    rival = null;
                }
                else if (energy > 0)
                {
                    // make them flee each other after the encounter
                    predator = rival;
                    transform.Rotate(0f, 180f, 0f);
                    rival.predator = this;

                    rival.rival = null;
                    rival = null;

                }
            }

            if (targetBlob.Equals(partner) && status.Equals(BlobStatusType.Providing) && energy > reserveEnergy)
            {
                var energyToProvide = Math.Abs(reserveEnergy - energy);
                energy -= energyToProvide;
                partner.energy += energyToProvide;
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

    private void SetSpeedAndColor(BlobSpeedType speedType, Color newColor, BlobStatusType newStatus)
    {
        currentSpeed = GetCurrentSpeed(speedType);
        currentRotationSpeed = GetCurrentRotation(speedType);
        status = newStatus;
        var colorType = Camera.main.GetComponent<CameraBehavior>().colorToggle;

        if (colorType.Equals(ColorDisplayType.Action)) GetComponent<Renderer>().material.color = newColor;
        else if (colorType.Equals(ColorDisplayType.None)) GetComponent<Renderer>().material.color = normalColor;
        else if (colorType.Equals(ColorDisplayType.Gender))
        {
            if (gender.Equals(GenderType.Male)) GetComponent<Renderer>().material.color = normalColor;
            else GetComponent<Renderer>().material.color = femaleColor;

        }
    }

    private float GetCurrentSpeed(BlobSpeedType speedType)
    {
        switch (speedType)
        {
            case BlobSpeedType.Walking: return speed;
            case BlobSpeedType.Jogging: return speed * jogModifier;
            case BlobSpeedType.Running: return speed * runModifier;
        }

        return speed;
    }

    private float GetCurrentRotation(BlobSpeedType speedType)
    {
        switch (speedType)
        {
            case BlobSpeedType.Walking: return rotationSpeed;
            case BlobSpeedType.Jogging: return rotationSpeed * jogRotationModifier;
            case BlobSpeedType.Running: return rotationSpeed * runRotationModifier;
        }

        return rotationSpeed;
    }
}

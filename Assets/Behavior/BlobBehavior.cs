using Assets;
using Assets.Enums;
using Assets.Utils;
using BlobSimulation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlobBehavior : MonoBehaviour
{
    public MapGenerator ground;
    public PerceptionBehavior perception;
    public GameObject blobPrefab;
    public GameObject fruitPrefab;
    public GameObject treePrefab;
    public static float heightOverWidth = 1.324292f;

    // Things that evolve
    internal float size = 1;
    private float mass = 1;
    internal float perceptionDepth = 3;
    internal float perceptionWidth = 2;
    internal float perceptionShift = 1 / 3;
    internal float speed = 4;
    internal float grazingModifier = 0.05f;
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
    internal float monogomy = 0.5f;
    internal bool isMonogomous = true;


    internal TreeGenes seedInPoop;
    private int rejectedMates = 0;

    // includes more options for predators, costs energy per movement
    private float _melee = 1f;
    internal float melee
    {
        get
        {
            return _melee;
        }
        set
        {
            // only allow values 1 or over
            if (value < 1) _melee = 1f;
            else _melee = value;
        }
    }

    // reduces predators for potential prey, costs energy per movement
    private float _armor = 1f;
    internal float armor {
        get {
            return _armor;
        }
        set
        {
            // only allow values 1 or over
            if (value < 1) _armor = 1f;
            else _armor = value;
        }
    }


    internal BlobStatusType status = BlobStatusType.Wandering;
    public GenderType gender = GenderType.Male;

    // Dependent on size
    internal float rotationSpeed = 10;
    internal float currentRotationSpeed = 5;

    internal int incubatedEnergy = 0;
    bool eaten = false;
    internal float currentSpeed = 4;

    internal int energy = 100000000;


    int energyPerFruit = 40000;
    int energyPerTreeFruit = 10000;


    GameObject food;
    BlobBehavior predator;
    BlobBehavior prey;
    internal BlobBehavior parent;
    BlobBehavior rival;
    public HashSet<BlobBehavior> selectedPartners;
    public BlobBehavior currentPartner;

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
    private Color grazingColor = new Color(0.5f, 0.85f, 1);
    private Color femaleColor = new Color(1, 0.08f, 1);
    private Color deadColor = new Color(0.75f, 0.75f, 0.75f);

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
            monogomy = 0.5f;
            isMonogomous = true;

            var stats = ground.GetComponent<Statistics>();

            stats.UpdateAverages(this, StatType.Birth);
        }

        if (generation < 2) currentPartner = this;

        mass = size * size * size;
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main.GetComponent<CameraBehavior>().paused) return;

        if (IsDead())
        {
            if (status != BlobStatusType.Dead)
            {
                stats.UpdateAverages(this, StatType.Death);
                status = BlobStatusType.Dead;
                SetColor(deadColor);

                transform.rotation = Quaternion.LookRotation(Vector3.up);
                Destroy(this.perception.gameObject);
            }

            size -= 0.001f;

            if (size <= 0 || eaten)
            {
                Destroy(this.gameObject);
                Destroy(this);
                return;
            }

            gameObject.transform.localScale = new Vector3(size, size * heightOverWidth, size);
            return;
        }

        ticksLived++;

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

                var partnerPosition = currentPartner.transform.position;

                SlowToTarget(partnerPosition);

                HeadToTarget(partnerPosition);
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
            case BlobStatusType.Scavenging:

                SlowToTarget(food.transform.position);

                HeadToTarget(food.transform.position);

                break;
            case BlobStatusType.Grazing:
                Graze();
                goto case BlobStatusType.Wandering;
            case BlobStatusType.Wandering:
            default:
                RandomPoop();
                RandomWalk();
                break;
        }
        
        MoveForward();
    }

    private void Graze()
    {
        energy += (int) Math.Floor( 10 * (1 - carnivorous));
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

        if (distanceToTarget < 5 * ground.scale)
        {
            currentSpeed /= 2;
        }

        return distanceToTarget;
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
                if (prey.status.Equals(BlobStatusType.Dead)) {
                    SetSpeedAndColor(BlobSpeedType.Jogging, hungryColor, BlobStatusType.Scavenging);
                }
                else
                {
                    SetSpeedAndColor(BlobSpeedType.Running, hungryColor, BlobStatusType.Chasing);
                }

                return;
            }
        }
        if (food != null)
        {
            SetSpeedAndColor(BlobSpeedType.Jogging, hungryColor, BlobStatusType.Foraging);
            return;
        }

        // don't decide to wander if you're grazing, or start grazing if you're wandering
        if (status == BlobStatusType.Grazing) {
            SetColor(grazingColor);
            return;
        }

        if (status == BlobStatusType.Wandering)
        {
            SetColor(normalColor);
            return;
        }

        if (UnityEngine.Random.value > carnivorous)
        {
            SetSpeedAndColor(BlobSpeedType.Grazing, grazingColor, BlobStatusType.Grazing);
            return;
        }

        SetSpeedAndColor(BlobSpeedType.Walking, normalColor, BlobStatusType.Wandering);
    }

    private bool ShouldProvide()
    {
        if (currentPartner == null) return false;

        return gender.Equals(GenderType.Male) 
            && IsMonogomous()
            && energy > reserveEnergy * 2 
            && currentPartner.currentPartner.Equals(this)
            && currentPartner != this
            && !currentPartner.ShouldReproduce();
    }

    private bool IsMonogomous()
    {
        return monogomy > 0.5;
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
        if (!isMonogomous && currentPartner == null && selectedPartners.Count > 0)
        {
            currentPartner = PickPartnerToBreedWith();
        }

        return (currentPartner != null && ticksLived > sexualMaturity && (energy > reproductionLimit || currentIncubation > 0));
    }

    private void FixHeightBug()
    {
        transform.position = new Vector3(transform.position.x, LocationUtil.GetHeight(transform.position, ground) + heightAdjust, transform.position.z);
    }

    private bool IsDead()
    {
        return status.Equals(BlobStatusType.Dead) || energy < 0 || ticksLived > sexualMaturity * 100 || eaten;
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

        if (perception.LatestBlob != null && perception.LatestBlob != this)
        {
            var newBlob = perception.LatestBlob;
            perception.LatestBlob = null;

            if (parent != null && (newBlob.Equals(parent) || (newBlob.parent != null && newBlob.parent.Equals(this)))) return;

            if (IsPredator(this, newBlob))
            {
                predator = newBlob;
                food = null;
            }
            else if (IsPrey(this, newBlob))
            {
                prey = newBlob;
                food = null;
            }
            else {

                var imMonogomous = IsMonogomous();

                if (LookingForAPartner(imMonogomous) && IsGoodPartner(newBlob))
                {

                    var theyreMonogomous = newBlob.IsMonogomous();

                    if (theyreMonogomous)
                    {
                        // they already have a partner
                        if (newBlob.currentPartner != null) SetRival(newBlob.currentPartner);
                        else AddPartner(newBlob);
                    }
                    else
                    {
                        AddPartner(newBlob);
                    }
                }
            }
        }
    }

    private bool LookingForAPartner(bool imMonogomous)
    {
        return (imMonogomous && currentPartner == null) || !imMonogomous;
    }

    private void SetRival(BlobBehavior potentialRival)
    {
        rival = potentialRival;
        rival.rival = this;

        LookAt(rival.transform.position);
        rival.LookAt(transform.position);
    }

    private void AddPartner(BlobBehavior newBlob)
    {
        if (isMonogomous) currentPartner = newBlob;
        else selectedPartners.Add(newBlob);

        if (newBlob.isMonogomous) newBlob.currentPartner = this;
        else newBlob.selectedPartners.Add(this);
    }

    private bool IsPredator(BlobBehavior thisBlob, BlobBehavior newBlob)
    {
        return IsPrey(newBlob, thisBlob);
    }

    private bool IsPrey(BlobBehavior thisBlob, BlobBehavior potentialPrey)
    {
        //float wantToEatSomeone = HowHungryForBlood(newBlob);

        return potentialPrey.status.Equals(BlobStatusType.Dead) || potentialPrey.mass * potentialPrey.armor < thisBlob.mass * thisBlob.melee * thisBlob.melee / thisBlob.predationLimit;
    }

    //private float HowHungryForBlood(BlobBehavior newBlob)
    //{
    //    var ticksishToStarving = energy / perceptionWidth * perceptionDepth * mass * melee * armor;

    //    carnivorous / ticksishToStarving
    //}

    private bool IsGoodPartner(BlobBehavior potentialPartner)
    {
        // no sense checking stuff if they're alraedy your partner
        // ... and don't pick yourself.  that's ridiculous.
        if ((!isMonogomous && selectedPartners.Contains(potentialPartner)) || currentPartner == potentialPartner || potentialPartner == this) return false;

        // both parters should be sexually mature
        if (potentialPartner.ticksLived < potentialPartner.sexualMaturity || ticksLived < sexualMaturity) return false;

        var isGoodPartner = IsSameSpecies(potentialPartner, isMateSelection: true);

        if (isGoodPartner)
        {
            rejectedMates = 0;
        }
        else
        {
            rejectedMates++;
        }

        return isGoodPartner;
    }

    public bool IsSameSpecies(BlobBehavior potentialPartner, bool isMateSelection = false)
    {
        var pickiness = mateLimit;

        if (isMateSelection)
        {
             pickiness = mateLimit + (float)rejectedMates * 0.01f;
        }
        
        if (potentialPartner.speed / speed > pickiness || speed / potentialPartner.speed > pickiness) return false;
        if (potentialPartner.jogModifier / jogModifier > pickiness || jogModifier / potentialPartner.jogModifier > pickiness) return false;
        if (potentialPartner.runModifier / runModifier > pickiness || runModifier / potentialPartner.runModifier > pickiness) return false;
        if (potentialPartner.sexualMaturity / sexualMaturity > pickiness || sexualMaturity / potentialPartner.sexualMaturity > pickiness) return false;
        if (potentialPartner.wantForPrey / wantForPrey > pickiness || wantForPrey / potentialPartner.wantForPrey > pickiness) return false;
        if (potentialPartner.aggression / aggression > pickiness || aggression / potentialPartner.aggression > pickiness) return false;
        if (potentialPartner.carnivorous / carnivorous > pickiness || carnivorous / potentialPartner.carnivorous > pickiness) return false;
        if (potentialPartner.fearOfPredator / fearOfPredator > pickiness || fearOfPredator / potentialPartner.fearOfPredator > pickiness) return false;
        if (potentialPartner.melee / melee > pickiness || melee / potentialPartner.melee > pickiness) return false;
        if (potentialPartner.armor / armor > pickiness || armor / potentialPartner.armor > pickiness) return false;
        if (potentialPartner.size / size > pickiness || size / potentialPartner.size > pickiness) return false;

        return true;
    }

    private void MakeNewBlob()
    {
        if (currentPartner == null)
        {
            currentIncubation = 0;
            incubatedEnergy = 0;
            return;
        }

        currentIncubation++;
        energy -= 1000 * (int)size;
        incubatedEnergy += (int)(1000 * size);

        if (currentIncubation >= incubationTicks)
        {
            GameObject newGameObject = Instantiate(gameObject, transform.position, transform.rotation);
            ReproductionUtil.ReproduceBlob(this, newGameObject, ground);

            if (!isMonogomous) currentPartner = null;
        }
    }

    private BlobBehavior PickPartnerToBreedWith()
    {
        ScrubSelectPartners();

        if (selectedPartners.Count == 0) return null;

        var random = new System.Random();

        return selectedPartners.OrderBy(x => random.Next()).Take(1).SingleOrDefault();
    }

    private void ScrubSelectPartners()
    {
        selectedPartners.Remove(null);

        var deadBlobs = new List<BlobBehavior>();

        foreach (var partner in selectedPartners)
        {
            if (partner.eaten) deadBlobs.Add(partner);
        }

        deadBlobs.ForEach(b => selectedPartners.Remove(b));
    }

    private void HeadToTarget(Vector3 targetLocation)
    {
        var targetPosition = targetLocation;
        targetPosition.y = 0;
        var direction = DirectionToTarget(targetPosition);
        direction.y = 0;

        if (direction.Equals(Vector3.zero)) return;

        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * currentRotationSpeed);
        energy -= (int)(mass * currentRotationSpeed * currentRotationSpeed);
    }

    private Vector3 DirectionToTarget(Vector3 targetPosition)
    {
        return (targetPosition - transform.position).normalized;
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

        energy -= EnergyForMovement();

        transform.Translate(moveAmount, Space.Self);
    }

    private int EnergyForMovement()
    {
        return (int)(mass * currentSpeed * currentSpeed * melee * armor);
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
            var targetBlob = triggerCollider.gameObject.GetComponent<BlobBehavior>();

            if (targetBlob == null) { targetBlob = triggerCollider.gameObject.GetComponentInParent<BlobBehavior>(); }

            HandleCollisionWithBlob(targetBlob);
        }
    }

    private void HandleCollisionWithBlob(BlobBehavior targetBlob)
    {
        if (targetBlob == null) return;

        if (IsPrey(this, targetBlob))
        {
            var deltaEnergy = (int)(energyPerFruit * targetBlob.size * carnivorous * 2);

            food = null;
            energy += deltaEnergy;
            energyFromBlobs += deltaEnergy;
            targetBlob.eaten = true;
            blobsEaten++;
            stats.numBlobsEaten++;

            if (stats.recordBlobsEaten < blobsEaten) stats.recordBlobsEaten = blobsEaten;

            RememberThisPlace();

            if (prey.status.Equals(BlobStatusType.Dead))
            {
                Destroy(prey.gameObject);
                Destroy(prey);
            }

            return;
        }
        else
        {
            if (prey = targetBlob) prey = null;
        }


        if (targetBlob.Equals(rival))
        {
            int hurtRivalAmount = (int)(mass * currentSpeed * currentSpeed * aggression * 100);
            int hurtSelfAmount = hurtRivalAmount / 2;
            int hurtFromRival = (int)(rival.mass * rival.currentSpeed * rival.currentSpeed * rival.aggression * 100);
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


            if (hurtRivalAmount > hurtFromRival && rival.currentPartner != null && currentPartner == null)
            {
                rival.currentPartner.currentPartner = this;
                currentPartner = rival.currentPartner;
                rival.currentPartner = null;
            }

            else if (hurtRivalAmount < hurtFromRival && rival.currentPartner == null && currentPartner != null)
            {
                currentPartner.currentPartner = rival;
                rival.currentPartner = currentPartner;
                currentPartner = null;
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

        if (targetBlob.Equals(currentPartner) && status.Equals(BlobStatusType.Providing) && energy > reserveEnergy)
        {
            var energyToProvide = Math.Abs(reserveEnergy - energy);
            energy -= energyToProvide;
            currentPartner.energy += energyToProvide;
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
        SetColor(newColor);
    }

    private void SetColor(Color newColor)
    {
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
            case BlobSpeedType.Grazing: return speed * grazingModifier;
            case BlobSpeedType.Jogging: return speed * jogModifier;
            case BlobSpeedType.Running: return speed * runModifier;
        }

        return speed;
    }

    private float GetCurrentRotation(BlobSpeedType speedType)
    {
        switch (speedType)
        {
            case BlobSpeedType.Walking:
            case BlobSpeedType.Grazing:
                return rotationSpeed;
            case BlobSpeedType.Jogging: return rotationSpeed * jogRotationModifier;
            case BlobSpeedType.Running: return rotationSpeed * runRotationModifier;
        }

        return rotationSpeed;
    }
}

using System;
using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlobBehavior : MonoBehaviour
{
    public GameObject ground;
    public PerceptionBehavior perception;
    public GameObject blobPrefab;
    public GameObject fruitPrefab;

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
    internal int sexualMaturity = 1000;
    internal int reserveEnergy = 100000;
    //internal int lifespan = 45000;


    internal BlobStatusType status = BlobStatusType.Wandering;
    public GenderType gender = GenderType.Male;

    // Dependent on size
    internal float rotationSpeed = 5;

    internal int incubatedEnergy = 0;
    bool die = false;
    internal float currentSpeed = 4;

    internal int energy = 100000;


    int energyPerFruit = 30000;


    GameObject food;
    BlobBehavior predator;
    BlobBehavior prey;
    internal BlobBehavior parent;
    BlobBehavior rival;
    public BlobBehavior partner;

    internal int currentIncubation = 0;
    float predationLimit = 1.3f;
    float mateLimit = 1.1f;
    Vector3 randomTarget;


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

    internal void Start()
    {
        stats = ground.GetComponent<Statistics>();

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

        if (!LeaveEdge())
        {
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
                    RandomWalk();
                    break;
            }
        }
        
        MoveForward();
    }
    
    private float SlowToTarget(Vector3 targetPosition)
    {
        var distanceToTarget = Vector3.Distance(targetPosition, transform.position);

        if (distanceToTarget < 3)
        {
            currentSpeed *= distanceToTarget / 3;
        }

        return distanceToTarget;
    }

    private void Die()
    {
        stats.UpdateAverages(this, StatType.Death);
        if (!die)
        {
            var fruit = Instantiate(fruitPrefab, transform.position, transform.rotation);
            fruit.name = "Fruit";
        }
        Destroy(this.gameObject);
        Destroy(this);
    }

    private void SetStatus()
    {

        if (ShouldProvide())
        {
            SetSpeedAndColor(speed * runModifier, incubationColor, BlobStatusType.Providing);
            return;
        }
        
        if (predator != null)
        {
            if (Vector3.Distance(predator.transform.position, transform.position) > fearOfPredator) predator = null;

            else
            {
                SetSpeedAndColor(speed * runModifier, scaredColor, BlobStatusType.Fleeing);
                return;
            }
        }
        //if (gender.Equals(GenderType.Female) && partner != null && partner.partner.Equals(this) && partner.status.Equals(BlobStatusType.Providing))
        //{
        //    SetSpeedAndColor(speed, normalColor, BlobStatusType.Wandering);
        //    return;
        //}
        if (rival != null)
        {
            // collision resets rival
            SetSpeedAndColor(speed * runModifier, angryColor, BlobStatusType.Fighting);
            return;
        }
        if (prey != null)
        {
            if (Vector3.Distance(prey.transform.position, transform.position) > wantForPrey) prey = null;

            else
            {
                SetSpeedAndColor(speed * runModifier, hungryColor, BlobStatusType.Chasing);
                return;
            }
        }
        if (food != null)
        {
            SetSpeedAndColor(speed * jogModifier, hungryColor, BlobStatusType.Foraging);
            return;
        }

        SetSpeedAndColor(speed, normalColor, BlobStatusType.Wandering);
    }

    private bool ShouldProvide()
    {
        return gender.Equals(GenderType.Male) 
            && energy > reserveEnergy * 2 
            && partner != null 
            && partner.partner != null
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
        transform.position = new Vector3(transform.position.x, ReproductionUtil.yConstant, transform.position.z);
    }

    private bool ShouldDie()
    {
        return energy < 0 || die;
    }

    private void RunAway()
    {
        var targetPosition = predator.transform.position;
        transform.rotation = LookAwayFrom(targetPosition); //Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private Quaternion LookAwayFrom(Vector3 objectToShun)
    {

        return Quaternion.LookRotation(transform.position - objectToShun);
    }

    private void UpdateTargetReferences()
    {
        energy -= (int)(perceptionWidth * perceptionDepth);

        food = perception.LatestFruit;

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
                LookAt(rival.transform.position);
                rival.LookAt(transform.position);
            }
        }
    }

    private bool IsRival(BlobBehavior newBlob)
    {
        // must both be male, and need to steal their partner
        return (gender == GenderType.Male && newBlob.gender == GenderType.Male && partner == null & newBlob.partner != null && rival == null && newBlob.rival == null && IsGoodPartner(newBlob.partner));
    }

    private bool IsGoodPartner(BlobBehavior blob)
    {
        // girls pick boys
        if (gender == GenderType.Male || blob.gender == GenderType.Female || blob.size / size > mateLimit || size / blob.size > mateLimit) return false;
        if (partner == null || partner == this) return true;

        if (partner.ticksLived == 0) return true;
        if (blob.ticksLived < sexualMaturity) return false;

        return partner.energy / partner.ticksLived < blob.energy / blob.ticksLived;
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
        targetPosition.y = 0;
        var direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void RandomWalk()
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
            LookAt(new Vector3(0, 0, 0));

            if (predator != null || prey != null)
            {
                if (Random.value > 0.5f) transform.Rotate(0f, 80f, 0f);
                else transform.Rotate(0f, -80f, 0f);
            }

            return true;
        }

        return false;
    }

    private void LookAt(Vector2 targetCoords)
    {
        var targetPosition = new Vector3(targetCoords.x, 0, targetCoords.y);
        transform.LookAt(targetPosition);
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
        if (triggerCollider.gameObject.name.StartsWith("Fruit"))
        {
            Destroy(triggerCollider.gameObject);
            food = null;
            energy += energyPerFruit;
            energyFromFruit += energyPerFruit;
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

                energy -= (hurtSelfAmount + hurtFromRival);
                rival.energy -= (hurtRivalAmount + hurtRivalSelf);
                
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

    private void SetSpeedAndColor(float newSpeed, Color newColor, BlobStatusType newStatus)
    {
        currentSpeed = newSpeed;
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
}

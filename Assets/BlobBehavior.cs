using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlobBehavior : MonoBehaviour
{
    public GameObject ground;
    public PerceptionBehavior perception;
    public GameObject blobPrefab;

    float geneticDrift = 0.1f;

    // Things that evolve
    public float size = 1;
    public float perceptionDepth = 3;
    public float perceptionWidth = 2;
    public float perceptionShift = 1 / 3;
    public float speed = 4;
    public float jogModifier = 2;
    public float runModifier = 3;
    public float randomRotation = 5;
    public float fearOfPredator = 5;
    public float wantForPrey = 5;
    public int incubationTicks = 100;
    public int reproductionLimit = 100000;

    // Dependent on size
    float rotationSpeed = 10;
    
    int incubatedEnergy = 0;
    bool die = false;
    public float currentSpeed = 4;

    public int energy = 100000;


    int energyPerFruit = 30000;
    

    GameObject target;
    BlobBehavior predator;
    BlobBehavior prey;

    int currentIncubation = 0;
    float predationLimit = 1.4f;
    Vector3 randomTarget;
    public static float yConstant = 0.47f;

    // stats
    public int fruitEaten = 0;
    public int energyFromFruit = 0;
    public int blobsEaten = 0;
    public int energyFromBlobs = 0;
    public int childen = 0;
    public DateTime birthday = DateTime.Now;

    void Start()
    {
        fruitEaten = 0;
        blobsEaten = 0;
        energyFromFruit = 0;
        energyFromBlobs = 0;
        childen = 0;
        birthday = DateTime.Now;
}

    // Update is called once per frame
    void Update()
    {
        if (ShouldDie())
        {
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
        print("Clicked!");
        Camera.main.GetComponent<CameraBehavior>().target = this;
    }

    private bool ShouldReproduce()
    {
        return energy > reproductionLimit || currentIncubation > 0;
    }

    private void FixHeightBug()
    {
        transform.position = new Vector3(transform.position.x, yConstant, transform.position.z);
    }

    private bool ShouldDie()
    {
        return energy < 0 || die;
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
        var perceptionStrength = perception.gameObject.transform.localScale.x;

        energy -= (int) (perceptionWidth * perceptionDepth);

        if (perception.LatestBlob != null)
        {
            var newBlob = perception.LatestBlob;

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
        currentIncubation++;
        energy -= 1000 * (int) size;
        incubatedEnergy += (int) (500 * size);

        if (currentIncubation >= incubationTicks)
        {
            GameObject gameObject = Instantiate(this.gameObject, transform.position, transform.rotation);

            float newSize = SetNewBlobSize(gameObject);
            float speedModifier = newSize * newSize;

            BlobBehavior newBlob = gameObject.GetComponent<BlobBehavior>();

            var vectorScale = perception.gameObject.transform.localScale;

            SetPerceptionFields(newBlob);
            newBlob.randomRotation = randomRotation * GetDrift();

            newBlob.transform.position = new Vector3(transform.position.x, yConstant, transform.position.z);

            newBlob.speed = speed * GetDrift();
            newBlob.jogModifier = jogModifier * GetDrift();
            newBlob.runModifier = runModifier * GetDrift();
            newBlob.fearOfPredator = fearOfPredator * GetDrift();
            newBlob.wantForPrey = wantForPrey * GetDrift();
            newBlob.incubationTicks = (int) (incubationTicks * GetDrift());
            newBlob.rotationSpeed /= speedModifier;
            newBlob.ground = ground;
            newBlob.blobPrefab = blobPrefab;
            newBlob.size = newSize;
            newBlob.energy = incubatedEnergy;
            newBlob.reproductionLimit = (int) (reproductionLimit * GetDrift());

            newBlob.Start();

            energy /= 3;

            currentIncubation = 0;
            childen++;
        }
    }

    private void SetPerceptionFields(BlobBehavior newBlob)
    {
        var perceptionTransform = newBlob.perception.gameObject.transform;

        newBlob.perceptionDepth = perceptionDepth * GetDrift();
        newBlob.perceptionWidth = perceptionWidth * GetDrift();

        // to keep from stagnating at 0
        newBlob.perceptionShift = (perceptionShift + GetDrift() - 1);

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

        energy -= (int) (size * size * size * currentSpeed * currentSpeed);

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
            target = null;
            energy += energyPerFruit;
            energyFromFruit += energyPerFruit;
            currentSpeed = speed;
            fruitEaten++;
        }

        if (triggerCollider.gameObject.name.StartsWith("Percepticon"))
        {
            var targetBlob = triggerCollider.gameObject.GetComponentInParent<BlobBehavior>();

            if (targetBlob == null || targetBlob.size < size / predationLimit)
            {
                var deltaEnergy = (int)(energyPerFruit * targetBlob.size * 2);

                target = null;
                energy += deltaEnergy;
                energyFromBlobs += deltaEnergy;
                currentSpeed = speed;
                targetBlob.die = true;

                blobsEaten++;
            }
        }
    }
}

using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public MapGenerator ground;
    public Material perceptionMaterial;
    public BlobBehavior target;
    public int fruitSpawnRate = 8;
    public ColorDisplayType colorToggle = ColorDisplayType.None;

    int amount = 1;
    float distanceToTarget = 10;
    public bool showStats = false;
    public DisplayType displayType = DisplayType.None;
    int numDigitsAfterPoint = 2;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        UserControl();

    }

    void OnGUI()
    {
        GUI.contentColor = Color.black;

        if (target != null)
        {
            if (showStats)
            {

                var timespan = TimeSpan.FromMilliseconds(target.ticksLived * 20);
                var timeString = string.Format("{0}h{1}m{2}s",
                    timespan.Hours,
                    timespan.Minutes,
                    timespan.Seconds);

                var thingsToSay = new List<string>
                    {
                        target.energy + " energy left",
                        target.fruitEaten + " fruit found",
                        target.energyFromFruit + " energy from fruit",
                        target.blobsEaten + " blobs cannibalized",
                        target.energyFromBlobs + " energy from blobs",
                        timeString + " lived",
                        target.size.ToString("F" + numDigitsAfterPoint) + " size",
                        target.useMemoryPercent.ToString("F" + numDigitsAfterPoint) + " memory",
                        target.perceptionDepth.ToString("F" + numDigitsAfterPoint) + " perceptionDepth",
                        target.perceptionWidth.ToString("F" + numDigitsAfterPoint) + " perceptionWidth",
                        target.perceptionShift.ToString("F" + numDigitsAfterPoint) + " perceptionShift",
                        target.speed.ToString("F" + numDigitsAfterPoint) + " speed",
                        target.jogModifier.ToString("F" + numDigitsAfterPoint) + " jogModifier",
                        target.runModifier.ToString("F" + numDigitsAfterPoint) + " runModifier",
                        target.randomRotation.ToString("F" + numDigitsAfterPoint) + " randomRotation",
                        target.aggression.ToString("F" + numDigitsAfterPoint) + " aggression",
                        target.fearOfPredator.ToString("F" + numDigitsAfterPoint) + " fearOfPredator",
                        target.wantForPrey.ToString("F" + numDigitsAfterPoint) + " wantForPrey",
                        target.incubationTicks + " incubationTicks",
                        target.incubatedEnergy + " incubatedEnergy",
                        target.reproductionLimit + " reproductionLimit",
                        target.children + " children sired",
                        target.generation + " generation",
                        target.childGenderRatio.ToString("F" + numDigitsAfterPoint) + "% female",
                        target.gender.ToString(),
                        target.status.ToString()
                    };

                DisplayStats(thingsToSay);
            }
        }
        else
        {


            switch (displayType)
            {
                case DisplayType.None:
                    break;
                case DisplayType.Records:
                    DisplayRecords();
                    break;
                case DisplayType.Statistics:
                    DisplayStatistics();
                    break;
            }
        }
    }

    private void DisplayStatistics()
    {

        var stats = ground.GetComponent<Statistics>();

        var thingsToSay = new List<string>
        {
            stats.numBlobs + " blobs",
            stats.totalFemales + " females",
            "Averages",
            ((float)stats.numFruitEaten / stats.numBlobs).ToString("F" + numDigitsAfterPoint) + " fruit eaten",
            ((float)stats.numBlobsEaten / stats.numBlobs).ToString("F" + numDigitsAfterPoint) + " blobs eaten",

            stats.averageSize.ToString("F" + numDigitsAfterPoint) + " size",
            stats.averageJogModifier.ToString("F" + numDigitsAfterPoint) + " jog speed",
            stats.averageRunModifier.ToString("F" + numDigitsAfterPoint) + " run speed",
            stats.averageRandomRotation.ToString("F" + numDigitsAfterPoint) + " rotation",
            stats.averageAggression.ToString("F" + numDigitsAfterPoint) + " aggression",
            stats.averageFearOfPredator.ToString("F" + numDigitsAfterPoint) + " fear",
            stats.averageWantOfPrey.ToString("F" + numDigitsAfterPoint) + " want",
            stats.averageIncubationTicks.ToString("F" + numDigitsAfterPoint) + " incubation",
            stats.averageReproductionLimit.ToString("F0") + " reproduction limit",
            stats.averageChildrenPerFemale.ToString("F" + numDigitsAfterPoint) + " children",
            stats.percentFemale.ToString("F" + numDigitsAfterPoint) + "% female"
        };

        DisplayStats(thingsToSay);
    }

    private void DisplayRecords()
    {
        var stats = ground.GetComponent<Statistics>();

        var thingsToSay = new List<string>
        {
            stats.numBlobs + " blobs",
            "",
            "Records",
            stats.recordBlobCount + " blobs",
            stats.recordBlobsEaten + " blobs eaten",
            stats.recordFruitEaten + " fruit eaten",
            stats.recordHighJogModifier + " high jog speed",
            stats.recordLowJogModifier + " low jog speed",
            stats.recordHighRunModifier + " high run speed",
            stats.recordLowRunModifier + " low run speed",
            stats.recordHighIncubationTicks + " high incubation",
            stats.recordLowIncubationTicks + " low incubation",
            stats.recordHighReproductionLimit + " high reproduction limit",
            stats.recordLowReproductionLimit + " low reproduction limit",
            stats.recordChildren + " children"
        };

        DisplayStats(thingsToSay);
    }

    private static void DisplayStats(List<string> thingsToSay)
    {
        var guiStyle = new GUIStyle();
        guiStyle.fontSize = 26;

        for (var i = 0; i < thingsToSay.Count; i++)
        {
            GUI.Label(new Rect(10, 30 * i + 12, 200, 20), thingsToSay[i], guiStyle);
        }
    }

    private void UserControl()
    {
        amount = 1;

        if (target == null)
        {
            HandleTranslation();

            HandleRotation();
        }
        else
        {
            FollowTarget();
        }

        HandlePerception();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            target = null;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (target != null) showStats = !showStats;
            else
            {
                switch(displayType)
                {
                    case DisplayType.None:
                        displayType = DisplayType.Statistics;
                        break;
                    case DisplayType.Statistics:
                        displayType = DisplayType.Records;
                        break;
                    case DisplayType.Records:
                        displayType = DisplayType.None;
                        break;
                }
            }
        }

        if (Input.GetKey(KeyCode.Comma) && Time.timeScale > 0.05f)
        {
            Time.timeScale -= 0.05f;
        }

        if (Input.GetKey(KeyCode.Period))
        {
            Time.timeScale += 0.05f;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            switch (colorToggle)
            {
                case ColorDisplayType.None:
                    colorToggle = ColorDisplayType.Action;
                    break;
                case ColorDisplayType.Action:
                    colorToggle = ColorDisplayType.Gender;
                    break;
                case ColorDisplayType.Gender:
                    colorToggle = ColorDisplayType.None;
                    break;

            }
        }
    }

    private void FollowTarget()
    {
        distanceToTarget -= Input.GetAxisRaw("Vertical") * 0.1f;
        if (distanceToTarget < 0.5) distanceToTarget = 0.5f;

        transform.LookAt(target.transform.position);
        var currentDistanceToTarget = Vector3.Distance(target.transform.position, transform.position);

        if (currentDistanceToTarget > distanceToTarget)
        {
            var speed = 1.01f;

            if (currentDistanceToTarget > distanceToTarget * 1.3) speed = 8;
            var moveAmount = new Vector3(0, 0, 1).normalized * target.currentSpeed * Time.deltaTime * speed;

            transform.Translate(moveAmount, Space.Self);
        }
    }

    private void HandlePerception()
    {
        if (Input.GetKey(KeyCode.LeftBracket) && perceptionMaterial.color.a > 0.01)
        {
            perceptionMaterial.color = new Color(perceptionMaterial.color.r, perceptionMaterial.color.g, perceptionMaterial.color.b, perceptionMaterial.color.a - 0.01f);
        }
        if (Input.GetKey(KeyCode.RightBracket) && perceptionMaterial.color.a < 1)
        {
            perceptionMaterial.color = new Color(perceptionMaterial.color.r, perceptionMaterial.color.g, perceptionMaterial.color.b, perceptionMaterial.color.a + 0.01f);
        }
    }

    private void HandleTranslation()
    {
        int height = 0;

        if (Input.GetKey(KeyCode.LeftControl) && transform.position.y > 1)
        {
            height = -1;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            height = 1;
        }

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), height, Input.GetAxisRaw("Vertical"));
        Vector3 direction = input.normalized;
        Vector3 velocity = (direction * amount) / 2;

        transform.Translate(velocity, Space.Self);

        if (transform.position.y < 1) transform.position = new Vector3(transform.position.x, 1, transform.position.z);
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(new Vector3(0, amount*2, 0));
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(new Vector3(0, -amount*2, 0));
        }

        if (Input.GetKey(KeyCode.R))
        {
            transform.Rotate(new Vector3(amount*2, 0, 0));
        }

        if (Input.GetKey(KeyCode.F))
        {
            transform.Rotate(new Vector3(-amount*2, 0, 0));
        }

        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }
}

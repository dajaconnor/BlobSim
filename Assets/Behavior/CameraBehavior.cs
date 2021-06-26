using Assets.Enums;
using Assets.Utils;
using BlobSimulation.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public MapGenerator ground;
    public Material perceptionMaterial;
    public Material treeMaterial;
    public BlobBehavior target;
    public int fruitSpawnRate = 4;
    public ColorDisplayType colorToggle = ColorDisplayType.None;

    float amount;
    float distanceToTarget = 10;
    public bool showStats = false;
    public DisplayType displayType = DisplayType.None;
    int numDigitsAfterPoint = 2;
    float speciationSample = 0;

    // Use this for initialization
    void Start()
    {
        treeMaterial.color = new Color(treeMaterial.color.r, treeMaterial.color.g, treeMaterial.color.b, 1);
        ground = FindObjectOfType<MapGenerator>();
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
                        target.status.ToString(),
                        "",
                        $"Height: {LocationUtil.GetHeight(target.transform.position, ground).ToString("F" + numDigitsAfterPoint)}"
                    };

                DisplayStats(thingsToSay);
            }
        }
        else
        {
            DisplayRecords(displayType);
        }
    }

    private void DisplayRecords(DisplayType displayType)
    {
        switch (displayType)
        {
            case DisplayType.PhysicalRecords:
                DisplayPhysicalRecords();
                break;
            case DisplayType.BehavioralRecords:
                DisplayBehavioralRecords();
                break;
            case DisplayType.ReproductiveRecords:
                DisplayReproductiveRecords();
                break;
        }
    }

    private void DisplayPhysicalRecords()
    {
        var stats = ground.GetComponent<Statistics>();

        DisplayStats(new List<string>
        {
            $"{stats.armor.PrintMinMax(false)} armor",
            $"{stats.melee.PrintMinMax(false)} melee",
            $"{stats.size.PrintMinMax(false)} size",
            $"{stats.speed.PrintMinMax(false)} base speed",
            $"{stats.jogModifier.PrintMinMax(false)} forage speed",
            $"{stats.runModifier.PrintMinMax(false)} run speed",
            $"{stats.rotationSpeed.PrintMinMax(false)} rotation",
            $"{stats.carnivorous.PrintMinMax(false)} carnivorousness",
        });
    }

    private void DisplayBehavioralRecords()
    {
        var stats = ground.GetComponent<Statistics>();

        DisplayStats(new List<string>
        {
            $"{stats.aggression.PrintMinMax(false)} aggression",
            $"{stats.wantForPrey.PrintMinMax(false)} want for prey",
            $"{stats.fearOfPredator.PrintMinMax(false)} fear of predator",
            $"{stats.randomRotation.PrintMinMax(false)} random rotation",
            $"{stats.useMemory.PrintMinMax(false)} memory",
            stats.recordBlobsEaten + " record blobs eaten",
            stats.recordFruitEaten + " record fruit eaten"
        });
    }


    private void DisplayReproductiveRecords()
    {
        var stats = ground.GetComponent<Statistics>();

        DisplayStats(new List<string>
        {
            $"{stats.recordBlobCount} {stats.numBlobs} blobs",
            stats.totalFemales + " females",
            ((float)stats.numFruitEaten / stats.numBlobs).ToString("F" + numDigitsAfterPoint) + " fruit eaten",
            ((float)stats.numBlobsEaten / stats.numBlobs).ToString("F" + numDigitsAfterPoint) + " blobs eaten",
            stats.percentFemale.ToString("F" + numDigitsAfterPoint) + "% female",
            $"{stats.sexualMaturity.PrintMinMax(true)} sexual maturity",
            $"{stats.incubationTicks.PrintMinMax(true)} incubation",
            $"{stats.reproductionLimit.PrintMinMax(true)} reproduction",
            stats.recordChildren + " children",
            $"{speciationSample.ToString("F" + 5)} speciation"
        });
    }




    private static void DisplayStats(List<string> thingsToSay)
    {
        var guiStyle = new GUIStyle();
        guiStyle.fontSize = 16;

        for (var i = 0; i < thingsToSay.Count; i++)
        {
            GUI.Label(new Rect(10, 20 * i + 12, 200, 20), thingsToSay[i], guiStyle);
        }
    }

    private void UserControl()
    {
        amount = 0.5f;

        if (target == null)
        {
            HandleTranslation();

            HandleRotation();
        }
        else
        {
            FollowTarget();
        }

        HandleTranslucence();

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
                        displayType = DisplayType.PhysicalRecords;
                        break;
                    case DisplayType.PhysicalRecords:
                        displayType = DisplayType.BehavioralRecords;
                        break;
                    case DisplayType.BehavioralRecords:
                        displayType = DisplayType.ReproductiveRecords;
                        speciationSample = ReproductionUtil.SampleSpeciation(10);
                        break;
                    case DisplayType.ReproductiveRecords:
                        displayType = DisplayType.None;
                        break;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.BackQuote) && displayType == DisplayType.ReproductiveRecords)
        {
            speciationSample = ReproductionUtil.SampleSpeciation(100);
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
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            distanceToTarget -= Input.GetAxisRaw("Vertical");
        }

        if (distanceToTarget < 0) distanceToTarget = 0;

        transform.LookAt(target.transform.position);
        var currentDistanceToTarget = Vector3.Distance(target.transform.position, transform.position);

        if (currentDistanceToTarget > distanceToTarget)
        {
            var speed = 1.01f;

            if (currentDistanceToTarget > distanceToTarget * 1.3) speed = 20;
            var moveAmount = new Vector3(0, 0, 1).normalized * target.currentSpeed * Time.deltaTime * speed;

            transform.Translate(moveAmount, Space.Self);
        }
    }

    private void HandleTranslucence()
    {
        if (Input.GetKey(KeyCode.Minus) && treeMaterial.color.a > 0.01)
        {
            treeMaterial.color = new Color(treeMaterial.color.r, treeMaterial.color.g, treeMaterial.color.b, treeMaterial.color.a - 0.01f);
        }
        if (Input.GetKey(KeyCode.Equals) && treeMaterial.color.a < 1)
        {
            treeMaterial.color = new Color(treeMaterial.color.r, treeMaterial.color.g, treeMaterial.color.b, treeMaterial.color.a + 0.01f);
        }
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
            height -= (int) ground.scale;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            height += (int) ground.scale;
        }

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), height, Input.GetAxisRaw("Vertical"));
        Vector3 direction = input.normalized;
        Vector3 velocity = direction * amount;

        transform.Translate(velocity, Space.Self);

        var groundHeight = LocationUtil.GetHeight(transform.position, ground) + 3;

        if (transform.position.y < groundHeight) transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);
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

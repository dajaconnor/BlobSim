using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{

    public Material perceptionMaterial;
    public BlobBehavior target;
    public int fruitSpawnRate = 4;

    int amount = 1;
    float distanceToTarget = 10;

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


        if (target != null)
        {
            var timespan = DateTime.Now - target.birthday;
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
                target.size + " size",
                target.perceptionDepth + " perceptionDepth",
                target.perceptionWidth + " perceptionWidth",
                target.perceptionShift + " perceptionShift",
                target.speed + " speed",
                target.jogModifier + " jogModifier",
                target.runModifier + " runModifier",
                target.randomRotation + " randomRotation",
                target.fearOfPredator + " fearOfPredator",
                target.wantForPrey + " wantForPrey",
                target.incubationTicks + " incubationTicks",
                target.reproductionLimit + " reproductionLimit",
                target.childen + " children sired"
            };

            for (var i = 0; i < thingsToSay.Count; i++)
            {
                GUI.Label(new Rect(10, 12 * i + 10, 200, 20), thingsToSay[i]);
            }
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

        if (Input.GetKey(KeyCode.Escape))
        {
            target = null;
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
            transform.Rotate(new Vector3(0, amount, 0));
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(new Vector3(0, -amount, 0));
        }

        if (Input.GetKey(KeyCode.R))
        {
            transform.Rotate(new Vector3(amount, 0, 0));
        }

        if (Input.GetKey(KeyCode.F))
        {
            transform.Rotate(new Vector3(-amount, 0, 0));
        }

        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }
}

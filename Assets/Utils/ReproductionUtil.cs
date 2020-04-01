using Assets.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Utils
{
    public static class ReproductionUtil
    {
        private static float geneticDrift = 0.1f;
        public static float yConstant = 0.47f;

        public static void ReproduceBlob(BlobBehavior mother, GameObject gameObject)
        {
            BlobBehavior newBlob = gameObject.GetComponent<BlobBehavior>();
            newBlob.generation = mother.generation + 1;

            newBlob.Start();

            newBlob.transform.position = new Vector3(mother.transform.position.x, yConstant, mother.transform.position.z);

            newBlob.size = SetNewBlobSize(MeOrMate(mother), gameObject);
            RandomizeTraits(mother, newBlob);

            newBlob.rotationSpeed /= newBlob.size * newBlob.size;
            newBlob.ground = mother.ground;
            newBlob.blobPrefab = mother.blobPrefab;
            
            newBlob.energy = mother.incubatedEnergy;

            newBlob.places = (Vector2[])mother.places.Clone();
            newBlob.latestPlace = mother.latestPlace;



            newBlob.gender = RandomGender(mother);



            newBlob.name = newBlob.gender.ToString() + "Blob";
            newBlob.parent = mother;

            mother.incubatedEnergy = 0;
            mother.currentIncubation = 0;
            mother.children++;

            // Because of starting positions.  Don't judgde me.
            if (mother != mother.partner) mother.partner.children++;
            mother.energy /= 3;


            var stats = mother.ground.GetComponent<Statistics>();
            stats.BothGenderRecords(mother.partner);
            stats.UpdateSurvivalStatistics(mother);
            stats.UpdateAverages(newBlob, StatType.Birth);
        }

        private static void RandomizeTraits(BlobBehavior mother, BlobBehavior newBlob)
        {
            SetPerceptionFields(mother, newBlob);
            newBlob.randomRotation = MeOrMate(mother).randomRotation * GetDrift();
            newBlob.aggression = MeOrMate(mother).aggression * GetDrift();
            newBlob.speed = MeOrMate(mother).speed * GetDrift();
            newBlob.jogModifier = MeOrMate(mother).jogModifier * GetDrift();
            newBlob.runModifier = MeOrMate(mother).runModifier * GetDrift();
            newBlob.fearOfPredator = MeOrMate(mother).fearOfPredator * GetDrift();
            newBlob.wantForPrey = MeOrMate(mother).wantForPrey * GetDrift();
            newBlob.childGenderRatio = MeOrMate(mother).childGenderRatio * GetDrift();
            newBlob.incubationTicks = (int)(mother.incubationTicks * GetDrift());
            newBlob.reproductionLimit = (int)(mother.reproductionLimit * GetDrift());
            newBlob.useMemoryPercent = MeOrMate(mother).useMemoryPercent * GetDrift();
            newBlob.reserveEnergy = (int)(MeOrMate(mother).reserveEnergy * GetDrift());
            //newBlob.lifespan = (int)(MeOrMate(mother).lifespan * GetDrift());

            if (newBlob.gender.Equals(GenderType.Female)) newBlob.sexualMaturity = (int)(mother.sexualMaturity * GetDrift());
            else newBlob.sexualMaturity = (int)(mother.partner.sexualMaturity * GetDrift());
        }

        private static GenderType RandomGender(BlobBehavior mother)
        {
            if (Random.value < mother.childGenderRatio) return GenderType.Female;
            else return GenderType.Male;
        }

        private static BlobBehavior MeOrMate(BlobBehavior mother)
        {
            if (Random.value < 0.5f) return mother;
            else return mother.partner;
        }

        private static void SetPerceptionFields(BlobBehavior mother, BlobBehavior newBlob)
        {
            var parent = MeOrMate(mother);

            var perceptionTransform = newBlob.perception.gameObject.transform;

            newBlob.perceptionDepth = parent.perceptionDepth * GetDrift();
            newBlob.perceptionWidth = parent.perceptionWidth * GetDrift();

            // to keep from stagnating at 0
            newBlob.perceptionShift = parent.perceptionShift + (GetDrift() - 0.999f);

            perceptionTransform.localScale = new Vector3(newBlob.perceptionWidth, newBlob.perceptionWidth, newBlob.perceptionDepth);

            perceptionTransform.localPosition = new Vector3(0, 0, perceptionTransform.localScale.z * newBlob.perceptionShift);
        }

        private static float SetNewBlobSize(BlobBehavior parent, GameObject gameObject)
        {
            var sizeModifier = GetDrift();
            gameObject.transform.localScale = gameObject.transform.localScale * sizeModifier;
            return parent.size * sizeModifier;
        }

        private static float GetDrift()
        {
            var mutationValue = Random.value;
            if (mutationValue > 0.01f) return Random.Range(1 - geneticDrift, 1 + geneticDrift);
            if (mutationValue > 0.001f) return Random.Range(1 - geneticDrift * 10, 1 + geneticDrift * 10);
            return Random.Range(1 - geneticDrift * 100, 1 + geneticDrift * 100);
        }
    }
}

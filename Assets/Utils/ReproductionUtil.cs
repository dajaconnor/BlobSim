using Assets.Enums;
using UnitSimulation.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Utils
{
    public static class ReproductionUtil
    {
        private static float geneticDrift = 0.1f;

        public static void ReproduceUnit(UnitBehavior mother, GameObject gameObject, MapGenerator map)
        {
            UnitBehavior newUnit = gameObject.GetComponent<UnitBehavior>();
            newUnit.generation = mother.generation + 1;

            newUnit.Start();

            newUnit.transform.position = new Vector3(mother.transform.position.x, LocationUtil.GetHeight(mother.transform.position, map) + UnitBehavior.heightAdjust, mother.transform.position.z);

            newUnit.size = SetNewUnitSize(MeOrMate(mother), gameObject);

            // compensate for size of baby
            mother.energy += (int) (100000 * (mother.size - newUnit.size));

            RandomizeTraits(mother, newUnit);

            newUnit.rotationSpeed /= newUnit.size * newUnit.size;
            newUnit.ground = mother.ground;
            newUnit.blobPrefab = mother.blobPrefab;
            newUnit.places = (Vector2[]) mother.places.Clone();
            newUnit.latestPlace = mother.latestPlace;

            newUnit.energy = mother.incubatedEnergy;

            newUnit.gender = RandomGender(mother);

            newUnit.name = newUnit.gender.ToString() + "Unit";
            newUnit.parent = mother;

            mother.incubatedEnergy = 0;
            mother.currentIncubation = 0;
            mother.children++;

            // Because of starting positions.  Don't judgde me.
            if (mother != mother.currentPartner) mother.currentPartner.children++;
            mother.energy /= 3;


            var stats = mother.ground.GetComponent<Statistics>();
            stats.BothGenderRecords(mother.currentPartner);
            stats.BothGenderRecords(mother);
            stats.UpdateSurvivalStatistics(mother);
            stats.UpdateAverages(newUnit, StatType.Birth);
        }

        public static void GerminateTree(TreeGenes genes, Vector3 position, GameObject gameObject, GameObject fruitPrefab, MapGenerator map)
        {
            TreeBehavior newTree = gameObject.GetComponent<TreeBehavior>();
            newTree.fruitPrefab = fruitPrefab;
            newTree.transform.position = new Vector3(position.x, LocationUtil.GetHeight(position, map), position.z);
            newTree.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
            newTree.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            newTree.Start();

            newTree.growDropRatio = genes.growDropRatio * GetDrift();
            newTree.lifespan =  (int)(genes.lifespan * GetDrift());
            newTree.fastGrowTime = (int)(genes.fastGrowTime * GetDrift());
            newTree.mediumGrowTime = (int)(genes.mediumGrowTime * GetDrift());
            newTree.slowGrowTime = (int)(genes.slowGrowTime * GetDrift());
            newTree.fiberInFruit = genes.fiber * GetDrift();
            newTree.age = 0;
        }

        public static float SampleSpeciation(int sampleSize)
        {
            var allUnits = Object.FindObjectsOfType<UnitBehavior>();
            var sampleUnits = allUnits;

            if (sampleSize < allUnits.Length)
            {
                var random = new System.Random();

                sampleUnits = allUnits.OrderBy(x => random.Next()).Take(sampleSize).ToArray();
            }
            else
            {
                sampleSize = allUnits.Length;
            }


            var viableUnits = 0;

            foreach(var blob in allUnits)
            {
                foreach(var sampleUnit in sampleUnits)
                {
                    if (blob != sampleUnit && blob.IsSameSpecies(sampleUnit)) viableUnits++;
                }
            }

            return (float) viableUnits / (allUnits.Length * sampleSize);
        }

        private static void RandomizeTraits(UnitBehavior mother, UnitBehavior newUnit)
        {
            SetPerceptionFields(mother, newUnit);
            newUnit.randomRotation = MeOrMate(mother).randomRotation * GetDrift();
            newUnit.aggression = MeOrMate(mother).aggression * GetDrift();
            newUnit.speed = MeOrMate(mother).speed * GetDrift();
            newUnit.jogModifier = MeOrMate(mother).jogModifier * GetDrift();
            newUnit.runModifier = MeOrMate(mother).runModifier * GetDrift();
            newUnit.fearOfPredator = MeOrMate(mother).fearOfPredator * GetDrift();
            newUnit.wantForPrey = MeOrMate(mother).wantForPrey * GetDrift();
            newUnit.childGenderRatio = MeOrMate(mother).childGenderRatio * GetDrift();
            newUnit.incubationTicks = (int)(mother.incubationTicks * GetDrift());
            newUnit.reproductionLimit = (int)(mother.reproductionLimit * GetDrift());
            newUnit.useMemoryPercent = MeOrMate(mother).useMemoryPercent * GetDrift();
            newUnit.reserveEnergy = (int)(MeOrMate(mother).reserveEnergy * GetDrift());
            newUnit.jogRotationModifier = MeOrMate(mother).jogRotationModifier * GetDrift();
            newUnit.runRotationModifier = MeOrMate(mother).runRotationModifier * GetDrift();
            newUnit.carnivorous = MeOrMate(mother).carnivorous * GetDrift();
            newUnit.melee = MeOrMate(mother).melee * GetDrift();
            newUnit.armor = MeOrMate(mother).armor * GetDrift();

            newUnit.monogomy = SameSexParent(mother, newUnit.gender).monogomy * GetDrift();
            newUnit.isMonogomous = newUnit.monogomy > 0.5f;

            if (!newUnit.isMonogomous) newUnit.selectedPartners = new HashSet<UnitBehavior>();

            if (newUnit.gender.Equals(GenderType.Female)) newUnit.sexualMaturity = (int)(mother.sexualMaturity * GetDrift());
            else newUnit.sexualMaturity = (int)(mother.currentPartner.sexualMaturity * GetDrift());
        }

        private static GenderType RandomGender(UnitBehavior mother)
        {
            if (Random.value < mother.childGenderRatio) return GenderType.Female;
            else return GenderType.Male;
        }

        private static UnitBehavior MeOrMate(UnitBehavior mother)
        {
            if (Random.value < 0.5f) return mother;
            else return mother.currentPartner;
        }

        private static UnitBehavior SameSexParent(UnitBehavior mother, GenderType gender)
        {
            if (gender == GenderType.Female) return mother;
            else return mother.currentPartner;
        }

        private static void SetPerceptionFields(UnitBehavior mother, UnitBehavior newUnit)
        {
            var parent = MeOrMate(mother);

            var perceptionTransform = newUnit.perception.gameObject.transform;

            newUnit.perceptionDepth = parent.perceptionDepth * GetDrift();
            newUnit.perceptionWidth = parent.perceptionWidth * GetDrift();

            // to keep from stagnating at 0
            newUnit.perceptionShift = parent.perceptionShift + (GetDrift() - 0.999f);

            perceptionTransform.localScale = new Vector3(newUnit.perceptionWidth, newUnit.perceptionWidth, newUnit.perceptionDepth);

            perceptionTransform.localPosition = new Vector3(0, 0, perceptionTransform.localScale.z * newUnit.perceptionShift);
        }

        private static float SetNewUnitSize(UnitBehavior parent, GameObject gameObject)
        {
            var sizeModifier = GetDrift();
            gameObject.transform.localScale *= sizeModifier;
            return parent.size * sizeModifier;
        }

        private static float GetDrift()
        {
            var mutationValue = Random.value;
            if (mutationValue > 0.001f) return Random.Range(1 - geneticDrift, 1 + geneticDrift);
            return Random.Range((1 - geneticDrift) / 10, (1 - geneticDrift) * 10);
        }
    }
}

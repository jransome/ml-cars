using RansomeCorp.AI.Evolution;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistanceSensors : MonoBehaviour
{
    private CarSpecies species;
    private List<float> SensorAngles;

    public void Initialise(CarSpecies carSpecies)
    {
        species = carSpecies;
        SensorAngles = species.SensorAngles.ToList();
    }

    public List<double> CalculateNormalisedDistances()
    {
        return SensorAngles.ConvertAll(angle => CheckDistance(angle) / species.SensorDistance);
    }

    private double CheckDistance(float angle)
    {
        Vector3 direction = CalculateDirectionFromAngle(angle);
        bool didHitSomething = Physics.Raycast(transform.position, direction, out RaycastHit hit, species.SensorDistance, species.SensorLayerMask);
        return didHitSomething ? hit.distance : species.SensorDistance;
    }

    private Vector3 CalculateDirectionFromAngle(float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.up) * new Vector3(transform.forward.x, 0f, transform.forward.z);
    }

    private void Update()
    {
        if (!species.DrawSensors) return;
        SensorAngles.ForEach(a =>
        {
            Debug.DrawRay(transform.position, CalculateDirectionFromAngle(a) * species.SensorDistance, CheckDistance(a) < species.SensorDistance ? Color.red : Color.green);
        });
    }
}

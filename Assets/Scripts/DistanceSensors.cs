using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistanceSensors : MonoBehaviour
{
    private CarSpecies species;
    private List<Quaternion> SensorRotations;
    private Gradient feedbackGradient = new Gradient();

    public void Initialise(CarSpecies carSpecies)
    {
        species = carSpecies;
        SensorRotations = species.SensorAngles.Select(angle => Quaternion.AngleAxis(angle, Vector3.up)).ToList();
    }

    public List<double> CalculateNormalisedDistances()
    {
        return SensorRotations.ConvertAll(rotation => CheckDistance(rotation) / species.SensorDistance);
    }

    private double CheckDistance(Quaternion sensorRotation)
    {
        Vector3 direction = sensorRotation * new Vector3(transform.forward.x, 0f, transform.forward.z);
        bool didHitSomething = Physics.Raycast(transform.position, direction, out RaycastHit hit, species.SensorDistance, species.SensorLayerMask);
        return didHitSomething ? species.SensorDistance - hit.distance : 0;
    }

    private void Start()
    {
        var colourKeys = new GradientColorKey[2];
        colourKeys[0].color = Color.green;
        colourKeys[0].time = 0.0f;
        colourKeys[1].color = Color.red;
        colourKeys[1].time = 1.0f;

        var alphaKeys = new GradientAlphaKey[1];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;

        feedbackGradient.SetKeys(colourKeys, alphaKeys);
    }

    private void Update()
    {
        if (!species.DrawSensors) return;
        SensorRotations.ForEach(r =>
        {
            Vector3 dir = r * new Vector3(transform.forward.x, 0f, transform.forward.z);
            Debug.DrawRay(transform.position, dir * species.SensorDistance, feedbackGradient.Evaluate((float)CheckDistance(r) / species.SensorDistance));
        });
    }
}

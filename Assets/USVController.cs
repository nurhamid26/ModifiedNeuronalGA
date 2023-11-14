using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class USVController : MonoBehaviour
{
    private Vector3 initialPosition, initialRotation;
    private NeuralNetwork network;

    [Range(-1f,1f)]
    public float a,t;

    [Header("Evaluation")]
    public float timeStart = 0f;
    public float totalTravelled = 0f;
    public float overallFitness;

    [Header("Fitness")]
    public float distanceMultipler = 1.5f;
    public float avgSpeedMultiplier = 0.3f;
    public float sensorMultiplier = 0.2f;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private Vector3 lastPosition;
    private float avgSpeed;

    private float aSensor,bSensor,cSensor,dSensor,eSensor;

    private void Awake() {
        initialPosition = transform.position;
        initialRotation = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>();
    }

    public void ResetWithNetwork (NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    public void Reset() {

        timeStart = 0f;
        totalTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = initialPosition;
        overallFitness = 0f;
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
    }

    private void OnCollisionEnter (Collision collision) {
        End();
    }

    private void FixedUpdate() {

        InputSensors();
        lastPosition = transform.position;

        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor, dSensor, eSensor);

        MoveUSV(a,t);

        timeStart += Time.deltaTime;

        CalculateFitness();

    }

    private void End ()
    {
        GameObject.FindObjectOfType<GASetting>().End(overallFitness, network);
    }

    private void CalculateFitness() {

        totalTravelled += Vector3.Distance(transform.position,lastPosition);
        avgSpeed = totalTravelled /timeStart;

        //Modified Fitness Function based on partially exponential function and inverse time variable
        overallFitness = (Mathf.Pow((totalTravelled * distanceMultipler), 2) + Mathf.Pow((avgSpeed * avgSpeedMultiplier), 2) + Mathf.Pow(((((aSensor + bSensor + cSensor + dSensor + eSensor) / 5) * sensorMultiplier)), 2)) * ((1 / timeStart * 3));
        
        if (timeStart > 20 && overallFitness < 40) {  //agent is too weak
            End();
        }

        if (transform.position.y < -4f || transform.position.y >= 6f) { //agent is sink under the water surface
            End();
        }

        if (overallFitness >= 200000) { //agent is reaching the expected fitness value
            End();
        }   

    }

    private void InputSensors() {

        //5 input sensor configuration 
        Vector3 a = (Quaternion.Euler(0f, -45f, 0f) * transform.forward);
        Vector3 b = (Quaternion.Euler(0f, -22.5f, 0f) * transform.forward);
        Vector3 c = (Quaternion.Euler(0f, 0f, 0f) * transform.forward);
        Vector3 d = (Quaternion.Euler(0f, 22.5f, 0f) * transform.forward);
        Vector3 e = (Quaternion.Euler(0f, 45f, 0f) * transform.forward);

        Ray r = new Ray(transform.position,a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.blue);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.black);
        }

        r.direction = d;

        if (Physics.Raycast(r, out hit))
        {
            dSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.blue);
        }

        r.direction = e;

        if (Physics.Raycast(r, out hit))
        {
            eSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

    }

    private Vector3 inp;
    public void MoveUSV (float v, float h) {
        inp = Vector3.Lerp(Vector3.zero,new Vector3(0,0,v*11.4f),0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);
    }

}

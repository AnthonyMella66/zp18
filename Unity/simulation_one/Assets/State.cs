
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
/*
* Tyler Philips
*
*State objects will have a timer that can be controlled with StartState and StopState functions
*this time will be used to keep track of user progression
*/
public class State : MonoBehaviour
{


    // Update is called once per frame
    //void Update () {
    //}

    //global variables
    private Stopwatch timer;
    private float id;
    private string output;

    public State(float identity)
    {
        this.timer = new Stopwatch();
        this.id = identity;
        this.output = "";
    }

    public void StartState()
    {
        this.timer.Start();
    }

    public void EndState()
    {
        this.timer.Stop();
        this.output = "Time elapsed in State " + this.id + " = " + this.timer.Elapsed;
    }

    public string GetOutput()
    {
        return this.output;
    }

    //writing to an output file
    //string[] stateStats = { "Score Processing\n=================\n~Time measured through each game state~", state0.GetOutput(), state1.GetOutput(), state2.GetOutput(), state3.GetOutput() };
    //System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", stateStats);


}
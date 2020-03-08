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
public class State : MonoBehaviour {


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
        this.output = "State " + this.id + " : " + this.timer.Elapsed;
    }

    public string GetOutput()
    {
        return this.output;
    }

    //writing to an output file
    //string[] stateStats = { state1., "Second line", "Third line" };
    // WriteAllLines creates a file, writes a collection of strings to the file,
    // and then closes the file.  You do NOT need to call Flush() or Close().
    //System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", stateStats);

    
}
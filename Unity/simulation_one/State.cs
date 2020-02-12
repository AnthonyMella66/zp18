using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Tyler Philips
*
* State objects will have a timer that can be controlled with StartState and StopState functions
*this time will be used to keep track of user progression
*/
public class State
{

    //global variables

    private Stopwatch timer;
    private float id;

    private void StartState(float identity)
    {
        this.timer.Start();
        this.id = identity;

    }

    private string StopState()
    {
        this.timer.Stop();
        string output = "State " + this.id + " : " + stopwatch.Elapsed;
        return output;

    }

    //writing to an output file
    //string[] stateStats = { state1., "Second line", "Third line" };
    // WriteAllLines creates a file, writes a collection of strings to the file,
    // and then closes the file.  You do NOT need to call Flush() or Close().
    //System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", stateStats);


    ////////
}


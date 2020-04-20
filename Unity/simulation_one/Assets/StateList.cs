
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
/*
* Tyler Philips
*
*This class is used to create a list of State objects as found in State.cs
* in the initialization of a StateList, the number of states (n) that will exist needs to be taken as a parameter
* the StateList will then create `n`State objects for the user.
*/
public class StateList : MonoBehaviour
{

    //global variables
    private List<State> list = new List<State>();
    private count;


    public StateList(float n)
    {

        var temp = new List<State>();

        //filling list with n State variables
        for (int i = 0; i < n; i++)
        {
            temp.Add(new State(i));
        }

        //initializing variables
        this.count = n;
        this.list = temp;
    }


    public float Average()
    {
        float average = 0;


        //calculate
       // for (int i = 0; i < this.count; i++)
        //{
            
        //}

        return average;

    }

}






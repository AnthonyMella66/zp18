﻿using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * McDSL: VR Simulation One
 * Aaska Shah
 * Dec 22 2018
 *
 * Configuration file parser. Config
 * file is indent-based, with keywords,
 * values, and attributes. This parsing
 * is done at execution-time.
 *
 * If parsing is successful, then the SimManager
 * can access the needed parameters here, as well.
 */
public class ConfigParser
{

    // Parsing
    private string configFilePath;
    private string dbConnection;
    private int dayCount;                               //total number of days in the config file, used for parseConfigs()
    private List<string> dayList = new List<string>();  //contains all day information ready to be split into dayConfigs
    private List<string> sim = new List<string>();      //contains all info about the simulation itself

    // Simulation parameters
    private DayConfiguration[] dayConfigs = null;
    private bool lowNauseaMode;
    private bool claustrophobicMode;
    private bool instructionsEnabled;
    private bool soundEnabled;

    // Day 0 Scoring / impairments - Nolan April 2019
    public List<Impairment> dayZeroImpairments = new List <Impairment> ();
    public static float DAY_0_DEFAULT_SCORE = 150.0f;  
    private float dayZeroScoreUnimpaired = 0.0f;        // How much the participant is required to earn before moving on to either the first paid day, or the impaired training period
    private float dayZeroScoreImpaired = 0.0f;          // Same, but in the impaired training period (if one is requested)
    public static int UNIMPAIRED = 0;   // For fetching day 0 score threshold (unimpaired)
    public static int IMPAIRED = 1;     // Same, but for the impaired threshold

    private float watervalue;
    private string name;
    private string[] output;
    private string description;
    private string instructions;
    private string sound;
    private string scene;

    /*
    * If not using a config file - use these default values (for testing)
    *
    * Structure (3 days):
    * - Day one: no impairment, 1 minute, 2* multiplier
    * - Day two: speed penalty (pay 50 / wait 15s), 2 minutes
    * - Day three: speed penalty, fog (fog @ 80%, pay 100 / wait 25s), 2 minutes
    */

    public ConfigParser()
    {

        // Metrics
        // TODO ... db stuff here
        dbConnection = null;

        // Data comes from static class, set by the main menu
        lowNauseaMode = ParticipantData.nauseaSensitive;
        claustrophobicMode = ParticipantData.claustrophicSensitive;
        instructionsEnabled = true;

        /*
        // Treatment argument ordering:
        float c_C,             // Cost function
        float c_a,
        float c_b,
        float c_c,

        float w_C,             // Wait function
        float w_a,
        float w_b,
        float w_c,

        float effProb,         // Probability that treatment will actually work
        float eff,             // The percentage (0.0 to 1.0) that the treatment will take away, if it is effective
        float delProb,         // Delay Penalty
        float del,             // "
        float deathProb        // Death Probability
        // ... Additional parameters here
        */

        // Day One
        Debug.Log("Setting Day One");
        Impairment[] dayOneImps = new Impairment[0];
        Treatment dayOneTreatment = null;   // TODO
        float dayOneDur = 120.0f;
        float dayOneMult = 2.0f;
        int dayOne = 1;

        // Day Two
        Debug.Log("Setting Day Two");
        float dayTwoDur = 120.0f;
        Impairment[] dayTwoImps = new Impairment[1];
        Treatment dayTwoTreatment = null;
        dayTwoImps[0] = new Impairment(Impairment.ImpairmentType.PHYSICAL_SHAKE, 0.6f);
        int dayTwo = 2;

        // Day Three
        Debug.Log("Setting Day Three");
        float dayThreeDur = 120.0f;
        Impairment[] dayThreeImps = new Impairment[1];
        Treatment dayThreeTreatment = new Treatment(
            100.0f,
            0.15f,
            3.0f,
            120.0f,
            100.0f,
            0.15f,
            3.0f,
            120.0f,
            1.0f,
            1.0f,
            0.0f,
            0.0f,
            0.0f
        );

        dayThreeImps[0] = new Impairment(Impairment.ImpairmentType.PHYSICAL_SHAKE, 0.6f);
        int dayThree = 3;


        // Set each day configuration with the above
        Debug.Log("Finalizing setup...");
        this.dayConfigs = new DayConfiguration[3];
        this.dayConfigs[0] = new DayConfiguration(dayOne, dayOneDur, dayOneImps, dayOneTreatment, dayOneMult);
        this.dayConfigs[1] = new DayConfiguration(dayTwo, dayTwoDur, dayTwoImps, dayTwoTreatment);
        this.dayConfigs[2] = new DayConfiguration(dayThree, dayThreeDur, dayThreeImps, dayThreeTreatment);
        Debug.Log("Config set up complete.");
    }


    /*
    * If using a configuration file, then load in day configurations
    * by parsing in the file if the path argument is valid.
    */
    public ConfigParser(string path)
    {
        lowNauseaMode = ParticipantData.nauseaSensitive;
        claustrophobicMode = ParticipantData.claustrophicSensitive;

        this.configFilePath = path;
        dbConnection = null;
        StreamReader reader = new StreamReader(this.configFilePath);
        string line = reader.ReadLine();
        char[] delimiter = { '\n', ':', '\t', '#', ' ', '%' }; //splitting a string requires chars therefore ConfigKeyword class cannot be used

        this.dayCount = 0;
        bool isSim = false;
        bool isDay = false;
        bool isTut = false;
        while (line != null)
        {
            string[] fields = line.Split(delimiter[0]);

            //splits information into 2 groups, Simulation and Day
            for (int i = 0; i < fields.Length; i++)
            {

                if (fields[i].Contains("#"))
                {
                    string[] split = fields[i].Split(delimiter[3]);
                    fields[i] = split[0];
                }
                if (fields[i] == string.Empty)
                {
                    continue;
                }
                else if (fields[i].Contains(ConfigKeyword.SIMULATION))
                {
                    isSim = true;
                    isDay = false;
                    isTut = false;
                    this.sim.Add(fields[i]);
                }
                else if (fields[i].Contains(ConfigKeyword.TUTORIAL))
                {
                    isTut = true;
                    isSim = false;
                    isDay = false;

                }
                // currently the day group is further split into it's separate lines for better parsing
                else if (fields[i].Contains(ConfigKeyword.DAY))
                {
                    this.dayCount++;
                    dayList.Add("Day " + dayCount.ToString());                    
                    isDay = true;
                    isTut = false;
                    isSim = false;
                }


                if ((fields[i].Contains(ConfigKeyword.INDENT)) && isSim)
                {
                    string simParam = fields[i].Replace(ConfigKeyword.INDENT, "");
                    this.sim.Add(simParam);
                }


                /* ****************************************************************************************************************************** */
                // Nolan April 2019
                // Fixing this whole section because this is really hacky. They could write any keyword instead of score and it would still work.
                else if ((fields[i].Contains(ConfigKeyword.INDENT)) && isTut)
                {
                    // Unimpaired portion of day 0 (standard tutorial round)
                    if (fields[i].ToUpper().Contains(ConfigKeyword.SCORE.ToUpper()) && !(fields[i].ToUpper().Contains(ConfigKeyword.IMP_SCORE.ToUpper()))) {

                        string score = fields[i].Replace(ConfigKeyword.INDENT, "");
                        this.dayZeroScoreUnimpaired = float.Parse(score.Split(delimiter[1])[1]);
                    }

                    // Impaired tutorial stage of day 0
                    else if (fields[i].ToUpper().Contains(ConfigKeyword.IMP_SCORE.ToUpper())) {
                        string score = fields[i].Replace(ConfigKeyword.INDENT, "");
                        this.dayZeroScoreImpaired = float.Parse(score.Split(delimiter[1])[1]);
                    }

                    // Parse a second-round day zero impairment
                    else if (fields[i].ToUpper().Contains(ConfigKeyword.IMPAIRMENT.ToUpper())) {

                        int trackInf = 0; int infiniteLoop = 100;

                        Impairment newDayZeroImp = new Impairment ();

                        line = reader.ReadLine();

                        while (true) {
                            
                            trackInf++;

                            // Set impairment type
                            if (line.ToUpper().Contains(ConfigKeyword.TYPE.ToUpper())) {

                                if (line.ToUpper().Contains(ConfigKeyword.FOG_IMP.ToUpper())) {
                                    newDayZeroImp.setType(Impairment.ImpairmentType.VISUAL_FOG);
                                }

                                else if (line.ToUpper().Contains(ConfigKeyword.SHAKE_IMP.ToUpper())) {
                                    newDayZeroImp.setType(Impairment.ImpairmentType.PHYSICAL_SHAKE);
                                }

                                else {
                                    Debug.Log("UNSUPPORTED IMPAIRMENT TYPE FOR DAY ZERO: " + fields[i]);
                                }


                                if (newDayZeroImp.getStrength() == -99.0f) {
                                    line = reader.ReadLine();
                                }

                                else {

                                    if (newDayZeroImp.getType() == Impairment.ImpairmentType.NULL || newDayZeroImp.getStrength() == -99.0f) {
                                        Debug.Log("Problem parsing day 0 impairment - not enough details for impairment.");
                                    }

                                    else {
                                        dayZeroImpairments.Add(newDayZeroImp);
                                        Debug.Log("New impairment for day 0 with type: " + newDayZeroImp.getType().ToString() + " and strength: " + newDayZeroImp.getStrength().ToString());
                                    }

                                    break; // We're done
                                }
                            }

                            // Set impairment strength
                            else if (line.ToUpper().Contains(ConfigKeyword.STRENGTH.ToUpper())) {

                                newDayZeroImp.setStrength (
                                    float.Parse(line.Split(':')[1].Split('#')[0].Replace("%", "").Trim()) / 100.0f
                                );

                                if (newDayZeroImp.getType() == Impairment.ImpairmentType.NULL) {
                                    line = reader.ReadLine();
                                }

                                else {

                                    if (newDayZeroImp.getType() == Impairment.ImpairmentType.NULL || newDayZeroImp.getStrength() == -99.0f) {
                                        Debug.Log("Problem parsing day 0 impairment - not enough details for impairment.");
                                    }

                                    else {
                                        dayZeroImpairments.Add(newDayZeroImp);
                                        Debug.Log("New impairment for day 0 with type: " + newDayZeroImp.getType().ToString() + " and strength: " + newDayZeroImp.getStrength().ToString());
                                    }

                                    break; // We're done
                                }
                            }

                            if (trackInf >= infiniteLoop) { Debug.Log("Problem parsing day 0 impairment - infinite loop."); break; }
                        }
                    }

                    else
                    {
                        Debug.Log ("Unexpected line during tutorial parsing: \"" + fields[i] + "\"");
                    }
                }
                /* ****************************************************************************************************************************** */

                else if ((fields[i].Contains(ConfigKeyword.INDENT)) && isDay)
                {
                    dayList.Add(fields[i].Replace(ConfigKeyword.INDENT, ""));

                }

            }

            line = reader.ReadLine();
        }

        parseSim();
        parseConfig();

    }

    private bool parseSim()
    {
        
        char[] delimiter = { '\n', ':', '\t', '#', ' ', '%', ',' };
        foreach (string i in this.sim)
        {
            if (i.Contains(ConfigKeyword.NAME))
            {
                string[] split = i.Split(delimiter[1]);
                this.name = split[1];
            }
            else if (i.Contains(ConfigKeyword.OUTPUT))
            {
                string[] split = i.Split(delimiter[1]);
                this.output = split[1].Split(delimiter[6]);
            }
            else if (i.Contains(ConfigKeyword.DESCRIPTION))
            {
                string[] split = i.Split(delimiter[1]);
                this.description = split[1];
            }
            else if (i.Contains(ConfigKeyword.INSTRUCTIONS))
            {
                string[] split = i.Split(delimiter[1]);
                this.instructions = split[1].ToLower();
                if (this.instructions.Contains("disabled"))
                {
                    this.instructionsEnabled = false;
                }
                else
                {
                    this.instructionsEnabled = true;
                }
            }
            else if (i.Contains(ConfigKeyword.SOUND))
            {
                string[] split = i.Split(delimiter[1]);
                this.sound = split[1].ToLower();
                if (this.sound.Contains("disabled"))
                {
                    this.soundEnabled = false;
                }
                else
                {
                    this.soundEnabled = true;
                }

            }
            else if (i.Contains(ConfigKeyword.SCENE))
            {
                string[] split = i.Split(delimiter[1]);
                this.scene = split[1];
            }
        }



        return true;
    }

    /*
	* Parse the file and create objects for each parsed item
    */
    private bool parseConfig()
    {

        try
        {
            this.dayConfigs = new DayConfiguration[this.dayCount];
            int day = 0, trackDays = -1, impair = -1;
            bool isImp = false;
            bool isTreat = false;
            bool isWait = false;
            bool isCost = false;
            bool isEff = false;
            float dur = 0.00f, wait = 0.00f, certainty = 1.00f, strength = 0.00f;
            float cost_C = 0.00f, cost_a = 0.00f, cost_b = 0.00f, cost_c = 0.00f, wait_C = 0.00f, wait_a = 0.00f, wait_b = 0.00f, wait_c = 0.00f;
            float watervalue = 1.00f, probability = 1.00f, effect = 1.00f;
            char[] delimiter = { '\n', ':', '\t', '#', ' ', '%' };
            List<Impairment> helperArray = new List<Impairment>();

            //    foreach (string i in this.dayList)
            for (int i = 0; i < this.dayList.Count; i++)
            {
                if (this.dayList[i].Contains(ConfigKeyword.DAY))
                {
                    string[] split = this.dayList[i].Split(delimiter[4]);
                    day = int.Parse(split[split.Length - 1]);
                    trackDays++;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.DURATION))
                {
                    string time = this.dayList[i].Split(new char[] { ':' }, 2)[1];
                    string[] split = time.Split(delimiter[1]);
                    dur = float.Parse(split[0]) * 60 + float.Parse(split[1]);

                }
                else if (this.dayList[i].Contains(ConfigKeyword.WATERVALUE))
                {
                    string[] split = this.dayList[i].Split(delimiter[1]);
                    watervalue = float.Parse(split[1]);

                }
                else if (this.dayList[i].Contains(ConfigKeyword.IMPAIRMENT))
                {
                    isImp = true; 
                    isTreat = false;
                    isWait = false;
                    isCost = false;
                    isEff = false;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.TREATMENT))
                {
                    isTreat = true;
                    isImp = false;
                    isWait = false;
                    isCost = false;
                    isEff = false;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.WAIT))
                {
                    isWait = true;
                    isImp = false;
                    isTreat = false;
                    isCost = false;
                    isEff = false;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.COST))
                {
                    isCost = true;
                    isTreat = false;
                    isImp = false;
                    isWait = false;
                    isEff = false;
                }
                else if (this.dayList[i].Contains(ConfigKeyword.EFFECTIVENESS))
                {            
                    isEff = true;
                    isTreat = false;
                    isImp = false;
                    isWait = false;
                    isCost = false;
                }
                if (isImp)
                {
                    if ((this.dayList[i].ToLower()).Contains("fog"))
                    {
                        impair = 0;
                    }
                    else if ((this.dayList[i].ToLower()).Contains("gravity"))
                    {
                        impair = 1;
                    }
                    else if ((this.dayList[i].ToLower()).Contains("shake"))
                    {
                        impair = 2;
                    }
                    else if ((this.dayList[i].ToLower()).Contains("speed"))
                    {
                        impair = 3;
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.STRENGTH))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        string[] splitPercent = split[1].Split(delimiter[5]);
                        if((float.Parse(splitPercent[0])) != 0)
                        {
                            strength = float.Parse(splitPercent[0]) / 100;
                        }         
                        else
                        {
                            strength = 0.00f;
                        }
                        

                    }

                }
                else if (isTreat)
                {
                     if (this.dayList[i].Contains(ConfigKeyword.CERTAINTY))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        string[] splitPercent = split[1].Split(delimiter[5]);
                        if((float.Parse(splitPercent[0])) != 0)
                        {
                            certainty = float.Parse(splitPercent[0]) / 100;
                        }         
                        else
                        {
                            certainty = 0.00f;
                        }

                    }


                }

                else if (isWait)
                {
                    if (this.dayList[i].Contains(ConfigKeyword.C))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        wait_C = float.Parse(split[1]);
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.a) && this.dayList[i].Contains(ConfigKeyword.WAIT) == false && this.dayList[i].Contains(ConfigKeyword.c) == false && this.dayList[i].Contains(ConfigKeyword.b) == false)
                    {
                        if ((this.dayList[i].ToLower()).Contains("default"))
                        {
                            wait_a = 1.00f /(dur / 60.0f);

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            wait_a = float.Parse(split[1]);

                            //DataTable dt_wait_a = new DataTable ();
                            //wait_a = (float) dt_wait_a.Compute (split[1], "");
                            //Debug.Log("Evaluated wait_a: " + wait_a.ToString());
                        }

                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.b))
                    {
                        if ((this.dayList[i].ToLower()).Contains("default"))
                        {
                            wait_b = 2;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);

                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            wait_b = float.Parse(split[1]);

                            //DataTable dt_wait_b = new DataTable ();
                            //wait_b = (float) dt_wait_b.Compute (split[1], "");
                            //Debug.Log("Evaluated wait_b: " + wait_b.ToString());
                        }
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.c))
                    {
                        if ((this.dayList[i].ToLower()).Contains("default"))
                        {
                            wait_c = dur / 60.0f;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            wait_c = float.Parse(split[1]);

                            //DataTable dt_wait_c = new DataTable ();
                            //wait_c = (float) dt_wait_c.Compute (split[1], "");
                            //Debug.Log("Evaluated wait_c: " + wait_c.ToString());
                        }
                    }
                }
                else if (isCost)
                {

                    if (this.dayList[i].Contains(ConfigKeyword.C) && this.dayList[i].Contains(ConfigKeyword.COST) == false)
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        cost_C = float.Parse(split[1]);
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.a) && this.dayList[i].Contains(ConfigKeyword.c) == false && this.dayList[i].Contains(ConfigKeyword.b) == false)
                    {
                        if ((this.dayList[i].ToLower()).Contains("default"))
                        {
                            cost_a = 1.00f / (dur / 60.0f);
                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            cost_a = float.Parse(split[1]);

                            //DataTable dt_cost_a = new DataTable ();
                            //cost_a = (float) dt_cost_a.Compute (split[1], "");
                            //Debug.Log("Evaluated cost_a: " + cost_a.ToString());
                        }

                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.b))
                    {
                        if ((this.dayList[i].ToLower()).Contains("default"))
                        {
                            cost_b = 2;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);

                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            cost_b = float.Parse(split[1]);

                            //DataTable dt_cost_b = new DataTable ();
                            //cost_b = (float) dt_cost_b.Compute (split[1], "");
                            //Debug.Log("Evaluated cost_b: " + cost_b.ToString());
                        }
                    }
                    else if (this.dayList[i].Contains(ConfigKeyword.c) )
                    {
                        if ((this.dayList[i].ToLower()).Contains("default"))
                        {
                            cost_c = dur/60.0f;

                        }
                        else
                        {
                            string[] split = this.dayList[i].Split(delimiter[1]);
                            
                            // Updating to allow for expressions inside these fields, e.g. "1/15" instead of "0.066667"
                            cost_c = float.Parse(split[1]);
                            //DataTable dt_cost_c = new DataTable ();
                            //cost_c = (float) dt_cost_c.Compute (split[1], "");
                            //Debug.Log("Evaluated cost_c: " + cost_c.ToString());
                        }

                    }

                }
                else if (isEff)
                {
                    if (this.dayList[i].Contains(ConfigKeyword.PROBABILITY))
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        string[] splitPercent = split[1].Split(delimiter[5]);
                        if((float.Parse(splitPercent[0])) != 0)
                        {
                            probability = float.Parse(splitPercent[0]) / 100;
                        }         
                        else
                        {
                            probability = 0.00f;
                        }
                    } 
                    else if (this.dayList[i].Contains(ConfigKeyword.EFFECT) && this.dayList[i].Contains(ConfigKeyword.EFFECTIVENESS) == false)
                    {
                        string[] split = this.dayList[i].Split(delimiter[1]);
                        string[] splitPercent = split[1].Split(delimiter[5]);
                        if((float.Parse(splitPercent[0])) != 0)
                        {
                            effect = float.Parse(splitPercent[0]) / 100;
                        }         
                        else
                        {
                            effect = 0.00f;
                        }
                        
                    }
                }

                if ((i + 1) == this.dayList.Count || (trackDays >= 0 && this.dayList[i + 1].Contains(ConfigKeyword.DAY) == true))
                {
                    Impairment impairObj;
                    Impairment[] dayImpairs;
                    Treatment dayTreats;


                    if (impair == -1)
                    {
                        dayImpairs = new Impairment[0];
                    }
                    else
                    {
                        impairObj = new Impairment((Impairment.ImpairmentType)impair, strength);

                        helperArray.Add(impairObj);

                        dayImpairs = helperArray.ToArray();
                    }

                    if (cost_C != 0.00f && wait_C != 0)
                    {
                        dayTreats = new Treatment(
                          cost_C,
                          cost_a,
                          cost_b,
                          cost_c,
                          wait_C,
                          wait_a,
                          wait_b,
                          wait_c,
                          probability,
                          effect,
                          0.0f,
                          0.0f,
                          0.0f);
                    }
                    else if(cost_C == 0.00f && cost_a == 0.00f && cost_b == 0.00f && cost_c == 0.00f && wait_C != 0.00f)
                    {
                        dayTreats = new Treatment(
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          wait_C,
                          wait_a,
                          wait_b,
                          wait_c,
                          probability,
                          effect,
                          0.0f,
                          0.0f,
                          0.0f);

                    }
                    else if (cost_C != 0.00f && wait_C == 0.00f && wait_a == 0.00f && wait_b == 0.00f && wait_c == 0.00f)
                    {
                        dayTreats = new Treatment(
                          cost_C,
                          cost_a,
                          cost_b,
                          cost_c,
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          Treatment.NONE,
                          probability,
                          effect,
                          0.0f,
                          0.0f,
                          0.0f);

                    }
                    else 
                    {
                        dayTreats = null;
                    }




                    this.dayConfigs[trackDays] = new DayConfiguration(day, dur, dayImpairs, dayTreats, watervalue);
                    isImp = false;
                    isTreat = false;
                    isCost = false;
                    isWait = false;
                    isEff = false;
                    impair = -1;
                    helperArray.Clear();
                    dur = 0.00f; watervalue = 1.00f; wait = 0.00f; certainty = 1.00f; strength = 0.00f; probability = 1.00f; effect = 1.00f ;
                    cost_C = 0.00f; cost_a = 0.00f; cost_b = 0.00f; cost_c = 0.00f; wait_C = 0.00f; wait_a = 0.00f; wait_b = 0.00f; wait_c = 0.00f;
                }




            }
            return true;
        }

        catch (UnauthorizedAccessException uae)
        {
            Debug.Log("Parsing exception: UnauthorizedAccessException");
            return false;
        }

        catch (System.IO.DirectoryNotFoundException dnfe)
        {
            Debug.Log("Parsing exception: DirectoryNotFoundException");
            return false;
        }

        catch (System.IO.IOException ioe)
        {
            Debug.Log("Parsing exception: IOException");
            return false;
        }

        catch (Exception e)
        {
            Debug.Log("Parsing exception: Exception");
            return false;
        }

        return false;
    }


    /*
    * Take measures to reduce nausea, e.g. put curtains up on windows
    */
    public bool lowNauseaModeEnabled()
    {
        return this.lowNauseaMode;
    }


    public bool claustrophobicModeEnabled()  {
        return this.claustrophobicMode;
    }


    /*
    * Returns all configurations; either parsed from file or default
    */
    public DayConfiguration[] getConfigs()
    {
        return this.dayConfigs;
    }


    public string getSimName()
    {
        return this.name;
    }


    public string[] getSimOutput()
    {
        return this.output;
    }


    public string getSimDescription()
    {
        return this.description;
    }


    public bool getSimInstruction()
    {
        return instructionsEnabled;
    }


    public bool getSimSound()
    {
        return soundEnabled;
    }


    public string getSimScene()
    {
        return this.scene;
    }


    public float getWaterValue()
    {
        return this.watervalue;
    }


    /*
    * Nolan April 2019
    * Updating this method to return an array instead of a single score
    * The array is indexed for impaired and unimpaired day 0 thresholds
    * so that a second round of training can be used if desired.
    */
    public float [] getDayZeroScore()
    {
        if(this.dayZeroScoreUnimpaired == 0)
            this.dayZeroScoreUnimpaired = DAY_0_DEFAULT_SCORE;

        if (this.dayZeroScoreImpaired == 0) 
            this.dayZeroScoreImpaired = DAY_0_DEFAULT_SCORE;

        float [] r      = new float[2];
        r[IMPAIRED]     = dayZeroScoreImpaired;
        r[UNIMPAIRED]   = dayZeroScoreUnimpaired;

        return r;
    }


    /*
    * Fetches list of all impairments active during the second round
    * of day 0 (impaired training)
    */
    public List<Impairment> getDayZeroImpairments () {
        return this.dayZeroImpairments;
    }


    public string dbConn()
    {
        return this.dbConnection;
    }


    public int numDays()
    {
        if (this.dayConfigs != null)
        {
            return this.dayConfigs.Length;
        }
        else
        {
            return -1;
        }
    }
}

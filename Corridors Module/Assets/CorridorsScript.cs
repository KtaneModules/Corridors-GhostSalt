using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class CorridorsScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMColorblindMode Colourblind;
    public KMSelectable[] Buttons;
    public TextMesh Display;
    public TextMesh ColourblindText;
    public GameObject ColourblindDevice;

    private int[,] Grid = { 
        { 8, 7, 9, 2, 5, 0, 3, 4, 1, 6 }, 
        { 3, 4, 1, 6, 8, 7, 9, 2, 5, 0 }, 
        { 7, 9, 2, 5, 0, 3, 4, 1, 6, 8 }, 
        { 1, 6, 8, 7, 9, 2, 5, 0, 3, 4 }, 
        { 2, 5, 0, 3, 4, 1, 6, 8, 7, 9 }, 
        { 0, 3, 4, 1, 6, 8, 7, 9, 2, 5 }, 
        { 9, 2, 5, 0, 3, 4, 1, 6, 8, 7 }, 
        { 5, 0, 3, 4, 1, 6, 8, 7, 9, 2 }, 
        { 4, 1, 6, 8, 7, 9, 2, 5, 0, 3 }, 
        { 6, 8, 7, 9, 2, 5, 0, 3, 4, 1 }
    };
    private int[][] Corridors = {
        new int[] { 1, 0, 1, 2, 1, 0, 1, 1 },
        new int[] { 0, 0, 1, 2, 2, 1, 1 },
        new int[] { 2, 2, 2, 1, 1, 0, 1, 1 },
        new int[] { 1, 1, 0, 0, 1, 2, 1 },
        new int[] { 0, 1, 2, 1, 2, 1, 0, 1 },
        new int[] { 2, 1, 0, 0, 1, 1, 1 },
        new int[] { 1, 2, 1, 1, 2, 1, 0, 1 },
        new int[] { 0, 1, 0, 1, 2, 1, 1 },
        new int[] { 2, 2, 2, 1, 0, 1, 0, 1 },
        new int[] { 1, 0, 1, 2, 2, 1, 1 }
    };
    private int CurrentMove;
    private int CurrentNumber;
    private int CurrentColour;
    private int CompletedStages;
    private string[] ColourNames = { "red", "orange", "yellow", "green", "blue", "magenta", "white" };
    private bool Solved;
    private bool ColourRuleApplied;
    private bool ColourblindEnabled;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        ColourblindEnabled = Colourblind.ColorblindModeActive;
        if (!ColourblindEnabled)
            ColourblindDevice.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            int x = i;
            Buttons[i].OnInteract += delegate { ButtonPress(x); return false; };
        }
        Calculate();
    }

    void Calculate()
    {
        CurrentNumber = Rnd.Range(0, 99);
        CurrentColour = Rnd.Range(0, 7);
        Display.text = CurrentNumber.ToString("00");
        switch (CurrentColour)
        {
            case 0:
                Display.color = new Color(1, 0, 0);
                ColourblindText.color = new Color(1, 0, 0);
                ColourblindText.text = "R";
                break;
            case 1:
                Display.color = new Color(1, 0.5f, 0);
                ColourblindText.color = new Color(1, 0.5f, 0);
                ColourblindText.text = "O";
                break;
            case 2:
                Display.color = new Color(1, 1, 0);
                ColourblindText.color = new Color(1, 1, 0);
                ColourblindText.text = "Y";
                break;
            case 3:
                Display.color = new Color(0, 1, 0);
                ColourblindText.color = new Color(0, 1, 0);
                ColourblindText.text = "G";
                break;
            case 4:
                Display.color = new Color(0, 0, 1);
                ColourblindText.color = new Color(0, 0, 1);
                ColourblindText.text = "B";
                break;
            case 5:
                Display.color = new Color(1, 0, 1);
                ColourblindText.color = new Color(1, 0, 1);
                ColourblindText.text = "M";
                break;
            default:
                Display.color = new Color(1, 1, 1);
                ColourblindText.color = new Color(1, 1, 1);
                ColourblindText.text = "W";
                break;
        }
        Debug.LogFormat("[Corridors #{0}] The device displays {1}, coloured {2}.",_moduleID, CurrentNumber.ToString("00"), ColourNames[CurrentColour]);
        Debug.LogFormat("[Corridors #{0}] Ignoring the colour rule, the current corridor is {1}.", _moduleID, Corridors[Grid[int.Parse(CurrentNumber.ToString("00")[0].ToString()), int.Parse(CurrentNumber.ToString("00")[1].ToString())]].Join(", ").Replace('0', 'L').Replace('1', 'F').Replace('2', 'R'));
    }

    private void ButtonPress(int pos) //Warning: this code sucks.
    {
        Buttons[pos].AddInteractionPunch();
        if (!Solved)
        {
            Audio.PlaySoundAtTransform("move", Buttons[pos].transform);
        }
        StartCoroutine(ButtonMove(pos));
        if (!Solved)
        {
            if (pos == Corridors[Grid[int.Parse(CurrentNumber.ToString("00")[0].ToString()), int.Parse(CurrentNumber.ToString("00")[1].ToString())]][CurrentMove])
            {
                if (!(Grid[int.Parse(CurrentNumber.ToString("00")[0].ToString()), int.Parse(CurrentNumber.ToString("00")[1].ToString())] % 2 == 0 && CurrentMove == 7) && !(Grid[int.Parse(CurrentNumber.ToString("00")[0].ToString()), int.Parse(CurrentNumber.ToString("00")[1].ToString())] % 2 == 1 && CurrentMove == 6))
                {
                    if (((CurrentMove == 0 && CurrentColour == 0) || (CurrentMove == 1 && CurrentColour == 1) || (CurrentMove == 2 && CurrentColour == 2) || (CurrentMove == 3 && CurrentColour == 3) || (CurrentMove == 4 && CurrentColour == 4) || (CurrentMove == 5 && CurrentColour == 5) || (CurrentMove == 6 && CurrentColour == 6)) && !ColourRuleApplied)
                    {
                        ColourRuleApplied = true;
                        Debug.LogFormat("[Corridors #{0}] The colour rule has been applied correctly.", _moduleID);
                    }
                    else
                    {
                        CurrentMove++;
                        switch (pos)
                        {
                            case 0:
                                Debug.LogFormat("[Corridors #{0}] You went left correctly.", _moduleID);
                                break;
                            case 1:
                                Debug.LogFormat("[Corridors #{0}] You went forward correctly.", _moduleID);
                                break;
                            default:
                                Debug.LogFormat("[Corridors #{0}] You went right correctly.", _moduleID);
                                break;
                        }
                    }
                    Display.text = Rnd.Range(0, 100).ToString("00");
                    switch (Rnd.Range(0, 7))
                    {
                        case 0:
                            Display.color = new Color(1, 0, 0);
                            ColourblindText.color = new Color(1, 0, 0);
                            ColourblindText.text = "R";
                            break;
                        case 1:
                            Display.color = new Color(1, 0.5f, 0);
                            ColourblindText.color = new Color(1, 0.5f, 0);
                            ColourblindText.text = "O";
                            break;
                        case 2:
                            Display.color = new Color(1, 1, 0);
                            ColourblindText.color = new Color(1, 1, 0);
                            ColourblindText.text = "Y";
                            break;
                        case 3:
                            Display.color = new Color(0, 1, 0);
                            ColourblindText.color = new Color(0, 1, 0);
                            ColourblindText.text = "G";
                            break;
                        case 4:
                            Display.color = new Color(0, 0, 1);
                            ColourblindText.color = new Color(0, 0, 1);
                            ColourblindText.text = "B";
                            break;
                        case 5:
                            Display.color = new Color(1, 0, 1);
                            ColourblindText.color = new Color(1, 0, 1);
                            ColourblindText.text = "M";
                            break;
                        default:
                            Display.color = new Color(1, 1, 1);
                            ColourblindText.color = new Color(1, 1, 1);
                            ColourblindText.text = "W";
                            break;
                    }
                }
                else
                {
                    if (((CurrentMove == 0 && CurrentColour == 0) || (CurrentMove == 1 && CurrentColour == 1) || (CurrentMove == 2 && CurrentColour == 2) || (CurrentMove == 3 && CurrentColour == 3) || (CurrentMove == 4 && CurrentColour == 4) || (CurrentMove == 5 && CurrentColour == 5) || (CurrentMove == 6 && CurrentColour == 6)) && !ColourRuleApplied)
                    {
                        ColourRuleApplied = true;
                        Debug.LogFormat("[Corridors #{0}] The colour rule has been applied correctly.", _moduleID);
                    }
                    else
                    {
                        switch (pos)
                        {
                            case 0:
                                Debug.LogFormat("[Corridors #{0}] You went left correctly.", _moduleID);
                                break;
                            case 1:
                                Debug.LogFormat("[Corridors #{0}] You went forward correctly.", _moduleID);
                                break;
                            default:
                                Debug.LogFormat("[Corridors #{0}] You went right correctly.", _moduleID);
                                break;
                        }
                        CurrentMove = 0;
                        CompletedStages++;
                        ColourRuleApplied = false;
                        if (CompletedStages == 3)
                        {
                            Module.HandlePass();
                            Debug.LogFormat("[Corridors #{0}] Three corridors have been correctly traversed. Module solved!", _moduleID);
                            Audio.PlaySoundAtTransform("solve", Display.transform);
                            Display.text = "";
                            ColourblindText.text = "";
                            Solved = true;
                        }
                        else
                        {
                            if (CompletedStages == 1)
                                Debug.LogFormat("[Corridors #{0}] You have correctly traversed a corridor. There are two more corridors.", _moduleID);
                            else
                                Debug.LogFormat("[Corridors #{0}] You have correctly traversed a corridor. There is one more corridor.", _moduleID);
                            Calculate();
                        }
                    }
                }
            }
            else
            {
                Module.HandleStrike();
                switch (pos)
                {
                    case 0:
                        Debug.LogFormat("[Corridors #{0}] You went left incorrectly. Strike!", _moduleID);
                        break;
                    case 1:
                        Debug.LogFormat("[Corridors #{0}] You went forward incorrectly. Strike!", _moduleID);
                        break;
                    default:
                        Debug.LogFormat("[Corridors #{0}] You went right incorrectly. Strike!", _moduleID);
                        break;
                }
                ColourRuleApplied = false;
                CurrentMove = 0;
                Calculate();
            }
        }
    }

    private IEnumerator ButtonMove(int pos)
    {
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition -= new Vector3(0, 0.002f, 0);
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            Buttons[pos].transform.localPosition += new Vector3(0, 0.002f, 0);
            yield return null;
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} l f r' to move left, then forward, then right. Use '!{0} colo(u)rblind' to toggle the colourblind device.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] CommandArray = command.Split(' ');
        string[] ValidCommands = { "l", "f", "r" };
        for (int i = 0; i < CommandArray.Length; i++)
        {
            if (!ValidCommands.Contains(CommandArray[i]) && command != "colourblind" && command != "colorblind")
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
            else if (command == "colourblind" || command == "colorblind")
            {
                yield return null;
                if (ColourblindEnabled)
                {
                    ColourblindEnabled = false;
                    ColourblindDevice.SetActive(false);
                }
                else
                {
                    ColourblindEnabled = true;
                    ColourblindDevice.SetActive(true);
                }
                break;
            }
            else
            {
                yield return null;
                if (CommandArray[i] == "l")
                    Buttons[0].OnInteract();
                else if (CommandArray[i] == "f")
                    Buttons[1].OnInteract();
                else
                    Buttons[2].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!Solved)
        {
            switch(Corridors[Grid[int.Parse(CurrentNumber.ToString("00")[0].ToString()), int.Parse(CurrentNumber.ToString("00")[1].ToString())]][CurrentMove])
            {
                case 0:
                    Buttons[0].OnInteract();
                    break;
                case 1:
                    Buttons[1].OnInteract();
                    break;
                default:
                    Buttons[2].OnInteract();
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    // You can't go back. Don't look back. It will kill you.
}

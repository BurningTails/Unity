using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StateController : MonoBehaviour
{

    public GameObject transitionBlock;
    public GameObject positionBlock;
    public GameObject dropdownNameBlock;

    public Text transitionText;
    public Dropdown positionDropdown;

    private string transitionName;
    private List<int> positionsMarkersCount = new List<int>();
    private List<string> positionsNames = new List<string>();


    public void outlineState(Color inputColor)
    {
        transitionBlock.GetComponent<SpriteRenderer>().color = inputColor;
        positionBlock.GetComponent<SpriteRenderer>().color = inputColor;
        dropdownNameBlock.GetComponent<SpriteRenderer>().color = inputColor;
    }


    public void setTransitionName(int indexTransition)
    {
        if(SkynetController.transitionsBase.Count > 0)
        {
            if (indexTransition == -1)
            {
                transitionName = "Start";
            }
            else
            {
                transitionName = SkynetController.transitionsBase[indexTransition].GetComponent<TransitionController>().transitionName;
            }

            transitionText.text = transitionName;
        }
    }


    public void setPositions(List<int> positions)
    {
        if (SkynetController.positionsBase.Count > 0)
        {
            if (positions.Count > 0)
            {
                for (int i = 0; i < SkynetController.positionsBase.Count; i++) 
                {
                    positionsNames.Add(SkynetController.positionsBase[i].GetComponent<PositionController>().positionName + " = " + positions[i]);
                }
                positionDropdown.AddOptions(positionsNames);
                positionsMarkersCount = new List<int>(positions);
            }
        }
    }


    private void OnMouseDown()
    {
        if (SkynetController.positionsBase.Count > 0)
        {
            if (positionsMarkersCount.Count > 0)
            {
                for (int i = 0; i < SkynetController.positionsBase.Count; i++)
                {
                    SkynetController.positionsBase[i].GetComponent<PositionController>().SetMarker(positionsMarkersCount[i]);
                }
            }
        }
    }


}

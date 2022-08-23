using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class TransitionController : MonoBehaviour
{
    public List<GameObject> LeftConnectors;
    public List<GameObject> RightConnectors;

    public GameObject leftConnectPlace;
    public GameObject rightConnectPlace;

    public GameObject transitionNameBlock;
    public InputField transitionNameInputField;

    public string transitionName = string.Empty;

    public Color standartColor;
    public Color outlineColor;

    private Vector3 startMousePosition;
    private Vector3 endMousePosition;
    private bool isAddSelected = false;
    private bool isRemoveSelected = false;


    public void BiasCoordinates(Vector3 inputCoordinates)
    {
        GetComponent<Transform>().position += inputCoordinates;

        if (LeftConnectors.Count > 0)
        {
            foreach (var connector in LeftConnectors)
            {
                connector.GetComponent<ConnectorController>().UpdatePositionConnector();
            }
        }
        if (RightConnectors.Count > 0)
        {
            foreach (var connector in RightConnectors)
            {
                connector.GetComponent<ConnectorController>().UpdatePositionConnector();
            }
        }
    }


    public void DeleteTransition()
    {
        if (LeftConnectors.Count > 0)
        {
            foreach (var connector in LeftConnectors)
            {
                connector.GetComponent<ConnectorController>().DeleteConnector(gameObject);
            }
        }
        if (RightConnectors.Count > 0)
        {
            foreach (var connector in RightConnectors)
            {
                connector.GetComponent<ConnectorController>().DeleteConnector(gameObject);
            }
        }

        if (SkynetController.transitionsBase.Contains(gameObject))
        {
            SkynetController.transitionsBase.Remove(gameObject);
        }

        Destroy(gameObject);
    }



    public void UpdateOutline(bool isOutline)
    {
        if (isOutline)
        {
            GetComponent<SpriteRenderer>().color = outlineColor;
            leftConnectPlace.GetComponent<SpriteRenderer>().color = outlineColor;
            rightConnectPlace.GetComponent<SpriteRenderer>().color = outlineColor;
            transitionNameBlock.GetComponent<SpriteRenderer>().color = outlineColor;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = standartColor;
            leftConnectPlace.GetComponent<SpriteRenderer>().color = standartColor;
            rightConnectPlace.GetComponent<SpriteRenderer>().color = standartColor;
            transitionNameBlock.GetComponent<SpriteRenderer>().color = standartColor;
        }
    }


    public void UpdateOutline(Color inputColor)
    {
        GetComponent<SpriteRenderer>().color = inputColor;
        leftConnectPlace.GetComponent<SpriteRenderer>().color = inputColor;
        rightConnectPlace.GetComponent<SpriteRenderer>().color = inputColor;
        transitionNameBlock.GetComponent<SpriteRenderer>().color = inputColor;
    }


    public void SetTransitionNameAutomatic()
    {
        transitionName = "T" + SkynetController.uniqueIDTransition.ToString();
        transitionNameInputField.text = transitionName;
    }

    public void SetTransitionNameFromField(InputField inputField)
    {
        if (inputField.text == string.Empty)
        {
            inputField.ActivateInputField();
        }
        else
        {
            transitionName = inputField.text;
            transitionNameInputField.text = transitionName;
        }
    }


    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!SkynetController.isPositionCreated && !SkynetController.isTransitionCreated && !SkynetController.isDragSelectedObjects && !SkynetController.isModifyMarkers && !SkynetController.isVisualiseGraph)
            {
                if (SkynetController.isConnectorCreated)
                {
                    Vector3 leftConnectPos = leftConnectPlace.GetComponent<Transform>().position;
                    Vector3 rightConnectPos = rightConnectPlace.GetComponent<Transform>().position;
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0;
                    float scaleSize = Mathf.Abs(leftConnectPlace.GetComponent<Transform>().lossyScale.x);

                    ref GameObject tempLeftConnect = ref SkynetController.tempConnector.GetComponent<ConnectorController>().LeftConnect;
                    ref GameObject tempRightConnect = ref SkynetController.tempConnector.GetComponent<ConnectorController>().RightConnect;


                    if (mousePos.x - leftConnectPos.x < leftConnectPlace.GetComponent<SpriteRenderer>().size.x * scaleSize && mousePos.x - leftConnectPos.x > 0)
                    {
                        if (tempRightConnect == null)
                        {
                            if (tempLeftConnect == null)
                            {
                                tempRightConnect = gameObject;
                            }
                            else
                            {
                                if (!tempLeftConnect.Equals(gameObject))
                                {
                                    if (!tempLeftConnect.TryGetComponent(out TransitionController tempTC))
                                    {
                                        tempRightConnect = gameObject;
                                    }
                                    else
                                    {
                                        SkynetController.globalRef.infoMessage("Select left side of Position");
                                    }
                                }
                                else
                                {
                                    SkynetController.globalRef.infoMessage("Select left side of Position");
                                }
                            }
                        }
                        else
                        {
                            if (tempRightConnect.TryGetComponent(out PositionController tempPC))
                            {
                                SkynetController.globalRef.infoMessage("Select right side of Transition");
                            }
                            else
                            {
                                SkynetController.globalRef.infoMessage("Select right side of Position");
                            }
                        }
                    }
                    else if (rightConnectPos.x - mousePos.x < rightConnectPlace.GetComponent<SpriteRenderer>().size.x * scaleSize && rightConnectPos.x - mousePos.x > 0)
                    {
                        if(tempLeftConnect == null)
                        {
                            if (tempRightConnect == null)
                            {
                                tempLeftConnect = gameObject;
                            }
                            else
                            {
                                if (!tempRightConnect.Equals(gameObject))
                                {
                                    if (!tempRightConnect.TryGetComponent(out TransitionController tempTC))
                                    {
                                        tempLeftConnect = gameObject;
                                    }
                                    else
                                    {
                                        SkynetController.globalRef.infoMessage("Select right side of Position");
                                    }
                                }
                                else
                                {
                                    SkynetController.globalRef.infoMessage("Select right side of Position");
                                }
                            }                          
                        }
                        else
                        {
                            if (tempLeftConnect.TryGetComponent(out PositionController tempPC))
                            {
                                SkynetController.globalRef.infoMessage("Select left side of Transition");
                            }
                            else
                            {
                                SkynetController.globalRef.infoMessage("Select left side of Position");
                            }
                        }
                    }
                }
                else
                {
                    if (isAddSelected)
                    {
                        if (!SkynetController.selectedTransitions.Contains(gameObject))
                        {
                            SkynetController.selectedTransitions.Add(gameObject);
                            UpdateOutline(true);
                        }
                    }
                    else if (isRemoveSelected)
                    {
                        if (SkynetController.selectedTransitions.Contains(gameObject))
                        {
                            SkynetController.selectedTransitions.Remove(gameObject);
                            UpdateOutline(false);
                        }
                    }
                    else
                    {
                        if (!SkynetController.selectedTransitions.Contains(gameObject))
                        {
                            SkynetController.globalRef.clearSelectedObjects();
                            SkynetController.selectedTransitions.Add(gameObject);
                            UpdateOutline(true);
                        }
                    }
                }
            }
        }
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isAddSelected = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isAddSelected = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRemoveSelected = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRemoveSelected = false;
        }
    }
}

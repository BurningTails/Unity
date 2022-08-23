using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class PositionController : MonoBehaviour
{
    public List<GameObject> LeftConnectors;
    public List<GameObject> RightConnectors;

    public GameObject positionBase;
    public GameObject singlePosition;
    public GameObject positionNameBlock;
    public Text positionCountText;
    public InputField positionNameInputField;

    public string positionName = string.Empty;

    public GameObject leftConnectPlace;
    public GameObject rightConnectPlace;

    public int numOfChips = 0;

    public Color standartColor;
    public Color outlineColor;

    private Vector3 startMousePosition;
    private Vector3 endMousePosition;
    private bool isAddSelected = false;
    private bool isRemoveSelected = false;



    public void AddMarker()
    {
        numOfChips++;
        UpdatePosition();
    }


    public void RemoveMarker()
    {
        numOfChips--;
        if (numOfChips < 0) numOfChips = 0;

        UpdatePosition();
    }


    public void SetMarker(int inputNum)
    {
        numOfChips = inputNum;
        UpdatePosition();
    }


    public void UpdatePosition()
    {
        if(numOfChips > 0 && !singlePosition.activeSelf)
        {
            singlePosition.SetActive(true);
            positionCountText.text = numOfChips.ToString();
        }
        else if(numOfChips == 0 && singlePosition.activeSelf)
        {
            singlePosition.SetActive(false);
            positionCountText.text = numOfChips.ToString();
        }
        else
        {
            positionCountText.text = numOfChips.ToString();
        }

    }

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

    public void DeletePosition(GameObject connector)
    {
        bool findConnector = false;

        if(LeftConnectors.Count > 0)
        {
            if(LeftConnectors.Contains(connector))
            {
                LeftConnectors.Remove(connector);
                findConnector = true;
            }
        }

        if(RightConnectors.Count > 0 && !findConnector)
        {
            if (RightConnectors.Contains(connector))
            {
                RightConnectors.Remove(connector);
            }
        }
    }

    public void DeletePosition()
    {
        if(LeftConnectors.Count > 0)
        {
            foreach(var connector in LeftConnectors)
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

        if(SkynetController.positionsBase.Contains(gameObject))
        {
            SkynetController.positionsBase.Remove(gameObject);
        }

        Destroy(gameObject);
    }




    public void SetPositionNameAutomatic()
    {
        positionName = "P" + SkynetController.uniqueIDPosition.ToString();
        positionNameInputField.text = positionName;
    }

    public void SetPositionNameFromField(InputField inputField)
    {
        if (inputField.text == string.Empty)
        {
            inputField.ActivateInputField();
        }
        else
        {
            positionName = inputField.text;
            positionNameInputField.text = positionName;
        }
    }




    public void UpdateOutline(bool isOutline)
    {
        if(isOutline)
        {
            positionBase.GetComponent<SpriteRenderer>().color = outlineColor;
            positionNameBlock.GetComponent<SpriteRenderer>().color = outlineColor;
            leftConnectPlace.GetComponent<SpriteRenderer>().color = outlineColor;
            rightConnectPlace.GetComponent<SpriteRenderer>().color = outlineColor;
        }
        else
        {
            positionBase.GetComponent<SpriteRenderer>().color = standartColor;
            positionNameBlock.GetComponent<SpriteRenderer>().color = standartColor;
            leftConnectPlace.GetComponent<SpriteRenderer>().color = standartColor;
            rightConnectPlace.GetComponent<SpriteRenderer>().color = standartColor;
        }
    }

    public void UpdateOutline(Color inputColor)
    {
        positionBase.GetComponent<SpriteRenderer>().color = inputColor;
        positionNameBlock.GetComponent<SpriteRenderer>().color = inputColor;
        leftConnectPlace.GetComponent<SpriteRenderer>().color = inputColor;
        rightConnectPlace.GetComponent<SpriteRenderer>().color = inputColor;
    }





    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!SkynetController.isPositionCreated && !SkynetController.isTransitionCreated && !SkynetController.isDragSelectedObjects && !SkynetController.isVisualiseGraph)
            {
                if (SkynetController.isConnectorCreated)
                {
                    Vector3 leftConnectPos = leftConnectPlace.GetComponent<Transform>().position;
                    Vector3 rightConnectPos = rightConnectPlace.GetComponent<Transform>().position;
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float scaleSize = Mathf.Abs(leftConnectPlace.GetComponent<Transform>().lossyScale.x);
                    mousePos.z = 0;

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
                                    if (!tempLeftConnect.TryGetComponent(out PositionController tempPC))
                                    {
                                        tempRightConnect = gameObject;
                                    }
                                    else
                                    {
                                        SkynetController.globalRef.infoMessage("Select left side of Transition");
                                    }
                                }
                                else
                                {
                                    SkynetController.globalRef.infoMessage("Select left side of Transition");
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
                        if (tempLeftConnect == null) 
                        {
                            if (tempRightConnect == null)
                            {
                                tempLeftConnect = gameObject;
                            }
                            else
                            {
                                if (!tempRightConnect.Equals(gameObject))
                                {
                                    if (!tempRightConnect.TryGetComponent(out PositionController tempPC))
                                    {
                                        tempLeftConnect = gameObject;
                                    }
                                    else
                                    {
                                        SkynetController.globalRef.infoMessage("Select right side of Transition");
                                    }
                                }
                                else
                                {
                                    SkynetController.globalRef.infoMessage("Select right side of Transition");
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
                else if(SkynetController.isModifyMarkers)
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                        AddMarker();
                    else
                        RemoveMarker();
                }
                else
                {
                    if (isAddSelected)
                    {
                        if (!SkynetController.selectedPositions.Contains(gameObject))
                        {
                            SkynetController.selectedPositions.Add(gameObject);
                            UpdateOutline(true);
                        }
                    }
                    else if (isRemoveSelected)
                    {
                        if (SkynetController.selectedPositions.Contains(gameObject))
                        {
                            SkynetController.selectedPositions.Remove(gameObject);
                            UpdateOutline(false);
                        }
                    }
                    else
                    {
                        if (!SkynetController.selectedPositions.Contains(gameObject))
                        {
                            SkynetController.globalRef.clearSelectedObjects();
                            SkynetController.selectedPositions.Add(gameObject);
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConnectorController : MonoBehaviour
{
    public GameObject LeftConnect;
    public GameObject RightConnect;

    public GameObject countConnectsBlock;
    public InputField countConnectsField;

    public int connectsCount = 0;

    public Material standartMaterial;
    public Material outlineMaterial;

    private bool isAddSelected = false;
    private bool isRemoveSelected = false;


    public void UpdatePositionConnector()
    {
        Vector3 leftGlobalConnectPlace;
        Vector3 rightGlobalConnectPlace;

        if (LeftConnect.TryGetComponent(out PositionController leftPosition))
        {
            leftGlobalConnectPlace = LeftConnect.GetComponent<PositionController>().rightConnectPlace.GetComponent<Transform>().position;
            rightGlobalConnectPlace = RightConnect.GetComponent<TransitionController>().leftConnectPlace.GetComponent<Transform>().position;
        }
        else
        {
            rightGlobalConnectPlace = RightConnect.GetComponent<PositionController>().leftConnectPlace.GetComponent<Transform>().position;
            leftGlobalConnectPlace = LeftConnect.GetComponent<TransitionController>().rightConnectPlace.GetComponent<Transform>().position;
        }

        DrawConnector(leftGlobalConnectPlace, rightGlobalConnectPlace);

        GetComponent<Transform>().position = (leftGlobalConnectPlace + rightGlobalConnectPlace) / 2.0f;
    }

    public void DrawConnector(Vector3 pos1, Vector3 pos2)
    {
        List<Vector3> resultPoints = new List<Vector3>();
        List<Vector2> resultPoints2D = new List<Vector2>();
        int tempBias = Mathf.CeilToInt((pos1 - pos2).magnitude / Camera.main.orthographicSize * 20);

        getSplinePoints(ref resultPoints, ref resultPoints2D, pos1, pos2, 0.8f, tempBias);

        GetComponent<LineRenderer>().positionCount = resultPoints.Count;
        GetComponent<LineRenderer>().SetPositions(resultPoints.ToArray());
        GetComponent<EdgeCollider2D>().SetPoints(resultPoints2D);
    }

    public void getSplinePoints(ref List<Vector3> resultPoints, ref List<Vector2> resultPoints2D, Vector3 point0, Vector3 point3, float coeffCurve, int countSteps)
    {
        resultPoints.Clear();
        resultPoints2D.Clear();

        float bias = Mathf.Abs(point3.x - point0.x) * coeffCurve;

        Vector3 point1 = new Vector3(point0.x + bias, point0.y, 0);
        Vector3 point2 = new Vector3(point3.x - bias, point3.y, 0);
        Vector3 tempPoint;

        Vector3 tempBias = (point0 + point3) / 2.0f;

        float stepCurve = 1.0f / countSteps;
        float t = 0.0f;

        for (int i = 0; i < countSteps + 1; i++) 
        {
            t = stepCurve * i;
            Vector3 coeff0 = Mathf.Pow(1 - t, 3) * point0;
            Vector3 coeff1 = 3 * t * Mathf.Pow(1 - t, 2) * point1;
            Vector3 coeff2 = 3 * Mathf.Pow(t, 2) * (1 - t) * point2;
            Vector3 coeff3 = Mathf.Pow(t, 3) * point3;

            tempPoint = coeff0 + coeff1 + coeff2 + coeff3;

            resultPoints.Add(tempPoint);
            resultPoints2D.Add(new Vector2(tempPoint.x - tempBias.x, tempPoint.y - tempBias.y));            
        }
    }




    public void DeleteConnector(GameObject fromObject)
    {
        if (LeftConnect.Equals(fromObject))
        {
            if (LeftConnect.TryGetComponent(out PositionController leftPosition))
            {
                TransitionController tempTC = RightConnect.GetComponent<TransitionController>();
                if(tempTC.LeftConnectors.Contains(gameObject))
                {
                    tempTC.LeftConnectors.Remove(gameObject);
                }
            }
            else
            {
                PositionController tempPC = RightConnect.GetComponent<PositionController>();
                if (tempPC.LeftConnectors.Contains(gameObject))
                {
                    tempPC.LeftConnectors.Remove(gameObject);
                }
            }
        }
        else
        {
            if (RightConnect.TryGetComponent(out PositionController leftPosition))
            {
                TransitionController tempTC = LeftConnect.GetComponent<TransitionController>();
                if (tempTC.RightConnectors.Contains(gameObject))
                {
                    tempTC.RightConnectors.Remove(gameObject);
                }
            }
            else
            {
                PositionController tempPC = LeftConnect.GetComponent<PositionController>();
                if (tempPC.RightConnectors.Contains(gameObject))
                {
                    tempPC.RightConnectors.Remove(gameObject);
                }
            }
        }


        if(SkynetController.connectorsBase.Contains(gameObject))
        {
            SkynetController.connectorsBase.Remove(gameObject);
        }

        Destroy(gameObject);
    }



    public void DeleteConnector()
    {
        if (LeftConnect != null)
        {
            if (LeftConnect.TryGetComponent(out PositionController leftPosition))
            {
                if (leftPosition.RightConnectors.Contains(gameObject))
                {
                    leftPosition.RightConnectors.Remove(gameObject);
                }
            }
            else
            {
                TransitionController tempTC = LeftConnect.GetComponent<TransitionController>();
                if (tempTC.RightConnectors.Contains(gameObject))
                {
                    tempTC.RightConnectors.Remove(gameObject);
                }
            }
        }
        if (RightConnect != null)
        {
            if (RightConnect.TryGetComponent(out PositionController rightPosition))
            {
                if (rightPosition.LeftConnectors.Contains(gameObject))
                {
                    rightPosition.LeftConnectors.Remove(gameObject);
                }
            }
            else
            {
                TransitionController tempTC = RightConnect.GetComponent<TransitionController>();
                if (tempTC.LeftConnectors.Contains(gameObject))
                {
                    tempTC.LeftConnectors.Remove(gameObject);
                }
            }
        }

        if (SkynetController.connectorsBase.Contains(gameObject))
        {
            SkynetController.connectorsBase.Remove(gameObject);
        }

        Destroy(gameObject);
    }


    public void UpdateOutline(bool isOutline)
    {
        if (isOutline)
        {
            GetComponent<LineRenderer>().material = outlineMaterial;
        }
        else
        {
            GetComponent<LineRenderer>().material = standartMaterial;
        }
    }


    public void SetConnectCountAutomatic()
    {
        connectsCount = 1;
        countConnectsField.text = connectsCount.ToString();
    }

    public void SetConnectCountFromField(InputField inputField)
    {
        if (inputField.text == string.Empty)
        {
            inputField.ActivateInputField();
        }
        else
        {
            if(int.TryParse(inputField.text, out connectsCount))
            {
                countConnectsField.text = connectsCount.ToString();
            }
            else
            {
                connectsCount = 0;
                inputField.ActivateInputField();
            }
        }
    }


    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!SkynetController.isPositionCreated && !SkynetController.isTransitionCreated && !SkynetController.isConnectorCreated && !SkynetController.isDragSelectedObjects && !SkynetController.isModifyMarkers && !SkynetController.isVisualiseGraph)
            {
                if (isAddSelected)
                {
                    if (!SkynetController.selectedConnectors.Contains(gameObject))
                    {
                        SkynetController.selectedConnectors.Add(gameObject);
                        UpdateOutline(true);
                    }
                }
                else if (isRemoveSelected)
                {
                    if (SkynetController.selectedConnectors.Contains(gameObject))
                    {
                        SkynetController.selectedConnectors.Remove(gameObject);
                        UpdateOutline(false);
                    }
                }
                else
                {
                    if (!SkynetController.selectedConnectors.Contains(gameObject))
                    {
                        SkynetController.globalRef.clearSelectedObjects();
                        SkynetController.selectedConnectors.Add(gameObject);
                        UpdateOutline(true);
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

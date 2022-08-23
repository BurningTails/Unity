using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointController : MonoBehaviour
{
    public List<GameObject> lines = new List<GameObject>();

    private bool isAddSelected = false;
    private bool isRemoveSelected = false;

    public bool ComparisonPossitionPoints(GameObject point_in)
    {
        return GetComponent<Transform>().position == point_in.GetComponent<Transform>().position;
    }

    public void AddLine(GameObject line_in)
    {
        lines.Add(line_in);
    }

    public void SetCoordinates(Vector3 inputCoordinates)
    {
        GetComponent<Transform>().position = inputCoordinates;
        if (lines.Count > 0)
        {
            foreach (var line in lines)
            {
                line.GetComponent<LineController>().SetCoordinates(inputCoordinates, this.gameObject);
            }
        }
    }

    public void BiasCoordinates(Vector3 inputCoordinates)
    {
        GetComponent<Transform>().position += inputCoordinates;
        if (lines.Count > 0)
        {
            foreach (var line in lines)
            {
                line.GetComponent<LineController>().BiasCoordinates(inputCoordinates, this.gameObject);
            }
        }
    }

    public void constrainCheck()
    {
        List<int> indexConstrain = new List<int>();

        if(SketchEditorController.constrainsBase.Count > 0)
            foreach(var constrain in SketchEditorController.constrainsBase)
            {
                int paramsCount = constrain.GetComponent<ConstrainController>().GetParamCount();
                for (int i = 0; i < paramsCount; i++)
                {
                    if(constrain.GetComponent<ConstrainController>().GetParam(i).point.Equals(this.gameObject))
                    {
                        indexConstrain.Add(SketchEditorController.constrainsBase.FindIndex(Elem => Elem.Equals(constrain)));
                        break;
                    }
                }
            }

        if(indexConstrain.Count > 0)
        {
            indexConstrain.Reverse();
            foreach(var index in indexConstrain)
            {
                SketchEditorController.constrainsBase[index].GetComponent<ConstrainController>().DeleteConstrain();
            }
        }
    }

    public void constrainDelete()
    {
        if(SketchEditorController.constrainsBase.Count > 0)
        {
            foreach (var constrain in SketchEditorController.constrainsBase)
            {
                int paramsCount = constrain.GetComponent<ConstrainController>().GetParamCount();
                for (int i = 0; i < paramsCount; i++)
                {
                    if(constrain.GetComponent<ConstrainController>().GetParam(i).point.Equals(gameObject))
                    {
                        constrain.GetComponent<ConstrainController>().DeleteConstrain();
                        break;
                    }
                }
            }
        }
    }

    public void SetOutlineFromCostrain(bool isHighlighted)
    {
        if(isHighlighted)
        {
            GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
        }
        else
        {
            if(lines.Count > 0)
            {
                foreach(var line in lines)
                {
                    line.GetComponent<LineController>().SetOutlineFromCostrain(false);
                }
            }
            GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColor;
        }
    }

    public void DeletePoint(GameObject line)
    {
        if(lines.Count <= 1)
        {
            SketchEditorController.pointsBase.Remove(gameObject);
            constrainCheck();
            Destroy(gameObject);
        }
        if(lines.Count > 1)
        {
            lines.Remove(line);
        }
    }

    public void DeletePoint()
    {
        if (lines.Count > 0)
        {
            foreach (var line in lines)
            {
                line.GetComponent<LineController>().DeleteLine(gameObject);
            }
        }
        SketchEditorController.pointsBase.Remove(gameObject);
        constrainCheck();
        Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!SketchEditorController.isLineCreating && !SketchEditorController.isPointCreating && !SketchEditorController.isDragSelectedObjects)
            {
                SketchEditorController.isPointSelected = true;

                if (!SketchEditorController.isConstrainCreating)
                {
                    if (SketchEditorController.selectedConstrains.Count > 0)
                    {
                        SketchEditorController.clearSelectedConstrains();
                    }

                    if (isAddSelected)
                    {
                        int index = SketchEditorController.selectedPoints.FindIndex(Elem => Elem.Equals(this.gameObject));

                        if (index == -1)
                        {
                            SketchEditorController.selectedPoints.Add(this.gameObject);
                            GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColor;
                        }
                    }
                    else if (isRemoveSelected)
                    {
                        int index = SketchEditorController.selectedPoints.FindIndex(Elem => Elem.Equals(this.gameObject));

                        if (index != -1)
                        {
                            SketchEditorController.selectedPoints.Remove(this.gameObject);
                            GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColor;
                        }
                    }
                    else
                    {
                        int index = SketchEditorController.selectedPoints.FindIndex(Elem => Elem.Equals(this.gameObject));

                        if (index == -1)
                        {
                            SketchEditorController.clearSelectedObjects();

                            SketchEditorController.selectedPoints.Add(this.gameObject);
                            GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColor;
                        }
                    }

                }
                else
                {
                    SketchEditorController.createdCostrain.GetComponent<ConstrainController>().AddPrimitive(this.gameObject);
                }
            }
        }
    }

    private void OnMouseUp()
    {
        SketchEditorController.isPointSelected = false;
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


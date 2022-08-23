using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 



public class LineController : MonoBehaviour
{
    public List<GameObject> points = new List<GameObject>();
    public LineController() {}

    private bool isAddSelected = false;
    private bool isRemoveSelected = false;

    public bool ComparisonPossitionLines(GameObject line_in)
    {
        Vector3 start = line_in.GetComponent<LineRenderer>().GetPosition(0);
        Vector3 end = line_in.GetComponent<LineRenderer>().GetPosition(1);
        bool first_ckeck = GetComponent<LineRenderer>().GetPosition(0) == start && GetComponent<LineRenderer>().GetPosition(1) == end;
        bool second_ckeck = GetComponent<LineRenderer>().GetPosition(0) == end && GetComponent<LineRenderer>().GetPosition(1) == start;

        return first_ckeck || second_ckeck;
    }

    public void AddPoint(GameObject point)
    {
        points.Add(point);
    }

    public void SetCoordinates(Vector3 inputCoordinates, GameObject point)
    {
        List<Vector2> currentPoints = new List<Vector2>();

        int index = points.FindIndex(Elem => Elem.Equals(point));
        Vector3 oldPosition = GetComponent<LineRenderer>().GetPosition(index);
        GetComponent<EdgeCollider2D>().GetPoints(currentPoints);
        GetComponent<LineRenderer>().SetPosition(index, inputCoordinates);


        for (int i = 0; i < 2; i++)
        {
            float tempX = points[i].GetComponent<Transform>().position.x;
            float tempY = points[i].GetComponent<Transform>().position.y;
            currentPoints[i] = new Vector2(tempX, tempY);
        }

        GetComponent<EdgeCollider2D>().SetPoints(currentPoints);
    }

    public void BiasCoordinates(Vector3 inputCoordinates, GameObject point)
    {
        List<Vector2> currentPoints = new List<Vector2>();

        int index = points.FindIndex(Elem => Elem.Equals(point));
        Vector3 oldPosition = GetComponent<LineRenderer>().GetPosition(index);        
        GetComponent<EdgeCollider2D>().GetPoints(currentPoints);


        for (int i = 0; i < 2; i++)
        {
            if (currentPoints[i].x == oldPosition.x && currentPoints[i].y == oldPosition.y)
            {
                float tempX = currentPoints[i].x + inputCoordinates.x;
                float tempY = currentPoints[i].y + inputCoordinates.y;
                currentPoints[i] = new Vector2(tempX, tempY);
               
                break;
            }
        }

        GetComponent<LineRenderer>().SetPosition(index, oldPosition + inputCoordinates);
        GetComponent<EdgeCollider2D>().SetPoints(currentPoints);
        currentPoints.Clear();
    }

    public void SetOutlineFromCostrain(bool isHighlighted)
    {
        if (isHighlighted)
        {
            GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterialConstrain;
        }
        else
        {
            GetComponent<LineRenderer>().material = SketchEditorController.staticStandartMaterial;
        }
    }

    public void DeleteLine(GameObject point_in)
    {
        if (points.Count > 0)
        {
            foreach (var point in points)
            {
                if (!point.Equals(point_in))
                {
                    point.GetComponent<PointController>().DeletePoint(gameObject);
                }
            }
        }
        SketchEditorController.linesBase.Remove(gameObject);
        Destroy(gameObject);
    }

    public void DeleteLine()
    {
        foreach (var point in points)
        {
            point.GetComponent<PointController>().DeletePoint(gameObject);
        }
        SketchEditorController.linesBase.Remove(gameObject);
        Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!SketchEditorController.isLineCreating && !SketchEditorController.isPointCreating && !SketchEditorController.isPointSelected && !SketchEditorController.isDragSelectedObjects)
            {
                if (!SketchEditorController.isConstrainCreating)
                {
                    if (SketchEditorController.selectedConstrains.Count > 0)
                    {
                        SketchEditorController.clearSelectedConstrains();
                    }

                    if (isAddSelected)
                    {
                        int index = SketchEditorController.selectedLines.FindIndex(Elem => Elem.Equals(this.gameObject));

                        if (index == -1)
                        {
                            SketchEditorController.selectedLines.Add(this.gameObject);
                            GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterial;
                        }
                    }
                    else if (isRemoveSelected)
                    {
                        int index = SketchEditorController.selectedLines.FindIndex(Elem => Elem.Equals(this.gameObject));

                        if (index != -1)
                        {
                            SketchEditorController.selectedLines.Remove(this.gameObject);
                            GetComponent<LineRenderer>().material = SketchEditorController.staticStandartMaterial;
                        }
                    }
                    else
                    {
                        int index = SketchEditorController.selectedLines.FindIndex(Elem => Elem.Equals(this.gameObject));

                        if (index == -1)
                        {
                            SketchEditorController.clearSelectedObjects();
                            SketchEditorController.selectedLines.Add(this.gameObject);
                            GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterial;
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            isAddSelected = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftControl))
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



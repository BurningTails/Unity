using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstrainController : MonoBehaviour
{
    public List<GameObject> points = new List<GameObject>();

    public Color standartColor;
    public Color outlineColor;
    public Material standartMaterial;
    public Material outlineMaterial;

    private bool isAddSelected = false;
    private bool isRemoveSelected = false;
    public static int createConstrainStep = 0;

    public virtual void InitConstrain() { }
    public virtual bool CreateConstrain() { return false; }
    public virtual void UpdatePositionConstrain() { }
    public virtual void UpdateOutline(bool isHighlighted) { }

    public void SetObjectsOutlineDefault()
    {
        if (points.Count > 0)
        {
            foreach (var point in points)
            {
                point.GetComponent<PointController>().SetOutlineFromCostrain(false);
            }
        }
    }

    public virtual int GetConstrainPriority() { return 0;  }

    public void DeleteConstrain()
    {
        int Index = SketchEditorController.constrainsBase.FindIndex(Elem => Elem.Equals(gameObject));
        if (Index != -1)
        {
            SketchEditorController.constrainsBase.Remove(gameObject);
        }
        SetObjectsOutlineDefault();
        Destroy(gameObject);
    }

    public virtual int GetEquationsCount() { return 0; }
    public virtual int GetParamCount() { return 0; }
    public virtual SketchEditorController.ConstrainParam GetParam(int index) { return new SketchEditorController.ConstrainParam(); }
    public virtual double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param) { return 0; }
    public virtual double GetEquationValue(int indexEquation) { return 0; }
    public virtual void AddPrimitive(GameObject primitive) { }

    private void OnMouseDown()
    {
        if (!SketchEditorController.isConstrainCreating && !SketchEditorController.isLineCreating && !SketchEditorController.isPointCreating && !SketchEditorController.isDragSelectedObjects)
        {
            if (SketchEditorController.selectedPoints.Count > 0 || SketchEditorController.selectedLines.Count > 0)
            {
                SketchEditorController.clearSelectedObjects();
            }
            if (isAddSelected)
            {
                int index = SketchEditorController.selectedConstrains.FindIndex(Elem => Elem.Equals(gameObject));

                if (index == -1)
                {
                    SketchEditorController.selectedConstrains.Add(gameObject);
                    UpdateOutline(true);
                }
            }
            else if (isRemoveSelected)
            {
                int index = SketchEditorController.selectedConstrains.FindIndex(Elem => Elem.Equals(gameObject));

                if (index != -1)
                {
                    SketchEditorController.selectedConstrains.Remove(gameObject);
                    UpdateOutline(false);
                }
            }
            else
            {
                int index = SketchEditorController.selectedConstrains.FindIndex(Elem => Elem.Equals(gameObject));

                if (index == -1)
                {
                    SketchEditorController.clearSelectedConstrains();

                    SketchEditorController.selectedConstrains.Add(gameObject);
                    UpdateOutline(true);
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


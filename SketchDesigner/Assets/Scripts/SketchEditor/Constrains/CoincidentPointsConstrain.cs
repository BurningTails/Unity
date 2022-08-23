using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoincidentPointsConstrain : ConstrainController
{

    public override bool CreateConstrain()
    {
        if (createConstrainStep == 0)
        {
            if (points.Count == 2)
                createConstrainStep++;
            else
                return false;
        }

        if (createConstrainStep == 1)
        {
            UpdatePositionConstrain();
            createConstrainStep = 0;
            return true;
        }

        return false;
    }


    public override void UpdatePositionConstrain()
    {
        Vector3 pos = points[0].GetComponent<Transform>().position;
        pos.y -= 0.5f;
        GetComponent<Transform>().position = pos;
    }


    public override void UpdateOutline(bool isHighlighted)
    {
        if (isHighlighted)
        {
            GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
        }
    }


    public override int GetEquationsCount()
    {
        return 2;
    }


    public override int GetParamCount()
    {
        return 4;
    }


    public override SketchEditorController.ConstrainParam GetParam(int index)
    {
        if (index == 0)
            return new SketchEditorController.ConstrainParam(points[0], true);
        if (index == 1)
            return new SketchEditorController.ConstrainParam(points[0], false);
        if (index == 2)
            return new SketchEditorController.ConstrainParam(points[1], true);
        return new SketchEditorController.ConstrainParam(points[1], false);
    }


    public override double GetEquationValue(int indexEquation)
    {
        if (indexEquation == 0)
            return points[0].GetComponent<Transform>().position.x - points[1].GetComponent<Transform>().position.x;
        return points[0].GetComponent<Transform>().position.y - points[1].GetComponent<Transform>().position.y;
    }


    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {

        if (indexEquation == 0 && param.isX && param.point.Equals(points[0]))
        {
            return 1;
        }
        if (indexEquation == 0 && param.isX && param.point.Equals(points[1]))
        {
            return -1;
        }
        if (indexEquation == 1 && !param.isX && param.point.Equals(points[0]))
        {
            return 1;
        }
        if (indexEquation == 1 && !param.isX && param.point.Equals(points[1]))
        {
            return -1;
        }

        return 0;
    }

    public override void AddPrimitive(GameObject primitive)
    {
        if (primitive.TryGetComponent(out PointController isPoint))
        {
            if (points.Count == 1)
            {
                if (!points.Contains(primitive))
                {
                    points.Add(primitive);
                    isPoint.SetOutlineFromCostrain(true);
                }
                else
                {
                    SketchEditorController.globalRef.infoMessage("Please, select another point!");
                }
            }
            else
            {
                points.Add(primitive);
                isPoint.SetOutlineFromCostrain(true);
            }
        }
        else
        {
            SketchEditorController.globalRef.infoMessage("Please, select point!");
        }
    }
}
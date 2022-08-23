using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalLineConstrain : ConstrainController
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
            SetObjectsOutlineDefault();
            createConstrainStep = 0;
            return true;
        }

        return false;
    }


    public override void UpdatePositionConstrain()
    {
        Vector3 pos1 = points[0].GetComponent<Transform>().position;
        Vector3 pos2 = points[1].GetComponent<Transform>().position;
        Vector3 result = (pos1 + pos2) / 2.0f;
        result.x += 0.5f;

        GetComponent<Transform>().position = result;
    }

    public override void UpdateOutline(bool isHighlighted)
    {
        if(isHighlighted)
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
        return 1;
    }

    public override int GetParamCount()
    {
        return 2;
    }


    public override SketchEditorController.ConstrainParam GetParam(int index)
    {
        if(index == 0)
            return new SketchEditorController.ConstrainParam(points[0], true);
        return new SketchEditorController.ConstrainParam(points[1], true);
    }

    public override double GetEquationValue(int indexEquation)
    {
        return points[0].GetComponent<Transform>().position.x - points[1].GetComponent<Transform>().position.x;
    }

    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {
        if (param.isX && param.point.Equals(points[0]))
        {
            return 1;
        }
        if (param.isX && param.point.Equals(points[1]))
        {
            return -1;
        }
        return 0;
    }

    public override void AddPrimitive(GameObject primitive)
    {
        if (primitive.TryGetComponent(out LineController isLine) && points.Count != 2)
        {
            isLine.SetOutlineFromCostrain(true);
            foreach (var point in isLine.points)
                points.Add(point);
        }
        else
        {
            SketchEditorController.globalRef.infoMessage("Please, select line!");
        }
    }

   
}

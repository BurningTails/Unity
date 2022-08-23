using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalLineConstrain : ConstrainController
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
        result.y -= 0.5f;

        GetComponent<Transform>().position = result;
        GetComponent<Transform>().rotation = Quaternion.Euler(0, 0, -90);
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
        return 1;
    }

    public override int GetParamCount()
    {
        return 2;
    }


    public override SketchEditorController.ConstrainParam GetParam(int index)
    {
        if (index == 0)
            return new SketchEditorController.ConstrainParam(points[0], false);
        return new SketchEditorController.ConstrainParam(points[1], false);
    }

    public override double GetEquationValue(int indexEquation)
    {
        return points[0].GetComponent<Transform>().position.y - points[1].GetComponent<Transform>().position.y;
    }

    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {
        if (!param.isX && param.point.Equals(points[0]))
        {
            return 1;
        }
        if (!param.isX && param.point.Equals(points[1]))
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
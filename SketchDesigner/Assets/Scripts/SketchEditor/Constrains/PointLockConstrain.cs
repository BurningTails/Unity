using System;
using System.Collections.Generic;
using UnityEngine;

public class PointLockConstrain : ConstrainController
{
    public float constPointX;
    public float constPointY;

    public override bool CreateConstrain()
    {
        if(createConstrainStep == 0)
        {
            if (points.Count == 1)
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
        Vector3 pos = points[0].GetComponent<Transform>().position;
        pos.x += 0.5f;
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
        return 2;
    }

    public override SketchEditorController.ConstrainParam GetParam(int index)
    {
        if (index == 0)
            return new SketchEditorController.ConstrainParam(points[0], true);
        return new SketchEditorController.ConstrainParam(points[0], false);
        //return new SketchEditorController.ConstrainParam();
    }

    public override double GetEquationValue(int indexEquation)
    {
        if (indexEquation == 0)
            return points[0].GetComponent<Transform>().position.x - constPointX;
        return points[0].GetComponent<Transform>().position.y - constPointY;
    }


    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {

        if (indexEquation == 0 && param.isX && param.point.Equals(points[0]))
        {
            return 1;
        }
        if (indexEquation == 1 && !param.isX && param.point.Equals(points[0]))
        {
            return 1;
        }

        return 0;
    }

    public override void AddPrimitive(GameObject primitive)
    {        
        if (primitive.TryGetComponent(out PointController isPoint))
        {
            isPoint.SetOutlineFromCostrain(true);
            points.Add(primitive);

            constPointX = points[0].GetComponent<Transform>().position.x;
            constPointY = points[0].GetComponent<Transform>().position.y;
        }
        else
        {
            SketchEditorController.globalRef.infoMessage("Please, select point!");
        }
    }


}

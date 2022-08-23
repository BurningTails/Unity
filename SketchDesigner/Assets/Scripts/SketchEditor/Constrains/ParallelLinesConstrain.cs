using System;
using System.Collections.Generic;
using UnityEngine;

public class ParallelLinesConstrain : ConstrainController
{
    public GameObject leftLine;
    public GameObject rightLine;


    public override bool CreateConstrain()
    {
        if (createConstrainStep == 0)
        {
            if (points.Count == 4)
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
        List<double> pointX = new List<double>();
        List<double> pointY = new List<double>();


        foreach (var point in points)
        {
            pointX.Add(point.GetComponent<Transform>().position.x);
            pointY.Add(point.GetComponent<Transform>().position.y);
        }

        double vectorX1 = pointX[1] - pointX[0];
        double vectorY1 = pointY[1] - pointY[0];

        double vectorX2 = pointX[3] - pointX[2];
        double vectorY2 = pointY[3] - pointY[2];



        Vector3 vectorCenterLeft = (points[0].GetComponent<Transform>().position + points[1].GetComponent<Transform>().position) / 2.0f;
        Vector3 vectorCenterRight = (points[2].GetComponent<Transform>().position + points[3].GetComponent<Transform>().position) / 2.0f;


        //Quaternion leftQuat = Quaternion.AngleAxis(Mathf.Rad2Deg * (float)Math.Atan2(vectorY2, vectorX2) - 90.0f, new Vector3(0, 0, 1));
        Quaternion quat = Quaternion.AngleAxis(Mathf.Rad2Deg * (float)Math.Atan2(vectorY1, vectorX1) - 90.0f, new Vector3(0, 0, 1));

        leftLine.GetComponent<Transform>().localPosition = vectorCenterLeft + quat * new Vector3(0.5f, 0, 0);
        rightLine.GetComponent<Transform>().localPosition = vectorCenterRight + quat * new Vector3(-0.5f, 0, 0);

        GetComponents<BoxCollider2D>()[0].offset = vectorCenterLeft + quat * new Vector3(0.5f, 0, 0);
        GetComponents<BoxCollider2D>()[1].offset = vectorCenterRight + quat * new Vector3(-0.5f, 0, 0);

        leftLine.GetComponent<Transform>().localRotation = quat;
        rightLine.GetComponent<Transform>().localRotation = quat;

        GetComponent<Transform>().position = new Vector3(0, 0, 0);

    }


    public override void UpdateOutline(bool isHighlighted)
    {
        if (isHighlighted)
        {
            leftLine.GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
            rightLine.GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
        }
        else
        {
            leftLine.GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
            rightLine.GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
        }
    }


    public override int GetEquationsCount()
    {
        return 1;
    }


    public override int GetParamCount()
    {
        return 8;
    }



    public override SketchEditorController.ConstrainParam GetParam(int index)
    {
        if (index == 0)
            return new SketchEditorController.ConstrainParam(points[0], true);
        if (index == 1)
            return new SketchEditorController.ConstrainParam(points[0], false);
        if (index == 2)
            return new SketchEditorController.ConstrainParam(points[1], true);
        if (index == 3)
            return new SketchEditorController.ConstrainParam(points[1], false);
        if (index == 4)
            return new SketchEditorController.ConstrainParam(points[2], true);
        if (index == 5)
            return new SketchEditorController.ConstrainParam(points[2], false);
        if (index == 6)
            return new SketchEditorController.ConstrainParam(points[3], true);
        return new SketchEditorController.ConstrainParam(points[3], false);
    }


    public override double GetEquationValue(int indexEquation)
    {
        if (indexEquation != 0)
            return 0;
        List<double> pointX = new List<double>();
        List<double> pointY = new List<double>();

        foreach (var point in points)
        {
            pointX.Add(point.GetComponent<Transform>().position.x);
            pointY.Add(point.GetComponent<Transform>().position.y);
        }

        return (pointX[1] - pointX[0]) * (pointY[3] - pointY[2]) - (pointX[3] - pointX[2]) * (pointY[1] - pointY[0]);
    }


    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {
        if (indexEquation != 0)
            return 0;

        List<double> pointX = new List<double>();
        List<double> pointY = new List<double>();

        foreach (var point in points)
        {
            pointX.Add(point.GetComponent<Transform>().position.x);
            pointY.Add(point.GetComponent<Transform>().position.y);
        }

        if (param.isX && param.point.Equals(points[0]))
            return pointY[2] - pointY[3];
        if (!param.isX && param.point.Equals(points[0]))
            return pointX[3] - pointX[2];
        if (param.isX && param.point.Equals(points[1]))
            return pointY[3] - pointY[2];
        if (!param.isX && param.point.Equals(points[1]))
            return pointX[2] - pointX[3];
        if (param.isX && param.point.Equals(points[2]))
            return pointY[1] - pointY[0];
        if (!param.isX && param.point.Equals(points[2]))
            return pointX[0] - pointX[1];
        if (param.isX && param.point.Equals(points[3]))
            return pointY[0] - pointY[1];
        if (!param.isX && param.point.Equals(points[3]))
            return pointX[1] - pointX[0];

        return 0;
    }


    public override void AddPrimitive(GameObject primitive)
    {
        if (primitive.TryGetComponent(out LineController isLine))
        {
            if (points.Count == 2)
            {
                if (!points.Contains(isLine.points[0]) || !points.Contains(isLine.points[1]))
                {
                    isLine.SetOutlineFromCostrain(true);
                    foreach (var point in isLine.points)
                        points.Add(point);
                }
                else
                {
                    SketchEditorController.globalRef.infoMessage("Please, select other line!");
                }

            }
            else
            {
                isLine.SetOutlineFromCostrain(true);
                foreach (var point in isLine.points)
                    points.Add(point);
            }
        }
        else
        {

            SketchEditorController.globalRef.infoMessage("Please, select line!");
        }
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

public class PerpendicularLinesConstrain : ConstrainController
{


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
            SetObjectsOutlineDefault();
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

        double Xcoeff1 = pointX[0] * vectorY1 * vectorX2;
        double Xcoeff2 = pointX[2] * vectorY2 * vectorX1;
        double Xcoeff3 = pointY[0] * vectorX1 * vectorX2;
        double Xcoeff4 = pointY[2] * vectorX1 * vectorX2;
        double Xcoeff5 = vectorY1 * vectorX2 - vectorY2 * vectorX1;

        double Ycoeff1 = pointY[0] * vectorX1 * vectorY2;
        double Ycoeff2 = pointY[2] * vectorX2 * vectorY1;
        double Ycoeff3 = pointX[0] * vectorY1 * vectorY2;
        double Ycoeff4 = pointX[2] * vectorY1 * vectorY2;
        double Ycoeff5 = vectorX1 * vectorY2 - vectorX2 * vectorY1;

        double crossingPointX = (Xcoeff1 - Xcoeff2 - Xcoeff3 + Xcoeff4) / Xcoeff5;
        double crossingPointY = (Ycoeff1 - Ycoeff2 - Ycoeff3 + Ycoeff4) / Ycoeff5;

        Quaternion quat = Quaternion.AngleAxis(Mathf.Rad2Deg * (float)Math.Atan2(vectorY1, vectorX1), new Vector3(0, 0, 1));

        GetComponent<Transform>().position = new Vector3((float)crossingPointX, (float)crossingPointY, 0) + quat * new Vector3(0.7f, 0.7f, 0);
        GetComponent<Transform>().rotation = quat;

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


        return (pointX[1] - pointX[0]) * (pointX[3] - pointX[2]) + (pointY[3] - pointY[2]) * (pointY[1] - pointY[0]);
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
            return pointX[2] - pointX[3];
        if (!param.isX && param.point.Equals(points[0]))
            return pointY[2] - pointY[3];
        if (param.isX && param.point.Equals(points[1]))
            return pointX[3] - pointX[2];
        if (!param.isX && param.point.Equals(points[1]))
            return pointY[3] - pointY[2];
        if (param.isX && param.point.Equals(points[2]))
            return pointX[0] - pointX[1];
        if (!param.isX && param.point.Equals(points[2]))
            return pointY[0] - pointY[1];
        if (param.isX && param.point.Equals(points[3]))
            return pointX[1] - pointX[0];
        if (!param.isX && param.point.Equals(points[3]))
            return pointY[1] - pointY[0];

        return 0;
    }



    public override void AddPrimitive(GameObject primitive)
    {
        if (primitive.TryGetComponent(out LineController isLine) && points.Count != 4)
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

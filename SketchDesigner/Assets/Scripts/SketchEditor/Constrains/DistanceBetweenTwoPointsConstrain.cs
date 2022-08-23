using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceBetweenTwoPointsConstrain : ConstrainController
{
    public double distanceBetweenPoints = 0;
    public double horizontalLineBiasFromCenter;
    public double biasFromLine;

    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject leftVerticalLine;
    public GameObject rightVerticalLine;
    public GameObject horizontalLine;
    public InputField inputLength;
    public Canvas canvas;

    private void CalculateDistanceAndUpdate()
    {
        UpdatePositionConstrain();

        distanceBetweenPoints = GetEquationValue(0);
        inputLength.text = distanceBetweenPoints.ToString();
    }


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
            CalculateDistanceAndUpdate();
            createConstrainStep = 0;
            return true;
        }

        return false;
    }

    public override void UpdatePositionConstrain()
    {
        double x1 = points[0].GetComponent<Transform>().position.x;
        double y1 = points[0].GetComponent<Transform>().position.y;
        double x2 = points[1].GetComponent<Transform>().position.x;
        double y2 = points[1].GetComponent<Transform>().position.y;

        double centerX = 0;
        double centerY = 0;
        double biasFromCenter = 0;

        if (x1 >= x2)
        {
            centerX = x2 + (x1 - x2) / 2;
            biasFromCenter += Math.Pow(centerX - x2, 2);
        }
        else
        {
            centerX = x1 + (x2 - x1) / 2;
            biasFromCenter += Math.Pow(centerX - x1, 2);
        }

        if (y1 >= y2)
        {
            centerY = y2 + (y1 - y2) / 2;
            biasFromCenter += Math.Pow(centerY - y2, 2);
        }
        else
        {
            centerY = y1 + (y2 - y1) / 2;
            biasFromCenter += Math.Pow(centerY - y1, 2);
        }

        biasFromCenter = Math.Sqrt(biasFromCenter);

        double angle = 0;
        angle = Math.Atan2(y1 - y2, x1 - x2);

        Vector3 tempPos;

        GetComponent<Transform>().rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * (float)angle, new Vector3(0, 0, 1));
        GetComponent<Transform>().position = new Vector3((float)centerX, (float)centerY, 0);

        tempPos = leftArrow.GetComponent<Transform>().localPosition;
        tempPos.x = -(float)biasFromCenter;
        tempPos.y = -(float)biasFromLine;
        leftArrow.GetComponent<Transform>().localPosition = tempPos;



        tempPos = rightArrow.GetComponent<Transform>().localPosition;
        tempPos.x = (float)biasFromCenter;
        tempPos.y = -(float)biasFromLine;
        rightArrow.GetComponent<Transform>().localPosition = tempPos;



        tempPos =  leftVerticalLine.GetComponent<LineRenderer>().GetPosition(0);
        tempPos.x = -(float)biasFromCenter;
        tempPos.y = 0.0f;
        leftVerticalLine.GetComponent<LineRenderer>().SetPosition(0, tempPos);

        tempPos = leftVerticalLine.GetComponent<LineRenderer>().GetPosition(1);
        tempPos.x = -(float)biasFromCenter;
        tempPos.y = -(float)biasFromLine - 0.1f;
        leftVerticalLine.GetComponent<LineRenderer>().SetPosition(1, tempPos);



        tempPos = rightVerticalLine.GetComponent<LineRenderer>().GetPosition(0);
        tempPos.x = (float)biasFromCenter;
        tempPos.y = 0.0f;
        rightVerticalLine.GetComponent<LineRenderer>().SetPosition(0, tempPos);

        tempPos = rightVerticalLine.GetComponent<LineRenderer>().GetPosition(1);
        tempPos.x = (float)biasFromCenter;
        tempPos.y = -(float)biasFromLine - 0.1f;
        rightVerticalLine.GetComponent<LineRenderer>().SetPosition(1, tempPos);


        List<Vector2> colliderPoints = new List<Vector2>();

        tempPos = horizontalLine.GetComponent<LineRenderer>().GetPosition(0);
        tempPos.x = (float)(- biasFromCenter + horizontalLineBiasFromCenter);
        tempPos.y = -(float)biasFromLine;
        horizontalLine.GetComponent<LineRenderer>().SetPosition(0, tempPos);
        colliderPoints.Add(tempPos);

        tempPos = horizontalLine.GetComponent<LineRenderer>().GetPosition(1);
        tempPos.x = (float)(biasFromCenter - horizontalLineBiasFromCenter);
        tempPos.y = -(float)biasFromLine;
        horizontalLine.GetComponent<LineRenderer>().SetPosition(1, tempPos);
        colliderPoints.Add(tempPos);

        tempPos = canvas.GetComponent<Transform>().localPosition;
        tempPos.y = -(float)(biasFromLine + 1.0);
        canvas.GetComponent<Transform>().localPosition = tempPos;

        double dAz = canvas.GetComponent<Transform>().rotation.eulerAngles.z;
        if (90.0f < dAz && dAz < 270.0f)
            canvas.GetComponent<Transform>().localScale = new Vector3(-0.1f, -0.1f, -0.1f);
        else
            canvas.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);

        GetComponent<EdgeCollider2D>().SetPoints(colliderPoints);
   
        inputLength.text = distanceBetweenPoints.ToString();
    }


    public override void UpdateOutline(bool isHighlighted)
    {
        if (isHighlighted)
        {
            leftArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
            rightArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
            leftVerticalLine.GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterialConstrain;
            rightVerticalLine.GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterialConstrain;
            horizontalLine.GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterialConstrain;
        }
        else
        {
            leftArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
            rightArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
            leftVerticalLine.GetComponent<LineRenderer>().material = SketchEditorController.staticStandartMaterialConstrain;
            rightVerticalLine.GetComponent<LineRenderer>().material = SketchEditorController.staticStandartMaterialConstrain;
            horizontalLine.GetComponent<LineRenderer>().material = SketchEditorController.staticStandartMaterialConstrain;
        }
    }


    public override int GetEquationsCount()
    {
        return 1;
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

    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {
        double x1 = points[0].GetComponent<Transform>().position.x;
        double y1 = points[0].GetComponent<Transform>().position.y;
        double x2 = points[1].GetComponent<Transform>().position.x;
        double y2 = points[1].GetComponent<Transform>().position.y;

        if (indexEquation == 0 && param.isX && param.point.Equals(points[0]))
        {
            return (2 * x1 - 2 * x2) / (2 * Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }
        if(indexEquation == 0 && !param.isX && param.point.Equals(points[0]))
        {
            return (2 * y1 - 2 * y2) / (2 * Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }
        if (indexEquation == 0 && param.isX && param.point.Equals(points[1]))
        {
            return -(2 * x1 - 2 * x2) / (2 * Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }
        if (indexEquation == 0 && !param.isX && param.point.Equals(points[1]))
        {
            return -(2 * y1 - 2 * y2) / (2 * Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }

        return 0;
    }

    public override double GetEquationValue(int indexEquation)
    {
        double x1 = points[0].GetComponent<Transform>().position.x;
        double y1 = points[0].GetComponent<Transform>().position.y;
        double x2 = points[1].GetComponent<Transform>().position.x;
        double y2 = points[1].GetComponent<Transform>().position.y;

        double value = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) - distanceBetweenPoints;

        return value;
    }



    public override void AddPrimitive(GameObject primitive)
    {
        if (primitive.TryGetComponent(out PointController isPoint))
        {
            isPoint.SetOutlineFromCostrain(true);
            points.Add(primitive);
        }
        else if (primitive.TryGetComponent(out LineController isLine))
        {
            if(points.Count == 0)
            {
                isLine.SetOutlineFromCostrain(true);
                foreach (var point in isLine.points)
                {
                    points.Add(point);
                }
            }
            else
            {
                SketchEditorController.globalRef.infoMessage("Please, select the other point!");
            }
        }
        else 
            SketchEditorController.globalRef.infoMessage("Please, select point or line!");
    }

    public void SetDistance(InputField input)
    {
        if(input.text != string.Empty)
        {
            if(double.TryParse(input.text, out distanceBetweenPoints))
                SketchEditorController.UpdateConstrains();
            else
                SketchEditorController.globalRef.infoMessage("String is Empty");
        }
        else
            SketchEditorController.globalRef.infoMessage("String is Empty");
    }
}

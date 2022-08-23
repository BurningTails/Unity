using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//НОРМАЛИЗОВЫВАТЬ ВЕКТОРА!


public class AngleBetweenTwoLinesConstran : ConstrainController
{
    public double angleBtwLines = 0.0;
    public double rangeArcCoeff = 5;
    public float minDistArc = 0.2f;
    public int curvePointsCount = 10;

    private bool isDirectionVectorSet = true;
    private bool bClockwise = true;    

    public InputField inputAngle;
    public GameObject arc;
    public GameObject rightArrow;
    public GameObject leftArrow;
    public Canvas canvas;

    /*private List<Vector3> MakeSmoothCurve(List<Vector3> arrayToCurve)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        pointsLength = arrayToCurve.Count;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            curvedPoints.Add(points[0]);
        }

        return curvedPoints;
    }*/


    public void makeCurve(Vector3 v1, Vector3 v2, float dist, ref List<Vector3> arrayToCurv, ref List<Vector2> arrayToCurv2D)
    {
        Vector3 tempV;
        Vector2 tempV2D;
        arrayToCurv.Clear();
        arrayToCurv2D.Clear();
        double countAngle;
        if (angleBtwLines < 0.001)
        {
           double dAngle =Vector3.Angle(v1, v2) * Mathf.Deg2Rad;
           countAngle = Math.Ceiling(dAngle * Mathf.Rad2Deg / 4);
            for (int i = 0; i <= countAngle; i++)
            {
                tempV = Vector3.LerpUnclamped(v1, v2, i / (float)countAngle);
                tempV.Normalize();
                tempV *= dist;
                arrayToCurv.Add(tempV);
                tempV2D = tempV;
                arrayToCurv2D.Add(tempV2D);
            }
            return;
        }
        
        countAngle = Math.Ceiling(angleBtwLines * Mathf.Rad2Deg / 4);

        if (countAngle < 5.0) countAngle = 5.0;

        Quaternion qStep = Quaternion.Euler(0, 0, (bClockwise ? 1.0f : -1.0f) * (float)(angleBtwLines * Mathf.Rad2Deg / countAngle));

        tempV = v1;

        for (int i = 0; i <= countAngle; i++)
        {
            tempV.Normalize();
            tempV *= dist;
            arrayToCurv.Add(tempV);
            tempV2D = tempV;
            arrayToCurv2D.Add(tempV2D);
            tempV = qStep * tempV;
        }        
    }

    public override void InitConstrain()
    {
        calculateDistanceAndUpdate();
    }

    private void calculateDistanceAndUpdate()
    {
        angleBtwLines = 0.0;
        double dSin = GetEquationValue(1);
        if (dSin < 0)
            bClockwise = false;
        //angleBtwLines = Math.PI/2.0;
        double dCos = GetEquationValue(0);
        angleBtwLines = dCos;
        inputAngle.text = (angleBtwLines * Mathf.Rad2Deg).ToString();
    }

    public override bool CreateConstrain()
    {
        if (createConstrainStep == 0)
        {
            if (points.Count == 4 && isDirectionVectorSet)
                createConstrainStep++;
            else
                return false;
        }

        if (createConstrainStep == 1)
        {
            SetObjectsOutlineDefault();
            //calculateDistanceAndUpdate();
            createConstrainStep = 0;
            return true;
        }

        return false;
    }

    private bool calculateCrossingPoint(Vector3 vec1, Vector3 point1, Vector3 vec2, Vector3 point2, ref Vector3 crossingPoint)
    {
        float fAngle = Vector3.Angle(vec1, vec2);
        if ((180.0f - fAngle) < 0.05 || fAngle < 0.05)
        {
            crossingPoint = calculateGeometricalCenter();
            return false;
        }

        if (Vector3.Distance(point1, point2) < SketchEditorController.constrainEPS)
        {
            crossingPoint = point1;
            return true;
        }

        double Xcoeff1 = point1.x * vec1.y * vec2.x;
        double Xcoeff2 = point2.x * vec2.y * vec1.x;
        double Xcoeff3 = point1.y * vec1.x * vec2.x;
        double Xcoeff4 = point2.y * vec1.x * vec2.x;
        double Xcoeff5 = vec1.y * vec2.x - vec2.y * vec1.x;

        double Ycoeff1 = point1.y * vec1.x * vec2.y;
        double Ycoeff2 = point2.y * vec2.x * vec1.y;
        double Ycoeff3 = point1.x * vec1.y * vec2.y;
        double Ycoeff4 = point2.x * vec1.y * vec2.y;
        double Ycoeff5 = vec1.x * vec2.y - vec2.x * vec1.y;

        double crossingPointX;
        double crossingPointY;


        crossingPointX = (Xcoeff1 - Xcoeff2 - Xcoeff3 + Xcoeff4) / Xcoeff5;
        crossingPointY = (Ycoeff1 - Ycoeff2 - Ycoeff3 + Ycoeff4) / Ycoeff5;

        crossingPoint = new Vector3((float)crossingPointX, (float)crossingPointY, 0);
        return true;

    }


    private Vector3 calculateGeometricalCenter()
    {
        Vector3 geometricalCenter = new Vector3(0, 0, 0);

        foreach (var point in points)
        {
            geometricalCenter += point.GetComponent<Transform>().position;
        }

        return geometricalCenter / 4.0f;
    }


    public override void UpdatePositionConstrain()
    {
        List<double> pointX = new List<double>();
        List<double> pointY = new List<double>();
        Vector3 vector1; 
        Vector3 vector2;
        Vector3 bisector;
        float dist = 0;

        foreach (var point in points)
        {
            pointX.Add(point.GetComponent<Transform>().position.x);
            pointY.Add(point.GetComponent<Transform>().position.y);
        }

        Vector3 center = new Vector3();

        Vector3 vec1 = new Vector3((float)(pointX[1] - pointX[0]), (float)(pointY[1] - pointY[0]), 0);
        Vector3 vec2 = new Vector3((float)(pointX[3] - pointX[2]), (float)(pointY[3] - pointY[2]), 0);

        Vector3 point1 = new Vector3((float)pointX[0], (float)pointY[0], 0);
        Vector3 point2 = new Vector3((float)pointX[2], (float)pointY[2], 0);


        if (calculateCrossingPoint(vec1, point1, vec2, point2, ref center))
        {
            vector1 = new Vector3((float)(pointX[1] - pointX[0]), (float)(pointY[1] - pointY[0]), 0);
            vector2 = new Vector3((float)(pointX[3] - pointX[2]), (float)(pointY[3] - pointY[2]), 0);

            if (vector1.magnitude > vector2.magnitude)
                dist = vector2.magnitude / 2;
            else
                dist = vector1.magnitude / 2;
            
            vector1.Normalize();
            vector2.Normalize();

            bisector = vector1 + vector2;
            bisector.Normalize();
        }
        else
        {
            if (angleBtwLines < SketchEditorController.constrainEPS) // угол = 0
            {
                List<GameObject> tempList = new List<GameObject>();
                bool isThreeElements = false;

                foreach(var point in points)
                {
                    tempList = points.FindAll(Elem => Elem.Equals(point));
                    if(tempList.Count > 1)
                    {
                        center = point.GetComponent<Transform>().position;
                        isThreeElements = true;
                        break;
                    }
                }

                if(isThreeElements) //если 3 точки - center = точка, принадлежащая двум векторам
                {
                    vector1 = vec1;
                    vector2 = vec2;

                    if (vector1.magnitude > vector2.magnitude)
                        dist = vector2.magnitude / 2;
                    else
                        dist = vector1.magnitude / 2;

                    vector1.Normalize();
                    vector2.Normalize();

                    bisector = vector1 + vector2;
                    bisector.Normalize();
                }
                else //если 4 точки - center = геометрический центр 4 точек
                {
                    vec1.Normalize();
                    vec2.Normalize();
                    var pt0 = points[0].GetComponent<Transform>().position;
                    float fDist = Vector3.Dot(vec1, center - pt0);
                    vector1 = pt0 + vec1 * (fDist + 4.0f) - center;

                    var pt2 = points[2].GetComponent<Transform>().position;
                    fDist = Vector3.Dot(vec2, center - pt2);
                    vector2 = pt2 + vec2 * (fDist + 4.0f) - center;

                    dist = vector1.magnitude;

                    vector1.Normalize();
                    vector2.Normalize();

                    bisector =  vector1 + vector2;
                    bisector.Normalize();
                }
            }
            else // угол равен 180
            {
                List<GameObject> tempList = new List<GameObject>();
                bool isThreeElements = false;

                foreach (var point in points)
                {
                    tempList = points.FindAll(Elem => Elem.Equals(point));
                    if (tempList.Count > 1)
                    {
                        center = point.GetComponent<Transform>().position;
                        isThreeElements = true;
                        break;
                    }
                }

                if (isThreeElements) //если 3 точки - center = точка, принадлежащая двум векторам
                {
                    vector1 = vec1;
                    vector2 = vec2;

                    if (vector1.magnitude > vector2.magnitude)
                        dist = vector2.magnitude / 2;
                    else
                        dist = vector1.magnitude / 2;

                    vector1.Normalize();
                    vector2.Normalize();

                    bisector = center + Quaternion.Euler(0, 0,bClockwise?90:-90) * vector1;
                    bisector.Normalize();                                 
                }
                else //если 4 точки - center = геометрический центр 4 точек
                {
                    vec1.Normalize();
                    vec2.Normalize();
                    var pt0 = points[0].GetComponent<Transform>().position;
                    float fDist = Vector3.Dot(vec1, center - pt0);
                    vector1 = pt0 + vec1 * (fDist + 4.0f) - center;

                    var pt2 = points[2].GetComponent<Transform>().position;
                    fDist = Vector3.Dot(vec2, center - pt2);
                    vector2 = pt2 + vec2 * (fDist + 4.0f) - center;
                    dist = vector1.magnitude;

                    vector1.Normalize();
                    vector2.Normalize();

                    bisector = Quaternion.Euler(0.0f, 0.0f, 90.0f) * vector1;
                    bisector.Normalize();
                }
            }
        }

        if (dist < minDistArc) dist = minDistArc;

        bisector *= dist;

        Quaternion mainRotate = Quaternion.AngleAxis(Mathf.Rad2Deg * (float)Math.Atan2(bisector.y, bisector.x) - 90.0f, new Vector3(0, 0, 1));
        Quaternion mainRotateRev = Quaternion.AngleAxis(90.0f - Mathf.Rad2Deg * (float)Math.Atan2(bisector.y, bisector.x), new Vector3(0, 0, 1));

        vector1 = mainRotateRev * vector1;
        vector2 = mainRotateRev * vector2;
        bisector = mainRotateRev * bisector;     

        float angleLeft = (float)Math.Atan2(vector1.y, vector1.x);
        float angleRight = (float)Math.Atan2(vector2.y, vector2.x);

        List<Vector3> curveKeyPoints = new List<Vector3>();
        List<Vector2> curveReyPoints2D = new List<Vector2>();

        makeCurve(vector1, vector2, dist, ref curveKeyPoints, ref curveReyPoints2D);

        if (curveKeyPoints.Count > 0)
        {
            arc.GetComponent<LineRenderer>().positionCount = curveKeyPoints.Count;
            arc.GetComponent<LineRenderer>().SetPositions(curveKeyPoints.ToArray());
            GetComponent<EdgeCollider2D>().SetPoints(curveReyPoints2D);
        }


        leftArrow.GetComponent<Transform>().localPosition = vector1 * dist;
        leftArrow.GetComponent<Transform>().localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * angleLeft + 180.0f, new Vector3(0, 0, 1));
        //leftArrow.GetComponent<Transform>().rotation;
        rightArrow.GetComponent<Transform>().localPosition = vector2 * dist;
        rightArrow.GetComponent<Transform>().localRotation = Quaternion.AngleAxis(Mathf.Rad2Deg * angleRight, new Vector3(0, 0, 1));
        //rightArrow.GetComponent<Transform>().rotation = Quaternion.FromToRotation(rightArrow.GetComponent<Transform>().rotation.eulerAngles, -vector2);


        if (angleBtwLines - SketchEditorController.constrainEPS > Math.PI)
            bisector = -bisector;

        canvas.GetComponent<Transform>().localPosition = bisector + bisector.normalized * 2;

        double dAz = canvas.GetComponent<Transform>().rotation.eulerAngles.z;
        if (90.0f < dAz && dAz < 270.0f)
            canvas.GetComponent<Transform>().localScale = new Vector3(-0.1f, -0.1f, -0.1f);
        else
            canvas.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);

        GetComponent<Transform>().position = new Vector3(center.x, center.y, 0);
        GetComponent<Transform>().rotation = mainRotate;

        inputAngle.text = (angleBtwLines * Mathf.Rad2Deg).ToString();
    }


    public override void UpdateOutline(bool isHighlighted)
    {
        if(isHighlighted)
        {
            leftArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
            rightArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticOutlineColorConstrain;
            arc.GetComponent<LineRenderer>().material = SketchEditorController.staticOutlineMaterialConstrain;
        }
        else
        {
            leftArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
            rightArrow.GetComponent<SpriteRenderer>().color = SketchEditorController.staticStandartColorConstrain;
            arc.GetComponent<LineRenderer>().material = SketchEditorController.staticStandartMaterialConstrain;
        }
    }

    public override int GetConstrainPriority()
    {
        return 10;
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


    public override double GetDerivative(int indexEquation, SketchEditorController.ConstrainParam param)
    {
        Vector3 vA = points[1].GetComponent<Transform>().position - points[0].GetComponent<Transform>().position;
        Vector3 vB = points[3].GetComponent<Transform>().position - points[2].GetComponent<Transform>().position;

        double vAx = vA.x;
        double vAy = vA.y;
        double vBx = vB.x;
        double vBy = vB.y;

        if (indexEquation == 1 && !bClockwise)
        {
            double tmp = vAx; vAx = vBx; vBx = tmp;
            tmp = vAy; vAy = vBy; vBy = tmp;
        }

        double N1 = -vBx * vAy + vAx * vBy;
        double N2 = vAx * vBx + vAy * vBy;
        double D1 = vAx * vAx + vAy * vAy;
        double D2 = vBx * vBx + vBy * vBy;


        double commonD1 = Math.Sqrt(1 - (N1 * N1) / (D1 * D2));
        double commonD2 = Math.Sqrt(1 - (N2 * N2) / (D1 * D2));
        double commonD3 = Math.Sqrt(D1 * D2);
        double commonD4 = Math.Sqrt(D1 * D2 * D2 * D2);
        double commonD5 = Math.Sqrt(D1 * D1 * D1 * D2);

        if (indexEquation == 1)
        {
            if (param.isX && param.point.Equals(points[0]))
            {
                return ((-vBy / commonD3) + (vAx * N1 / commonD5)) / commonD1;
            }
            if (!param.isX && param.point.Equals(points[0]))
            {
                return ((vBx / commonD3) + (vAy * N1 / commonD5)) / commonD1;
            }
            if (param.isX && param.point.Equals(points[1]))
            {
                return ((vBy / commonD3) - (vAx * N1 / commonD5)) / commonD1;
            }
            if (!param.isX && param.point.Equals(points[1]))
            {
                return ((-vBx / commonD3) - (vAy * N1 / commonD5)) / commonD1;
            }
            if (param.isX && param.point.Equals(points[2]))
            {
                return ((vAy / commonD3) + (vBx * N1 / commonD4)) / commonD1;
            }
            if (!param.isX && param.point.Equals(points[2]))
            {
                return ((-vAx / commonD3) + (vBy * N1 / commonD4)) / commonD1;
            }
            if (param.isX && param.point.Equals(points[3]))
            {
                return ((-vAy / commonD3) - (vBx * N1 / commonD4)) / commonD1;
            }
            if (!param.isX && param.point.Equals(points[3]))
            {
                return ((vAx / commonD3) - (vBy * N1 / commonD4)) / commonD1;
            }
        }
        else
        {
            if (!bClockwise)
                N1 = -N1;
            double dCoef = -1.0;
            if (N1 / commonD3 < 0.0)
            {
                dCoef = 1.0;
            }
            if (param.isX && param.point.Equals(points[0]))
            {
                return dCoef*((-vBx / commonD3) + (vAx * N2 / commonD5)) / commonD2;
            }
            if (!param.isX && param.point.Equals(points[0]))
            {
                return dCoef * ((-vBy / commonD3) + (vAy * N2 / commonD5)) / commonD2;
            }
            if (param.isX && param.point.Equals(points[1]))
            {
                return dCoef * ((vBx / commonD3) - (vAx * N2 / commonD5)) / commonD2;
            }
            if (!param.isX && param.point.Equals(points[1]))
            {
                return dCoef * ((vBy / commonD3) - (vAy * N2 / commonD5)) / commonD2;
            }
            if (param.isX && param.point.Equals(points[2]))
            {
                return dCoef * ((-vAx / commonD3) + (vBx * N2 / commonD4)) / commonD2;
            }
            if (!param.isX && param.point.Equals(points[2]))
            {
                return dCoef * ((-vAy / commonD3) + (vBy * N2 / commonD4)) / commonD2;
            }
            if (param.isX && param.point.Equals(points[3]))
            {
                return dCoef * ((vAx / commonD3) - (vBx * N2 / commonD4)) / commonD2;
            }
            if (!param.isX && param.point.Equals(points[3]))
            {
                return dCoef * ((vAy / commonD3) - (vBy * N2 / commonD4)) / commonD2;
            }
        }

        return 0;

        //             vA.Normalize();
        //         vB.Normalize();
        // 
        //         double returnValue = 0;
        // 
        //         if (indexEquation == 0)
        //         {
        // 
        //             if (param.isX && param.point.Equals(points[0]))
        //             {
        //                 returnValue += -vB.y;
        //             }
        //             if (!param.isX && param.point.Equals(points[0]))
        //             {
        //                 returnValue += vB.x;
        //             }
        //             if (param.isX && param.point.Equals(points[1]))
        //             {
        //                 returnValue += vB.y;
        //             }
        //             if (!param.isX && param.point.Equals(points[1]))
        //             {
        //                 returnValue += -vB.x;
        //             }
        //             if (param.isX && param.point.Equals(points[2]))
        //             {
        //                 returnValue += vA.y;
        //             }
        //             if (!param.isX && param.point.Equals(points[2]))
        //             {
        //                 returnValue += -vA.x;
        //             }
        //             if (param.isX && param.point.Equals(points[3]))
        //             {
        //                 returnValue += -vA.y;
        //             }
        //             if (!param.isX && param.point.Equals(points[3]))
        //             {
        //                 returnValue += vA.x;
        //             }            
        //             returnValue *= 0.9;
        // 
        //             if(!bClockwise)
        //                 returnValue = -returnValue;
        //         }
        //         else
        //         {
        //             if (param.isX && param.point.Equals(points[0]))
        //             {
        //                 returnValue += -vB.x;
        //             }
        //             if (!param.isX && param.point.Equals(points[0]))
        //             {
        //                 returnValue += -vB.y;
        //             }
        //             if (param.isX && param.point.Equals(points[1]))
        //             {
        //                 returnValue += vB.x;
        //             }
        //             if (!param.isX && param.point.Equals(points[1]))
        //             {
        //                 returnValue += vB.y;
        //             }
        //             if (param.isX && param.point.Equals(points[2]))
        //             {
        //                 returnValue += -vA.x;
        //             }
        //             if (!param.isX && param.point.Equals(points[2]))
        //             {
        //                 returnValue += -vA.y;
        //             }
        //             if (param.isX && param.point.Equals(points[3]))
        //             {
        //                 returnValue += vA.x;
        //             }
        //             if (!param.isX && param.point.Equals(points[3]))
        //             {
        //                 returnValue += vA.y;
        //             }
        //             returnValue *= 0.9;
        //         }
        //         return returnValue;
    }

    public override double GetEquationValue(int indexEquation)
    {
        if (points.Count == 0)
            return 0.0;

        Vector3 vA = points[1].GetComponent<Transform>().position - points[0].GetComponent<Transform>().position;
        Vector3 vB = points[3].GetComponent<Transform>().position - points[2].GetComponent<Transform>().position;


        double vAx = vA.x;
        double vAy = vA.y;
        double vBx = vB.x;
        double vBy = vB.y;

//         if (indexEquation == 1 && !bClockwise)
//         {
//             double tmp = vAx; vAx = vBx; vBx = tmp;
//             tmp = vAy; vAy = vBy; vBy = tmp;
//         }


        double N1 = -vBx * vAy + vAx * vBy;
        double N2 = vAx * vBx + vAy * vBy;
        double D1 = vAx * vAx + vAy * vAy;
        double D2 = vBx * vBx + vBy * vBy;

        double commonD3 = Math.Sqrt(D1 * D2);

        //         if (indexEquation == 1)
        //         {
        //             double dSin = Math.Asin(N1 / commonD3);
        // 
        //             return Math.Min(dSin - angleBtwLines, Math.Min(dSin + Math.PI - angleBtwLines, dSin + 2.0 * Math.PI - angleBtwLines));
        //         }
        //double dSin = Math.Asin(N1 / commonD3);
        if (indexEquation == 1)
            return N1 / commonD3;
        if (!bClockwise)
            N1 = -N1;
        double dCos = Math.Acos(N2 / commonD3);
        if (N1 / commonD3 < 0.0)
        {
            dCos = 2.0 * Math.PI - dCos;
        }

        if (Math.Abs(dCos - angleBtwLines - 2.0 * Math.PI) < Math.Abs(dCos - angleBtwLines))
            return dCos - angleBtwLines - 2.0 * Math.PI;

        return dCos - angleBtwLines;
        




            //         vA.Normalize();
            //         vB.Normalize();
            // 
            //         if (indexEquation == 0)
            //         {
            //              double dCross = vA.x * vB.y - vA.y * vB.x;
            //             if (!bClockwise)
            //                 dCross = -dCross;
            //              double dSin = Math.Sin(angleBtwLines);
            // //             bool bFirst = Math.Abs(dCross - dSin) < Math.Abs(dSin + dCross);
            //             return (dCross - dSin);
            //         }
            // 
            //         return (vA.x * vB.x + vA.y * vB.y) - Math.Cos(angleBtwLines);

        }


    public override void AddPrimitive(GameObject primitive)
    {
        if (primitive.TryGetComponent(out PointController isPoint))
        {            
            if ((points.Count == 2 || points.Count == 4) && !isDirectionVectorSet)
            {
                int index = points.FindIndex(Elem => Elem.Equals(primitive));

                if (index != -1)
                {
                    isPoint.SetOutlineFromCostrain(true);

                    if (index % 2 == 0)
                    {
                        GameObject temp = points[index];
                        points[index] = points[index + 1];
                        points[index + 1] = temp;
                    }

                    isDirectionVectorSet = true;
                }
                else
                {
                    SketchEditorController.globalRef.infoMessage("Please, select the point on selected line!");
                }
            }
            else
            {
                SketchEditorController.globalRef.infoMessage("Please, select the line!");
            }
        }


        if (primitive.TryGetComponent(out LineController isLine))
        {
            if (isDirectionVectorSet)
            {
                if (points.Count != 4)
                {
                    isLine.SetOutlineFromCostrain(true);

                    foreach (var point in isLine.points)
                    {
                        points.Add(point);
                    }
                    isDirectionVectorSet = false;
                }
            }
            else
            {
                SketchEditorController.globalRef.infoMessage("Please, select the point on selected line!");
            }
        }
    }


    public void SetAngle(InputField input) //МОДИФИЦИРОВАТЬ ДЛЯ ELSE 
    {
        if (input.text != string.Empty)
        {
            double temp = 0;

            if (double.TryParse(input.text, out temp))
            {
                angleBtwLines = temp * Mathf.Deg2Rad;
                SketchEditorController.UpdateConstrains();
            }              
            else
            {
                input.text = (angleBtwLines * Mathf.Rad2Deg).ToString();
                SketchEditorController.globalRef.infoMessage("String is Empty");
            }
                
        }
        else
        {
            input.text = (angleBtwLines * Mathf.Rad2Deg).ToString();
            SketchEditorController.globalRef.infoMessage("String is Empty");
        }
    }


}

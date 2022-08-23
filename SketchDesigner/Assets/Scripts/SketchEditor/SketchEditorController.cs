using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SketchEditorController : MonoBehaviour
{
    public struct ConstrainParam
    {
        public ConstrainParam(GameObject point_in, bool isX_in) { point = point_in; isX = isX_in; }
        public GameObject point;
        public bool isX;
    }

    public static List<GameObject> linesBase = new List<GameObject>();
    public static List<GameObject> pointsBase = new List<GameObject>();
    public static List<GameObject> constrainsBase = new List<GameObject>();
    private static List<double> paramsSaved = new List<double>();

    public static List<GameObject> selectedPoints = new List<GameObject>();
    public static List<GameObject> selectedLines = new List<GameObject>();
    public static List<GameObject> selectedConstrains = new List<GameObject>();


    public Material standartMaterial;
    public Material outlineMaterial;
    public Material outlineMaterialConstrain;
    public Material standartMaterialConstrain;

    public Color standartColor;
    public Color outlineColor;
    public Color outlineColorConstrain;
    public Color standartColorConstrain;

    public Text info;

    public static Material staticStandartMaterial;
    public static Material staticOutlineMaterial;
    public static Material staticStandartMaterialConstrain;
    public static Material staticOutlineMaterialConstrain;

    public static Text staticInfo;

    public static Color staticStandartColor;
    public static Color staticOutlineColor;
    public static Color staticStandartColorConstrain;
    public static Color staticOutlineColorConstrain;


    public static float dragEpsSelectedObjects = 0.01f;
    public static bool isDragSelectedObjects = false;
    private int dragStep = 0;
    private List<GameObject> dragSelectedObjects = new List<GameObject>();
    private Vector3 biasDrag;
    public float scaleSize = 1.0f;
    public float standartCameraScale = 10;

    public GameObject linePrefab;
    public GameObject pointPrefab;
    public static bool isLineCreating = false;
    public static bool isPointCreating = false;
    public static bool isPointSelected = false;

    private int drawStep = 0;
    private float deltaPoint = 0.2f;

    private Vector3 startMousePosition = new Vector3();
    private Vector3 endMousePosition = new Vector3();


    private GameObject[] tempPoints = new GameObject[2];
    private GameObject tempLine = null;
    public GameObject DrawZone;

    public static double constrainEPS = 1e-4;
    //public int addConstrainStep = 0;
    public static bool isConstrainCreating = false;
    public static GameObject createdCostrain;

    public GameObject pointLockConstrainPrefab;
    public GameObject distanceBetweenTwoPointsConstrainPrefab;
    public GameObject angleBetweenTwoLinesConstrainPrefab;
    public GameObject verticalLineConstrainPrefab;
    public GameObject horizontalLineConstrainPrefab;
    public GameObject perpendicularLinesConstrainPrefab;
    public GameObject parallelLinesConstrainPrefab;
    public GameObject coincidentPointsConstrainPrefab;
    public GameObject pointBelongToLineConstrainPrefab;

    public static SketchEditorController globalRef;


    public GameObject loadingScreen;
    public Slider slider;
    public Text progressText;


    private void Start()
    {
        Camera.main.GetComponent<Camera>().orthographicSize = standartCameraScale;
        staticStandartMaterial = standartMaterial;
        staticOutlineMaterial = outlineMaterial;
        staticStandartMaterialConstrain = standartMaterialConstrain;
        staticOutlineMaterialConstrain = outlineMaterialConstrain;


        staticStandartColor = standartColor;
        staticOutlineColor = outlineColor;
        staticStandartColorConstrain = standartColorConstrain;
        staticOutlineColorConstrain = outlineColorConstrain;

        staticInfo = info;
        globalRef = this;
    }

    public void infoMessage(string input)
    {
        StartCoroutine(messageCoroutine(input));
    }

    public static IEnumerator messageCoroutine(string inputText)
    {
        Color textColor;
        if(staticInfo.color.a  >= 0.01f)
        {
            yield break;
        }

        textColor = staticInfo.color;
        textColor.a = 1.0f;
        staticInfo.color = textColor;

        if (inputText.Length != 0)
        {
            staticInfo.text = "[Info]: " + inputText;

            yield return new WaitForSeconds(1.0f);

            for (float i = 1.0f; i >= 0; i -= 0.01f)
            {
                textColor = staticInfo.color;
                textColor.a = i;
                staticInfo.color = textColor;
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    public void createPoint()
    {
        EscapeSequence();
        isPointCreating = true;
    }
    
    public void createLine()
    {
        EscapeSequence();
        isLineCreating = true;
    }

    private void createConstrain(GameObject prefab)
    {
        createdCostrain = Instantiate(prefab, new Vector2(50000, 0), Quaternion.identity);
    }

    public void createPointLockConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(pointLockConstrainPrefab);
    }

    public void createDistanceBetweenTwoPointsConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(distanceBetweenTwoPointsConstrainPrefab);
    }

    public void createAngleBetweenTwoLinesConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(angleBetweenTwoLinesConstrainPrefab);
    }

    public void createVerticalLineConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(verticalLineConstrainPrefab);
    }

    public void createHorizontalLineConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(horizontalLineConstrainPrefab);
    }

    public void createPerpendicularLinesConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(perpendicularLinesConstrainPrefab);
    }

    public void createParallelLinesConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(parallelLinesConstrainPrefab);
    }

    public void createCoincidentPointsConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(coincidentPointsConstrainPrefab);
    }

    public void createPointBelongToLineConstrain()
    {
        EscapeSequence();
        isConstrainCreating = true;
        createConstrain(pointBelongToLineConstrainPrefab);
    }

    public void zoomAll()
    {
        if (pointsBase.Count > 0)
        {
            Bounds bounds = new Bounds(pointsBase[0].GetComponent<Transform>().position, new Vector3());

            foreach (var point in pointsBase)
            {
                bounds.Encapsulate(point.GetComponent<Transform>().position);
            }

            //bounds.Expand(bounds.size.magnitude /** 1.01f*/);

            Bounds screenBounds = GetWorldDrawZone();

            float scaleX = bounds.size.x / screenBounds.size.x;
            float scaleY = bounds.size.y / screenBounds.size.y;

            float finalScale = (scaleX > scaleY ? scaleX : scaleY);

            Camera.main.GetComponent<Transform>().position = new Vector3(bounds.center.x, bounds.center.y, -10);
            Camera.main.GetComponent<Camera>().orthographicSize *= finalScale;
        }
        else
        {
            Camera.main.GetComponent<Transform>().position = new Vector3(0, 0, -10);
            Camera.main.GetComponent<Camera>().orthographicSize = standartCameraScale;
        }

    }

    public void ExitFromSketcher()
    {
        Application.Quit();
    }

    public void ReturnToMenu()
    {
        StartCoroutine(LoadAsynchronously("MainMenuScene"));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            slider.value = progress;
            progressText.text = progress * 100f + "%";

            yield return null;
        }
    }

    public static void clearSelectedConstrains()
    {
        if(selectedConstrains.Count > 0)
        {
            foreach(var constrain in selectedConstrains)
            {
                constrain.GetComponent<ConstrainController>().UpdateOutline(false);
            }
            selectedConstrains.Clear();
        }
    }

    public static void clearSelectedObjects()
    {
        if (selectedLines.Count > 0)
        {
            foreach (var line in selectedLines)
            {
                line.GetComponent<LineRenderer>().material = staticStandartMaterial;
            }
            selectedLines.Clear();
        }

        if (selectedPoints.Count > 0)
        {
            foreach (var point in selectedPoints)
            {
                point.GetComponent<SpriteRenderer>().color = staticStandartColor;
            }
            selectedPoints.Clear();
        }
    }

    private static bool calculateB(ref List<double> B)
    {
        B.Clear();
        bool tolerance = true;
        double equationValue;
        if (constrainsBase.Count > 0)
            foreach(var constrain in constrainsBase)
            {
                int equationCount = constrain.GetComponent<ConstrainController>().GetEquationsCount();
                for (int i = 0; i < equationCount; i++)
                {
                    equationValue = constrain.GetComponent<ConstrainController>().GetEquationValue(i);

                    if (Math.Abs(equationValue) > constrainEPS)
                    {
                        tolerance = false;
                    }
                    B.Add(equationValue);
                }
            }
        return tolerance;
    }

    public static void SolveConsistent(List<List<double>> A, List<double> B, ref double[] X)
    {
        var rows = A.Count;
        if (rows == 0)
            return;
        var cols = A[0].Count;

        double t = 0.0;

        for (int r = 0; r < rows; r++)
        {

            var mr = r;
            double max = 0.0;
            for (int rr = r; rr < rows; rr++)
            {
                if (Math.Abs(A[rr][r]) <= max) continue;
                max = Math.Abs(A[rr][r]);
                mr = rr;
            }

            if (max < constrainEPS) continue;

            for (int c = 0; c < cols; c++)
            {
                t = A[r][c];
                A[r][c] = A[mr][c];
                A[mr][c] = t;
            }

            t = B[r];
            B[r] = B[mr];
            B[mr] = t;

            for( int rr = r + 1; rr < rows; rr++)
            {

                double coef = A[rr][r] / A[r][r];
                for(int c = 0; c < cols; c++)
                {
                    A[rr][c] -= A[r][c] * coef;
                }
                B[rr] -= B[r] * coef;
            }
        }

        for (int r = rows - 1; r >= 0; r--)
        {
            if (Math.Abs(A[r][r]) < constrainEPS) continue;
            double xx = B[r] / A[r][r];
            for (int rr = rows - 1; rr > r; rr--)
            {
                xx -= X[rr] * A[r][rr] / A[r][r];
            }
            X[r] = xx;
        }
    }

    public static void SolveLeastSquaresConsistent(List<List<double>> A, List<double> B, ref List<double> X)
    {
        List<List<double>> AAT = new List<List<double>>();
        Stopwatch timer = new Stopwatch();

        timer.Start();


        // A^T * A * X = A^T * B
        var rows = A.Count;
        if (rows == 0)
            return;
        var cols = A[0].Count;

        for (int r = 0; r < rows; r++)
        {
            AAT.Add(new List<double>());
            for (int c = 0; c < rows; c++)
            {
                double sum = 0.0;
                for (int i = 0; i < cols; i++)
                {
                    if (A[c][i] == 0 || A[r][i] == 0) continue;
                    sum += A[r][i] * A[c][i];
                }
                AAT[r].Add(sum);
            }
        }

        double[] Z = new double[B.Count];
        
        SolveConsistent(AAT, B, ref Z);

        for (int c = 0; c < cols; c++)
        {
            double sum = 0.0;
            for (int r = 0; r < rows; r++)
            {
                sum += Z[r] * A[r][c];
            }
            X.Add(sum);
        }



        timer.Stop();


        //UnityEngine.Debug.LogFormat("Consistent = {0}", timer.ElapsedTicks);
    }

    public static void SolveParallel(double[,] A, List<double> B, ref double[] X)
    {
        var rows = A.GetLength(0);
        if (rows == 0) return;
        var cols = A.GetLength(1);
        double t = 0.0;

        for (int r = 0; r < rows; r++)
        {

            var mr = r;
            double max = 0.0;
            for (int rr = r; rr < rows; rr++)
            {
                if (Math.Abs(A[rr,r]) <= max) continue;
                max = Math.Abs(A[rr,r]);
                mr = rr;
            }

            if (max < constrainEPS) continue;

            Parallel.For(0, cols, (c) =>
            {
                t = A[r,c];
                A[r,c] = A[mr,c];
                A[mr,c] = t;
            });

            t = B[r];
            B[r] = B[mr];
            B[mr] = t;

            Parallel.For(r + 1, rows, (rr) =>
            {

                double coef = A[rr,r] / A[r,r];
                for (int c = 0; c < cols; c++)
                {
                    A[rr,c] -= A[r,c] * coef;
                }
                B[rr] -= B[r] * coef;
            });
        }

        for (int r = rows - 1; r >= 0; r--)
        {
            if (Math.Abs(A[r,r]) < constrainEPS) continue;
            double xx = B[r] / A[r,r];
            for (int rr = rows - 1; rr > r; rr--)
            {
                xx -= X[rr] * A[r,rr] / A[r,r];
            }
            X[r] = xx;
        }
    }

    public static void SolveLeastSquaresParallel(List<List<double>> A, List<double> B, double[] X)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        var rows = A.Count;
        if (rows == 0) return;
        var cols = A[0].Count;
        double[,] AAT = new double[rows, rows];

        Parallel.For(0, rows, (r) =>
        {
            for (int c = 0; c < rows; c++)
            {
                double sum = 0.0;
                for (int i = 0; i < cols; i++)
                {
                    if (A[c][i] == 0 || A[r][i] == 0) continue;
                    sum += A[r][i] * A[c][i];
                }
                AAT[r,c] = sum;
            }
        });

        double[] Z = new double[B.Count];

        SolveParallel(AAT, B, ref Z);

        Parallel.For(0, cols, (c) =>
        {
            double sum = 0.0;
            for (int r = 0; r < rows; r++)
            {
                sum += Z[r] * A[r][c];
            }
            X[c] = sum;
        });

        timer.Stop();

        //UnityEngine.Debug.LogFormat("Parallel = {0}", timer.ElapsedTicks);
    }


    public static void SaveOrLoadParameters(bool isSaveParams)
    {     
        if (constrainsBase.Count > 0)
        {
            List<ConstrainParam> tempParamsList = new List<ConstrainParam>();

            GetConstrainParamsList(ref tempParamsList);

            if (tempParamsList.Count > 0)
            {
                if (isSaveParams)
                {
                    paramsSaved.Clear();

                    foreach (var param in tempParamsList)
                    {
                        if (param.isX)
                        {
                            paramsSaved.Add(param.point.GetComponent<Transform>().position.x);
                        }
                        else
                        {
                            paramsSaved.Add(param.point.GetComponent<Transform>().position.y);
                        }
                    }
                }
                else
                {
                    if (paramsSaved.Count > 0)
                    {
                        for (int i = 0; i < tempParamsList.Count; i++)
                        {
                            Vector3 pos = tempParamsList[i].point.GetComponent<Transform>().position;

                            if (tempParamsList[i].isX)
                                pos.x = (float)paramsSaved[i];
                            else
                                pos.y = (float)paramsSaved[i];

                            tempParamsList[i].point.GetComponent<PointController>().SetCoordinates(pos);
                        }
                    }
                }
            }

            tempParamsList.Clear();
        }
    }

    public static void GetConstrainParamsList(ref List<ConstrainParam> paramsOut)
    {
        paramsOut.Clear();

        foreach (var constrain in constrainsBase)
        {
            int paramsCount = constrain.GetComponent<ConstrainController>().GetParamCount();
            for (int i = 0; i < paramsCount; i++)
            {
                var curParam = constrain.GetComponent<ConstrainController>().GetParam(i);
                if (!paramsOut.Contains(curParam)) //ПРОВЕРИТЬ НА ИДЕНТИЧНОСТЬ ОБЪЕКТОВ
                {
                    paramsOut.Add(curParam);
                }
            }

        }
    }

    private static bool isOverConstrain(int paramsCurrentCount)
    {
        int numOfEquation = 0;

        foreach (var constrain in constrainsBase)
        {
            numOfEquation += constrain.GetComponent<ConstrainController>().GetEquationsCount();
        }

        return numOfEquation > paramsCurrentCount;
    }



    public static void UpdateConstrains()
    {

        List<ConstrainParam> paramsCurrent = new List<ConstrainParam>();        
        List<double> valueParams = new List<double>();
        List<double> BConsistent = new List<double>();
        List<double> XConsistent;
        List<List<double>> JacConsistent = new List<List<double>>();

        int maxSteps = 500;

        if(constrainsBase.Count > 0)
        {
            GetConstrainParamsList(ref paramsCurrent);

            if (!isOverConstrain(paramsCurrent.Count))
            {

                while (!calculateB(ref BConsistent) && maxSteps > 0)
                {
                    maxSteps--;
                    JacConsistent.Clear();

                    //составление Якобиана

                    foreach (var constrain in constrainsBase)
                    {
                        int numOfEquations = constrain.GetComponent<ConstrainController>().GetEquationsCount();
                        for (int i = 0; i < numOfEquations; i++)
                        {
                            JacConsistent.Add(new List<double>());
                            foreach (var curParam in paramsCurrent)
                            {
                                double deriv = constrain.GetComponent<ConstrainController>().GetDerivative(i, curParam);
                                JacConsistent[JacConsistent.Count - 1].Add(deriv);
                            }
                        }
                    }

                    //Решение МНК

                    if (constrainsBase.Count < 60)
                    {
                        double[] XParallel = new double[paramsCurrent.Count];
                        SolveLeastSquaresParallel(JacConsistent, BConsistent, XParallel);
                        XConsistent = new List<double>(XParallel);
                    }
                    else
                    {
                        XConsistent = new List<double>();
                        SolveLeastSquaresConsistent(JacConsistent, BConsistent, ref XConsistent);
                    }

                    //Выставление смещений согласно найденному решению

                    for (int i = 0; i < paramsCurrent.Count; i++)
                    {
                        Vector3 pos = paramsCurrent[i].point.GetComponent<Transform>().position;

                        if (paramsCurrent[i].isX)
                            pos.x -= (float)XConsistent[i];
                        else
                            pos.y -= (float)XConsistent[i];

                        paramsCurrent[i].point.GetComponent<PointController>().SetCoordinates(pos);
                    }
                }

                if (maxSteps == 0)
                {
                    SaveOrLoadParameters(false);
                }

                foreach (var constrain in constrainsBase)
                {
                    constrain.GetComponent<ConstrainController>().UpdatePositionConstrain();
                }
            }
            else SaveOrLoadParameters(false);
        }
        
    }

    private int AddPointToBase(GameObject tempPoint)
    {
        int tempPointIndex = pointsBase.FindIndex(Elem => Elem.GetComponent<PointController>().ComparisonPossitionPoints(tempPoint));
        if (tempPointIndex == -1)
        {
            GameObject newPoint = Instantiate(tempPoint);
            pointsBase.Add(newPoint);
            tempPointIndex = pointsBase.FindIndex(Elem => Elem.Equals(newPoint));
        }
        return tempPointIndex;
    }

    private void AddLineToBase(GameObject tempLine, GameObject[] tempPoints)
    {
        int[] tempPointsIndex = new int[2];
        int tempLineIndex;

        for (int i = 0; i < 2; i++)
        {
            tempPointsIndex[i] = AddPointToBase(tempPoints[i]);
        }
        
        tempLineIndex = linesBase.FindIndex(Elem => Elem.GetComponent<LineController>().ComparisonPossitionLines(tempLine));
                     
        if (tempLineIndex == -1)
        {
            GameObject newLine = Instantiate(tempLine);

            for (int i = 0; i < 2; i++)
                newLine.GetComponent<LineController>().AddPoint(pointsBase[tempPointsIndex[i]]);

            for (int i = 0; i < 2; i++)
                pointsBase[tempPointsIndex[i]].GetComponent<PointController>().AddLine(newLine);

            linesBase.Add(newLine);
        }
    }

    private Vector3 Magnetic(Vector3 point)
    {
        if (pointsBase.Count > 0)
        {
            foreach (var savedPoint in pointsBase)
            {
                float deltaX = Mathf.Abs(point.x - savedPoint.GetComponent<Transform>().position.x);
                float deltaY = Mathf.Abs(point.y - savedPoint.GetComponent<Transform>().position.y);
                if (deltaX < deltaPoint && deltaY < deltaPoint)
                {
                    return savedPoint.GetComponent<Transform>().position;
                }
            }
        }
        return point;
    }

    private Bounds GetWorldDrawZone()
    {
        Vector2 minPoint = new Vector2 ();
        Vector2 maxPoint = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        maxPoint += DrawZone.GetComponent<RectTransform>().offsetMax;
        minPoint += DrawZone.GetComponent<RectTransform>().offsetMin;

        Bounds bounds = new Bounds();
        bounds.min = Camera.main.ScreenToWorldPoint(new Vector3(minPoint.x, minPoint.y, 0));
        bounds.max = Camera.main.ScreenToWorldPoint(new Vector3(maxPoint.x, maxPoint.y, 0));

        return bounds;
    }

    private Vector3 AroundPanal(Vector3 point)
    {
        Bounds bounds = GetWorldDrawZone();
        return bounds.ClosestPoint(point);
    }

    private void Point_Update()
    {
        if (drawStep == 0)
        {
            tempPoints[0] = Instantiate(pointPrefab, new Vector2(0, 0), Quaternion.identity);
            drawStep++;
        }
        if (drawStep == 1)
        {
            startMousePosition = Magnetic(AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            startMousePosition.z = 0;
            tempPoints[0].GetComponent<Transform>().position = startMousePosition;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (drawStep == 1)
            {
                AddPointToBase(tempPoints[0]);
                tempPoints[0].GetComponent<PointController>().DeletePoint();
                drawStep = 0;
            }
        }
    }

    private void Line_Update()
    {
        if(drawStep == 0)
        {
            tempLine = Instantiate(linePrefab, new Vector2(0, 0), Quaternion.identity);
            for (int i = 0; i < 2; i++)
                tempPoints[i] = Instantiate(pointPrefab, new Vector2(0, 0), Quaternion.identity);
            tempLine.SetActive(false);
            tempPoints[1].SetActive(false);
            drawStep++;
        }               
        if (drawStep == 1)
        {
            startMousePosition = Magnetic(AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            startMousePosition.z = 0;
            tempPoints[0].GetComponent<Transform>().position = startMousePosition;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (drawStep == 1)
            {
                startMousePosition = Magnetic(AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
                startMousePosition.z = 0;
                tempLine.GetComponent<LineRenderer>().SetPosition(0, startMousePosition );      
                tempLine.SetActive(true);
                tempPoints[1].GetComponent<Transform>().position = startMousePosition;
                tempPoints[1].SetActive(true);
            }
            drawStep++;
        }
        if (drawStep == 2)
        {
            endMousePosition = Magnetic(AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            endMousePosition.z = 0;
            tempLine.GetComponent<LineRenderer>().SetPosition(1, endMousePosition);
            tempPoints[1].GetComponent<Transform>().position = endMousePosition;
        }
        if (drawStep == 3)
        {
            List<Vector2> colliderPoints = new List<Vector2>();
            for (int i = 0; i < 2; i++)
                colliderPoints.Add(tempLine.GetComponent<LineRenderer>().GetPosition(i));
            tempLine.GetComponent<EdgeCollider2D>().SetPoints(colliderPoints);

            AddLineToBase(tempLine, tempPoints);

            tempLine.GetComponent<LineController>().DeleteLine();
            for (int i = 0; i < 2; i++)
                tempPoints[i].GetComponent<PointController>().DeletePoint();
            drawStep = 0;
        }
    }

    private void EscapeSequence()
    {
        if (isPointCreating)
        {
            isPointCreating = false;
            Destroy(tempPoints[0]);
            drawStep = 0;
        }
        if (isLineCreating)
        {
            isLineCreating = false;
            Destroy(tempLine);
            for (int i = 0; i < 2; i++)
                Destroy(tempPoints[i]);
            drawStep = 0;
        }
        if (isConstrainCreating)
        {
            isConstrainCreating = false;
            createdCostrain.GetComponent<ConstrainController>().DeleteConstrain();
        }

        if (!isDragSelectedObjects && (selectedPoints.Count > 0 || selectedLines.Count > 0 || selectedConstrains.Count > 0))
        {
            clearSelectedConstrains();
            clearSelectedObjects();
        }

    }    

    private void Update()
    {
        if (isPointCreating) Point_Update();
        if (isLineCreating) Line_Update();
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (selectedLines.Count != 0)
            {
                foreach (var line in selectedLines)
                {
                    
                    line.GetComponent<LineController>().DeleteLine();
                }
                selectedLines.Clear();

            }

            if (selectedPoints.Count != 0)
            {
                foreach (var point in selectedPoints)
                {
                    pointsBase.Remove(point);
                    point.GetComponent<PointController>().DeletePoint();
                }
                selectedPoints.Clear();                
            }

            if(selectedConstrains.Count != 0)
            {
                foreach(var constrain in selectedConstrains)
                {
                    constrain.GetComponent<ConstrainController>().DeleteConstrain();
                }
                selectedConstrains.Clear();
            }


            UpdateConstrains();
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            EscapeSequence();
        }
        if (Input.GetMouseButton(2))
        {
            
            Vector3 currentCameraPosition = Camera.main.GetComponent<Transform>().position;
            currentCameraPosition.x -= Input.GetAxis("Mouse X") * Camera.main.GetComponent<Camera>().orthographicSize / standartCameraScale;
            currentCameraPosition.y -= Input.GetAxis("Mouse Y") * Camera.main.GetComponent<Camera>().orthographicSize / standartCameraScale;
            Camera.main.GetComponent<Transform>().position = currentCameraPosition;
        }
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > dragEpsSelectedObjects)
        {

            float cameraSize = Camera.main.GetComponent<Camera>().orthographicSize;

            cameraSize -= Input.mouseScrollDelta.y * scaleSize;
            if (cameraSize < 1f) cameraSize = 1.0f;
            Camera.main.GetComponent<Camera>().orthographicSize = cameraSize;
        }
        if (Input.GetMouseButton(0) && (selectedPoints.Count > 0 || selectedLines.Count > 0) && (Mathf.Abs(Input.GetAxis("Mouse X")) > dragEpsSelectedObjects || Mathf.Abs(Input.GetAxis("Mouse Y")) > dragEpsSelectedObjects) && !isDragSelectedObjects)
        {
            isDragSelectedObjects = true;
        }
        if (Input.GetMouseButtonUp(0) && (selectedPoints.Count > 0 || selectedLines.Count > 0) && isDragSelectedObjects)
        {
            isDragSelectedObjects = false;
            dragStep = 0;
            dragSelectedObjects.Clear();
        }
        if (isDragSelectedObjects)
        {
            EscapeSequence();

            float scaleCoeff = Camera.main.GetComponent<Camera>().orthographicSize / standartCameraScale;
            biasDrag = new Vector3(Input.GetAxis("Mouse X") * scaleCoeff, Input.GetAxis("Mouse Y") * scaleCoeff, 0);

            if(dragStep == 0)
            {
                if (selectedLines.Count > 0)
                {
                    foreach (var line in selectedLines)
                    {
                        foreach (var point in line.GetComponent<LineController>().points)
                        {
                            int indexPoint = dragSelectedObjects.FindIndex(Elem => Elem.Equals(point));
                            if (indexPoint == -1)
                            {
                                dragSelectedObjects.Add(point);
                            }
                        }
                    }
                }

                if (selectedPoints.Count > 0)
                {
                    foreach (var point in selectedPoints)
                    {
                        int indexPoint = dragSelectedObjects.FindIndex(Elem => Elem.Equals(point));
                        if (indexPoint == -1)
                        {
                            dragSelectedObjects.Add(point);
                        }
                    }
                }

                dragStep++;
            }

            if(dragStep == 1)
            {
                SaveOrLoadParameters(true);

                foreach (var point in dragSelectedObjects)
                {
                    point.GetComponent<PointController>().BiasCoordinates(biasDrag);
                }

                UpdateConstrains();
            }
        }
        if (isConstrainCreating)
        {
            if (createdCostrain.GetComponent<ConstrainController>().CreateConstrain())
            {
                GameObject temp = Instantiate(createdCostrain);
                temp.GetComponent<ConstrainController>().InitConstrain();

                constrainsBase.Add(temp);

                constrainsBase.Sort(delegate (GameObject constrain1, GameObject constrain2)
                {
                    if (constrain1.GetComponent<ConstrainController>().GetConstrainPriority() < constrain2.GetComponent<ConstrainController>().GetConstrainPriority())
                        return -1;
                    else if (constrain1.GetComponent<ConstrainController>().GetConstrainPriority() > constrain2.GetComponent<ConstrainController>().GetConstrainPriority())
                        return 1;
                    else
                        return 0;
                });


                isConstrainCreating = false;
                createdCostrain.GetComponent<ConstrainController>().DeleteConstrain();

                SaveOrLoadParameters(true);
                UpdateConstrains();
            }
        }
    }
}

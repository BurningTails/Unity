using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkynetController : MonoBehaviour
{
    public static List<GameObject> positionsBase = new List<GameObject>();
    public static List<GameObject> transitionsBase = new List<GameObject>();
    public static List<GameObject> connectorsBase = new List<GameObject>();

    public static List<GameObject> selectedPositions = new List<GameObject>();
    public static List<GameObject> selectedTransitions = new List<GameObject>();
    public static List<GameObject> selectedConnectors = new List<GameObject>();

    private List<List<int>> matrixW_minus = new List<List<int>>();
    private List<List<int>> matrixW_plus = new List<List<int>>();
    private List<List<int>> matrixW = new List<List<int>>();
    private List<int> bannedMatrixColumns = new List<int>();
    private List<int> bannedMatrixRows = new List<int>();

    private List<GameObject> statesBase = new List<GameObject>();
    private List<GameObject> linesBase = new List<GameObject>();

    public GameObject positionPrefab;
    public GameObject transitionPrefab;
    public GameObject connectorPrefab;
    public GameObject statePrefab;
    public GameObject linePrefab;

    public GameObject DrawZone;
    public GameObject modifyMarkersButton;

    public Color outlineBanned;
    public Color standartColorGraph;
    public Color outlineDeadEndColorGraph;
    public Color outlineLoopColorGraph;

    public static GameObject tempPosition;
    public static GameObject tempTransition;
    public static GameObject tempConnector;

    private Vector3 startMousePosition;
    private Vector3 endMousePosition;
    private Vector3 biasDrag;
    private Vector3 basePointToGraph;

    public float graphLevelWidth;
    public float graphLevelHeight;
    public Vector3 biasForGraph;
    public Vector3 biasForLines;


    public static bool isPositionCreated = false;
    public static bool isTransitionCreated = false;
    public static bool isConnectorCreated = false;
    public static bool isDragSelectedObjects = false;
    public static bool isModifyMarkers = false;
    public static bool isAddMarker = false;
    public static bool isVisualiseGraph = false;

    private int positionCreateStep = 0;
    private int transitionCreateStep = 0;
    private int connectorCreateStep = 0;

    public static float dragEpsSelectedObjects = 0.01f;
    public float scaleSize = 1.0f;
    public float standartCameraScale = 10;

    public static int uniqueIDPosition = 0;
    public static int uniqueIDTransition = 0;

    public Text info;
    public static Text staticInfo;
    public static SkynetController globalRef;


    public struct GraphLevel
    {
        public List<int> currentState;
        public int indexTransition;
        public List<GraphLevel> depthStates;
        public bool isLoop;

        public GraphLevel(List<int> in_currentState, int in_indexTransition, List<GraphLevel> in_depthStates, bool in_isLoop)
        {
            currentState = new List<int>(in_currentState);
            indexTransition = in_indexTransition;
            depthStates = new List<GraphLevel>(in_depthStates);
            isLoop = in_isLoop;
        }
    }


    private void Start()
    {
        globalRef = this;
        staticInfo = info;
    } 

    public void infoMessage(string input)
    {
        StartCoroutine(messageCoroutine(input));
    }

    public static IEnumerator messageCoroutine(string inputText)
    {
        Color textColor;
        if (staticInfo.color.a >= 0.01f)
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


    public void createPosition()
    {
        EscapeSequence();
        isPositionCreated = true;
    }

    public void createTransition()
    {
        EscapeSequence();
        isTransitionCreated = true;
    }

    public void createConnector()
    {
        EscapeSequence();
        isConnectorCreated = true;
    }

    public void changeMarkers()
    {
        EscapeSequence();
        isModifyMarkers = true;
        modifyMarkersButton.GetComponent<Button>().interactable = false;
    }


    public void calculateReachabilityGraph()
    {
        EscapeSequence();
        isVisualiseGraph = true;

        if (!calculateStartGraphPos())
        {
            infoMessage("Empty graph");
        }
        else
        {
            GraphLevel graph = new GraphLevel();
            bool isCalculated = calculateGraph(ref graph);

            if (isCalculated)
            {
                int emptyCol = recursiveVisualiseGraph(graph, 0, 0, new Vector3(0, 0, 0));
            }
        }
    }   

    private bool fillMatrixM_minus()
    {
        bool findConnector;

        //вертикаль - позиции
        //горизонталь - переходы
        //пересечение - количество связей из позиции в переход
        //добавить очищение массивов матриц


        if (positionsBase.Count > 0 && transitionsBase.Count > 0)
        {
            foreach (var pos in positionsBase)
            {
                List<int> tempRow = new List<int>();

                foreach (var trans in transitionsBase)
                {
                    PositionController tempPC = pos.GetComponent<PositionController>();
                    TransitionController tempTC = trans.GetComponent<TransitionController>();
                    findConnector = false;

                    if (tempPC.RightConnectors.Count > 0)
                    {
                        foreach (var connector in tempPC.RightConnectors)
                        {
                            if (connector.GetComponent<ConnectorController>().RightConnect.Equals(trans))
                            {
                                tempRow.Add(connector.GetComponent<ConnectorController>().connectsCount);
                                findConnector = true;
                                break;
                            }
                        }

                        if (!findConnector)
                        {
                            tempRow.Add(0);
                        }
                    }
                    else
                    {
                        tempRow.Add(0);
                    }
                }

                matrixW_minus.Add(tempRow);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool fillMatrixM_plus()
    {
        bool findConnector;

        //вертикаль - позиции
        //горизонталь - переходы
        //пересечение - количество связей из позиции в переход
        //добавить очищение массивов матриц



        if (positionsBase.Count > 0 && transitionsBase.Count > 0)
        {
            foreach (var pos in positionsBase)
            {
                List<int> tempRow = new List<int>();

                foreach (var trans in transitionsBase)
                {
                    PositionController tempPC = pos.GetComponent<PositionController>();
                    TransitionController tempTC = trans.GetComponent<TransitionController>();
                    findConnector = false;

                    if (tempPC.LeftConnectors.Count > 0)
                    {
                        foreach (var connector in tempPC.LeftConnectors)
                        {
                            if (connector.GetComponent<ConnectorController>().LeftConnect.Equals(trans))
                            {
                                tempRow.Add(connector.GetComponent<ConnectorController>().connectsCount);
                                findConnector = true;
                                break;
                            }
                        }

                        if (!findConnector)
                        {
                            tempRow.Add(0);
                        }
                    }
                    else
                    {
                        tempRow.Add(0);
                    }
                }

                matrixW_plus.Add(tempRow);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private void fillMatrixM()
    {
        for (int i = 0; i < matrixW_minus.Count; i++) 
        {
            List<int> tempRow = new List<int>();

            for (int j = 0; j < matrixW_minus[i].Count; j++) 
            {
                tempRow.Add(matrixW_plus[i][j] - matrixW_minus[i][j]);
            }

            matrixW.Add(tempRow);
        }
    }

    private bool findBannedColumns()
    {
        bool isNull;

        if (matrixW_minus.Count > 0)
        {
            for (int j = 0; j < matrixW_minus[0].Count; j++)
            {
                isNull = true;

                for (int i = 0; i < matrixW_minus.Count; i++)
                {
                    if (matrixW_minus[i][j] != 0)
                    {
                        isNull = false;
                        break;
                    }
                }

                if (isNull)
                {
                    if (!bannedMatrixColumns.Contains(j))
                    {
                        bannedMatrixColumns.Add(j);
                    }
                }
            }
        }

        if (matrixW_plus.Count > 0)
        {
            for (int j = 0; j < matrixW_plus[0].Count; j++)
            {
                isNull = true;

                for (int i = 0; i < matrixW_plus.Count; i++)
                {
                    if (matrixW_plus[i][j] != 0)
                    {
                        isNull = false;
                        break;
                    }
                }

                if (isNull)
                {
                    if (!bannedMatrixColumns.Contains(j))
                    {
                        bannedMatrixColumns.Add(j);
                    }
                }
            }
        }

        if (bannedMatrixColumns.Count > 0) return false;
        return true;
    }

    private bool findBannedRows()
    {
        bool isLeftEmpty;
        bool isRightEmpty;

        for (int i = 0; i < matrixW_minus.Count; i++)
        {
            isLeftEmpty = true;
            isRightEmpty = true;

            for (int j = 0; j < matrixW_minus[i].Count; j++)
            {
                if (matrixW_minus[i][j] != 0)
                {
                    isRightEmpty = false;
                    break;
                }
            }

            for (int j = 0; j < matrixW_plus[i].Count; j++)
            {
                if (matrixW_plus[i][j] != 0)
                {
                    isLeftEmpty = false;
                    break;
                }
            }

            if (isLeftEmpty && isRightEmpty)
            {
                if (!bannedMatrixRows.Contains(i))
                {
                    bannedMatrixRows.Add(i);
                }
            }
        }

        if (bannedMatrixRows.Count > 0) return false;
        return true;
    }

    private bool findBannedColumnsAndRows()
    {
        bool isRowsBanned = findBannedRows();
        bool isColumnsBanned = findBannedColumns();

        if (!isRowsBanned || !isColumnsBanned) return false;
        return true; 
    }

    private bool findLoop(List<int> currentState, List<List<int>> allBranchStates)
    {
        if (allBranchStates.Count == 0) return false;

        bool isEqual;

        foreach(var state in allBranchStates)
        {
            isEqual = true;

            for (int i = 0; i < state.Count; i++)
            {
                if (state[i] != currentState[i])
                {
                    isEqual = false;
                    break;
                }
            }

            if(isEqual)
            {
                return true;
            }
        }

        return false;
    }




    public GraphLevel recursiveCreateGraph(List<int> currentState, int indexTransition, List<List<int>> allBranchStates)
    {
        List<int> activeTransitions = new List<int>();
        bool isActive;

        //вычисление индексов активных переходов

        for (int trans = 0; trans < transitionsBase.Count; trans++)
        {
            isActive = true;

            for(int pos = 0; pos < positionsBase.Count; pos++)
            {
                if(matrixW_minus[pos][trans] > currentState[pos])
                {
                    isActive = false;
                    break;
                }
            }

            if(isActive)
            {
                activeTransitions.Add(trans);
            }
        }

        
        if(activeTransitions.Count == 0)
        {
            //Тупиковое состояние - нет активных переходов

            return new GraphLevel(currentState, indexTransition, new List<GraphLevel>(), false);
        }

        //если переходы имеются, цикл по всем переходам

        List<GraphLevel> tempListGL = new List<GraphLevel>();

        foreach(var aTrans in activeTransitions)
        {
            //расчёт следующего состояния по текущему переходу: 

            List<int> nextState = new List<int>();

            for (int i = 0; i < matrixW.Count; i++)
            {
                nextState.Add(matrixW[i][aTrans] + currentState[i]);                
            }

            if(findLoop(nextState, allBranchStates))
            {
                //если нашел цикл

                tempListGL.Add(new GraphLevel(nextState, aTrans, new List<GraphLevel>(), true));
                continue;
            }

            List<List<int>> tempAllBranchStates = new List<List<int>>(allBranchStates);
            tempAllBranchStates.Add(currentState);

            GraphLevel tempGL = recursiveCreateGraph(nextState, aTrans, tempAllBranchStates);

            tempListGL.Add(tempGL);
        }


        //составление структуры

        return new GraphLevel(currentState, indexTransition, tempListGL, false);
    }


    private void findCurrentState(ref List<int> currentState)
    {
        currentState.Clear();

        if(positionsBase.Count > 0)
        {
            foreach(var pos in positionsBase)
            {
                currentState.Add(pos.GetComponent<PositionController>().numOfChips);
            }
        }
    }

    private Vector3 createState(GraphLevel currentDepth, int numberOfDepth, int emptyCol, Color inputColor)
    {
        float tempX = basePointToGraph.x + graphLevelWidth * emptyCol;
        float tempY = basePointToGraph.y - graphLevelHeight * numberOfDepth;

        Vector3 tempPositionPos = new Vector3(tempX, tempY, 0);

        GameObject tempState = Instantiate(statePrefab, tempPositionPos, Quaternion.identity);
        tempState.GetComponent<StateController>().setTransitionName(currentDepth.indexTransition);
        tempState.GetComponent<StateController>().setPositions(currentDepth.currentState);
        tempState.GetComponent<StateController>().outlineState(inputColor);

        statesBase.Add(tempState);

        return tempPositionPos;
    }

    private void createLine(Vector3 pos1, Vector3 pos2)
    {
        GameObject tempLine = Instantiate(linePrefab, pos1, Quaternion.identity);

        tempLine.GetComponent<LineRenderer>().SetPosition(0, pos1);
        tempLine.GetComponent<LineRenderer>().SetPosition(1, pos2);

        linesBase.Add(tempLine);
    }

    private int recursiveVisualiseGraph(GraphLevel currentState, int numberOfDepth, int emptyCol, Vector3 positionParent)
    {
        Vector3 currentPosition;
        int currentEmptyCol = emptyCol;

        if (currentState.depthStates.Count == 0)
        {
            if (!currentState.isLoop)
                currentPosition = createState(currentState, numberOfDepth, emptyCol, outlineDeadEndColorGraph);
            else
                currentPosition = createState(currentState, numberOfDepth, emptyCol, outlineLoopColorGraph);

            if (numberOfDepth != 0 || emptyCol != 0) createLine(currentPosition, positionParent);

            return emptyCol + 1;
        }

        currentPosition = createState(currentState, numberOfDepth, emptyCol, standartColorGraph);
        if (numberOfDepth != 0 || emptyCol != 0) createLine(currentPosition, positionParent);


        foreach (var levelState in currentState.depthStates)
        {
            currentEmptyCol = recursiveVisualiseGraph(levelState, numberOfDepth + 1, currentEmptyCol, currentPosition);            
        }

        return currentEmptyCol;
    }



    private bool calculateStartGraphPos()
    {
        Bounds allObjectsBounds = calculateZoomBounds();

        if(allObjectsBounds.size == Vector3.zero)
        {
            return false;
        }

        basePointToGraph = allObjectsBounds.extents + biasForGraph;

        return true;
    }

    private bool calculateGraph(ref GraphLevel graph)
    {
        if(!fillMatrixM_minus())
        {
            infoMessage("Schema not defined");
            return false;
        }
        if(!fillMatrixM_plus())
        {
            infoMessage("Schema not defined");
            return false;
        }
        if(!findBannedColumnsAndRows())
        {
            infoMessage("Schema has invalid Positions or Transitions");
            outlineBannedPosAndTrans(true);
            return false;
        }

        fillMatrixM();

        List<int> currentState = new List<int>();
        List<List<int>> allBranchStates = new List<List<int>>();

        findCurrentState(ref currentState);

        graph = recursiveCreateGraph(currentState, -1, allBranchStates);

        return true;
    }

    private Bounds calculateZoomBounds()
    {
        Bounds bounds = new Bounds();

        if (positionsBase.Count > 0)
        {
            foreach (var pos in positionsBase)
            {
                bounds.Encapsulate(pos.GetComponent<Transform>().position);
            }
        }

        if (transitionsBase.Count > 0)
        {
            foreach (var trans in transitionsBase)
            {
                bounds.Encapsulate(trans.GetComponent<Transform>().position);
            }
        }

        return bounds;
    }

    private void outlineBannedPosAndTrans(bool isOutline)
    {
        if(positionsBase.Count > 0)
        {
            if(bannedMatrixRows.Count > 0)
            {
                for (int i = 0; i < bannedMatrixRows.Count; i++)
                {
                    if(isOutline)
                        positionsBase[bannedMatrixRows[i]].GetComponent<PositionController>().UpdateOutline(outlineBanned);
                    else
                        positionsBase[bannedMatrixRows[i]].GetComponent<PositionController>().UpdateOutline(false);
                }
            }
        }
        if (transitionsBase.Count > 0)
        {
            if (bannedMatrixColumns.Count > 0)
            {
                for (int i = 0; i < bannedMatrixColumns.Count; i++)
                {
                    if (isOutline)
                        transitionsBase[bannedMatrixColumns[i]].GetComponent<TransitionController>().UpdateOutline(outlineBanned);
                    else
                        transitionsBase[bannedMatrixColumns[i]].GetComponent<TransitionController>().UpdateOutline(false);
                }
            }
        }

    }

    public void clearSelectedObjects()
    {
        if(selectedPositions.Count > 0)
        {
            foreach(var position in selectedPositions)
            {
                position.GetComponent<PositionController>().UpdateOutline(false);
            }
            selectedPositions.Clear();
        }

        if (selectedTransitions.Count > 0)
        {
            foreach (var transition in selectedTransitions)
            {
                transition.GetComponent<TransitionController>().UpdateOutline(false);
            }
            selectedTransitions.Clear();
        }

        if (selectedConnectors.Count > 0)
        {
            foreach(var connector in selectedConnectors)
            {
                connector.GetComponent<ConnectorController>().UpdateOutline(false);
            }
            selectedConnectors.Clear();
        }

    }

    private void DeleteSelectedObjects()
    {
        List<GameObject> tempConnectorsList = new List<GameObject>();
        
        if(selectedPositions.Count > 0)
        {
            foreach(var pos in selectedPositions)
            {
                PositionController tempPC = pos.GetComponent<PositionController>();

                if(tempPC.LeftConnectors.Count > 0)
                {
                    foreach(var connector in tempPC.LeftConnectors)
                    {
                        if (!tempConnectorsList.Contains(connector))
                        {
                            tempConnectorsList.Add(connector);
                        }
                    }
                }

                if (tempPC.RightConnectors.Count > 0)
                {
                    foreach (var connector in tempPC.RightConnectors)
                    {
                        if (!tempConnectorsList.Contains(connector))
                        {
                            tempConnectorsList.Add(connector);
                        }
                    }
                }
            }
        }

        if (selectedTransitions.Count > 0)
        {
            foreach (var trans in selectedTransitions)
            {
                TransitionController tempTC = trans.GetComponent<TransitionController>();

                if (tempTC.LeftConnectors.Count > 0)
                {
                    foreach (var connector in tempTC.LeftConnectors)
                    {
                        if (!tempConnectorsList.Contains(connector))
                        {
                            tempConnectorsList.Add(connector);
                        }
                    }
                }

                if (tempTC.RightConnectors.Count > 0)
                {
                    foreach (var connector in tempTC.RightConnectors)
                    {
                        if (!tempConnectorsList.Contains(connector))
                        {
                            tempConnectorsList.Add(connector);
                        }
                    }
                }
            }
        }

        if(selectedConnectors.Count > 0)
        {
            foreach(var connector in selectedConnectors)
            {
                if (!tempConnectorsList.Contains(connector))
                {
                    tempConnectorsList.Add(connector);
                }
            }
        }

        if(tempConnectorsList.Count > 0)
        {
            foreach (var connector in tempConnectorsList)
            {
                connector.GetComponent<ConnectorController>().DeleteConnector();
            }
        }

        if (selectedPositions.Count > 0)
        {
            foreach (var position in selectedPositions)
            {
                position.GetComponent<PositionController>().DeletePosition();
            }
        }

        if (selectedTransitions.Count > 0)
        {
            foreach (var transition in selectedTransitions)
            {
                transition.GetComponent<TransitionController>().DeleteTransition();
            }
        }

        clearSelectedObjects();
    }

    private Bounds GetWorldDrawZone()
    {
        Vector2 minPoint = new Vector2();
        Vector2 maxPoint = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        maxPoint += DrawZone.GetComponent<RectTransform>().offsetMax;
        minPoint += DrawZone.GetComponent<RectTransform>().offsetMin;

        Bounds bounds = new Bounds();
        bounds.min = Camera.main.ScreenToWorldPoint(new Vector3(minPoint.x + 10.0f, minPoint.y + 10.0f, 0));
        bounds.max = Camera.main.ScreenToWorldPoint(new Vector3(maxPoint.x - 10.0f, maxPoint.y - 10.0f, 0));

        return bounds;
    }

    public Vector3 AroundPanal(Vector3 point)
    {
        Bounds bounds = GetWorldDrawZone();
        return bounds.ClosestPoint(point);
    }

    private void EscapeSequence()
    {
        if (isPositionCreated)
        { 
            tempPosition.GetComponent<PositionController>().DeletePosition();
            isPositionCreated = false;
            positionCreateStep = 0;
        }
        if (isTransitionCreated)
        {
            tempTransition.GetComponent<TransitionController>().DeleteTransition();
            isTransitionCreated = false;
            transitionCreateStep = 0;
        }
        if (isConnectorCreated)
        {
            tempConnector.GetComponent<ConnectorController>().DeleteConnector();
            isConnectorCreated = false;
            connectorCreateStep = 0;
        }
        if (selectedPositions.Count > 0 || selectedTransitions.Count > 0 || selectedConnectors.Count > 0)
        {
            clearSelectedObjects();
        }       
        if(matrixW_minus.Count > 0)
        {
            matrixW_minus.Clear();
        }
        if (matrixW_plus.Count > 0)
        {
            matrixW_plus.Clear();
        }
        if (matrixW.Count > 0)
        {
            matrixW.Clear();
        }
        if (isModifyMarkers)
        {
            modifyMarkersButton.GetComponent<Button>().interactable = true;
            isAddMarker = false;
            isModifyMarkers = false;
        }
        if (isVisualiseGraph) 
        {
            if(statesBase.Count > 0)
            {
                for(int i = statesBase.Count - 1; i >= 0; i--)
                {
                    Destroy(statesBase[i]);
                }
            }
            if(linesBase.Count > 0)
            {
                for (int i = linesBase.Count - 1; i >= 0; i--)
                {
                    Destroy(linesBase[i]);
                }
            }

            outlineBannedPosAndTrans(false);

            if (bannedMatrixColumns.Count > 0) bannedMatrixColumns.Clear();
            if (bannedMatrixRows.Count > 0) bannedMatrixRows.Clear();

            isVisualiseGraph = false;
        }
    }
   
    private void PositionCreate()
    {
        if (positionCreateStep == 0)
        {
            startMousePosition = AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            startMousePosition.z = 0;
            tempPosition = Instantiate(positionPrefab, startMousePosition, Quaternion.identity);
            positionCreateStep++;
        }
        if (positionCreateStep == 1)
        {
            startMousePosition = AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            startMousePosition.z = 0;
            tempPosition.GetComponent<Transform>().position = startMousePosition;
        }
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) && positionCreateStep == 1)
        {
            tempPosition.GetComponent<PositionController>().SetPositionNameAutomatic();
            positionCreateStep = 3;
        }
        if (Input.GetMouseButtonDown(0) && positionCreateStep == 1)
        {
            tempPosition.GetComponent<PositionController>().positionNameInputField.interactable = true;
            tempPosition.GetComponent<PositionController>().positionNameInputField.ActivateInputField();
            infoMessage("Please, enter name of Position");
            positionCreateStep++;
        }
        if (positionCreateStep == 2)
        {
            if(tempPosition.GetComponent<PositionController>().positionName != string.Empty)
            {
                tempPosition.GetComponent<PositionController>().positionNameInputField.interactable = false;
                positionCreateStep++;
            }
        }
        if (positionCreateStep == 3)
        {
            GameObject tempAddToBase = Instantiate(tempPosition);

            if (!positionsBase.Contains(tempAddToBase))
            {
                positionsBase.Add(tempAddToBase);
                uniqueIDPosition++;
            }

            tempPosition.GetComponent<PositionController>().DeletePosition();

            positionCreateStep = 0;
        }
    }

    private void TransitionCreate()
    {
        if(transitionCreateStep == 0)
        {
            startMousePosition = AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            startMousePosition.z = 0;
            tempTransition = Instantiate(transitionPrefab, startMousePosition, Quaternion.identity);
            transitionCreateStep++;
        }
        if(transitionCreateStep == 1)
        {
            startMousePosition = AroundPanal(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            startMousePosition.z = 0;
            tempTransition.GetComponent<Transform>().position = startMousePosition;
        }
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) && transitionCreateStep == 1)
        {
            tempTransition.GetComponent<TransitionController>().SetTransitionNameAutomatic();
            transitionCreateStep = 3;
        }
        if (Input.GetMouseButtonDown(0) && transitionCreateStep == 1)
        {
            tempTransition.GetComponent<TransitionController>().transitionNameInputField.interactable = true;
            tempTransition.GetComponent<TransitionController>().transitionNameInputField.ActivateInputField();
            infoMessage("Please, enter name of Transition");
            transitionCreateStep++;
        }
        if (transitionCreateStep == 2)
        {
            if (tempTransition.GetComponent<TransitionController>().transitionName != string.Empty)
            {
                tempTransition.GetComponent<TransitionController>().transitionNameInputField.interactable = false;
                transitionCreateStep++;
            }
        }
        if (transitionCreateStep == 3)
        {
            GameObject tempAddToBase = Instantiate(tempTransition);

            if (!transitionsBase.Contains(tempAddToBase))
            {
                transitionsBase.Add(tempAddToBase);
                uniqueIDTransition++;
            }

            tempTransition.GetComponent<TransitionController>().DeleteTransition();

            transitionCreateStep = 0;
        }
    }

    private void ConnectorCreate()
    {
        if (connectorCreateStep == 0)
        {
            tempConnector = Instantiate(connectorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            tempConnector.SetActive(false);
            connectorCreateStep++;
        }
        if (Input.GetMouseButtonDown(0) && connectorCreateStep == 1)
        {
            if (tempConnector.GetComponent<ConnectorController>().LeftConnect != null || tempConnector.GetComponent<ConnectorController>().RightConnect != null)
            {
                connectorCreateStep++;
            }
        }
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) && connectorCreateStep == 2)
        {
            if (tempConnector.GetComponent<ConnectorController>().LeftConnect != null && tempConnector.GetComponent<ConnectorController>().RightConnect != null)
            {
                tempConnector.SetActive(true);
                tempConnector.GetComponent<ConnectorController>().UpdatePositionConnector();
                tempConnector.GetComponent<ConnectorController>().SetConnectCountAutomatic();
                tempConnector.GetComponent<ConnectorController>().countConnectsField.interactable = false;
                connectorCreateStep = 4;
            }
        }
        if (Input.GetMouseButtonDown(0) && connectorCreateStep == 2)
        {
            if (tempConnector.GetComponent<ConnectorController>().LeftConnect != null && tempConnector.GetComponent<ConnectorController>().RightConnect != null)
            {
                tempConnector.SetActive(true);
                tempConnector.GetComponent<ConnectorController>().UpdatePositionConnector();

                tempConnector.GetComponent<ConnectorController>().countConnectsField.interactable = true;
                tempConnector.GetComponent<ConnectorController>().countConnectsField.ActivateInputField();
                infoMessage("Please, enter correct number of connects");

                connectorCreateStep++;
            }
        }
        if (connectorCreateStep == 3)
        {
            if(tempConnector.GetComponent<ConnectorController>().connectsCount != 0)
            {
                tempConnector.GetComponent<ConnectorController>().countConnectsField.interactable = false;
                connectorCreateStep++;
            }
        }              
        if (connectorCreateStep == 4)
        {
            GameObject tempAddToBase = Instantiate(tempConnector);

            if(tempAddToBase.GetComponent<ConnectorController>().LeftConnect.TryGetComponent(out PositionController tempPC))
            {
                tempPC.RightConnectors.Add(tempAddToBase);
                tempAddToBase.GetComponent<ConnectorController>().RightConnect.GetComponent<TransitionController>().LeftConnectors.Add(tempAddToBase);
            }
            else
            {
                tempAddToBase.GetComponent<ConnectorController>().LeftConnect.GetComponent<TransitionController>().RightConnectors.Add(tempAddToBase);
                tempAddToBase.GetComponent<ConnectorController>().RightConnect.GetComponent<PositionController>().LeftConnectors.Add(tempAddToBase);
            }

            GameObject tempLeftConnect = tempAddToBase.GetComponent<ConnectorController>().LeftConnect;
            GameObject tempRightConnect = tempAddToBase.GetComponent<ConnectorController>().RightConnect;

            if (!connectorsBase.Exists(Elem => Elem.GetComponent<ConnectorController>().LeftConnect.Equals(tempLeftConnect) && Elem.GetComponent<ConnectorController>().RightConnect.Equals(tempRightConnect)))
            {
                connectorsBase.Add(tempAddToBase);
            }
            else
            {
                tempAddToBase.GetComponent<ConnectorController>().DeleteConnector();
            }

            tempConnector.GetComponent<ConnectorController>().DeleteConnector();

            connectorCreateStep = 0;
        }
    }

    private void Update()
    {
        if (isPositionCreated) PositionCreate();
        if (isTransitionCreated) TransitionCreate();
        if (isConnectorCreated) ConnectorCreate();
        if (isModifyMarkers)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                isAddMarker = true;
            else
                isAddMarker = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) EscapeSequence();
        if (Input.GetKeyDown(KeyCode.Delete)) DeleteSelectedObjects();
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
        if (Input.GetMouseButton(0) && (selectedPositions.Count > 0 || selectedTransitions.Count > 0) && (Mathf.Abs(Input.GetAxis("Mouse X")) > dragEpsSelectedObjects || Mathf.Abs(Input.GetAxis("Mouse Y")) > dragEpsSelectedObjects))
        {
            float scaleCoeff = Camera.main.GetComponent<Camera>().orthographicSize / standartCameraScale;
            biasDrag = new Vector3(Input.GetAxis("Mouse X") * scaleCoeff, Input.GetAxis("Mouse Y") * scaleCoeff, 0);

            if (selectedPositions.Count > 0)
            {
                foreach (var pos in selectedPositions)
                {
                    pos.GetComponent<PositionController>().BiasCoordinates(biasDrag);
                }
            }

            if (selectedTransitions.Count > 0)
            {
                foreach (var trans in selectedTransitions)
                {
                    trans.GetComponent<TransitionController>().BiasCoordinates(biasDrag);
                }
            }
        }
    }
}

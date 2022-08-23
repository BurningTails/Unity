using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct State
{
    public State(int[,] inputMatrix, int inputEvalFunc, int inputClosedIndex)
    {
        Matrix = inputMatrix;
        EvalFunc = inputEvalFunc;
        ClosedIndex = inputClosedIndex;
    }
        
    public int[,] Matrix { get; }
    public int EvalFunc { get; }
    public int ClosedIndex { get; }
}


public class DeskController : MonoBehaviour
{
    public List<GameObject> fieldsBase = new List<GameObject>();
    public GameObject fieldPrefab;
    public Vector2 deskSize = new Vector2();
    public MainController mainControllerRef;   

    public Coroutine animationCoroutine;
    public Coroutine showChangesCoroutine;
    public Coroutine calculationDeskCoroutin;

    public void InitDesk(Vector2 initDeskSize)
    {
        DeleteDesk();
        deskSize.x = initDeskSize.x;
        deskSize.y = initDeskSize.y;

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
            {
                fieldsBase.Add(Instantiate(fieldPrefab, new Vector3(0, 0, 0), Quaternion.identity));
                fieldsBase[fieldsBase.Count - 1].GetComponent<FieldContoller>().InitField(i,j);
                fieldsBase[fieldsBase.Count - 1].transform.parent = gameObject.transform;
                fieldsBase[fieldsBase.Count - 1].GetComponent<Transform>().localPosition = new Vector3(j, -i, 0);             
            }
        
        GetComponent<Transform>().position = new Vector3(0.5f + (-deskSize.y * 0.5f), -0.5f + (deskSize.x * 0.5f), 0);
    }

    public void DeleteDesk()
    {
        if(fieldsBase.Count > 0)
        {
            for(int i = fieldsBase.Count - 1; i >= 0; i --)
            {
                fieldsBase[i].GetComponent<FieldContoller>().DeleteField();
                fieldsBase.RemoveAt(i);
            }
        }
    }

    public void FillDesk()
    {
        List<int> numbers = new List<int>();

        for (int i = 0; i < deskSize.x * deskSize.y; i++)
        {
            if (fieldsBase[i].GetComponent<FieldContoller>().GetNumber() != -1) numbers.Add(i + 1);
        }

        System.Random rnd = new System.Random();
        int randomIndex;

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
            {
                if (fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().GetNumber() != -1)
                {
                    randomIndex = rnd.Next(0, numbers.Count);
                    fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().SetNumber(numbers[randomIndex]);
                    numbers.RemoveAt(randomIndex);
                }
            }
    }

    public int CheckCorrectDesk()
    {
        List<int> numbers = new List<int>();
        int tempNumber;       

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
            {
                tempNumber = fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().GetNumber();

                if (tempNumber == -1) continue;
                if (tempNumber == 0) return -1;
                if (tempNumber > deskSize.x * deskSize.y) return -2;
                if (fieldsBase[tempNumber - 1].GetComponent<FieldContoller>().GetNumber() == -1) return -4;
                if (numbers.Contains(tempNumber)) return -3;
                else numbers.Add(tempNumber);
            }

        if (numbers.Count == 0) return -5;

        return 0;
    }




    public int calculateEvalFunc(ref int[,] currentMatrix)
    {
        int countBlockElements = 0;
        int rightPlaced = 0;
        int hX = 0;

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
            {
                if (currentMatrix[i, j] == -1) countBlockElements++;
                else if (currentMatrix[i, j] == i * (int)deskSize.y + j + 1) rightPlaced++;
                else
                {
                    int tempRow = currentMatrix[i, j] / (int)deskSize.y;
                    int tempCol = currentMatrix[i, j] - tempRow * (int)deskSize.y;

                    hX += Math.Abs(i - tempRow) + Math.Abs(j - tempCol);
                }
            }

        int gX = (int)(deskSize.x * deskSize.y) - countBlockElements - rightPlaced;

        return gX + hX;
    }

    private void TransposeTwoElements(ref List<State> openList, ref List<State> closedList, ref int[,] currentMatrix, int row1, int col1, int row2, int col2)
    {
        int tempValue;
                       
        tempValue = currentMatrix[row1, col1];
        currentMatrix[row1, col1] = currentMatrix[row2, col2];
        currentMatrix[row2, col2] = tempValue;

        int fX = calculateEvalFunc(ref currentMatrix);

        openList.Add(new State(currentMatrix.Clone() as int[,], fX, closedList.Count - 1));

        tempValue = currentMatrix[row2, col2];
        currentMatrix[row2, col2] = currentMatrix[row1, col1];
        currentMatrix[row1, col1] = tempValue;
    }

    private int createTransposeStates(ref List<State> openList, ref List<State> closedList, ref int[,] currentMatrix, int curRow, int curCol)
    {
        bool isPossible = false;

        if((curRow - 1) * (int)deskSize.y + curCol >= 0)
        {
            if (currentMatrix[curRow - 1, curCol] != -1)
            {
                TransposeTwoElements(ref openList, ref closedList, ref currentMatrix, curRow, curCol, curRow - 1, curCol);
                isPossible = true;
            }
        }
        if ((curRow + 1) * (int)deskSize.y + curCol < (int)(deskSize.x * deskSize.y))
        {
            if (currentMatrix[curRow + 1, curCol] != -1)
            {
                TransposeTwoElements(ref openList, ref closedList, ref currentMatrix, curRow, curCol, curRow + 1, curCol);
                isPossible = true;
            }
        }
        if (curRow * (int)deskSize.y + (curCol + 1) < (int)(deskSize.x * deskSize.y) && curCol + 1 < (int)deskSize.y)            
        {
            if (currentMatrix[curRow, curCol + 1] != -1)
            {
                TransposeTwoElements(ref openList, ref closedList, ref currentMatrix, curRow, curCol, curRow, curCol + 1);
                isPossible = true;
            }
        }
        if (curRow * (int)deskSize.y + (curCol - 1) >= 0 && curCol - 1 > 0)
        {
            if (currentMatrix[curRow, curCol - 1] != -1)
            {
                TransposeTwoElements(ref openList, ref closedList, ref currentMatrix, curRow, curCol, curRow, curCol - 1);
                isPossible = true;
            }
        }

        if (isPossible) return 0;
        return -1;
    }

    private bool isEqualsMatrix(int[,] matrix1, ref int[,] matrix2)
    {
        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
                if (matrix1[i, j] != matrix2[i, j]) return false;
        return true;
    }

    private int createNewStates(ref List<State> openList, ref List<State> closedList)
    {        
        int[,] tempMatrix = null;

        while (openList.Count > 0)
        {
            tempMatrix = openList[0].Matrix.Clone() as int[,];

            if (closedList.FindIndex(Elem => isEqualsMatrix(Elem.Matrix, ref tempMatrix)) == -1)
            {
                closedList.Add(openList[0]);
                openList.RemoveAt(0);
                if (closedList[closedList.Count - 1].EvalFunc == 0) return 1;
                break;
            }
            else
            {
                openList.RemoveAt(0);
                if (openList.Count == 0) return -1;
            }
        }               

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
                if(tempMatrix[i,j] != -1) 
                    if(createTransposeStates(ref openList, ref closedList, ref tempMatrix, i, j) == -1) return -1;
            
        return 0;
    }

    private void ShowSolution(ref int[,] currentMatrix)
    {
        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
                if(currentMatrix[i,j] != -1) fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().SetNumber(currentMatrix[i,j]);
    }

    private int CompareStates(State st1, State st2)
    {
        return st1.EvalFunc - st2.EvalFunc;
    }

    private int calculateStatesCount(ref List<State> closedList)
    {
        int tempIndex = closedList.Count - 1;
        int counter = 0;

        while (tempIndex != 0)
        {
            tempIndex = closedList[tempIndex].ClosedIndex;
            counter++;
        }

        Debug.Log(counter.ToString());
        Debug.Log("Total count in closed list: " + closedList.Count.ToString());

        return counter;
    }

    public IEnumerator showChangesCoroutineEnum(List<Vector2> tempPoses)
    {
        FieldContoller field1 = fieldsBase[(int)tempPoses[0].x * (int)deskSize.y + (int)tempPoses[0].y].GetComponent<FieldContoller>();
        FieldContoller field2 = fieldsBase[(int)tempPoses[1].x * (int)deskSize.y + (int)tempPoses[1].y].GetComponent<FieldContoller>();

        Color tempColor;

        for (float i = 0.01f; field1.getColor() != field1.outlineColor; i+=0.01f)
        {
            tempColor = field1.getColor();
            tempColor.b = field1.standartColor.b - i;
            tempColor.r = field1.standartColor.r - i;

            field1.setColor(tempColor);
            field2.setColor(tempColor);

            yield return new WaitForSeconds(1.0f/mainControllerRef.speedOfAnimation);
        }

        for (float i = 0.01f; field1.getColor() != field1.standartColor; i += 0.01f)
        {
            tempColor = field1.getColor();
            tempColor.b = field1.outlineColor.b + i;
            tempColor.r = field1.outlineColor.r + i;

            field1.setColor(tempColor);
            field2.setColor(tempColor);

            yield return new WaitForSeconds(1.0f/ mainControllerRef.speedOfAnimation);
        }

        yield break;
    }

    private List<Vector2> findChanges(ref int[,] tempMatrix1, ref int[,] tempMatrix2)
    {
        List<Vector2> returnPoses = new List<Vector2>();

        bool isFindFirst = false;

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
                if (tempMatrix1[i, j] != tempMatrix2[i, j])
                {
                    if (!isFindFirst)
                    {
                        returnPoses.Add(new Vector2(i,j));
                        isFindFirst = true;
                    }
                    else
                    {
                        returnPoses.Add(new Vector2(i, j));
                        return returnPoses;
                    }
                }

        return null;
    }

    private void stopShowChangesCoroutine()
    {
        if(showChangesCoroutine != null)
        {
            StopCoroutine(showChangesCoroutine);

            if(fieldsBase.Count > 0)
            {
                Color tempColor = fieldsBase[0].GetComponent<FieldContoller>().standartColor;
                for (int i = 0; i < deskSize.x; i++)
                    for (int j = 0; j < deskSize.y; j++)
                        if(fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().GetNumber() != -1) fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().setColor(tempColor);
            }

            showChangesCoroutine = null;
        }
    }

    public IEnumerator animationCoroutineEnum(List<int[,]> tempList)
    {
        int[,] tempMatrixPrev;
        int[,] tempMatrixNext;

        if (tempList.Count > 0)
        {
            tempMatrixPrev = new int[(int)deskSize.x, (int)deskSize.y];

            for (int i = 0; i < deskSize.x; i++)
                for (int j = 0; j < deskSize.y; j++)
                    tempMatrixPrev[i, j] = fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().GetNumber();

            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                tempMatrixNext = tempList[i];
                List<Vector2> tempPoses = findChanges(ref tempMatrixPrev, ref tempMatrixNext);

                if (tempPoses.Count > 0)
                {
                    showChangesCoroutine = StartCoroutine(showChangesCoroutineEnum(tempPoses));
                    yield return showChangesCoroutine;

                    ShowSolution(ref tempMatrixNext);
                    yield return new WaitForSeconds(10.0f/mainControllerRef.speedOfAnimation);
                }

                tempMatrixPrev = tempMatrixNext;
            }
        }

        yield break;
    }

    public void stopAnimationCoroutine()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            stopShowChangesCoroutine();
            animationCoroutine = null;                
        }
    }

    private void startAnimationCoroutine(ref List<State> closedList)
    {
        stopAnimationCoroutine();

        int tempIndex = closedList.Count - 1;

        List<int[,]> tempList = new List<int[,]>();

        while (tempIndex != 0)
        {
            tempList.Add(closedList[tempIndex].Matrix.Clone() as int[,]);
            tempIndex = closedList[tempIndex].ClosedIndex;
        }

        mainControllerRef.startInfoCoroutine("Count iterations: " + tempList.Count.ToString());

        animationCoroutine = StartCoroutine(animationCoroutineEnum(tempList));
    }

    public void startCalculateDeskCoroutine(bool isAnimation)
    {
        stopCalculateDeskCoroutine();
        calculationDeskCoroutin = StartCoroutine(CalculateDeskEnum(isAnimation));
    }

    public void stopCalculateDeskCoroutine()
    {
        if (calculationDeskCoroutin != null)
        {
            StopCoroutine(calculationDeskCoroutin);
            StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
            stopAnimationCoroutine();
            calculationDeskCoroutin = null;
        }
    }

    public IEnumerator CalculateDeskEnum(bool isAnimation)
    {
        int errorCode = CheckCorrectDesk();
        int numberOfIteration = 0;
        int tempValue;

        if (errorCode != 0)
        {
            yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());

            if (errorCode == -1)
            {
                mainControllerRef.startInfoCoroutine("Desk has empty positions. Please, set a number at every not blocked position");
                yield break;
            }
            if (errorCode == -2)
            {
                mainControllerRef.startInfoCoroutine("Desk has numbers more than max value. Please, set only integer numbers more than 0 and less than " + ((int)(deskSize.x * deskSize.y) + 1).ToString());
                yield break;
            }
            if (errorCode == -3)
            {
                mainControllerRef.startInfoCoroutine("Desk has some equal numbers. Please, set unique number in each position");
                yield break;
            }
            if (errorCode == -4)
            {
                mainControllerRef.startInfoCoroutine("Number's position is blocked. Please, set block only at empty positions, which haven't his number at desk");
                yield break;
            }
            if (errorCode == -5)
            {
                mainControllerRef.startInfoCoroutine("All positions haven't number. Please, set number in any position");
                yield break;
            }
        }
        else
        {
            yield return StartCoroutine(mainControllerRef.ShowProgressBarBlockEnum());
        }

        List<State> openList = new List<State>();
        List<State> closedList = new List<State>();

        int[,] currentMatrix = new int[(int)deskSize.x,(int)deskSize.y];

        for (int i = 0; i < deskSize.x; i++)
            for (int j = 0; j < deskSize.y; j++)
                currentMatrix[i,j] = fieldsBase[i * (int)deskSize.y + j].GetComponent<FieldContoller>().GetNumber();    

        openList.Add(new State(currentMatrix, calculateEvalFunc(ref currentMatrix), 0));

        float startTime = Time.time;

        while (openList.Count > 0)
        {
            yield return StartCoroutine(mainControllerRef.SetProgressValueEnum(mainControllerRef.speedOfProgressBar, numberOfIteration%mainControllerRef.speedOfProgressBar, numberOfIteration, true));

            numberOfIteration++;

            tempValue = createNewStates(ref openList, ref closedList);
            if (tempValue == 1)
            {
                //mainControllerRef.startInfoCoroutine("Solution found");
                break;
            }
            if (tempValue == -1)
            {
                yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
                mainControllerRef.startInfoCoroutine("Can't find solution. Desk has blocked positions");
                yield break;
            }
            openList.Sort(CompareStates);
        }

        float endTime = Time.time;

        yield return StartCoroutine(mainControllerRef.SetProgressValueEnum(mainControllerRef.speedOfProgressBar, mainControllerRef.speedOfProgressBar, numberOfIteration, false));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());

        int tempIndex = closedList.Count - 1;

        List<int[,]> tempList = new List<int[,]>();

        while (tempIndex != 0)
        {
            tempList.Add(closedList[tempIndex].Matrix.Clone() as int[,]);
            tempIndex = closedList[tempIndex].ClosedIndex;
        }

        if (isAnimation)
        {
            mainControllerRef.startInfoCoroutine("Iterations count: " + tempList.Count.ToString() + "  Time solving: " + (endTime - startTime).ToString() + " seconds");
            animationCoroutine = StartCoroutine(animationCoroutineEnum(tempList));
            yield return animationCoroutine;
        }
        else
        {
            mainControllerRef.startInfoCoroutine("Iterations count: " + tempList.Count.ToString() + "  Time solving: " + (endTime - startTime).ToString() + " seconds");
            currentMatrix = closedList[closedList.Count - 1].Matrix;
            ShowSolution(ref currentMatrix);
        }

        yield break;
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public struct State
{
    public State(int[,] inputMatrix, float inputEvalFunc)
    {
        Matrix = inputMatrix;
        EvalFunc = inputEvalFunc;
    }

    public int[,] Matrix { get; }
    public float EvalFunc { get; }
}




public class DeskController : MonoBehaviour
{
    public List<GameObject> fieldsBase = new List<GameObject>();
    public GameObject fieldPrefab;
    public MainController mainControllerRef;
    public int deskSize = 0;
    private Coroutine showChangesCoroutine;
    private Coroutine calculationCoroutine;

    public IEnumerator showChangesCoroutineEnum(List<State> closedList, List<string> lettersSet)
    {
        for (int i = 0; i < closedList.Count; i++)
        {
            showFillDesk(ref lettersSet, closedList[i]);
            yield return new WaitForSeconds(1.5f);
        }

        yield return null;
    }

    private void startShowChangesCoroutine(ref List<State> closedList, ref List<string> lettersSet)
    {
        stopShowChangesCoroutine();    
        showChangesCoroutine = StartCoroutine(showChangesCoroutineEnum(closedList, lettersSet));
    }

    private void stopShowChangesCoroutine()
    {
        if (showChangesCoroutine != null)
        {
            StopCoroutine(showChangesCoroutine);
            showChangesCoroutine = null;
        }
    }


    public void InitDesk(int initDeskSize)
    {
        DeleteDesk();
        deskSize = initDeskSize;

        for (int i = 0; i < deskSize; i++)
        {
            for (int j = 0; j < deskSize; j++)
            {
                fieldsBase.Add(Instantiate(fieldPrefab, new Vector3(0, 0, 0), Quaternion.identity));
                fieldsBase[fieldsBase.Count - 1].GetComponent<FieldContoller>().InitField(i, j);
                fieldsBase[fieldsBase.Count - 1].transform.parent = gameObject.transform;
                fieldsBase[fieldsBase.Count - 1].GetComponent<Transform>().localPosition = new Vector3(j, -i, 0);
            }
        }

        GetComponent<Transform>().position = new Vector3(0.5f + (-deskSize * 0.5f), -0.5f + (deskSize * 0.5f), 0);
    }

    public void DeleteDesk()
    {
        if (fieldsBase.Count > 0)
        {
            for (int i = fieldsBase.Count - 1; i >= 0; i--)
            {
                fieldsBase[i].GetComponent<FieldContoller>().DeleteField();
                fieldsBase.RemoveAt(i);
            }
        }
    }

    public List<string> GetLettersSet()
    {
        List<string> finalLetter = new List<string>();
        string tempLetter;

        if (fieldsBase.Count > 0)
        {
            foreach (var field in fieldsBase)
            {
                tempLetter = field.GetComponent<FieldContoller>().GetLetter();

                if (tempLetter != string.Empty)
                {
                    if (!finalLetter.Contains(tempLetter)) finalLetter.Add(tempLetter);
                }

                field.GetComponent<FieldContoller>().SetModify();
            }
        }

        return finalLetter;
    }


    public int CheckCorrectDesk()
    {
        List<string> tempStr = GetLettersSet();

        if (tempStr.Count == 0) return -1;
        if (tempStr.Count < deskSize) return -2;

        return 0;
    }


    public void FillFirstMatrix(ref int[,] currentMatrix, ref List<string> lettersSet)
    {
        string tempStr = string.Empty;

        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
            {
                tempStr = fieldsBase[i * deskSize + j].GetComponent<FieldContoller>().GetLetter();
                if(tempStr != string.Empty) currentMatrix[i,j] = lettersSet.IndexOf(tempStr) + 1;    
            }
    }

    public float calculateEvalFunction(ref int[,] currentMatrix, ref List<int> lettersSetIndexes)
    {
        //вариант 1 - не находит 2 решение
        //         int horizontalSum = 0;
        //         int verticalSum = 0;
        //         int filledFields;
        //         int emptyFields;
        // 
        //         for (int i = 0; i < deskSize; i++)
        //         {
        //             filledFields = 0;
        //             emptyFields = 0;
        // 
        //             for (int j = 0; j < deskSize; j++)
        //             {
        //                 if (currentMatrix[i, j] == 0) emptyFields++;
        //                 else filledFields++;
        //             }
        // 
        //             horizontalSum += filledFields * (emptyFields * 2);
        //         }
        // 
        //         for (int j = 0; j < deskSize; j++)
        //         {
        //             filledFields = 0;
        //             emptyFields = 0;
        // 
        //             for (int i = 0; i < deskSize; i++)
        //             {
        //                 if (currentMatrix[i, j] == 0) emptyFields++;
        //                 else filledFields++;
        //             }
        // 
        //             verticalSum += filledFields * (emptyFields * 2);
        //         }
        // 
        //         if (verticalSum > horizontalSum) return horizontalSum;
        //         return verticalSum;


//         int[,] tempSumMatrix = new int[deskSize, deskSize];
// 
//         for (int i = 0; i < deskSize; i++)
//             for (int j = 0; j < deskSize; j++)
//             {
//                 for (int letter = 0; letter < lettersSetIndexes.Count; letter++)
//                 {
//                     if (checkConditions(ref currentMatrix, lettersSetIndexes[letter], i, j)) tempSumMatrix[i,j]++;
//                 }
//             }
// 
//         int numbersSum = 0;
// 
//         for (int i = 0; i < deskSize; i++)
//             for (int j = 0; j < deskSize; j++)
//                 numbersSum += tempSumMatrix[i, j];
// 
//         return numbersSum;






        //вариант 3 - находит оба, но не просто "слезу"

        int[,] tempSumMatrix = new int[deskSize, deskSize];

        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
            {
                for (int letter = 0; letter < lettersSetIndexes.Count; letter++)
                {
                    if (currentMatrix[i, j] != 0) tempSumMatrix[i, j] = lettersSetIndexes.Count;
                    else if (checkConditions(ref currentMatrix, lettersSetIndexes[letter], i, j)) tempSumMatrix[i,j]++;
                }
            }

        int minNumber = deskSize * deskSize;
        int countMinNumbers = 0;

        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
                if(tempSumMatrix[i, j] < minNumber) minNumber = tempSumMatrix[i, j];

        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
                if (tempSumMatrix[i, j] == minNumber) countMinNumbers++;

        return deskSize*deskSize - countMinNumbers;


        //Вариант нерабочий *******************************************************************

        /*       int horizontalSum = 0;
               int verticalSum = 0;
               int filledFields;
               int emptyFields;

               for (int i = 0; i < deskSize; i++)
               {
                   filledFields = 0;
                   emptyFields = 0;

                   for (int j = 0; j < deskSize; j++)
                   {
                       if (currentMatrix[i, j] == 0) emptyFields++;
                       else filledFields++;
                   }

                   horizontalSum += filledFields * (emptyFields * 2);
               }

               for (int j = 0; j < deskSize; j++)
               {
                   filledFields = 0;
                   emptyFields = 0;

                   for (int i = 0; i < deskSize; i++)
                   {
                       if (currentMatrix[i, j] == 0) emptyFields++;
                       else filledFields++;
                   }

                   verticalSum += filledFields * (emptyFields * 2);
               }

               //if (verticalSum > horizontalSum) return horizontalSum;
               return verticalSum + horizontalSum;*/
    }

    public bool checkConditions(ref int[,] currentMatrix, int letterNumber, int indexX, int indexY)
    {
        if (currentMatrix[indexX, indexY] != 0) return false;

        for (int i = 0; i < deskSize; i++)
        {
            if (i != indexX && currentMatrix[i, indexY] == letterNumber) return false;
            if (i != indexY && currentMatrix[indexX, i] == letterNumber) return false;
        }

        if (indexX == indexY)
        {
            for (int i = 0; i < deskSize; i++)
            {
                if (currentMatrix[i, i] == letterNumber) return false;
            }
        }

        if((indexX + indexY) == ((int)deskSize - 1))
        {
            for (int i = 0; i < deskSize; i++)
            {
                if (currentMatrix[i, deskSize - i - 1] == letterNumber) return false;
            }
        }
         
        return true;
    }


    public int checkFillingMatrix(ref int[,] currentMatrix)
    {
        int tempSum = 0;

        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
                if (currentMatrix[i, j] == 0) tempSum++;

        return tempSum;
    }

    public int RecursiveFillOneLettersSet(ref int[,] currentMatrix, ref List<int> lettersSetIndexes, int currentLetterIndex, ref List<State> openList, ref List<State> deadList)
    {
        bool isFindLetterPosition = false;
        int tempReturnValue;

        if (checkFillingMatrix(ref currentMatrix) == 0)
        {
            openList.Add(new State(currentMatrix.Clone() as int[,], calculateEvalFunction(ref currentMatrix, ref lettersSetIndexes)));
            return 0;
        }
        if (currentLetterIndex < lettersSetIndexes.Count)
        {
            for (int letterIndex = currentLetterIndex; letterIndex < lettersSetIndexes.Count; letterIndex++)
            {
                for (int i = 0; i < deskSize; i++)
                {
                    for (int j = 0; j < deskSize; j++)
                    {
                        if (checkConditions(ref currentMatrix, lettersSetIndexes[letterIndex], i, j))
                        {
                            if (!isFindLetterPosition) isFindLetterPosition = true;
                            currentMatrix[i, j] = lettersSetIndexes[letterIndex];
                            tempReturnValue = RecursiveFillOneLettersSet(ref currentMatrix, ref lettersSetIndexes, letterIndex + 1, ref openList, ref deadList);
                            currentMatrix[i, j] = 0;

                            if (tempReturnValue == 0) return 0;
                            if (tempReturnValue == -1) return -1;
                        }
                    }
                }

                if (isFindLetterPosition) break;
            }

            if (!isFindLetterPosition)
            {
                if (currentLetterIndex == 0) return -1;
                openList.Add(new State(currentMatrix.Clone() as int[,], calculateEvalFunction(ref currentMatrix, ref lettersSetIndexes)));
            }
        }
        else
        {
            openList.Add(new State(currentMatrix.Clone() as int[,], calculateEvalFunction(ref currentMatrix, ref lettersSetIndexes)));
            return 1;
        }

        return 1;
    }

    private int CompareStates(State st1, State st2)
    {
        if (st1.EvalFunc == st2.EvalFunc) return 0;
        if (st1.EvalFunc < st2.EvalFunc) return -1;
        return 1;
    }

    private void showFillDesk(ref List<string> lettersSet, State currentMatrix)
    {
        string tempStr;

        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
            {
                if (currentMatrix.Matrix[i, j] != 0) tempStr = lettersSet[currentMatrix.Matrix[i, j] - 1];
                else tempStr = string.Empty;

                fieldsBase[i * deskSize + j].GetComponent<FieldContoller>().SetLetter(tempStr);
            }
    }

    private bool isEqualsMatrix(int[,] matrix1, ref int[,] matrix2)
    {
        for (int i = 0; i < deskSize; i++)
            for (int j = 0; j < deskSize; j++)
                if (matrix1[i, j] != matrix2[i, j]) return false;
        return true;
    }



    public void startCalculationCoroutine(int iterationsCount)
    {
        stopCalculationCoroutine();
        calculationCoroutine = StartCoroutine(calculationCoroutineEnum(iterationsCount));
    }

    public void stopCalculationCoroutine()
    {
        if (calculationCoroutine != null)
        {
            StopCoroutine(calculationCoroutine);
            StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
            calculationCoroutine = null;
        }
    }

    public IEnumerator calculationCoroutineEnum(int iterationsCount)
    {
        int errorCode = CheckCorrectDesk();
        if (errorCode != 0)
        {
            if (errorCode == -1)
            {
                yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
                mainControllerRef.startInfoCoroutine("No one letter in desk. Please, fill some empty position with letters");
                yield break;
            }
            if (errorCode == -2)
            {
                yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
                mainControllerRef.startInfoCoroutine("Not enough letters. Count letters will be more than width or height of desk");
                yield break;
            }
        }
        else
        {
            yield return StartCoroutine(mainControllerRef.ShowProgressBarBlockEnum());
        }


        float startTime = Time.time;

        int tempIndex = 0;

        List<State> openList = new List<State>();
        List<State> closedList = new List<State>();
        List<State> deadList = new List<State>();

        int[,] startMatrix = new int[deskSize, deskSize];
        int[,] parentMatrix = new int[deskSize, deskSize];
        int[,] currentMatrix = new int[deskSize, deskSize];

        List<string> lettersSet = GetLettersSet();
        //lettersSet.Sort();

        List<int> lettersSetIndexes = new List<int>();
        for (int i = 0; i < lettersSet.Count; i++) lettersSetIndexes.Add(i + 1);

        FillFirstMatrix(ref startMatrix, ref lettersSet);

        bool isFindSolution = false;

        closedList.Add(new State(startMatrix, calculateEvalFunction(ref startMatrix, ref lettersSetIndexes)));

        for (int k = 0; k < iterationsCount; k++)
        {
            yield return StartCoroutine(mainControllerRef.SetProgressValueEnum(iterationsCount, k, true));

            if (closedList.Count == 0)
            {
                yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
                mainControllerRef.startInfoCoroutine("Solution is absent");
                yield break;
            }

            closedList.Sort(CompareStates);
            currentMatrix = closedList[0].Matrix;
            openList.Clear();
            openList.Add(new State(currentMatrix, calculateEvalFunction(ref currentMatrix, ref lettersSetIndexes)));

            do
            {
                openList.Sort(CompareStates);
                bool bBreak = false;
                do
                {
                    if (openList.Count == 0)
                    {
                        bBreak = true;
                        break;
                    }
                    currentMatrix = openList[0].Matrix;
                    openList.RemoveAt(0);
                } while (deadList.FindIndex(Elem => isEqualsMatrix(Elem.Matrix, ref currentMatrix)) != -1);

                if (bBreak)
                {
                    deadList.Add(new State(startMatrix, -1));
                    int nFound = closedList.FindIndex(Elem => isEqualsMatrix(Elem.Matrix, ref startMatrix));
                    if (nFound != -1)
                    {
                        closedList.RemoveAt(nFound);
                    }
                    break;
                }

                openList.Clear();
                tempIndex = RecursiveFillOneLettersSet(ref currentMatrix, ref lettersSetIndexes, 0, ref openList, ref deadList);

                if (tempIndex == 0)
                {
                    isFindSolution = true;
                    break;
                }

                if (openList.Count == 0 || tempIndex == -1)
                {
                    tempIndex = -1;
                    deadList.Add(new State(currentMatrix, -1));
                    int nFound = closedList.FindIndex(Elem => isEqualsMatrix(Elem.Matrix, ref currentMatrix));
                    if (nFound != -1)
                    {
                        closedList.RemoveAt(nFound);
                    }
                    break;
                }

                startMatrix = currentMatrix;
                if (closedList.FindIndex(Elem => isEqualsMatrix(Elem.Matrix, ref startMatrix)) == -1)
                    closedList.Add(new State(startMatrix, calculateEvalFunction(ref startMatrix, ref lettersSetIndexes)));

            } while (tempIndex == 1);

            if (isFindSolution)
            {
                float endTime = Time.time;
                yield return StartCoroutine(mainControllerRef.SetProgressValueEnum(iterationsCount, iterationsCount, false));
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
                showFillDesk(ref lettersSet, openList[openList.Count - 1]);
                mainControllerRef.startInfoCoroutine("Iterations count: " + k.ToString() + "  Time solving: " + (endTime - startTime).ToString() + " seconds");
                yield break;
            }
        }

        yield return StartCoroutine(mainControllerRef.HideProgressBarBlockEnum());
        mainControllerRef.startInfoCoroutine("Can't find solution. Please, modify the desk or increase iterations count");
        yield break;
    }

// 
// 
// 
// 
// 
// 
//             parentMatrix = startMatrix;
//         openList.Add(new State(startMatrix, calculateEvalFunction(ref startMatrix, ref lettersSetIndexes)));
// 
//         for (int k = 0; k < iterationsCount; k++) 
//         {
//             if(k > 0)
//             {
//                 openList.Clear();
// 
//                 closedList.Sort(CompareStates);
//                 currentMatrix = closedList[0].Matrix;
//                 closedList.RemoveAt(0);
// 
//                 openList.Add(new State(currentMatrix, calculateEvalFunction(ref currentMatrix, ref lettersSetIndexes)));
//             }
//             
//             stepNumber = 0;
// 
//             do
//             {
//                 //                 if (openList.Count == 0)
//                 //                 {
//                 //                     isFindSolution = false;
//                 //                     break;
//                 //                 }
// 
//                 if (openList.Count == 0)
//                 {
//                     if (closedList.Count != 0)
//                     {
//                         closedList.Sort(CompareStates);
//                         currentMatrix = closedList[0].Matrix;
//                         closedList.RemoveAt(0);
// 
//                         isOpenListMatrix = false;
//                     }
//                     else
//                     {
//                         Debug.Log(k.ToString());
//                         return -4;
//                     }
//                 }
//                 else
//                 {
//                     openList.Sort(CompareStates);
//                     currentMatrix = openList[0].Matrix;
//                     openList.RemoveAt(0);
// 
//                     isOpenListMatrix = true;
//                 }
// 
//                 if (deadList.FindIndex(Elem => Elem.Matrix.Equals(currentMatrix)) != -1)
//                 {
//                     if (openList.Count == 0) deadList.Add(new State(parentMatrix, -1));
//                     continue;
//                 }
// 
//                 if (closedList.FindIndex(Elem => Elem.Matrix.Equals(currentMatrix)) == -1 && isOpenListMatrix)
//                 {
//                     closedList.Add(new State(currentMatrix, calculateEvalFunction(ref currentMatrix, ref lettersSetIndexes)));
//                 }
// 
//                 openList.Clear();
//                 
//                 tempIndex = RecursiveFillOneLettersSet(ref currentMatrix, ref lettersSetIndexes, 0, ref openList, ref deadList);
// 
//                 if (tempIndex == 0)
//                 {
//                     isFindSolution = true;
//                     break;
//                 }
// 
//                 if (openList.Count == 0)
//                 {
//                     deadList.Add(new State(currentMatrix, -1));
//                     tempIndex = -1;
//                 }
//                 else
//                 {
//                     parentMatrix = currentMatrix;
//                 }
// 
//                 stepNumber++;
// 
//             } while (tempIndex == 1);
// 
//             if(isFindSolution)
//             {
//                 closedList.Add(openList[openList.Count - 1]);
// 
//                 if (isAnimation)
//                 {
//                     startShowChangesCoroutine(ref closedList, ref lettersSet);
//                 }
//                 else showFillDesk(ref lettersSet, openList[openList.Count - 1]);
// 
//                 Debug.Log((stepNumber + 1).ToString());
//                 return k;
//             }
// 
// 
//             Debug.Log(k.ToString());
//         }
}

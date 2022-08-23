using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainController : MonoBehaviour
{
    public int deskSize = 0;
    public int iterationsCount = 0;

    public GameObject desk;
    public GameObject dialogueCreateDesk;
    public GameObject progressBarBlock;
    public InputField inputSideLengthField;
    public Text infoText;
    public Text progressBarText;
    public Text currentIterationText;
    public Slider progressBarSlider;
    private Coroutine infoCoroutine = null;

    public void startInfoCoroutine(string input)
    {
        stopInfoCoroutine();
        infoCoroutine = StartCoroutine(infoCoroutineEnum(input));
    }

    public void stopInfoCoroutine()
    {
        if (infoCoroutine != null)
        {
            StopCoroutine(infoCoroutine);
            Color textColor = infoText.color;
            textColor.a = 0.0f;
            infoText.color = textColor;
            infoText.text = string.Empty;
            infoCoroutine = null;
        }
    }

    public IEnumerator infoCoroutineEnum(string inputText)
    {
        Color textColor;
        if (infoText.color.a >= 0.01f)
        {
            yield break;
        }

        textColor = infoText.color;
        textColor.a = 1.0f;
        infoText.color = textColor;

        if (inputText.Length != 0)
        {
            infoText.text = "[Info]: " + inputText;

            yield return new WaitForSeconds(5.0f);

            for (float i = 1.0f; i >= 0; i -= 0.01f)
            {
                textColor = infoText.color;
                textColor.a = i;
                infoText.color = textColor;
                yield return new WaitForSeconds(0.01f);
            }

            infoCoroutine = null;
        }
        else
        {
            textColor.a = 0.0f;
            infoText.color = textColor;

            infoCoroutine = null;
        }
    }

    public void SetSideLengthFromField(InputField currentField)
    {
        if (currentField.text == string.Empty)
        {
            deskSize = 0;
            deskSize = 0;
            currentField.text = deskSize.ToString();
        }
        else
        {
            int tempInt = int.Parse(currentField.text);

            if (tempInt < 0)
            {
                deskSize = 0;
                deskSize = 0;
            }
            else
            {
                deskSize = tempInt;
                deskSize = tempInt;
            }
            currentField.text = deskSize.ToString();
        }
    }


    public void SetIterationsCountFromField(InputField currentField)
    {
        if (currentField.text == string.Empty)
        {
            iterationsCount = 0;
            currentField.text = iterationsCount.ToString();
        }
        else
        {
            iterationsCount = int.Parse(currentField.text);

            if (iterationsCount < 0) iterationsCount = 0;
            currentField.text = iterationsCount.ToString();
        }
    }


    public void CancelDialogeButton()
    {
        inputSideLengthField.text = "0";
        dialogueCreateDesk.SetActive(false);
    }

    public void ApplyDialogeButton()
    {
        if (deskSize == 0)
        {
            startInfoCoroutine("SideLength should be positive integer");
            return;
        }      
        if (deskSize == 1)
        {
            startInfoCoroutine("Wrong desk size. Minimal side length is 2");
            return;
        }

        CancelDialogeButton();

        desk.GetComponent<DeskController>().InitDesk(deskSize);
    }

    public void CreateDeskButton()
    {
        dialogueCreateDesk.SetActive(true);
    }

    public IEnumerator ShowProgressBarBlockEnum()
    {
        progressBarBlock.SetActive(true);
        yield break;
    }

    public IEnumerator HideProgressBarBlockEnum()
    {
        progressBarBlock.SetActive(false);
        currentIterationText.text = "0";
        progressBarSlider.value = 0.0f;
        progressBarText.text = "0";
        yield break;
    }

    public IEnumerator SetProgressValueEnum(int countIterations, int currentIteration, bool isChangeIterations)
    {
        if(isChangeIterations) currentIterationText.text = currentIteration.ToString();
        float tempValue = (float)currentIteration / countIterations;
        progressBarSlider.value = tempValue;
        progressBarText.text = (tempValue * 100).ToString();
        yield break;
    }


    public void CalculateDeskButton()
    {
        EscapeSequence();
        desk.GetComponent<DeskController>().startCalculationCoroutine(iterationsCount + 1);
    }

    public void CancelCalculationButton()
    {
        EscapeSequence();
        desk.GetComponent<DeskController>().stopCalculationCoroutine();
    }

    public void EscapeSequence()
    {
        
    }

}

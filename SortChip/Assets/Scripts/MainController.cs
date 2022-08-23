using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainController : MonoBehaviour
{
    public Vector2 deskSize = new Vector2(0,0);

    public GameObject desk;
    public GameObject dialogueCreateDesk;
    public GameObject stopAnimationBlock;
    public InputField inputWidthField;
    public InputField inputHeightField;
    public Text infoText;

    public GameObject progressBarBlock;
    public Text currentIterationText;
    public Slider progressBarSlider;
    public int speedOfProgressBar = 500;
    public Slider speedSlider;

    private bool isAnimation = false;
    public float speedOfAnimation;
    private Coroutine infoCoroutine = null;

    private void Start()
    {
        speedOfAnimation = speedSlider.value * 1000 + 1;
    }


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

    public void SetWidthFromField(InputField currentField)
    {
        if (currentField.text == string.Empty)
        {
            deskSize.y = 0;
            currentField.text = deskSize.x.ToString();
        }
        else
        {
            deskSize.y = int.Parse(currentField.text);

            if (deskSize.y < 0)
            {
                deskSize.y = 0;
                currentField.text = deskSize.x.ToString();
            }
        }
    }

    public void SetHeightFromField(InputField currentField)
    {
        if (currentField.text == string.Empty)
        {
            deskSize.x = 0;
            currentField.text = deskSize.y.ToString();
        }
        else
        {
            deskSize.x = int.Parse(currentField.text);

            if (deskSize.x < 0)
            {
                deskSize.x = 0;
                currentField.text = deskSize.x.ToString();
            }
        }
    }

    public void CancelDialogeButton()
    {
        inputWidthField.text = "0";
        inputHeightField.text = "0";
        dialogueCreateDesk.SetActive(false);
    }

    public void ApplyDialogeButton()
    {
        if (deskSize.x == 0)
        {
            startInfoCoroutine("Height should be positive integer");
            return;
        }
        if (deskSize.y == 0)
        {
            startInfoCoroutine("Width should be positive integer");
            return;
        }
        if (deskSize.x == 1 && deskSize.y == 1)
        {
            startInfoCoroutine("Wrong desk size. Please, select size more than 1 position");
            return;
        }

        CancelDialogeButton();

        desk.GetComponent<DeskController>().InitDesk(deskSize);
    }

    public void CreateDeskButton()
    {
        EscapeSequence();

        dialogueCreateDesk.SetActive(true);
    }

    public void stopAnimationButton()
    {
        EscapeSequence();
    }


    public void AnimationToggle(Toggle isAnimationToggle)
    {
        isAnimation = isAnimationToggle.isOn;
        if (!isAnimation) stopAnimationButton();
        stopAnimationBlock.SetActive(isAnimation);
    }


    public void FillDeskButton()
    {
        EscapeSequence(); 

        if(deskSize.x == 0 || deskSize.y == 0)
        {
            startInfoCoroutine("Missing desk. Please, create desk first");
            return;
        }

        desk.GetComponent<DeskController>().FillDesk();
    }

    public void CancelCalculationButton()
    {
        EscapeSequence();
    }

    public void CalculateDesk()
    {
        EscapeSequence();

        if (deskSize.x == 0 || deskSize.y == 0)
        {
            startInfoCoroutine("Missing desk. Please, create desk first");
            return;
        }

        desk.GetComponent<DeskController>().startCalculateDeskCoroutine(isAnimation);       
        
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
        yield break;
    }

    public IEnumerator SetProgressValueEnum(int maxIterations, int procentOfBar, int currentIteration, bool isChangeIterations)
    {
        if (isChangeIterations) currentIterationText.text = currentIteration.ToString();
        float tempValue = (float)procentOfBar / maxIterations;
        progressBarSlider.value = tempValue;
        yield break;
    }

    public void changeSpeedAnimation()
    {
        speedOfAnimation = speedSlider.value * 1000 + 1;
    }

    public void EscapeSequence()
    {
        stopInfoCoroutine();
        desk.GetComponent<DeskController>().stopCalculateDeskCoroutine();
    }

}

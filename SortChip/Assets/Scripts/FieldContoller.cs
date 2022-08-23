using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FieldContoller : MonoBehaviour
{
    private Vector2 indexField = new Vector2();
    private int number = 0;
    public bool isKeyDown = false;
    public bool isBlockedField = false;
    public bool isUserChangeValue = true;

    public InputField numberField;
    public Color standartColor;
    public Color outlineColor;
    public Color blockColor;

    public void InitField(int indexRow, int indexColomn)
    {
        indexField.x = indexRow;
        indexField.y = indexColomn;
        setColor(standartColor);
        isUserChangeValue = true;
    }

    public Vector2 GetIndexField()
    {
        return indexField;
    }

    public int GetNumber()
    {
        return number;
    }

    public void SetNumber(int inputNumber)
    {
        if (!isBlockedField)
        {
            number = inputNumber;
            isUserChangeValue = false;
            numberField.text = number.ToString();
            isUserChangeValue = true;
        }
    }

    public void SetNumberFromField(InputField currentField)
    {
        if (isUserChangeValue)
        {
            if (currentField.text == string.Empty)
            {
                number = 0;
            }
            else
            {
                number = int.Parse(currentField.text);
                if (number == 0)
                {
                    isUserChangeValue = false;
                    currentField.text = string.Empty;
                    isUserChangeValue = true;
                }
            }
        }
    }

    public void setColor(Color inputColor)
    {
        numberField.image.color = inputColor;
    }

    public Color getColor()
    {
        return numberField.image.color;
    }

    public bool CompareFieldByIndex(Vector2 inputIndex)
    {
        return (indexField.x == inputIndex.x && indexField.y == inputIndex.y);
    }       

    public void DeleteField()
    {
        Destroy(gameObject);
    }


    private void OnMouseDown()
    {
        if (isKeyDown)
        {
            if (!isBlockedField)
            {
                isBlockedField = true;
                number = -1;
                numberField.enabled = false;
                setColor(blockColor);
            }
            else
            {
                isBlockedField = false;
                number = 0;
                numberField.enabled = true;
                setColor(standartColor);
            }

            isUserChangeValue = false;
            numberField.text = string.Empty;
            isUserChangeValue = true;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isKeyDown = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            isKeyDown = false;
        }
    }

}

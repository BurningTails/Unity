using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FieldContoller : MonoBehaviour
{
    private Vector2 indexField = new Vector2();
    private string letter = string.Empty;
    private bool isModify = false;
    public InputField letterField;
    public Color standartColor;
    public Color outlineColor;

    public void InitField(int indexRow, int indexColomn)
    {
        indexField.x = indexRow;
        indexField.y = indexColomn;
    }

    public Vector2 GetIndexField()
    {
        return indexField;
    }

    public string GetLetter()
    {
        return letter;
    }

    public void SetLetter(string inputString)
    {
        if (!letter.Equals(inputString))
        {
            letter = inputString;
            letterField.text = letter;
            letterField.image.color = standartColor;
        }
    }

    public void SetLetterFromField(InputField currentField)
    {
        letter = currentField.text;
        if (letter == string.Empty) currentField.image.color = standartColor;
        else currentField.image.color = outlineColor;
    }

    public void SetModify()
    {
        if (letter == string.Empty) isModify = true;
        else isModify = false;
    }

    public bool GetModify()
    {
        return isModify;
    }

    public void DeleteField()
    {
        Destroy(gameObject);
    }

}

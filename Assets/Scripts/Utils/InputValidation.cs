using System.Text.RegularExpressions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputValidation
{
    public static int ValidateInt(string text, int defaultValue)
    {
        bool isValid = int.TryParse(text, out int result);

        if (!isValid)
        {
            result = defaultValue;
        }

        return result;
    }

    public static float ValidateFloat(string text, float defaultValue)
    {
        bool isValid = float.TryParse(text, out float result);
        result = (float) Math.Round(result, 2);

        if (!isValid)
        {
            result = defaultValue;
        }

        return result;
    }

    public static char ValidateCharAsUnsignedInt(char addedChar)
    {
        return ValidateChar(addedChar, Constants.UnsignedIntegerPattern);
    }

    private static char ValidateChar(char addedChar, string validationPattern)
    {
        if (Regex.IsMatch(addedChar.ToString(), validationPattern))
            return addedChar;

        // Return empty char if pattern isn't matched
        return '\0';
    }
    
    public static Vector3 Round(Vector3 vector3, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);

    }
}

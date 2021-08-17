using System.Collections;
using System.Collections.Generic;

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

        if (!isValid)
        {
            result = defaultValue;
        }

        return result;
    }
}

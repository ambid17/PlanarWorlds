using System.Text.RegularExpressions;

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
}

using System.Collections.Generic;

public enum BracketType
{
    NONE, SQUARE, PARENTHESES, CURLY, ANGLE
}

class Utilities
{
    private static Dictionary<BracketType, string> bracketMap = new()
    {
        { BracketType.NONE, "" },
        { BracketType.SQUARE, "[]" },
        { BracketType.PARENTHESES, "()" },
        { BracketType.CURLY, "{}" },
        { BracketType.ANGLE, "<>" },
    };

    public static string Array3DToString<T>(T[,,] array, char delimeter = ',', BracketType bracketType = BracketType.SQUARE)
    {
        string brackets = bracketMap[bracketType];
        string ret = string.Format(
            "Array of dimension ({0}, {1}, {2}): {3}",
            array.GetLength(0), array.GetLength(1), array.GetLength(2), brackets[0]
        );

        for (int i = 0; i < array.GetLength(0); i++)
        {
            if (i > 0) ret += delimeter;
            string layer = brackets[..1];
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (j > 0) layer += delimeter;
                string column = brackets[..1];
                for (int k = 0; k < array.GetLength(2); k++)
                {
                    if (k > 0) column += delimeter;
                    column += array[i, j, k].ToString();
                }
                layer += column + brackets[1];
            }
            ret += layer + brackets[1];
        }

        return ret + brackets[1];
    }
    public static string Join2D(char[,] array, string delimeter = "")
    {
        string ret = "";
        for (int i = 0; i < array.GetLength(0); i++)
        {
            if (i > 0) ret += '\n';
            for (int j = array.GetLength(1)-1; j >= 0; j--)
            {
                if (j > 0) ret += delimeter;
                ret += array[i, j];
            }
        }
        return ret;
    }
}
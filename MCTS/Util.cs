using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Util
{
    public static char[][] deepcloneArray(char[][] sourceArr)
    {
        char[][] clonedArr = new char[sourceArr.Length][];
        for (int i = 0; i < clonedArr.Length; i++)
        {
            clonedArr[i] = new char[sourceArr[i].Length];
            for (int j = 0; j < clonedArr[i].Length; j++)
            {
                clonedArr[i][j] = sourceArr[i][j];
            }
        }

        return clonedArr;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Util
{
    public static int[][] deepcloneArray(int[][] sourceArr)
    {
        int[][] clonedArr = new int[sourceArr.Length][];
        for (int i = 0; i < clonedArr.Length; i++)
        {
            clonedArr[i] = new int[sourceArr[i].Length];
            for (int j = 0; j < clonedArr[i].Length; j++)
            {
                clonedArr[i][j] = sourceArr[i][j];
            }
        }

        return clonedArr;
    }
}

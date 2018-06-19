using System;

public class MyRandom : Random {

    public MyRandom(int seed) : base(seed) {
        UnityEngine.Debug.Log("random initialized");
    }

    /// <summary>
    /// Returns a random odd integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned.maxValue must be greater than or equal to minValue.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; 
    /// that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
    public int NextOdd(int minValue, int maxValue) {
        if(minValue == maxValue) {
            return minValue;
        }
        int res;
        do {
            res = this.Next(minValue, maxValue);
        } while (res % 2 == 0);
        return res;
    }
    
}

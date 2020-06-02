using System;

/// <summary>
///  Utility methods to use.
/// </summary>
/// 
public static class Util {
    /// <summary>
    /// Compares two floating point numbers to see if they are equal. PLEASE use this instead of the equality operator (== and !=).
    /// </summary>
    public static bool isEqual(float f1, float f2) {
        return Math.Abs(f2 - f1) < 0.001f;
    }
}
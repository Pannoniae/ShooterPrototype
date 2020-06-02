using System;

public static class Util {
    public static bool isEqual(float f1, float f2) {
        return Math.Abs(f2 - f1) < 0.001f;
    }
}
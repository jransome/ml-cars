using System;

public static class Activation
{
    public static double BinaryStep(double input) => input > 0 ? 1 : 0;

    public static double TanH(double input) => Math.Tanh(input);

    public static double Sigmoid(double value) //(aka logistic softstep)
    {
        double k = Math.Exp(value);
        return k / (1.0f + k);
    }

    public static double LeakyRelu(double input) => input > 0 ? input : 0.01 * input;

    public static double Relu(double input) => input > 0 ? input : 0;
}

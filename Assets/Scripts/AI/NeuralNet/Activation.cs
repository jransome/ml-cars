using System.Collections.Generic;
using System;

namespace RansomeCorp.AI.NeuralNet
{
    public enum ActivationType
    {
        BinaryStep,
        TanH,
        Sigmoid,
        LeakyRelu,
        Relu,
    }

    public static class Activation
    {
        public static Dictionary<ActivationType, Func<double, double>> Functions = new Dictionary<ActivationType, Func<double, double>>
        {
            { ActivationType.BinaryStep, BinaryStep },
            { ActivationType.TanH, TanH },
            { ActivationType.Sigmoid, Sigmoid },
            { ActivationType.LeakyRelu, LeakyRelu },
            { ActivationType.Relu, Relu },
        };

        static double BinaryStep(double input) => input > 0 ? 1 : 0;

        static double TanH(double input) => Math.Tanh(input);

        static double Sigmoid(double value) //(aka logistic softstep)
        {
            double k = Math.Exp(value);
            return k / (1.0f + k);
        }

        static double LeakyRelu(double input) => input > 0 ? input : 0.01 * input;

        static double Relu(double input) => input > 0 ? input : 0;
    }
}
using System;

namespace Evo.Core
{
    /// <summary>Feedforward network, 11 inputs, 6 tanh hidden, 4 tanh outputs. Allocates nothing per call.</summary>
    public sealed class Brain
    {
        readonly float[] hidden = new float[BrainLayout.Hidden];
        public readonly float[] Inputs = new float[BrainLayout.Inputs];
        public readonly float[] Outputs = new float[BrainLayout.Outputs];

        public void Forward(float[] w)
        {
            Forward(w, Inputs, Outputs);
        }

        public void Forward(float[] w, float[] input, float[] output)
        {
            const int H = BrainLayout.Hidden;
            const int I = BrainLayout.Inputs;
            const int O = BrainLayout.Outputs;
            for (int h = 0; h < H; h++)
            {
                float sum = w[BrainLayout.B1Offset + h];
                for (int i = 0; i < I; i++) sum += input[i] * w[BrainLayout.W1Offset + i * H + h];
                hidden[h] = Tanh(sum);
            }
            for (int o = 0; o < O; o++)
            {
                float sum = w[BrainLayout.B2Offset + o];
                for (int h = 0; h < H; h++) sum += hidden[h] * w[BrainLayout.W2Offset + h * O + o];
                output[o] = Tanh(sum);
            }
        }

        static float Tanh(float x)
        {
            if (x > 9f) return 1f;
            if (x < -9f) return -1f;
            float e = (float)Math.Exp(2.0 * x);
            return (e - 1f) / (e + 1f);
        }
    }
}

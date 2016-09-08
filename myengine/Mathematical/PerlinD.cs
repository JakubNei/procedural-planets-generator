/* coherent noise function over 1, 2 or 3 dimensions */
/* (copyright Ken Perlin) */
/* ported to C# by aeroson */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using OpenTK;

namespace MyEngine
{
    public class PerlinD
    {
        const int PERLIN_SAMPLE_SIZE = 2048;

        #region Settings for Composite return
        int mOctaves;
        double mFrequency;
        double mAmplitude;
        int mSeed;
        double mPersistance;
        #endregion


        int[] p = new int[PERLIN_SAMPLE_SIZE + PERLIN_SAMPLE_SIZE + 2];

        // gradient arrays dimension size
        const int g_arrays_dimension_size = (PERLIN_SAMPLE_SIZE + PERLIN_SAMPLE_SIZE + 2);

        // gradients for 1d 2d and 3d perlin
        double[] g3 = new double[g_arrays_dimension_size * 3];
        double[] g2 = new double[g_arrays_dimension_size * 2];
        double[] g1 = new double[g_arrays_dimension_size * 1];

        public PerlinD(int seed, int octaves = 1, double freq = 1.0f, double amp = 1.0f, double persistance = 2.0f)
        {
            mOctaves = octaves;
            mFrequency = freq;
            mAmplitude = amp;
            mSeed = seed;
            mPersistance = persistance;

            Init();
        }



        #region Defines and macros

        const int B = PERLIN_SAMPLE_SIZE;
        const int BM = (PERLIN_SAMPLE_SIZE - 1);

        const int N = 0x1000;
        const int NP = 12;   /* 2^N */
        const int NM = 0xfff;

        
        double s_curve(ref double t)
        {
            return (t * t * (3.0f - 2.0f * t));
        }
        
        double lerp(ref double t, ref double a, ref double b)
        {
            return (a + t * (b - a));
        }
        
        void setup(ref double i, out int b0, out int b1, out double r0, out double r1, out double t)
        {
            t = i + N;
            b0 = ((int)t) & BM;
            b1 = (b0 + 1) & BM;
            r0 = t - (int)t;
            r1 = r0 - 1.0f;
        }

        #endregion


        public double Get1D(double arg)
        {
            int bx0 = 0, bx1 = 0;
            double rx0 = 0, rx1 = 0, sx = 0, t = 0, u = 0, v = 0;

            setup(ref arg, out bx0, out bx1, out rx0, out rx1, out t);

            sx = s_curve(ref rx0);

            u = rx0 * g1[p[bx0]];
            v = rx1 * g1[p[bx1]];

            return lerp(ref sx, ref u, ref v);
        }


        
        double at2(ref double rx, ref double ry, ref int index)
        {
            return (rx * g2[index] + ry * g2[index + 1]);
        }
        public double Get2D(Vector2d pos)
        {
            int bx0, bx1, by0, by1, b00, b10, b01, b11;
            double rx0, rx1, ry0, ry1, sx, sy, a, b, t, u, v;
            int i, j;

            setup(ref pos.X, out bx0, out bx1, out rx0, out rx1, out t);
            setup(ref pos.Y, out by0, out by1, out ry0, out ry1, out t);

            i = p[bx0];
            j = p[bx1];

            b00 = p[i + by0];
            b10 = p[j + by0];
            b01 = p[i + by1];
            b11 = p[j + by1];

            sx = s_curve(ref rx0);
            sy = s_curve(ref ry0);


            u = at2(ref rx0, ref ry0, ref b00);
            v = at2(ref rx1, ref ry0, ref b10);
            a = lerp(ref sx, ref u, ref v);

            u = at2(ref rx0, ref ry1, ref b01);
            v = at2(ref rx1, ref ry1, ref b11);
            b = lerp(ref sx, ref u, ref v);

            return lerp(ref sy, ref a, ref b);
        }



        
        double at3(ref double rx, ref double ry, ref double rz, int index)
        {
            return (rx * g3[index + 0] + ry * g3[index + 1] + rz * g3[index + 2]);
        }
        public double Get3D(Vector3d pos)
        {
            int bx0, bx1, by0, by1, bz0, bz1, b00, b10, b01, b11;
            double rx0, rx1, ry0, ry1, rz0, rz1, sy, sz, a, b, c, d, t, u, v;
            int i, j;


            setup(ref pos.X, out bx0, out bx1, out rx0, out rx1, out t);
            setup(ref pos.Y, out by0, out by1, out ry0, out ry1, out t);
            setup(ref pos.Z, out bz0, out bz1, out rz0, out rz1, out t);

            i = p[bx0];
            j = p[bx1];

            b00 = p[i + by0];
            b10 = p[j + by0];
            b01 = p[i + by1];
            b11 = p[j + by1];

            t = s_curve(ref rx0);
            sy = s_curve(ref ry0);
            sz = s_curve(ref rz0);

            u = at3(ref rx0, ref ry0, ref rz0, b00 + bz0);
            v = at3(ref rx1, ref ry0, ref rz0, b10 + bz0);
            a = lerp(ref t, ref u, ref v);

            u = at3(ref rx0, ref ry1, ref rz0, b01 + bz0);
            v = at3(ref rx1, ref ry1, ref rz0, b11 + bz0);
            b = lerp(ref t, ref u, ref v);

            c = lerp(ref sy, ref a, ref b);

            u = at3(ref rx0, ref ry0, ref rz1, b00 + bz1);
            v = at3(ref rx1, ref ry0, ref rz1, b10 + bz1);
            a = lerp(ref t, ref u, ref v);

            u = at3(ref rx0, ref ry1, ref rz1, b01 + bz1);
            v = at3(ref rx1, ref ry1, ref rz1, b11 + bz1);
            b = lerp(ref t, ref u, ref v);

            d = lerp(ref sy, ref a, ref b);

            return lerp(ref sz, ref c, ref d);
        }


        
        void Normalize2DVec(double[] v, int index)
        {
            double s;
            s = (double)Math.Sqrt(v[index + 0] * v[index + 0] + v[index + 1] * v[index + 1]);
            s = 1.0f / s;
            v[index + 0] = v[index + 0] * s;
            v[index + 1] = v[index + 1] * s;
        }
        
        void Normalize3DVec(double[] v, int index)
        {
            double s;

            s = (double)Math.Sqrt(v[index + 0] * v[index + 0] + v[index + 1] * v[index + 1] + v[index + 2] * v[index + 2]);
            s = 1.0f / s;

            v[index + 0] = v[index + 0] * s;
            v[index + 1] = v[index + 1] * s;
            v[index + 2] = v[index + 2] * s;
        }



        // simulate C srand() and rand()
        Random random;
        
        void srand(int seed)
        {
            random = new Random(seed);
        }
        
        int rand()
        {
            return random.Next();
        }

        void Init()
        {
            srand(mSeed);

            int i = 0, j = 0, k = 0;

            for (i = 0; i < B; i++)
            {
                p[i] = i;
                g1[i] = (double)((rand() % (B + B)) - B) / B;
                for (j = 0; j < 2; j++)
                    g2[i * 2 + j] = (double)((rand() % (B + B)) - B) / B;
                Normalize2DVec(g2, i);
                for (j = 0; j < 3; j++)
                    g3[i * 3 + j] = (double)((rand() % (B + B)) - B) / B;
                Normalize3DVec(g3, i);
            }

            while (--i > 0)
            {
                k = p[i];
                p[i] = p[j = rand() % B];
                p[j] = k;
            }

            for (i = 0; i < B + 2; i++)
            {
                p[B + i] = p[i];
                g1[B + i] = g1[i];
                for (j = 0; j < 2; j++)
                    g2[(B + i) * 2 + j] = g2[i * 2 + j];
                for (j = 0; j < 3; j++)
                    g3[(B + i) * 3 + j] = g3[i * 3 + j];
            }

        }


        public double Get2DComposeOctaves(Vector2d pos)
        {
            int terms = mOctaves;
            double freq = mFrequency;
            double result = 0.0f;
            double amp = mAmplitude;

            pos *= mFrequency;

            for (int i = 0; i < terms; i++)
            {
                result += Get2D(pos) * amp;
                pos *= mPersistance;
                amp /= mPersistance;
            }

            return result;
        }

    }
}
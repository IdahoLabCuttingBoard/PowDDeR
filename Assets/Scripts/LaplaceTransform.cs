

/*
� 2020 Battelle Energy Alliance, LLC
ALL RIGHTS RESERVED

Prepared by Battelle Energy Alliance, LLC
Under Contract No.DE-AC07-05ID14517
With the U. S.Department of Energy

NOTICE:  This computer software was prepared by Battelle Energy
Alliance, LLC, hereinafter the Contractor, under Contract
No.AC07-05ID14517 with the United States (U.S.) Department of
Energy (DOE).  The Government is granted for itself and others acting on
its behalf a nonexclusive, paid-up, irrevocable worldwide license in this
data to reproduce, prepare derivative works, and perform publicly and
display publicly, by or on behalf of the Government.There is provision for
the possible extension of the term of this license.Subsequent to that
period or any extension granted, the Government is granted for itself and
others acting on its behalf a nonexclusive, paid-up, irrevocable worldwide
license in this data to reproduce, prepare derivative works, distribute
copies to the public, perform publicly and display publicly, and to permit
others to do so.The specific term of the license can be identified by
inquiry made to Contractor or DOE.NEITHER THE UNITED STATES NOR THE UNITED
STATES DEPARTMENT OF ENERGY, NOR CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
IMPLIED, OR ASSUMES ANY LIABILITY OR RESPONSIBILITY FOR THE USE, ACCURACY,
COMPLETENESS, OR USEFULNESS OR ANY INFORMATION, APPARATUS, PRODUCT, OR
PROCESS DISCLOSED, OR REPRESENTS THAT ITS USE WOULD NOT INFRINGE PRIVATELY
OWNED RIGHTS.

Authors:
Tim McJunkin
Craig Rieger
Thomas Szewczyk
James Money
Randall Reese
*/



using System;
using System.Collections.Generic;
using System.Text;

namespace ComportMath
{
    class Laplace
    {
        public delegate double FunctionDelegate(double t);
        static double[] V;       //  Stehfest coefficients
        static double ln2;       //  log of 2
        const int DefaultStehfestN = 14;

        static Laplace()
        {
            InitStehfest(DefaultStehfestN);
        }

        public static double Transform(FunctionDelegate F, double s)
        {
            const int DefaultIntegralN = 5000;
            double du = 0.5 / (double)DefaultIntegralN;
            double y =  - F(0) / 2.0;
            double u = 0;
            double limit = 1.0 - 1e-10;
            while (u < limit)
            {
                u += du;
                y += 2.0 * Math.Pow(u, s - 1) * F(-Math.Log(u));
                u += du;
                y += Math.Pow(u, s - 1) * F(-Math.Log(u));
            }
            return 2.0 * y * du / 3.0;
        }

        public static double InverseTransform(FunctionDelegate f, double t)
        {
			if (t == 0) {
				return 0;
			}

            double ln2t = ln2 / t;
            double x = 0;
            double y = 0;
            for (int i = 0; i < V.Length; i++)
            {
                //x += ln2t;
				y += V[i] * f(ln2t*i);
            }
            return ln2t * y;
        }
		// beta = denom
		// alpha = numer
		public static double InverseTransform2(double[] alpha, double[] beta, double t)
		{
			if (t == 0) {
				return 0.0;
			}
				
			if (beta [0] < 0.0000001) {
				return alpha [2] / beta [2] * (1.0 - Math.Exp (-t * beta [2] / beta [1]));
			} 
			else {
			
				double sqrpart = beta [1] * beta [1] - 4 * beta [0] * beta [2];
				if (sqrpart >= 0.0) {
					if (sqrpart < 0.0000001) {
						double rootS = -beta [1] / (2.0 * beta [0]);
						double a = alpha [2] / beta [2];
						double b = -alpha [2] / beta [2];
						double c = (alpha [1] / beta [2] - alpha [2] * beta [1]) / (beta [2] * beta [0]) + alpha [2] * rootS / beta [2];

						return a + b * Math.Exp (rootS * t) + c * t * Math.Exp (rootS * t);
					} else {
						double[] roots = { 0.0, 0.0 };
						roots [0] = (-beta [1] + Math.Sqrt (sqrpart)) / (2 * beta [0]);
						roots [1] = (-beta [1] - Math.Sqrt (sqrpart)) / (2 * beta [0]);

						double a = alpha [2] / beta [2];
						double b = (alpha [1] * -roots [0] + alpha [2]) / (beta [0] * -roots [0] * (-roots [0] + roots [1]));
						double c = (alpha [1] * -roots [1] + alpha [2]) / (beta [0] * -roots [1] * (-roots [1] + roots [0]));

						return a + b * Math.Exp (roots [0] * t) + c * Math.Exp (roots [1] * t);
					}
				} else {
					double real = -beta [1] / (2 * beta [0]);
					double im = Math.Sqrt (-sqrpart) / (2.0 * beta [0]);

					if (alpha [0] == 0.0) {
						double a = alpha [2] / beta [2];
						return a * (1.0 - Math.Exp (real * t) * Math.Cos (im * t));
					} else {
						double eqAlpha = alpha [0] / alpha [1];
						double eqPhi = Math.Atan (im / (eqAlpha - real));

						double a = (alpha [2] / beta [2]) * Math.Sqrt (Math.Pow ((eqAlpha - real), 2.0) + Math.Pow (im, 2.0)) / im;
						return a * (1.0 - Math.Exp (real * t) * Math.Cos (im * t + eqPhi));
					}

				}
			}
		}

		public static double Factorial(double n)
		{
			double result;
			if (n <= 1) return 1;
			result = Factorial(n - 1) * n;
			return result;
		}

		public static long binomial(int n, int k)
		{
			// This function gets the total number of unique combinations based upon N and K.
			// N is the total number of items.
			// K is the size of the group.
			// Total number of unique combinations = N! / ( K! (N - K)! ).
			// This function is less efficient, but is more likely to not overflow when N and K are large.
			// Taken from:  http://blog.plover.com/math/choose.html
			//
			long r = 1;
			long d;
			if (k > n) return 0;
			for (d = 1; d <= k; d++)
			{
				r *= n--;
				r /= d;
			}
			return r;
		}

		public static double factorial(int number) {
			double result = 1.0;

			for (int factor = 2; factor <= number; factor++) {
				result *= factor;
			}

			return result;
		}

//		public static double fixedTalbot(FunctionDelegate f, double t, double M)
//		{
//			for (int k = 0; k < M - 1; k++) {
//				double p = 
//
//				f(p
//			}
//		}

		public static double gwr(FunctionDelegate f, double t, double M)
		{
			double tau = 0;
			if(t != 0)
			{
				tau = Math.Log10(2.0) / t;
			}

			bool broken = false;
			double[] Fi = new double[(int)(2*M)];
			for (int i = 0; i < Fi.Length; i++)
			{
					Fi[i] = f(i * tau);
			}

			double[] G0 = new double[(int)M+1];
			for(int n = 1; n < (int)M; n++)
			{
				double sm = 0.0;

				for (int i = 0; i < n; i++)
				{
					sm += binomial (n, i) * Math.Pow(-1.0,(double)i) * Fi [n + i];
				}
								
				G0 [n] = tau * factorial (2 * n) / (factorial (n) * factorial (n - 1)) * sm;
			}
			double[] Gm = new double[(int)M+1];
			double[] Gp = new double[(int)M+1];
			double best = G0 [(int)M - 1];
			for( int k = 0; k < (int)M-2; k++)
			{
				for(int n = (int)M-2-k; n > 0; n--)
				{
					double expr = G0 [n + 2] - G0 [n + 1];
					if (expr == 0){
						broken = true;
						break;
					}
					expr = Gm[n+2] + (k+1)/expr;
					Gp[n+1] = expr;
					if ((k%2)!=0 && n == (int)M - 2 - k)
					{
						best = expr;
					}
				}
				if (broken){
					break;
				}
				for(int n = 0; n<((int)M-k);n++)
				{
					Gm [n + 1] = G0 [n + 1];
					G0 [n + 1] = Gp [n + 1];
				}
			}
					
			return best;				
		}

        //public static double Integrate(FunctionDelegate f, double Min, double Max)
        //{
        //    return Integrate(f, Min, Max, 100);
        //}

        //public static double Integrate(FunctionDelegate f, double XMin, double XMax, int N)
        //{
        //    double dx = (XMax - XMin) / (double)N / 2.0;
        //    double y = (f(XMin) - f(XMax))/2.0;
        //    double x = XMin;
        //    double limit = XMax - 1e-10;
        //    while (x < limit)
        //    {
        //        x += dx;
        //        y += 2.0*f(x);
        //        x += dx;
        //        y += f(x);
        //    }
        //    return 2.0 * y * dx / 3.0;
        //}

        public static void InitStehfest()
        {
            InitStehfest(DefaultStehfestN);
        }

        public static void InitStehfest(int N)
        {
            ln2 = Math.Log(2.0);
            int N2 = N / 2;
            int NV = 2 * N2;
            V = new double[NV];
            int sign = 1;
            if ((N2 % 2) != 0)
                sign = -1;
            for (int i = 0; i < NV; i++)
            {
                int kmin = (i + 2) / 2;
                int kmax = i + 1;
                if (kmax > N2)
                    kmax = N2;
                V[i] = 0;
                sign = -sign;
                for (int k = kmin; k <= kmax; k++)
                {
                    V[i] = V[i] + (Math.Pow(k, N2) / Factorial(k)) * (Factorial(2 * k)
                        / Factorial(2 * k - i - 1)) / Factorial(N2 - k) / Factorial(k - 1)
                        / Factorial(i + 1 - k);
                }
                V[i] = sign * V[i];
            }
        }


    }
}

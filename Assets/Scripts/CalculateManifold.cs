﻿/*
© 2020 Battelle Energy Alliance, LLC
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



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComportMath;
using Vectrosity;
using System;

public class CalculateManifold : MonoBehaviour {

	public List<Vector3> points = new List<Vector3>();
	public List<Vector3> pointsLine = new List<Vector3>();
	public List<float> Pt = new List<float>();
	public List<float> PTransfer = new List<float>();
	public List<float> Qt = new List<float>();
	public List<float> QTransfer = new List<float>();
	public int timeStepsPerSecond;
	public int polarSteps;

	public float alpha1,alpha2,alpha3;
	public float beta1, beta2, beta3;


	public List<Vector3> getLineAtTime(Asset asset, float currentTime, int polarSteps)
	{
		pointsLine.Clear ();

		for (int i = 0; i < polarSteps; i++) {
			pointsLine.Add (points [(int)currentTime * polarSteps * timeStepsPerSecond + i]);
		}

		return pointsLine;
	}

	public double f(double s)
	{
		if (s == 0) {
			return 0.0;
		}
		else{
			return (((beta3 * Math.Pow(s,2.0) + beta2 * s + beta1) / (alpha3 * Math.Pow(s,2.0) + alpha2 * s + alpha1)) * (1 / s));
		}
	}


	public List<Vector3> buildOneManifold(Asset asset, int totalTimeSeconds, int timeStepsPerSecond, int polarSteps, float euLatency)
	{
		points.Clear ();
		Pt.Clear ();
		Qt.Clear ();
		PTransfer.Clear ();
		this.polarSteps = polarSteps;
		this.timeStepsPerSecond = timeStepsPerSecond;

		double[] pnumer = new double[3];
		double[] pdenom = new double[3];

		double[] qnumer = new double[3];
		double[] qdenom = new double[3];

		//set the alpha and betas
//		alpha3 = asset.alpha3;
//		alpha2 = asset.alpha2;
//		alpha1 = asset.alpha1;
//		beta3 = asset.beta3;
//		beta2 = asset.beta2;
//		beta1 = asset.beta1;

		pnumer [0] = asset.palpha1;
		pnumer [1] = asset.palpha2;
		pnumer [2] = asset.palpha3;
		pdenom [0] = asset.pbeta1;
		pdenom [1] = asset.pbeta2;
		pdenom [2] = asset.pbeta3;

		qnumer [0] = asset.qalpha1;
		qnumer [1] = asset.qalpha2;
		qnumer [2] = asset.qalpha3;
		qdenom [0] = asset.qbeta1;
		qdenom [1] = asset.qbeta2;
		qdenom [2] = asset.qbeta3;

		// new latency with added eu
		float latency = asset.latency + euLatency;

		//grid size for mesh point generation
		float incrementT = 1.0f/(float)timeStepsPerSecond;
		float incrementP = 2*Mathf.PI/(float)polarSteps;

		int maxCountT = Mathf.FloorToInt (totalTimeSeconds / incrementT);
		int maxCountP = polarSteps;

		float currentTime = 0;
		float currentPolar = 0;

		float maxS = Mathf.Max (asset.maxP, asset.maxQ);

		// the calculated point
		Vector3 point = Vector3.zero;

		List<Vector2> linePoints = new List<Vector2> ();

		for (int i = 0; i < maxCountT; i++) {
//			PTransfer.Add(laplace.InverseTransform (this.f, i));
//
			Pt.Add (0.0f);
			Qt.Add (0.0f);
			// if time hasn't reached latency time set to 0
			if (currentTime < latency) {
				Pt [i] = 0.0f;
				Qt [i] = 0.0f;
			} else {

				if (pnumer [0] == 0.0 && pnumer [1] == 0.0 && pnumer [2] == 0.0 && pdenom [0] == 0.0 &&  pdenom [1] == 0.0 && pdenom [2] == 0.0) {

					//handle p agility
					if (currentTime < (latency + asset.agilityP)) {
						Pt [i] = asset.maxP * (currentTime - latency) / asset.agilityP;
					} else {
						Pt [i] = asset.maxP;
					}
				} else {
					//handle p transfer function
					Pt [i] = (float)(asset.maxP * Laplace.InverseTransform2 (pnumer, pdenom, currentTime-latency));

				}


				if (qnumer [0] == 0.0 && qnumer [1] == 0.0 && qnumer [2] == 0.0 && qdenom [0] == 0.0 &&  qdenom [1] == 0.0 && qdenom [2] == 0.0) {

					// handle q agility
					if (currentTime < (latency + asset.agilityQ)) {
						Qt [i] = asset.maxQ * (currentTime - latency) / asset.agilityQ;			
					} else {
						Qt [i] = asset.maxQ;
					}
				} else {
					//handle q transfer function
					Qt [i] = (float)(asset.maxQ * (Laplace.InverseTransform2 (qnumer, qdenom, currentTime-latency)));

				}
			}

			//Pt.Add((float)Laplace.InverseTransform (this.f, (double)currentTime));
			//Qt.Add((float)Laplace.InverseTransform (this.f, (double)currentTime));
//
//			Pt.Add((float)Laplace.gwr (this.f, (double)currentTime,14));
//			Qt.Add((float)Laplace.gwr (this.f, (double)currentTime,14));
//
//			currentPolar = 0.0f;
//			float test =  (float)Laplace.gwr (this.f, (double)300, 380);
//
			if (linePoints.Count <= 380) {
			//	linePoints.Add(new Vector2(currentTime, (float)Laplace.InverseTransform (this.f, (double)currentTime)));
				linePoints.Add(new Vector2(currentTime, (float)(Laplace.InverseTransform2 (pnumer, pdenom, currentTime))));
			}

			currentPolar = 0.0f;

			for (int j = 0; j < maxCountP; j++) {

				point.z = currentTime;

				if ((Pt [i] * Qt [i]) > 0.0f) {
					float x = maxS * Mathf.Cos (currentPolar);
					float y = maxS * Mathf.Sin (currentPolar);

					point.x = Mathf.Sign (x) * Mathf.Min (Mathf.Abs (x), Pt [i]); //limit to the maximum of Pt and Qt
					point.y = Mathf.Sign (y) * Mathf.Min (Mathf.Abs (y), Qt [i]); //limit to the maximum of Pt and Qt

					float pointAngle = Mathf.Atan2(point.y, point.x);

					if (Mathf.Abs (currentPolar - pointAngle) > 0.001f) {
						float testX = point.y / Mathf.Tan (currentPolar);
						float testY = point.x * Mathf.Tan (currentPolar);

						if (Mathf.Abs (testX) < Pt [i]) {
							point.x = testX;
						} else {
							point.x = Mathf.Sign (testX) * Pt [i];
						}

						if (Mathf.Abs (testY) < Qt [i]) {
							point.y = testY;
						} else {
							point.y = Mathf.Sign (testY) * Qt [i];
						}
					}
				} else {

					if (Mathf.Abs (Mathf.Cos (currentPolar)) == 1) {
						point.x = Mathf.Sign (Mathf.Cos (currentPolar)) * Pt [i];
					} else {
						point.x = 0.0f;
					}

					if (Mathf.Abs (Mathf.Sin (currentPolar)) == 1) {
						point.y = Mathf.Sign (Mathf.Sin (currentPolar)) * Qt [i];
					} else {
						point.y = 0.0f;
					}
				}

				currentPolar += incrementP;
				points.Add (point);
			}
			currentTime += incrementT;
		}
		float test = 0.0f;
		if (asset.energy > 0) {

			// sum up each "column" of data
			// if that sum is greater than energy zeroize the rest of that column
			// since each row is appended end to end, need to figure out the start of that column, which is just the number of polarsteps


			//shift the "columns" after going through each row
			for (int i = 0; i < maxCountP; i++){

				float sumIt = 0.0f;
				bool maxedOut = false;

				//shift the row down and sum, once done reset and move onto next column
				for (int j = 0; j < maxCountT; j++) {

					sumIt += points [j*maxCountP + i].x;

					if ((Mathf.Abs (sumIt * incrementT) > asset.energy) || maxedOut) {
						Vector3 tempPoint = points [j*maxCountP + i];
						tempPoint.x = 0.0f;
						tempPoint.y = 0.0f;
						points [j*maxCountP + i] = tempPoint;
						maxedOut = true;
					}
				}
				test = sumIt;
			}
		}


		VectorLine line2d = new VectorLine ("line2d", linePoints, 1.0f, LineType.Continuous, Joins.Fill);
		line2d.Draw ();
		return points;
	}
}
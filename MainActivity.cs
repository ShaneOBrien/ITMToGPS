using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using DotSpatial.Projections;

namespace ITMtoGPS
{
	[Activity (Theme = "@android:style/Theme.Holo.NoActionBar", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		ProjectionInfo convertFrom = KnownCoordinateSystems.Projected.NationalGrids.IRENET95IrishTranverseMercator;//   Geographic.Europe.IRENET95;
		ProjectionInfo convertTo = KnownCoordinateSystems.Geographic.World.WGS1984;

		double xCoordsITM = 600000, yCoordsITM = 750000, xCoordsGPS = 53, yCoordsGPS = -6.5;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);


			// Get our button from the layout resource,
			// and attach an event to it
			Button convertToGPS = FindViewById<Button>(Resource.Id.convertITMToGPS);
			Button convertToITM = FindViewById<Button> (Resource.Id.convertGPSToITM);
			Button convertToDecimal = FindViewById<Button> (Resource.Id.convertLatLongDeg);

			Button openGMaps = FindViewById<Button>(Resource.Id.openGoogleMaps);
			Button openOSI = FindViewById<Button>(Resource.Id.openOSI);

			EditText eastingField = FindViewById<EditText>(Resource.Id.eastings);
			EditText northingField = FindViewById<EditText>(Resource.Id.northings);

			EditText latitudeField = FindViewById<EditText>(Resource.Id.latitude);
			EditText longtitudeField = FindViewById<EditText>(Resource.Id.longtitude);

			convertToGPS.Click += delegate 
			{
				xCoordsITM = double.Parse(eastingField.Text);
				yCoordsITM = double.Parse(northingField.Text);
				if(xCoordsITM > 400000 && xCoordsITM < 800000 || yCoordsITM > 500000 && yCoordsITM < 999999)
				{
					CalculateIRENET95ToWGS1984();
				}
			};
			convertToITM.Click += delegate 
			{
				xCoordsGPS = double.Parse(latitudeField.Text);
				yCoordsGPS = double.Parse(longtitudeField.Text);
				if(xCoordsGPS > 51 && xCoordsGPS < 55.999999997222226 || yCoordsGPS > -10 && yCoordsGPS < -4.000000002777778)
				{
					CalculateWGS1984ToIRENET95();
				}
			};
			convertToDecimal.Click += delegate 
			{
				CalculateDecimalGPS();
			};

			openGMaps.Click += delegate 
			{
				var geoUri = Android.Net.Uri.Parse ("geo:"+latitudeField.Text+"," + longtitudeField.Text+"?q="+latitudeField.Text+"," + longtitudeField.Text);
				var mapIntent = new Intent (Intent.ActionView, geoUri);
				StartActivity (mapIntent);
			};
			openOSI.Click += delegate 
			{
				var uri = Android.Net.Uri.Parse ("http://maps.osi.ie/publicviewer/#V1,"+eastingField.Text+","+northingField.Text+",7,10");
				var intent = new Intent (Intent.ActionView, uri); 
				StartActivity (intent);     
			};
		}

		void CalculateIRENET95ToWGS1984()
		{
			EditText eastingField = FindViewById<EditText>(Resource.Id.eastings);
			EditText northingField = FindViewById<EditText>(Resource.Id.northings);

			EditText latitudeField = FindViewById<EditText>(Resource.Id.latitude);
			EditText longtitudeField = FindViewById<EditText>(Resource.Id.longtitude);

			double[] ITMXY = new double[2];
			ITMXY[0] = xCoordsITM;
			ITMXY[1] = yCoordsITM;

			double[] zValue = new double[1];
			zValue[0] = 0;

			Reproject.ReprojectPoints(ITMXY, zValue, convertFrom, convertTo, 0, zValue.Length);

			xCoordsGPS = ITMXY [1];
			yCoordsGPS = ITMXY [0];

			eastingField.Text = xCoordsITM.ToString ();
			northingField.Text = yCoordsITM.ToString();

			string lat = string.Format ("{0:0.000000}", xCoordsGPS);
			xCoordsGPS = double.Parse (lat);
			latitudeField.Text = lat;
			string lon = string.Format ("{0:0.000000}", yCoordsGPS);
			yCoordsGPS = double.Parse (lon);
			longtitudeField.Text = lon;
			CalculateDegMinSecGPS ();
		}

		void CalculateWGS1984ToIRENET95()
		{

			EditText eastingField = FindViewById<EditText>(Resource.Id.eastings);
			EditText northingField = FindViewById<EditText>(Resource.Id.northings);

			EditText latitudeField = FindViewById<EditText>(Resource.Id.latitude);
			EditText longtitudeField = FindViewById<EditText>(Resource.Id.longtitude);

			double[] ITMXY = new double[2];
			ITMXY[1] = xCoordsGPS;
			ITMXY[0] = yCoordsGPS;

			double[] zValue = new double[1];
			zValue[0] = 0;

			Reproject.ReprojectPoints(ITMXY, zValue, convertTo, convertFrom, 0, zValue.Length);

			xCoordsITM = ITMXY [0];
			yCoordsITM = ITMXY [1];

			eastingField.Text = Math.Round(ITMXY [0]).ToString ();
			northingField.Text = Math.Round(ITMXY [1]).ToString ();

			string lat = string.Format ("{0:0.000000}", xCoordsGPS);
			xCoordsGPS = double.Parse (lat);
			latitudeField.Text = lat;
			string lon = string.Format ("{0:0.000000}", yCoordsGPS);
			yCoordsGPS = double.Parse (lon);
			longtitudeField.Text = lon;
			CalculateDegMinSecGPS ();
		}

		void CalculateDecimalGPS()
		{
			EditText latitudeField = FindViewById<EditText>(Resource.Id.latitude);
			EditText longtitudeField = FindViewById<EditText>(Resource.Id.longtitude);

			EditText latDeg = FindViewById<EditText>(Resource.Id.latDeg);
			EditText latMin = FindViewById<EditText>(Resource.Id.latMin);
			EditText latSec = FindViewById<EditText>(Resource.Id.latSec);

			EditText longDeg = FindViewById<EditText>(Resource.Id.longDeg);
			EditText longMin = FindViewById<EditText>(Resource.Id.longMin);
			EditText longSec = FindViewById<EditText>(Resource.Id.longSec);

			double latDegTemp = double.Parse(latDeg.Text);
			double latMinTemp = double.Parse(latMin.Text) / 60;
			double latSecTemp = double.Parse(latSec.Text) / 3600;
			xCoordsGPS =  latDegTemp + latMinTemp + latSecTemp;
			string lat = string.Format ("{0:0.000000}", xCoordsGPS);
			xCoordsGPS = double.Parse (lat);
			latitudeField.Text = lat;

			double longDegTemp = double.Parse(longDeg.Text) *-1;
			double longMinTemp = double.Parse(longMin.Text) / 60;
			double longSecTemp = double.Parse(longSec.Text) / 3600;
			yCoordsGPS = longDegTemp + longMinTemp + longSecTemp;
			yCoordsGPS*=-1;
			string lon = string.Format ("{0:0.000000}", yCoordsGPS);
			yCoordsGPS = double.Parse (lon);
			longtitudeField.Text = lon;

			if(xCoordsGPS > 51 && xCoordsGPS < 55.999999997222226 || yCoordsGPS > -10 && yCoordsGPS < -4.000000002777778)
			{
				CalculateWGS1984ToIRENET95();
			}
		}

		void CalculateDegMinSecGPS()
		{
			EditText latitudeField = FindViewById<EditText>(Resource.Id.latitude);
			EditText longtitudeField = FindViewById<EditText>(Resource.Id.longtitude);

			EditText latDeg = FindViewById<EditText>(Resource.Id.latDeg);
			EditText latMin = FindViewById<EditText>(Resource.Id.latMin);
			EditText latSec = FindViewById<EditText>(Resource.Id.latSec);

			EditText longDeg = FindViewById<EditText>(Resource.Id.longDeg);
			EditText longMin = FindViewById<EditText>(Resource.Id.longMin);
			EditText longSec = FindViewById<EditText>(Resource.Id.longSec);

			double latDegTemp = (int)xCoordsGPS;
			double latMinTemp = (int)((xCoordsGPS - latDegTemp) * 60);
			double latSecTemp = (xCoordsGPS - latDegTemp - latMinTemp / 60) * 3600;
			latSecTemp = Math.Round (latSecTemp, 3);

			double longDegTemp = (int)yCoordsGPS;
			double longMinTemp = (int)((yCoordsGPS - longDegTemp) * 60);
			double longSecTemp = (yCoordsGPS - longDegTemp - longMinTemp / 60) * 3600;
			longSecTemp = Math.Round (longSecTemp, 3);
			longMinTemp *= -1;
			longSecTemp *= -1;

			latDeg.Text = latDegTemp.ToString();
			latMin.Text = latMinTemp.ToString();
			latSec.Text = latSecTemp.ToString();

			longDeg.Text = longDegTemp.ToString();
			longMin.Text = longMinTemp.ToString();
			longSec.Text = longSecTemp.ToString();
		}
	}
}



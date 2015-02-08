using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace XamarinLightRecorder
{
	using Android.Hardware;

	[Activity (Label = "XamarinLightRecorder", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, Android.Hardware.ISensorEventListener
	{
		TextView illuminationView;
		TextView historyView;
		private Android.Hardware.SensorManager manager;
		private Android.Hardware.Sensor lightSensor;
		private bool detectable = false;
		private DateTime lastRecordTime = DateTime.MinValue;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			illuminationView = FindViewById<TextView> (Resource.Id.Illumination);
			historyView = FindViewById<TextView> (Resource.Id.History);
			manager = (SensorManager)GetSystemService (SensorService);
			lightSensor = manager.GetDefaultSensor (SensorType.Light);

			var executeButton = FindViewById<Button> (Resource.Id.Execute);
			executeButton.Click += (sender, e) => {
				detectable = !detectable;
				executeButton.Text = detectable ? "停止" : "開始";

				if (detectable) {
					RegisterLightListener();
				}
				else {
					UnRegisterLightListener();
				}
			};

			var clearButton = FindViewById<Button> (Resource.Id.Clear);
			clearButton.Click += (sender, e) => historyView.Text = "";
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			if (detectable) RegisterLightListener ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			UnRegisterLightListener ();
		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy){}

		public void OnSensorChanged(SensorEvent e)
		{
			if (e.Sensor.Type == SensorType.Light) {
				var lux = e.Values [0];
				var now = DateTime.Now;
				illuminationView.Text = lux.ToString ();

				if (lastRecordTime.AddSeconds(5) < now) {
					historyView.Text += now.ToString ("hh:mm:ss") + " > " + lux.ToString () + " lx" + System.Environment.NewLine;
					lastRecordTime = now;
				}
			}
		}

		// ボタンクリックとOnResume/OnPauseに対応するためにメソッドへと切り出した
		private void RegisterLightListener()
		{
			var batchEnabled = manager.RegisterListener (this, lightSensor, SensorDelay.Normal, 5000000);

			var textView = FindViewById<TextView> (Resource.Id.BatchMode);
			textView.Text = batchEnabled ? "有効" : "無効";
		}

		private void UnRegisterLightListener()
		{
			manager.UnregisterListener (this);
			lastRecordTime = DateTime.MinValue;
		}
	}
}
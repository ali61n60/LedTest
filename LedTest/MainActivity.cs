using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace LedTest
{
    [Activity(Label = "LedTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        Button button;
        bool LedIsOn = false;
        Camera camera;
        int onTime;
        int offTime;
        int defautOnTime = 100;// 100 milisecond
        int defaultOffTime = 100;

        EditText editTextOnTime;
        EditText editTextOffTime;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            button = FindViewById<Button>(Resource.Id.MyButton);
            editTextOnTime = FindViewById<EditText>(Resource.Id.editTextOnTime);
            editTextOffTime = FindViewById<EditText>(Resource.Id.editTextOffTime);


        }

        protected override void OnResume()
        {
            if (!PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCamera))
            {
                Toast.MakeText(this, "No back-facing camera available", ToastLength.Long).Show();
                return;
            }

            if (!PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCameraFlash))
            {
                Toast.MakeText(this, "No camera flash available", ToastLength.Long).Show();
                return;
            }

            camera = Camera.Open();
            button.Click += button_Click;
            base.OnResume();
        }

        protected override void OnPause()
        {
            button.Click -= button_Click;
            camera.Release();

            base.OnStop();
        }




        void button_Click(object sender, EventArgs e)
        {
            string buttonText = "";
            try
            {
                onTime = int.Parse(editTextOnTime.Text);
                offTime = int.Parse(editTextOffTime.Text);
            }
            catch (Exception ex)
            {
                onTime = defautOnTime;
                offTime = defaultOffTime;
            }
            if (LedIsOn)
            {
                turnOffLight();
                buttonText = "Off";
            }
            else
            {
                turnOnLight(onTime, offTime);
                buttonText = "On";
            }
            button.Text = buttonText;
        }

        private void turnOnLight(int onTime,int offTime)
        {
            Thread lighThread = new Thread(run);
            lighThread.IsBackground = true;
            int[] onTimeOffTime=new int[2];
            onTimeOffTime[0] = onTime;
            onTimeOffTime[1] = offTime;
            lighThread.Start(onTimeOffTime);
            LedIsOn = true;

        }

        private void run(object o)
        {
            int[] onTimeOffTime = (int[]) o;
            int onTime = onTimeOffTime[0];
            int offTime = onTimeOffTime[1];
            var parameters = camera.GetParameters();
            while (LedIsOn)
            {
                parameters.FlashMode = Camera.Parameters.FlashModeTorch;
                camera.SetParameters(parameters);
                camera.StartPreview();
                Thread.Sleep(onTime);
                parameters.FlashMode = Camera.Parameters.FlashModeOff;
                camera.SetParameters(parameters);
                camera.StopPreview();
                Thread.Sleep(offTime);
            }
        }

        private void turnOffLight()
        {
            var parameters = camera.GetParameters();
            parameters.FlashMode = Camera.Parameters.FlashModeOff;
            camera.SetParameters(parameters);
            camera.StopPreview();
            LedIsOn = false;
        }

    }
}


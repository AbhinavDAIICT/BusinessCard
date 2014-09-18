using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OCR_trial1.Resources;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using WindowsPreview.Media.Ocr;
using Microsoft.Devices;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;



namespace OCR_trial1
{
    public partial class MainPage : PhoneApplicationPage
    {
        

        /// <Camera Variables>
        private int savedCounter = 0;
        PhotoCamera cam;
        MediaLibrary library = new MediaLibrary();
        String fileName;
        /// </Camera Variables>
        OcrEngine ocrEngine;
        UInt32 width;
        UInt32 height;
        BitmapImage bi;
        WriteableBitmap bitmap;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ocrEngine = new OcrEngine(OcrLanguage.English);
        }
    
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            String path = "abc.png";
            WriteableBitmap wbit;
            byte[] pix;
            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            using (var fs = isf.OpenFile(path, System.IO.FileMode.Open, FileAccess.Read))
            {
                var image = new BitmapImage();
                image.SetSource(fs);
               wbit = new WriteableBitmap(image);
               height = (UInt32)wbit.PixelHeight;
                width = (UInt32)wbit.PixelWidth;
               int[] p = wbit.Pixels;
               int len = p.Length * 4;
               pix = new byte[len]; // ARGB
               System.Buffer.BlockCopy(p, 0, pix, 0, len);
            }

                // Extract text from image.

                OcrResult result = await ocrEngine.RecognizeAsync(height, width, pix);
                OcrText.Text = "Here 1";
                // Check whether text is detected.
                if (result.Lines != null)
                {
                    // Collect recognized text.
                    string recognizedText = "";
                    OcrText.Text = "Here 2";

                    foreach (var line in result.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            OcrText.Text = "Here 3";

                            recognizedText += word.Text + " ";
                        }
                        recognizedText += System.Environment.NewLine;
                    }

                    // Display recognized text.
                    OcrText.Text = recognizedText;
                    await WriteToFile(recognizedText);

                 //   OcrText.Text = "Here 4";
                }
                else
                {
                    txtDebug.Text = "No Text found";
                }
            }


        //Code for initialization, capture completed, image availability events; also setting the source for the viewfinder.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            // Check to see if the camera is available on the phone.
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true)
            {
                // Initialize the camera, when available.
              
                    // Otherwise, use standard camera on back of phone.
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
            

                // Event is fired when the PhotoCamera object has been initialized.
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                // Event is fired when the capture sequence is complete.
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);

                // Event is fired when the capture sequence is complete and an image is available.
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

                // Event is fired when the capture sequence is complete and a thumbnail image is available.
            //   cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);

                //Set the VideoBrush source to the camera.
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                // The camera is not supported on the phone.
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    OcrText.Text = "A Camera is not available on this phone.";
                });

            }
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
          //     cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
            }
        }
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    OcrText.Text = "Camera initialized.";
                });
            }
        }

        void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            savedCounter++;
            
        }

        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            fileName = "abc.png";

            try
            {
               
                // Save photo as JPEG to the local folder.
             using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
             {
                 using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                 {
                     // Initialize the buffer for 4KB disk pages.
                     byte[] readBuffer = new byte[4096];
                     int bytesRead = -1;
                     // Copy the image to the local folder. 
                     while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                     {
                         targetStream.Write(readBuffer, 0, bytesRead);
                     }
                 }
             }

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    OcrText.Text = "Photo has been saved to the local folder.";
                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }

        private void viewfinder_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        OcrText.Text = ex.Message;
                    });
                }
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}
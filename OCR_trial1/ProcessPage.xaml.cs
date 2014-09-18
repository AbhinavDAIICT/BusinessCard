using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Storage;
using System.IO;

namespace OCR_trial1
{
    public partial class ProcessPage : PhoneApplicationPage
    {
        public ProcessPage()
        {

            InitializeComponent();
            reader();
        }

       
        private async void reader(){
        StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

        if (local != null)
        {
            this.problk.Text = "Initialized";
            // Get the DataFolder folder.
            //var dataFolder = await local.GetFolderAsync("DataFolder");

            //// Get the file.
            //var file = await dataFolder.OpenStreamForReadAsync("DataFile.txt");

            //// Read the data.
            //using (StreamReader streamReader = new StreamReader(file))
            //{
            //    this.problk.Text = streamReader.ReadToEnd();
            //}
        }
        }
    }
}
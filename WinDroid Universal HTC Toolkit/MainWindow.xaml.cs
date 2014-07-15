using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Net;
using MahApps.Metro.Controls;
using RegawMOD.Android;
using System.Diagnostics;

namespace WinDroid_Universal_HTC_Toolkit
{
    public partial class MainWindow : MetroWindow
    {
        private AndroidController _android;
        private Device _device;
        
        public MainWindow()
        {
            InitializeComponent();
            deviceRecognition.DoWork += deviceRecognition_DoWork;
            noReturnADBCommand.DoWork += noReturnADBCommand_DoWork;
            noReturnADBCommand.RunWorkerCompleted += noReturnADBCommand_RunWorkerCompleted;
            tokenID.DoWork += tokenID_DoWork;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            deviceRecognition.RunWorkerAsync();
        }

        private readonly BackgroundWorker deviceRecognition = new BackgroundWorker();
        private readonly BackgroundWorker noReturnADBCommand = new BackgroundWorker();
        private readonly BackgroundWorker tokenID = new BackgroundWorker();

        private void deviceRecognition_DoWork(object sender, DoWorkEventArgs e)
        {
            _android = AndroidController.Instance;
            try
            {
                _android.UpdateDeviceList();
                if (_android.HasConnectedDevices)
                {
                    _device = _android.GetConnectedDevice(_android.ConnectedDevices[0]);
                    switch (_device.State.ToString())
                    {
                        case "ONLINE":
                            if (_device.BuildProp.GetProp("ro.product.model") == null)
                            {
                            }
                            else
                            {
                                MessageBox.Show(_device.BuildProp.GetProp("ro.product.model"));
                                DeviceTextBox.Text = _device.BuildProp.GetProp("ro.product.model");
                            }
                            break;

                        case "FASTBOOT":

                            break;

                        case "RECOVERY":

                            break;

                        case "UNKNOWN":

                            break;
                    }

                }
                else
                {

                }
                _android.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private void noReturnADBCommand_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(AndroidLib.InitialCmd));
                _android.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private void noReturnADBCommand_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                switch(AndroidLib.Selector)
                {
                    case "tokenID":
                        AndroidLib.InitialCmd = "oem";
                        AndroidLib.SecondaryCmd = "get_identifier_token";
                        tokenID.RunWorkerAsync();
                        break;
                }

                GetSuperCIDButton.IsEnabled = true;
            }
            catch (Exception)
            {

            }
        }

        private static string GetStringBetween(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start, StringComparison.InvariantCulture) + start.Length;
            int endIndex = source.IndexOf(end, startIndex, StringComparison.InvariantCulture);
            int length = endIndex - startIndex;
            return source.Substring(startIndex, length);
        }

        private void tokenID_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (StreamWriter sw = File.CreateText("./Data/token.txt"))
                {
                    string rawReturn = Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(_device, AndroidLib.InitialCmd, AndroidLib.SecondaryCmd));
                    string rawToken = GetStringBetween(rawReturn, "< Please cut following message >\r\n",
                        "\r\nOKAY");
                    string cleanedToken = rawToken.Replace("(bootloader) ", "");
                    sw.WriteLine(cleanedToken);
                    sw.WriteLine(" ");
                    sw.WriteLine(
                        "PLEASE COPY EVERYTHING ABOVE THIS LINE!");
                    sw.WriteLine(" ");
                    sw.WriteLine("NEXT, SIGN IN TO YOUR HTC DEV ACCOUNT ON THE WEBPAGE THAT JUST OPENED!");
                    sw.WriteLine(
                        "IF YOU DO NOT HAVE ONE, CREATE AND ACTIVATE AN ACCOUNT WITH A VALID EMAIL ADDRESS THEN COME BACK TO THIS LINK:");
                    sw.WriteLine("http://www.htcdev.com/bootloader/unlock-instructions/page-3");
                    sw.WriteLine(
                        "THEN, PASTE THE TOKEN ID YOU JUST COPIED AT THE BOTTOM OF THE HTCDEV WEBPAGE THAT JUST OPENED!");
                    sw.WriteLine("HIT SUBMIT, AND WAIT FOR THE EMAIL WITH THE UNLOCK BINARY FILE!");
                    sw.WriteLine(" ");
                    sw.WriteLine(
                        "ONCE YOU HAVE RECEIVED THE UNLOCK FILE IN YOUR EMAIL, YOU CAN CONTINUE ON TO THE NEXT STEP!");
                    sw.WriteLine("THIS FILE IS SAVED AS token.txt WITHIN THE DATA FOLDER IF NEEDED FOR FUTURE USE!");
                }
                Process.Start("http://www.htcdev.com/bootloader/unlock-instructions/page-3");
                Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + "./Data/token.txt");
                _android.Dispose();
            }
            catch (Exception)
            {

            }
        }

        #region Nested type: AndroidLib

        private static class AndroidLib
        {
            public static string InitialCmd = "";
            public static string SecondaryCmd = "";
            public static string Selector = "";
        }

        #endregion Nested type: AndroidLib

        private void GetSuperCIDButton_Click(object sender, RoutedEventArgs e)
        {
            AndroidLib.InitialCmd = "reboot";
            noReturnADBCommand.RunWorkerAsync();
            GetSuperCIDButton.IsEnabled = false;
        }

        private void GetTokenIDButton(object sender, RoutedEventArgs e)
        {
            AndroidLib.InitialCmd = "reboot bootloader";
            AndroidLib.Selector = "tokenID";
            noReturnADBCommand.RunWorkerAsync();
        }

        private void UnlockBootloaderButton(object sender, RoutedEventArgs e)
        {

        }
    }
}

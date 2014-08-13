using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR.Drone.Client;
using AR.Drone.Client.Command;
using AR.Drone.Client.Configuration;
using AR.Drone.Data;
using AR.Drone.Data.Navigation;
using AR.Drone.Data.Navigation.Native;
using AR.Drone.Media;
using AR.Drone.Video;
using AR.Drone.Avionics;
using AR.Drone.Avionics.Objectives;
using AR.Drone.Avionics.Objectives.IntentObtainers;
using ARDrone.Capture;
using AR.Drone.Data.Navigation.Native.Options;
using AR.Drone.Data.Navigation.Native;
using AR.Drone.Data.Navigation;
using System.Runtime.InteropServices;


using System.Drawing.Imaging;
using System.IO;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
namespace AR.Drone.WinApp
{
    
    public partial class MainForm : Form
    {
        //private VideoRecorder videoRecorder;
        //private SnapshotRecorder snapshotRecorder;
        
        
        private const string ARDroneTrackFileExt = ".ardrone";
        private const string ARDroneTrackFilesFilter = "AR.Drone track files (*.ardrone)|*.ardrone";

        //private readonly NavdataConverter _testaltt;
        private readonly DroneClient _droneClient;
        private readonly List<PlayerForm> _playerForms;
        private readonly VideoPacketDecoderWorker _videoPacketDecoderWorker;
        private Settings _settings;
        private VideoFrame _frame;
        private Bitmap _frameBitmap;
        private uint _frameNumber;
        private NavigationData _navigationData;
        private NavigationPacket _navigationPacket;
        private PacketRecorder _packetRecorderWorker;
        private FileStream _recorderStream;
        private Autopilot _autopilot;

/*
        // member variables ///////////////////////////////////////////////////////////////////////////
        private Capture capWebcam = null;
        private bool blnCapturingInProcess = false;
        private Image<Bgr, Byte> imgSub;
        private Image<Bgr, Byte> imgOriginal;
        private Image<Gray, Byte> imgProcessed;

        bool blnStartTrack = false;
        int loopGo = 0;

        private Image<Bgr, Byte> getBitmapConverEmgu;
        private Bitmap getBitmapFromtmr;
*/


        ///////////////////////////////////////////////////////////////////////////////////////////////
        private float altitudeOnTrack = 0;
        private float altitudeOutTrack = 0;
        private float altitudeTotle = 0;
        ///////////////////////////////////////////////////////////////////////////////////////////////
        public Bitmap droneSteaming = null;
        Bitmap stvideo = null;
        Bitmap stvideo2 = null;
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        int red;
        int blue;
        int green;

        Bitmap videoClon;
        //Bitmap video2;
        Graphics g;
        int setMode = 0;
        int modeTrackObject = 0;
        
        //bool OnOff = false;
        //int testTT = 5;
        //private FilterInfoCollection CaptureDevice;
        //private VideoCaptureDevice FinalFrame;

        private Byte[] buffer = new Byte[1];
        private bool serialok = false;
        private float map = 0f;
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        int ECrange = 120;

        private int objectX;
        private int objectY;
        private int switchCam = 0; //  0 = HD , 1 = VGA

        int areaObjcetX = 0;
        int areaObjcetY = 0;

        Color ECcolor = Color.Red;
        int Wobject = 0;
        int Hobject = 0;
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        public MainForm()
        {
            InitializeComponent();

            _videoPacketDecoderWorker = new VideoPacketDecoderWorker(AR.Drone.Video.PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _videoPacketDecoderWorker.Start();

            _droneClient = new DroneClient("192.168.1.1");
            _droneClient.NavigationPacketAcquired += OnNavigationPacketAcquired;
            _droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
            _droneClient.NavigationDataAcquired += data => _navigationData = data;

            tmrStateUpdate.Enabled = true;
            tmrVideoUpdate.Enabled = true;

            _playerForms = new List<PlayerForm>();

            _videoPacketDecoderWorker.UnhandledException += UnhandledException;
        }

        private void UnhandledException(object sender, Exception exception)
        {
            MessageBox.Show(exception.ToString(), "Unhandled Exception (Ctrl+C)", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text += Environment.Is64BitProcess ? " [64-bit]" : " [32-bit]";
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_autopilot != null)
            {
                _autopilot.UnbindFromClient();
                _autopilot.Stop();
            }

            StopRecording();

            _droneClient.Dispose();
            _videoPacketDecoderWorker.Dispose();

            base.OnClosed(e);
        }

        private void OnNavigationPacketAcquired(NavigationPacket packet)
        {
            if (_packetRecorderWorker != null && _packetRecorderWorker.IsAlive)
                _packetRecorderWorker.EnqueuePacket(packet);

            _navigationPacket = packet;
        }

        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (_packetRecorderWorker != null && _packetRecorderWorker.IsAlive)
                _packetRecorderWorker.EnqueuePacket(packet);
            if (_videoPacketDecoderWorker.IsAlive)
                _videoPacketDecoderWorker.EnqueuePacket(packet);
        }

        private void OnVideoPacketDecoded(VideoFrame frame)
        {
            _frame = frame;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _droneClient.Start();

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _droneClient.Stop();
        }

        private void tmrVideoUpdate_Tick(object sender, EventArgs e)
        {
            if (_frame == null || _frameNumber == _frame.Number)
                return;
            _frameNumber = _frame.Number;

            if (_frameBitmap == null)
            {
                _frameBitmap = VideoHelper.CreateBitmap(ref _frame);
            }
            else
            {
                VideoHelper.UpdateBitmap(ref _frameBitmap, ref _frame);
            }

            pbVideo.Image = _frameBitmap;
            ////////////////////////////////////////////////////////////////////////////
            droneSteaming = _frameBitmap;
            stvideo = (Bitmap)pbVideo.Image.Clone();
            stvideo2 = _frameBitmap;
            switch (setMode)
            {
                case 1:
                    {
                        EuclideanColorFiltering ECfilter = new EuclideanColorFiltering();
                        ECfilter.CenterColor = new AForge.Imaging.RGB(ECcolor);
                        ECfilter.Radius = (short)ECrange;
                        ECfilter.ApplyInPlace(stvideo);

                        BlobCounter blobcounter = new BlobCounter();
                        blobcounter.MinHeight = 20;
                        blobcounter.MinWidth = 20;
                        blobcounter.ObjectsOrder = ObjectsOrder.Size;
                        blobcounter.ProcessImage(stvideo);
                        Rectangle[] rect = blobcounter.GetObjectsRectangles();
                        if (rect.Length > 0)
                        {
                            Rectangle objec = rect[0];
                            Graphics graphic = Graphics.FromImage(stvideo);
                            using (Pen pen = new Pen(Color.White, 3))
                            {
                                graphic.DrawRectangle(pen, objec);

                                Wobject = objec.Width;
                                Hobject = objec.Height;

                                PointF drawPoin = new PointF(objec.X, objec.Y);
                                objectX = objec.X + objec.Width / 2 - stvideo.Width / 2;
                                objectY = stvideo.Height / 2 - (objec.Y + objec.Height / 2);
                                String Blobinformation = "X= " + objectX.ToString() + "\nY= " + objectY.ToString() + "\nSize=" + objec.Size.ToString();
                                graphic.DrawString(Blobinformation, new Font("Arial", 16), new SolidBrush(Color.Blue), drawPoin);
                                
                                if (serialok == true)
                                {
                                    int second = 0;
                                    int offset = 300;
                                    second = offset - Math.Abs(objectX);
                                    map = (float)0.85 * second;
                                    buffer[0] = (byte)Math.Abs((int)map);
                                    // serialPort1.Write(buffer, 0, 1);
                                }
                            }
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            txtXYRadius.AppendText("position = x = " + objectX.ToString().PadLeft(4) + " y = " + objectY.ToString().PadLeft(4) + "\n");
                            txtXYRadius.ScrollToCaret();
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            graphic.Dispose();
                        }
                        
                        
                        pictureBox1.Image = stvideo;
                        pictureBox2.Image = stvideo2;

                        //////////////////////////////////////////////// Controll Move //////////////////////////////////////////////////////////////
                        #region Controll Move
                        int moveWay = 9;
                        string txtTestWay = null;
                        if (objectY > 0)
                        {
                            if (objectX > 0)
                            { 
                                txtTestWay = "Top Left";
                                moveWay = 0;
                            }
                            else if (objectX == 0)
                            {
                                txtTestWay = "Top";
                                moveWay = 1;
                            }
                            else
                            {
                                txtTestWay = "Top Right";
                                moveWay = 2;
                            }
                        }
                        else if (objectY < 0)
                        {
                            if (objectX > 0)
                            {
                                txtTestWay = "Under Right";
                                moveWay = 6;
                            }
                            else if (objectX == 0)
                            {
                                txtTestWay = "Under";
                                moveWay = 7;
                            }
                            else
                            {
                                txtTestWay = "Under Left";
                                moveWay = 8;
                            }
                        }
                        else // objectY == 0
                        {
                            if (objectX > 0)
                            {
                                txtTestWay = "Right";
                                moveWay = 3;
                            }
                            else if (objectX == 0)
                            {
                                txtTestWay = "Center";
                                moveWay = 4;
                            }
                            else
                            {
                                txtTestWay = "Right";
                                moveWay = 5;
                            }
                        }
                        #endregion

                        textBox3.AppendText(txtTestWay + "  \n");
                        textBox3.ScrollToCaret();

                        /////////////////////////////////////////////// Check Camera And Command /////////////////////////////////////////////////////
                        int areaObject = areaObjcetX * areaObjcetY;
                        int areaObjectNew = Wobject * Hobject;
                        #region Command
                        if (modeTrackObject == 1)
                        {
                            if (switchCam == 0)
                            {
                                switch (moveWay)
                                {
                                    case 0:
                                        _droneClient.Progress(FlightMode.Progressive, roll: -0.05f); //Left
                                        _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f); //Top
                                        
                                        break;
                                    case 1:
                                        _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f); //Top
                                        break;
                                    case 2:
                                        _droneClient.Progress(FlightMode.Progressive, roll: 0.05f); //Right
                                        _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f); //Top
                                        break;
                                    case 3:
                                        _droneClient.Progress(FlightMode.Progressive, roll: -0.05f); //Left
                                        break;
                                    case 4:
                                        if (areaObject != 0)
                                        {
                                            if (areaObject > areaObjectNew) _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f); //Forward
                                            else if (areaObject < areaObjectNew) _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f); //Back
                                        }
                                        else
                                        {
                                            if (areaObjectNew > 300) _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f); //Forward
                                            else if (areaObjectNew < 300) _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f); //Back
                                        }
                                        break;
                                    case 5:
                                        _droneClient.Progress(FlightMode.Progressive, roll: 0.05f); //Right
                                        break;
                                    case 6:
                                        _droneClient.Progress(FlightMode.Progressive, roll: -0.05f); //Left
                                        _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f); //Down
                                        break;
                                    case 7:
                                        _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f); //Down
                                        break;
                                    case 8:
                                        _droneClient.Progress(FlightMode.Progressive, roll: 0.05f); //Right
                                        _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f); //Down
                                        break;
                                }

                            }
                            else
                            {
                                switch (moveWay)
                                {
                                    case 0:
                                        _droneClient.Progress(FlightMode.Progressive, roll: 0.05f); //Right
                                        _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f); //Forward
                                        break;
                                    case 1:
                                        _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f); //Forward
                                        break;
                                    case 2:
                                        _droneClient.Progress(FlightMode.Progressive, roll: -0.05f); //Left
                                        _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f); //Forward
                                        break;
                                    case 3:
                                        _droneClient.Progress(FlightMode.Progressive, roll: 0.05f); //Right
                                        break;
                                    case 4:
                                        if (areaObject != 0)
                                        {
                                            if (areaObject > areaObjectNew) _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f); //Up
                                            else if (areaObject < areaObjectNew) _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f); //Down
                                        }
                                        else
                                        {
                                            if (areaObjectNew > 300) _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f); //Up
                                            else if (areaObjectNew < 300) _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f); //Down
                                        }
                                        break;
                                    case 5:
                                        _droneClient.Progress(FlightMode.Progressive, roll: -0.05f); //Left
                                        break;
                                    case 6:
                                        _droneClient.Progress(FlightMode.Progressive, roll: 0.05f); //Right
                                        _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f); //Back
                                        break;
                                    case 7:
                                        _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f); //Back
                                        break;
                                    case 8:
                                        _droneClient.Progress(FlightMode.Progressive, roll: -0.05f); //Left
                                        _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f); //Back
                                        break;
                                }
                            }
                        }
                        #endregion
                    }
                    break;
            }
            

            //ibOriginal.Image = new Image<Bgr, Byte>(_frameBitmap);
            //getBitmapFromtmr = (Bitmap)pbVideo.Image.Clone();
        }

        private void tmrStateUpdate_Tick(object sender, EventArgs e)
        {
            tvInfo.BeginUpdate();

            TreeNode node = tvInfo.Nodes.GetOrCreate("ClientActive");
            node.Text = string.Format("Client Active: {0}", _droneClient.IsActive);

            node = tvInfo.Nodes.GetOrCreate("Navigation Data");
            if (_navigationData != null)
            {
                DumpBranch(node.Nodes, _navigationData);

                //////////////////////////// Get Altitude ////////////////////////////
                lblAltitude.Text = Convert.ToString(_navigationData.Altitude);
                //////////////////////////////////////////////////////////////////////
            }
            node = tvInfo.Nodes.GetOrCreate("Configuration");
            if (_settings != null) DumpBranch(node.Nodes, _settings);

            TreeNode vativeNode = tvInfo.Nodes.GetOrCreate("Native");

            NavdataBag navdataBag;
            if (_navigationPacket.Data != null && NavdataBagParser.TryParse(ref _navigationPacket, out navdataBag))
            {
                var ctrl_state = (CTRL_STATES) (navdataBag.demo.ctrl_state >> 0x10);
                node = vativeNode.Nodes.GetOrCreate("ctrl_state");
                node.Text = string.Format("Ctrl State: {0}", ctrl_state);

                var flying_state = (FLYING_STATES) (navdataBag.demo.ctrl_state & 0xffff);
                node = vativeNode.Nodes.GetOrCreate("flying_state");
                node.Text = string.Format("Ctrl State: {0}", flying_state);

                DumpBranch(vativeNode.Nodes, navdataBag);
            }
            tvInfo.EndUpdate();

            if (_autopilot != null && !_autopilot.Active && btnAutopilot.ForeColor != Color.Black)
                btnAutopilot.ForeColor = Color.Black;
        }

        private void DumpBranch(TreeNodeCollection nodes, object o)
        {
            Type type = o.GetType();
         
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                TreeNode node = nodes.GetOrCreate(fieldInfo.Name);
                object value = fieldInfo.GetValue(o);

                DumpValue(fieldInfo.FieldType, node, value);
            }

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                TreeNode node = nodes.GetOrCreate(propertyInfo.Name);
                object value = propertyInfo.GetValue(o, null);

                DumpValue(propertyInfo.PropertyType, node, value);
            }
        }

        private void DumpValue(Type type, TreeNode node, object value)
        {
            if (value == null)
                node.Text = node.Name + ": null";
            else
            {
                if (type.Namespace.StartsWith("System") || type.IsEnum)
                    node.Text = node.Name + ": " + value;
                else
                    DumpBranch(node.Nodes, value);
            }
        }

        private void btnFlatTrim_Click(object sender, EventArgs e)
        {
            _droneClient.FlatTrim();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _droneClient.Takeoff();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _droneClient.Land();
        }

        private void btnEmergency_Click(object sender, EventArgs e)
        {
            _droneClient.Emergency();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _droneClient.ResetEmergency();
        }

        private void btnSwitchCam_Click(object sender, EventArgs e)
        {
            var configuration = new Settings();
            configuration.Video.Channel = VideoChannelType.Next;
            _droneClient.Send(configuration);

            /////////////////////////////////////////////////////////////
            if (switchCam == 0) switchCam = 1;
            else switchCam = 0;
        }

        private void btnHover_Click(object sender, EventArgs e)
        {
            _droneClient.Hover();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f);
        }

        private void btnTurnLeft_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, yaw: 0.25f);
        }

        private void btnTurnRight_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, yaw: -0.25f);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, roll: -0.05f);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, roll: 0.05f);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f);
        }

        private void btnReadConfig_Click(object sender, EventArgs e)
        {
            Task<Settings> configurationTask = _droneClient.GetConfigurationTask();
            configurationTask.ContinueWith(delegate(Task<Settings> task)
                {
                    if (task.Exception != null)
                    {
                        Trace.TraceWarning("Get configuration task is faulted with exception: {0}", task.Exception.InnerException.Message);
                        return;
                    }

                    _settings = task.Result;
                });
            configurationTask.Start();
        }

        private void btnSendConfig_Click(object sender, EventArgs e)
        {
            var sendConfigTask = new Task(() =>
                {
                    if (_settings == null) _settings = new Settings();
                    Settings settings = _settings;

                    if (string.IsNullOrEmpty(settings.Custom.SessionId) ||
                        settings.Custom.SessionId == "00000000")
                    {
                        // set new session, application and profile
                        _droneClient.AckControlAndWaitForConfirmation(); // wait for the control confirmation

                        settings.Custom.SessionId = Settings.NewId();
                        _droneClient.Send(settings);
                        
                        _droneClient.AckControlAndWaitForConfirmation();

                        settings.Custom.ProfileId = Settings.NewId();
                        _droneClient.Send(settings);
                        
                        _droneClient.AckControlAndWaitForConfirmation();

                        settings.Custom.ApplicationId = Settings.NewId();
                        _droneClient.Send(settings);
                        
                        _droneClient.AckControlAndWaitForConfirmation();
                    }

                    settings.General.NavdataDemo = false;
                    settings.General.NavdataOptions = NavdataOptions.All;

                    settings.Video.BitrateCtrlMode = VideoBitrateControlMode.Dynamic;
                    settings.Video.Bitrate = 1000;
                    settings.Video.MaxBitrate = 2000;

                    //settings.Leds.LedAnimation = new LedAnimation(LedAnimationType.BlinkGreenRed, 2.0f, 2);
                    //settings.Control.FlightAnimation = new FlightAnimation(FlightAnimationType.Wave);

                    // record video to usb
                    //settings.Video.OnUsb = true;
                    // usage of MP4_360P_H264_720P codec is a requirement for video recording to usb
                    //settings.Video.Codec = VideoCodecType.MP4_360P_H264_720P;
                    // start
                    //settings.Userbox.Command = new UserboxCommand(UserboxCommandType.Start);
                    // stop
                    //settings.Userbox.Command = new UserboxCommand(UserboxCommandType.Stop);


                    //send all changes in one pice
                    _droneClient.Send(settings);
                });
            sendConfigTask.Start();
        }

        private void StopRecording()
        {
            if (_packetRecorderWorker != null)
            {
                _packetRecorderWorker.Stop();
                _packetRecorderWorker.Join();
                _packetRecorderWorker = null;
            }
            if (_recorderStream != null)
            {
                _recorderStream.Dispose();
                _recorderStream = null;
            }
        }

        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            string path = string.Format("flight_{0:yyyy_MM_dd_HH_mm}" + ARDroneTrackFileExt, DateTime.Now);

            using (var dialog = new SaveFileDialog {DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter, FileName = path})
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    _recorderStream = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
                    _packetRecorderWorker = new PacketRecorder(_recorderStream);
                    _packetRecorderWorker.Start();
                }
            }
        }

        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void btnReplay_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog {DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter})
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    var playerForm = new PlayerForm {FileName = dialog.FileName};
                    playerForm.Closed += (o, args) => _playerForms.Remove(o as PlayerForm);
                    _playerForms.Add(playerForm);
                    playerForm.Show(this);
                }
            }
        }

        // Make sure '_autopilot' variable is initialized with an object
        private void CreateAutopilot()
        {
            if (_autopilot != null) return;

            _autopilot = new Autopilot(_droneClient);
            _autopilot.OnOutOfObjectives += Autopilot_OnOutOfObjectives;
            _autopilot.BindToClient();
            _autopilot.Start();
        }

        // Event that occurs when no objectives are waiting in the autopilot queue
        private void Autopilot_OnOutOfObjectives()
        {
            _autopilot.Active = false;
        }

        // Create a simple mission for autopilot
        private void CreateAutopilotMission()
        {
            _autopilot.ClearObjectives();

            // Do two 36 degrees turns left and right if the drone is already flying
            if (_droneClient.NavigationData.State.HasFlag(NavigationState.Flying))
            {
                const float turn = (float)(Math.PI / 5);
                float heading = _droneClient.NavigationData.Yaw;

                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading + turn, aCanBeObtained: true)));
                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading - turn, aCanBeObtained: true)));
                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading, aCanBeObtained: true)));
            }
            else // Just take off if the drone is on the ground
            {
                _autopilot.EnqueueObjective(new FlatTrim(1000));
                _autopilot.EnqueueObjective(new Takeoff(3500));
            }

            // One could use hover, but the method below, allows to gain/lose/maintain desired altitude
            _autopilot.EnqueueObjective(
                Objective.Create(3000,
                    new VelocityX(0.0f),
                    new VelocityY(0.0f),
                    new Altitude(1.0f)
                )
            );

            _autopilot.EnqueueObjective(new Land(5000));
        }

        // Activate/deactive autopilot
        private void btnAutopilot_Click(object sender, EventArgs e)
        {
            if (!_droneClient.IsActive) return;

            CreateAutopilot();
            if (_autopilot.Active) _autopilot.Active = false;
            else
            {
                CreateAutopilotMission();
                _autopilot.Active = true;
                btnAutopilot.ForeColor = Color.Red;
            }
        }

        private void btSnapshot_Click(object sender, EventArgs e)
        {
        //    string path_image = string.Format("flight_{0:yyyy_MM_dd_HH_mm}" + ARDroneTrackFileExt, DateTime.Now);

        //    using (var dialog = new SaveFileDialog { DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter, FileName = path_image })
        //    {
        //        if (dialog.ShowDialog(this) == DialogResult.OK)
        //        {
        //            StopRecording();

        //            _recorderStream = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
        //            _packetRecorderWorker = new PacketRecorder(_recorderStream);
        //            _packetRecorderWorker.Start();
        //        }
        //    }
        }

        
        public navdata_altitude_t altitude;
        public NavigationData Altitude;
        private void btGetAltitude_Click(object sender, EventArgs e)
        {
            if (altitudeOnTrack == 0)
            {
                altitudeTotle = 0;
                altitudeOutTrack = 0;
                altitudeOnTrack = Convert.ToSingle(lblAltitude.Text);

            }
            else
            {
                altitudeOutTrack = Convert.ToSingle(lblAltitude.Text);
                altitudeTotle = altitudeOutTrack - altitudeOnTrack;
                tbTotleAltitube.Text = Convert.ToString(altitudeTotle);
                altitudeOnTrack = 0;
            }
            
        }

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        
        //private void button5_Click(object sender, EventArgs e)
        //{
        //    setMode = 1;
        //}
              
        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            ECrange = Convert.ToInt32(numericUpDown1.Value);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            ECcolor = colorDialog1.Color;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            
            textBox1.Text = Wobject.ToString();
            areaObjcetX = Wobject;
            textBox2.Text = Hobject.ToString();
            areaObjcetY = Hobject;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            areaObjcetX = 0;
            areaObjcetY = 0;
            textBox1.Text = null;
            textBox2.Text = null;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (setMode == 0)
            {
                setMode = 1;
                lblTrackC.Text = "On";
            }
            else 
            { 
                setMode = 0;
                lblTrackC.Text = "OFF";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (modeTrackObject == 0)
            {
                modeTrackObject = 1;
                lblTrackO.Text = "On";
            }
            else 
            {
                modeTrackObject = 0;
                lblTrackO.Text = "Off";
            }
        }

        
        //////////////////////////////////////////////////////////////////////////////////////////////////////


    }
}


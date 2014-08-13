

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ARDrone.Control.Commands;
using ARDrone.Control.Data;
using ARDrone.Basics.Serialization;

namespace ARDrone.Control
{
    [Serializable()]
    public class DroneConfig
    {
        public const SupportedFirmwareVersion DefaultSupportedFirmwareVersion = SupportedFirmwareVersion.Firmware_164_Or_Above;
        private const String serializationFileName = "droneConfig.xml";

        private SerializationUtils serializationUtils;

        private String standardOwnIpAddress;      // Only used when no drone network could be found
        private String droneIpAddress;
        private String droneNetworkIdentifierStart;

        private int videoPort;
        private int navigationPort;
        private int commandPort;
        private int controlInfoPort;

        private int timeoutValue;

        private bool useSpecificFirmwareVersion;
        private SupportedFirmwareVersion firmwareVersion;

        private DroneCameraMode defaultCameraMode;

        private bool droneConfigInitialized = false;

        public void setDroneVersion(Int16 version)
        {
            if (version == 1)
            {
                this.droneNetworkIdentifierStart = "ardrone_";
            }
            if (version == 2)
            {
                this.droneNetworkIdentifierStart = "ardrone2_";
            }
        }

        public DroneConfig()
        {
            serializationUtils = new SerializationUtils();

            droneConfigInitialized = false;

            standardOwnIpAddress = "192.168.1.2";
            droneIpAddress = "192.168.1.1";
            droneNetworkIdentifierStart = "ardrone2_";

            videoPort = 5555;
            navigationPort = 5554;
            commandPort = 5556;
            controlInfoPort = 5559;

            useSpecificFirmwareVersion = false;
            firmwareVersion = DroneConfig.DefaultSupportedFirmwareVersion;

            timeoutValue = 500;
            defaultCameraMode = DroneCameraMode.FrontCamera;
        }

        private void CopySettingsFrom(DroneConfig droneConfig)
        {
            this.StandardOwnIpAddress = droneConfig.StandardOwnIpAddress;
            this.DroneIpAddress = droneConfig.DroneIpAddress;
            this.DroneNetworkIdentifierStart = droneConfig.DroneNetworkIdentifierStart;

            this.VideoPort = droneConfig.VideoPort;
            this.NavigationPort = droneConfig.NavigationPort;
            this.CommandPort = droneConfig.CommandPort;
            this.ControlInfoPort = droneConfig.ControlInfoPort;

            this.UseSpecificFirmwareVersion = droneConfig.UseSpecificFirmwareVersion;
            this.FirmwareVersion = droneConfig.FirmwareVersion;
        }

        public void Initialize()
        {
            droneConfigInitialized = true;
        }

        public void Deinitialize()
        {
            droneConfigInitialized = false;
        }

        private void CheckForDroneConfigState()
        {
            if (droneConfigInitialized)
                throw new InvalidOperationException("Changing the drone configuration is not possible after it has been used");
        }

        /// <summary>
        /// Only used when no drone network could be found
        /// </summary>
        public String StandardOwnIpAddress
        {
            get { return standardOwnIpAddress; }
            set { CheckForDroneConfigState(); standardOwnIpAddress = value; }
        }

        public String DroneIpAddress
        {
            get { return droneIpAddress; }
            set { CheckForDroneConfigState(); droneIpAddress = value; }
        }

        public String DroneNetworkIdentifierStart
        {
            get { return droneNetworkIdentifierStart; }
            set { CheckForDroneConfigState(); droneNetworkIdentifierStart = value; }
        }

        public int VideoPort
        {
            get { return videoPort; }
            set { CheckForDroneConfigState(); videoPort = value; }
        }

        public int NavigationPort
        {
            get { return navigationPort; }
            set { CheckForDroneConfigState(); navigationPort = value; }
        }

        public int CommandPort
        {
            get { return commandPort; }
            set { CheckForDroneConfigState(); commandPort = value; }
        }

        public int ControlInfoPort
        {
            get { return controlInfoPort; }
            set { CheckForDroneConfigState(); controlInfoPort = value; }
        }

        public int TimeoutValue
        {
            get { return timeoutValue; }
            set { CheckForDroneConfigState(); timeoutValue = value; }
        }

        public bool UseSpecificFirmwareVersion
        {
            get { return useSpecificFirmwareVersion; }
            set { useSpecificFirmwareVersion = value; }
        }

        public SupportedFirmwareVersion FirmwareVersion
        {
            get { return firmwareVersion; }
            set { CheckForDroneConfigState(); firmwareVersion = value; }
        }

        public DroneCameraMode DefaultCameraMode
        {
            get { return defaultCameraMode; }
            set { CheckForDroneConfigState(); defaultCameraMode = value; }
        }

        public void Load()
        {
            CheckForDroneConfigState();

            DroneConfig droneConfig = new DroneConfig();
            try
            {
                droneConfig = (DroneConfig)serializationUtils.Deserialize(this.GetType(), serializationFileName);
            }
            catch (Exception)
            { }

            CopySettingsFrom(droneConfig);
        }

        public void Save()
        {
            serializationUtils.Serialize(this, serializationFileName);
        }
    }
}

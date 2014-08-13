using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR.Drone.WinApp
{
    class longcode
    {
        private int objectX;
        private int objectY;

        int lineX;
        int lineY;
        int obSize;
        int quadrant = 99999;
        string printf;
        double vx = 0.0, vy = 0.0, vz = 0.0, vr = 0.0;

        string commanFly = null;

        private void longCodeDo(EventArgs e)
        {
            if (objectY > 0)
            {
                if (objectX > 0) quadrant = 0;
                else if (objectX == 0) quadrant = 1;
                else quadrant = 2;
            }
            else if (objectY < 0)
            {
                if (objectX > 0) quadrant = 8;
                else if (objectX == 0) quadrant = 7;
                else quadrant = 6;
            }
            else // objectY == 0
            {
                if (objectX > 0) quadrant = 5;
                else if (objectX == 0) quadrant = 4;
                else quadrant = 3;
            }

            switch (quadrant)
            {
                case 0:
                    printf = (" ascend + left \n ");
                    vz = 1.0; // ascend
                    vr = 1.0; // l e f t
                    break;

                case 1:
                    printf = (" ascend \n ");
                    vz = 1.0; // ascend
                    break;

                case 2:
                    printf = (" ascend + right \n ");
                    vz = 1.0; // ascend
                    vr = -1.0; // r i g h t
                    break;

                case 3:
                    printf = (" left \n ");
                    vr = 1.0; // l e f t
                    break;

                case 4:
                    if (obSize < 1500)
                    { // so that it stay sacertain	distance away from the object
                        vx = 1.0;
                        printf = (" forward \n ");
                    }
                    break;

                case 5:
                    printf = ("right \n ");
                    vr = -1.0; // right
                    break;

                case 6:
                    printf = (" Descend + left \n ");
                    vz = -1.0; // ascend
                    vr = 1.0; // l e f t
                    break;

                case 7:
                    printf = (" Descend \n ");
                    vz = -1.0; // descend
                    break;

                case 8:
                    printf = (" Descend + right \n ");
                    vz = -1.0; // descend
                    vr = 1.0; // r i g h t
                    break;
            }




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


        }
    }
}

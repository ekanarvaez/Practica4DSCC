using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

//Estas referencias son necesarias para usar GLIDE
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace Practica4DSCC
{
    public partial class Program
    {
        //Objetos de interface gráfica GLIDE
        private GHI.Glide.Display.Window iniciarWindow;
        private Button btn_inicio;
        HttpRequest request;
        GT.Timer timerRest;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            this.timerRest = new GT.Timer(20000);
            this.timerRest.Tick += timerRest_Tick;
            request = HttpHelper.CreateHttpGetRequest("http://api.thingspeak.com/channels/120875/field/1/last");
            request.ResponseReceived += request_ResponseReceived;
            //Carga la ventana principal
            iniciarWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.inicioWindow));
            GlideTouch.Initialize();

            initialize_ethernet();

            ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
            ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;
            //Inicializa el boton en la interface
            btn_inicio = (Button)iniciarWindow.GetChildByName("button_iniciar");
            btn_inicio.Enabled = false;
            btn_inicio.TapEvent += btn_inicio_TapEvent;

            //Selecciona iniciarWindow como la ventana de inicio
            Glide.MainWindow = iniciarWindow;
        }

        void timerRest_Tick(GT.Timer timer)
        {
            request.SendRequest();
        }

        void request_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            this.btn_inicio.Text = response.Text;
        }

        void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("entra ethernetJ11D_NetworkUp");
            //initialize_ethernet();
            this.btn_inicio.Enabled = true;
            //this.iniciarWindow.
            TextBlock text = (TextBlock)this.iniciarWindow.GetChildByName("text_net_status");
            text.Text = ethernetJ11D.NetworkSettings.IPAddress;
            Glide.MainWindow = iniciarWindow;
        }

        void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("no entra ethernetJ11D_NetworkDown");
            TextBlock text = (TextBlock)this.iniciarWindow.GetChildByName("text_net_status");
            text.Text = "No networking";
            this.btn_inicio.Enabled = true;
            Glide.MainWindow = iniciarWindow;
        }

        void initialize_ethernet()
        {
            ethernetJ11D.NetworkInterface.Open();
            ethernetJ11D.NetworkInterface.EnableDhcp();
            ethernetJ11D.UseThisNetworkInterface();
        }

        void btn_inicio_TapEvent(object sender)
        {
            Debug.Print("Iniciar");
            this.timerRest.Start();
        }
    }
}

﻿using Bing.Maps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Wander
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DataController datacontroller;
        ViewSettings settings;
        ResumeSession resume;
        MapShapeLayer polygonLayer;
        Help help;
        Geolocator geo = null;
        Location currentLocation;
        Pushpin location = new Pushpin();
        wander wander = new wander();
        MapPolyline walked = new MapPolyline();
        String calculatedDistanceToNextPoint;

        public MainPage()
        {
            this.InitializeComponent();
            GeofenceMonitor.Current.Geofences.Clear();
            GeofenceMonitor.Current.GeofenceStateChanged += Current_GeofenceStateChanged;
            polygonLayer = new MapShapeLayer();
            bingMap.ShapeLayers.Add(polygonLayer);
            datacontroller = DataController.getInstance();
            findSession();
            sightList.ItemsSource = datacontroller.giveStringsOfLoadedSights();

            geo = new Geolocator();
            geo.DesiredAccuracy = PositionAccuracy.High;
            geo.PositionChanged += geolocator_PositionChanged;
            bingMap.Children.Add(location);

            //resume = new ResumeSession();
            //GridRoot.Children.Add(resume);
            datacontroller.setSightsWithGeofences(bingMap);
            drawRoute();
            setPinListeners();
            polygonLayer.Shapes.Add(walked);
            //bingMap.Children.Add(walked);
            NetworkInformation.NetworkStatusChanged += internetConnectionEventHandler;
        }

        public async void findSession()
        {
            if (datacontroller.getFirstTime() == true)
            {
                resume = new ResumeSession(this);
                GridRoot.Children.Add(resume);
            }
            else return;
		}
        private async void updateDistanceTextbox(String geofence)
        {
            await datacontroller.calculateToNextPoint(bingMap, geofence);
            calculatedDistanceToNextPoint = (int)datacontroller.distance + " Meter";
            distanceTextbox.DataContext = calculatedDistanceToNextPoint;
        }

        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (settings == null)
                settings = new ViewSettings(this);
            if(!GridRoot.Children.Contains(settings))
            {
                GridRoot.Children.Add(settings);
            }
        }

        public void setHelp(Boolean refresh)
        {
            help = new Help(this, refresh);
            GridRoot.Children.Add(help);
            removeChild(settings);
        }

        public void removeChild(UIElement e)
        {
            GridRoot.Children.Remove(e);
        }

        public async void showErrorMessage(string title, string content)
        {
            var dialog = new MessageDialog(content, title);
            await dialog.ShowAsync();
        }

        private void setPinListeners()
        {
            foreach(Pushpin pin in bingMap.Children)
            {

                pin.Tapped += Current_location_pushpin_tapped;

            }
        }

        void Current_location_pushpin_tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedItem = ((Pushpin)sender).Text;
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(Message), selectedItem);
            }
        }

        private async void sightList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.AddedItems[0];
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(Message), selectedItem);
            }
        }

        private async void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(
            () =>
            {
                wander.mapcontroller.addPointToListIfDistanceToPreviousIsGreatEnough(new Location(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude));
                if (currentLocation == null)
                {
                    currentLocation = new Location(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                    bingMap.SetView(currentLocation, 16);
                }
                else
                    currentLocation = new Location(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                MapLayer.SetPosition(location, currentLocation);
                drawWalkedRoute(wander.mapcontroller.locations());
            }));
        }

        private async void internetConnectionEventHandler(object sender)
        {

            if (!IsInternet())
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(
            () =>
            {
                try
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    string content = loader.GetString("internetErrorContent");
                    string title = loader.GetString("internetErrorHeader");
                    new MessageDialog(content, title).ShowAsync();
                }
                catch { }
            }));

            }
        }
        
        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        async void Current_GeofenceStateChanged(GeofenceMonitor sender, object args)
        {
            var reports = sender.ReadReports();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {

                foreach (GeofenceStateChangeReport report in reports)
                {
                    GeofenceState state = report.NewState;

                    Geofence geofence = report.Geofence;

                    if (state == GeofenceState.Entered)
                    {
                        if (geofence.Id.EndsWith("_20m"))
                        {
                            try
                            {
                                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                                string near = loader.GetString("near");
                                updateDistanceTextbox(((String)geofence.Id).Split('_').First());
                                playSound.Play();
                                var message = new MessageDialog(((String)geofence.Id).Split('_').First(),near);
                                await message.ShowAsync();
                            }
                            catch
                            {

                            }
                            
                        }
                        else if (geofence.Id.EndsWith("_5m"))
                        {
                            foreach(Pushpin pin in bingMap.Children)
                            {
                                if(pin.Text == ((String)geofence.Id).Split('_').First())
                                {
                                    pin.Background = new SolidColorBrush(Colors.Black);
                                    datacontroller.setSightSeenTrue(((String)geofence.Id).Split('_').First());
                                }
                            }
                        }
                        
                    }
                }
            });
        }

        private void drawWalkedRoute(List<Location> locs)
        {
            //walked = new MapPolyline();
            walked.Color = Windows.UI.Colors.Blue;
            walked.Width = 3;

            walked.Locations = new LocationCollection();

            foreach (Bing.Maps.Location location in locs)
            {
                walked.Locations.Add(location);
            }


            walked.Visible = true;


            
        }

        private void drawRoute()
        {
            MapPolyline polyline = new MapPolyline(); 
            polyline.Color = Windows.UI.Colors.Red;
            polyline.Width = 3;

            polyline.Locations = new LocationCollection();
            List<Location> locations = datacontroller.getWaypointLocations();

            foreach (Location location in locations)
            {
                polyline.Locations.Add(location);
            }


            polyline.Visible = true;


            polygonLayer.Shapes.Add(polyline);
        }


        internal void setGrid(RouteSelection routes)
        {
            GridRoot.Children.Add(routes);
        }
    }
}

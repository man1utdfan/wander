﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Storage;

namespace Wander
{
    class DataController
    {
        private static DataController instance;
        private List<WanderLib.Waypoint> loadedSights { get; set; }
        public WanderLib.Session session { get; set; }
        public int selectedLanguage { get; set; }


        public static DataController getInstance()
        {
           
            if (instance == null)
                instance = new DataController();
            return instance;
        }

        public List<string> giveStringsOfLoadedSights()
        {
            if (loadedSights == null)
                loadedSights = giveAllWaypointsOnRoute();
            List<string> strings = new List<string>();
            foreach(WanderLib.Waypoint sight in loadedSights)
            {
                    if (sight.GetType() == (typeof(WanderLib.Sight)))
                        strings.Add(((WanderLib.Sight)sight).name);
            }
            return strings;
        }

        public List<WanderLib.Language> giveAllLanguages()
        {
            List<WanderLib.Language> languages = new List<WanderLib.Language>();
            languages.Add(new WanderLib.Language("Nederlands"));
            languages.Add(new WanderLib.Language("English"));
            languages.Add(new WanderLib.Language("日本人"));
            return languages;
        }

        public List<WanderLib.Waypoint> giveAllWaypointsOnRoute(string Route="")
        {
            System.Xml.Linq.XDocument GPSpoints = System.Xml.Linq.XDocument.Load("Assets/GPS2.xml");
            var data = from item in GPSpoints.Descendants("waypoint")
                       select new
                       {
                           Lat = item.Element("Latitude").Value,
                           Long = item.Element("Longitude").Value,
                           name = item.Element("Site").Value
                       };
            string informationfile = "";
            switch(session.language.name)
            {
                case "English":
                    informationfile = "Languages/en/information.xml";
                    break;
                case "Nederlands":
                    informationfile = "Languages/nl/information.xml";
                    break;
                default:
                    informationfile = "Languages/nl/information.xml";
                    break;

            }
            System.Xml.Linq.XDocument informations = System.Xml.Linq.XDocument.Load(informationfile);
            var data2 = from item in informations.Descendants("item")
                       select new
                       {
                           Name = item.Element("Name").Value,
                           Text = item.Element("Text").Value,

                       };
            //string lorips = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur at rhoncus sem. Etiam placerat aliquet dapibus. Nulla nisi massa, iaculis sed nibh non, consequat semper orci. Donec id magna turpis. Aliquam consequat lorem a sapien consequat vulputate. Duis id nulla ultricies, mollis justo id, posuere sem. Donec gravida felis at felis malesuada pretium. Sed lorem purus, eleifend commodo tellus non, vulputate imperdiet mauris. Pellentesque vel sapien a nunc lacinia molestie non ac risus. Duis sit amet mi et massa dignissim tristique. Ut id posuere quam, a ornare felis. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nunc feugiat augue diam, at pulvinar dui mattis et. Donec scelerisque blandit tellus, luctus hendrerit arcu volutpat quis. Suspendisse nec interdum est. Curabitur risus metus, tincidunt eu dolor ut, consequat imperdiet nulla.\n\nVestibulum vehicula at odio a dapibus. Donec sodales purus ligula, a varius nunc bibendum quis. Vestibulum at velit consequat, volutpat metus nec, suscipit lacus. Suspendisse venenatis magna ac cursus auctor. Praesent a nisl nisi. Nunc non purus pulvinar, mollis eros vel, posuere nisi. In eu sapien vitae metus lobortis hendrerit id id augue. Nullam mauris ligula, volutpat vel interdum ac, viverra at nunc. Donec imperdiet vestibulum urna, sed dignissim enim tincidunt a. Nam tempus pellentesque est ut tempus.\n\nSuspendisse potenti. Pellentesque felis dolor, faucibus at volutpat id, ullamcorper in purus. Quisque urna nulla, dignissim quis molestie quis, egestas sed purus. Integer in urna ut nisi eleifend interdum non sed metus. Aliquam nulla nunc, mollis at consequat at, ultricies quis nulla. Donec auctor lacus nisi, nec congue libero cursus at. Suspendisse potenti. In eleifend neque ut sollicitudin porta. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.\n\nSed a magna in lacus bibendum laoreet. Interdum et malesuada fames ac ante ipsum primis in faucibus. Maecenas non mollis libero. Vestibulum porta ultricies libero, quis malesuada nisi scelerisque vitae. Morbi ut laoreet tortor, sed vehicula nisl. Phasellus aliquet arcu id orci ultricies, tristique lacinia arcu ultrices. Vestibulum iaculis tortor interdum, hendrerit est sit amet, varius felis. Suspendisse a orci vel ante placerat volutpat quis vitae nisl. Vestibulum et placerat magna.\n\nSed pulvinar at arcu in consequat. Ut eleifend fermentum arcu a imperdiet. Curabitur tempus pulvinar elit. Phasellus euismod, justo in tincidunt scelerisque, lorem turpis ornare elit, sit amet congue odio mi sit amet nisi. Fusce at justo enim. Donec aliquam vel velit at faucibus. Sed imperdiet iaculis orci sit amet sollicitudin. Ut rutrum magna orci.";
            List<WanderLib.Waypoint> list = new List<WanderLib.Waypoint>();
            foreach(var dat in data)
            {
                string text = "";
                foreach(var dat2 in data2)
                {
                    if (dat.name.ToLower() == dat2.Name.ToLower())
                        text = dat2.Text;
                }
                if (dat.name!="")
                    list.Add(new WanderLib.Sight(null, dat.name, text, new WanderLib.Location(dat.Lat, dat.Long)) as WanderLib.Waypoint);
                else
                    list.Add(new WanderLib.Waypoint(text, new WanderLib.Location(dat.Lat, dat.Long)));
            }
            //TODO vervang stub
            return list;
        }
    }
}

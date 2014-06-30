﻿using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace InterceptNuGet
{
    class InterceptFormatting
    {
        public static XElement MakeFeed(string feedBaseAddress, string method, IEnumerable<JToken> packages, string id)
        {
            return MakeFeed(feedBaseAddress, method, packages, Enumerable.Repeat(id, packages.Count()).ToArray());
        }

        public static XElement MakeFeed(string feedBaseAddress, string method, IEnumerable<JToken> packages, string[] id)
        {
            XNamespace atom = XNamespace.Get(@"http://www.w3.org/2005/Atom");
            XElement feed = new XElement(atom + "feed");
            feed.Add(new XElement(atom + "id", string.Format("{0}/api/v2/{1}", feedBaseAddress, method)));
            feed.Add(new XElement(atom + "title", method));
            int i = 0;
            foreach (JToken package in packages)
            {
                feed.Add(MakeEntry(feedBaseAddress, id[i++], package));
            }
            return feed;
        }

        static XElement MakeEntry(string feedBaseAddress, string id, JToken package)
        {
            XNamespace atom = XNamespace.Get(@"http://www.w3.org/2005/Atom");
            XNamespace d = XNamespace.Get(@"http://schemas.microsoft.com/ado/2007/08/dataservices");
            XNamespace m = XNamespace.Get(@"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");

            XElement entry = new XElement(atom + "entry");

            entry.Add(new XElement(atom + "id", string.Format("{0}/api/v2/Packages(Id='{1}',Version='{2}')", feedBaseAddress, id, package["version"])));
            entry.Add(new XElement(atom + "title", id));
            entry.Add(new XElement(atom + "author", new XElement(atom + "name", "SHIM")));

            // the content URL should come from the json
            entry.Add(new XElement(atom + "content",
                new XAttribute("type", "application/zip"),
                new XAttribute("src", string.Format("http://www.nuget.org/api/v2/package/{0}/{1}", id, package["version"]))));

            XElement properties = new XElement(m + "properties");
            entry.Add(properties);

            properties.Add(new XElement(d + "Version", package["version"].ToString()));

            // the following fields should come from the json
            properties.Add(new XElement(d + "Description", "SHIM"));
            properties.Add(new XElement(d + "IsLatestVersion", new XAttribute(m + "type", "Edm.Boolean"), "true"));
            properties.Add(new XElement(d + "IsAbsoluteLatestVersion", new XAttribute(m + "type", "Edm.Boolean"), "true"));
            properties.Add(new XElement(d + "IsPrerelease", new XAttribute(m + "type", "Edm.Boolean"), "false"));

            JToken dependencies;
            if (((JObject)package).TryGetValue("dependencies", out dependencies))
            {
                StringBuilder sb = new StringBuilder();

                foreach (JToken group in dependencies["group"])
                {
                    string targetFramework = string.Empty;
                    JToken tf;
                    if (((JObject)group).TryGetValue("targetFramework", out tf))
                    {
                        targetFramework = tf.ToString();
                    }

                    foreach (JToken dependency in group["dependency"])
                    {
                        sb.AppendFormat("{0}:{1}:{2}|", dependency["id"].ToString().ToLowerInvariant(), dependency["range"], targetFramework);
                    }

                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                }

                properties.Add(new XElement(d + "Dependencies", sb.ToString()));
            }

            // license information should come from the json
            bool license = false;

            properties.Add(new XElement(d + "RequireLicenseAcceptance", new XAttribute(m + "type", "Edm.Boolean"), license.ToString().ToLowerInvariant()));

            if (license)
            {
                properties.Add(new XElement(d + "LicenseUrl", "http://shim/test"));
            }

            // the following properties required for GetUpdates (from the UI)

            // the following properties should come from the json
            bool iconUrl = false;
            if (iconUrl)
            {
                properties.Add(new XElement(d + "IconUrl", "http://tempuri.org/"));
            }

            properties.Add(new XElement(d + "DownloadCount", new XAttribute(m + "type", "Edm.Int32"), 123456));
            properties.Add(new XElement(d + "GalleryDetailsUrl", "http://tempuri.org/"));
            properties.Add(new XElement(d + "Published", new XAttribute(m + "type", "Edm.DateTime"), "2014-02-25T02:04:38.407"));
            properties.Add(new XElement(d + "Tags", "SHIM.Tags"));

            // title is optional, if it is not there the UI uses the Id
            //properties.Add(new XElement(d + "Title", "SHIM.Title"));
            properties.Add(new XElement(d + "ReleaseNotes", "SHIM.ReleaseNotes"));

            return entry;
        }

        //  The search service currently returns a slightly different JSON format (this will be fixed)

        public static XElement MakeFeedFromSearch(string feedBaseAddress, string method, IEnumerable<JToken> packages, string id)
        {
            XNamespace atom = XNamespace.Get(@"http://www.w3.org/2005/Atom");
            XElement feed = new XElement(atom + "feed");
            feed.Add(new XElement(atom + "id", string.Format("{0}/api/v2/{1}", feedBaseAddress, method)));
            feed.Add(new XElement(atom + "title", method));
            int i = 0;
            foreach (JToken package in packages)
            {
                feed.Add(MakeEntrySearch(feedBaseAddress, "", package));
            }
            return feed;
        }

        static XElement MakeEntrySearch(string feedBaseAddress, string id, JToken package)
        {
            XNamespace atom = XNamespace.Get(@"http://www.w3.org/2005/Atom");
            XNamespace d = XNamespace.Get(@"http://schemas.microsoft.com/ado/2007/08/dataservices");
            XNamespace m = XNamespace.Get(@"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");

            XElement entry = new XElement(atom + "entry");

            string registrationId = package["PackageRegistration"]["Id"].ToString();
            string version = package["Version"].ToString();

            entry.Add(new XElement(atom + "id", string.Format("{0}/api/v2/Packages(Id='{1}',Version='{2}')", feedBaseAddress, registrationId, version)));
            entry.Add(new XElement(atom + "title", registrationId));
            entry.Add(new XElement(atom + "author", new XElement(atom + "name", package["Authors"].ToString())));

            // the content URL should come from the json
            entry.Add(new XElement(atom + "content",
                new XAttribute("type", "application/zip"),
                new XAttribute("src", string.Format("http://www.nuget.org/api/v2/package/{0}/{1}", registrationId, version))));

            XElement properties = new XElement(m + "properties");
            entry.Add(properties);

            properties.Add(new XElement(d + "Version", package["Version"].ToString()));

            NuGetVersion nugetVersion = NuGetVersion.Parse(version);

            // the following fields should come from the json
            properties.Add(new XElement(d + "Description", package["Description"].ToString()));
            properties.Add(new XElement(d + "IsLatestVersion", new XAttribute(m + "type", "Edm.Boolean"), package["IsLatestStable"].ToString().ToLowerInvariant()));
            properties.Add(new XElement(d + "IsAbsoluteLatestVersion", new XAttribute(m + "type", "Edm.Boolean"), package["IsLatest"].ToString().ToLowerInvariant()));
            properties.Add(new XElement(d + "IsPrerelease", new XAttribute(m + "type", "Edm.Boolean"), nugetVersion.IsPrerelease.ToString().ToLowerInvariant()));

            JToken flattenedDependencies;
            if (((JObject)package).TryGetValue("FlattenedDependencies", out flattenedDependencies))
            {
                properties.Add(new XElement(d + "Dependencies", flattenedDependencies.ToString()));
            }

            // license information should come from the json
            bool license = false;
            bool.TryParse(package["RequiresLicenseAcceptance"].ToString().ToLowerInvariant(), out license);

            properties.Add(new XElement(d + "RequireLicenseAcceptance", new XAttribute(m + "type", "Edm.Boolean"), license.ToString().ToLowerInvariant()));

            if (license)
            {
                properties.Add(new XElement(d + "LicenseUrl", package["LicenseUrl"].ToString()));
            }

            JToken iconUrl;
            if (((JObject)package).TryGetValue("IconUrl", out iconUrl))
            {
                properties.Add(new XElement(d + "IconUrl", iconUrl.ToString()));
            }

            string downloadCount = package["PackageRegistration"]["DownloadCount"].ToString();

            DateTime published = DateTime.Parse(package["Published"].ToString());

            properties.Add(new XElement(d + "DownloadCount", new XAttribute(m + "type", "Edm.Int32"), downloadCount));
            properties.Add(new XElement(d + "GalleryDetailsUrl", "http://tempuri.org/"));
            properties.Add(new XElement(d + "Published", new XAttribute(m + "type", "Edm.DateTime"), published.ToString("O")));
            properties.Add(new XElement(d + "Tags", package["Tags"].ToString()));

            // title is optional, if it is not there the UI uses the Id

            JToken title;
            if (((JObject)package).TryGetValue("Title", out title))
            {
                properties.Add(new XElement(d + "Title", title.ToString()));
            }

            string releaseNotes = package["ReleaseNotes"].ToString();

            if (releaseNotes == "null")
            {
                properties.Add(new XElement(d + "ReleaseNotes", string.Empty));
            }
            else
            {
                properties.Add(new XElement(d + "ReleaseNotes", releaseNotes));
            }

            return entry;
        }
    }
}

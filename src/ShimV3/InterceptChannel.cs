using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace InterceptNuGet
{
    class InterceptChannel
    {
        string _baseAddress;
        string _searchBaseAddress;
        string _passThroughAddress;

        public InterceptChannel(string baseAddress, string searchBaseAddress, string passThroughAddress)
        {
            _baseAddress = baseAddress.TrimEnd('/');
            _searchBaseAddress = searchBaseAddress.TrimEnd('/');
            _passThroughAddress = passThroughAddress.TrimEnd('/');
        }

        public static InterceptChannel Create(string source)
        {
            if (source.StartsWith("https://preview-api.nuget.org/ver3", StringComparison.OrdinalIgnoreCase))
            {
                string baseAddress = "http://nuget3.blob.core.windows.net/feed/resolver";
                string searchBaseAddress = "http://nuget-dev-0-search.cloudapp.net/search/query";
                string passThroughAddress = "http://nuget.org";

                return new InterceptChannel(baseAddress, searchBaseAddress, passThroughAddress);
            }

            return null;
        }

        //public static async Task<InterceptChannel> Create(string source)
        //{
        //    HttpClient client = new HttpClient();
        //    HttpResponseMessage response = await client.GetAsync(source);
        //    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //    {
        //        HttpResponseMessage rootResponse = await client.GetAsync(source + "/root.xml");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            string text = await rootResponse.Content.ReadAsStringAsync();

        //            XNamespace shim = XNamespace.Get("http://schema.nuget.org/shim");

        //            XElement interceptionSpecification = XElement.Parse(text);

        //            string baseAddress = interceptionSpecification.Elements(shim + "baseAddress").First().Value;
        //            string searchBaseAddress = interceptionSpecification.Elements(shim + "searchBaseAddress").First().Value;
        //            string passThroughAddress = interceptionSpecification.Elements(shim + "passThroughAddress").First().Value;

        //            return new InterceptChannel(baseAddress, searchBaseAddress, passThroughAddress);
        //        }
        //    }

        //    return null;
        //}

        public async Task Root(InterceptCallContext context, string feedName = null)
        {
            context.Log(string.Format("Root: {0}", feedName ?? string.Empty), ConsoleColor.Magenta);

            if (feedName == null)
            {
                Stream stream = GetResourceStream("xml.Root.xml");
                XElement xml = XElement.Load(stream);
                await context.WriteResponse(xml);
            }
            else
            {
                Stream stream = GetResourceStream("xml.FeedRoot.xml");
                string s = (new StreamReader(stream)).ReadToEnd();
                string t = string.Format(s, feedName);
                XElement xml = XElement.Load(new StringReader(t), LoadOptions.SetBaseUri);
                await context.WriteResponse(xml);
            }
        }

        public async Task Metadata(InterceptCallContext context, string feed = null)
        {
            context.Log(string.Format("Metadata: {0}", feed ?? string.Empty), ConsoleColor.Magenta);

            Stream stream = GetResourceStream(feed == null ? "xml.Metadata.xml" : "xml.FeedMetadata.xml");
            XElement xml = XElement.Load(stream);
            await context.WriteResponse(xml);
        }

        public async Task Count(InterceptCallContext context, string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, string feedName)
        {
            context.Log(string.Format("Count: {0}", searchTerm), ConsoleColor.Magenta);

            JObject obj = await FetchJson(context, MakeCountAddress(searchTerm, isLatestVersion, targetFramework, includePrerelease, feedName));

            string count = obj != null ? count = obj["totalHits"].ToString() : "0";

            await context.WriteResponse(count);
        }

        public async Task Search(InterceptCallContext context, string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, int skip, int take, string feedName)
        {
            context.Log(string.Format("Search: {0} ({1},{2})", searchTerm, skip, take), ConsoleColor.Magenta);

            JObject obj = await FetchJson(context, MakeSearchAddress(searchTerm, isLatestVersion, targetFramework, includePrerelease, skip, take, feedName));

            IEnumerable<JToken> data = (obj != null) ? data = obj["data"] : Enumerable.Empty<JToken>();

            XElement feed = InterceptFormatting.MakeFeedFromSearch(_passThroughAddress, "Packages", data, "");
            await context.WriteResponse(feed);
        }

        public async Task GetPackage(InterceptCallContext context, string id, string version, string feedName)
        {
            context.Log(string.Format("GetPackage: {0} {1}", id, version), ConsoleColor.Magenta);

            JObject resolverBlob = await FetchJson(context, MakeResolverAddress(id));
            JToken desiredPackage = null;

            if (resolverBlob != null)
            {
                NuGetVersion desiredVersion = NuGetVersion.Parse(version);

                foreach (JToken package in resolverBlob["package"])
                {
                    NuGetVersion currentVersion = NuGetVersion.Parse(package["version"].ToString());
                    if (currentVersion == desiredVersion)
                    {
                        desiredPackage = package;
                        break;
                    }
                }
            }

            if (desiredPackage == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "unable to find version {0} of package {1}", version, id));
            }

            XElement feed = InterceptFormatting.MakeFeed(_passThroughAddress, "Packages", new List<JToken> { desiredPackage }, id);
            await context.WriteResponse(feed);
        }

        public async Task GetLatestVersionPackage(InterceptCallContext context, string id, bool includePrerelease)
        {
            context.Log(string.Format("GetLatestVersionPackage: {0} {1}", id, includePrerelease ? "[include prerelease]" : ""), ConsoleColor.Magenta);

            JObject resolverBlob = await FetchJson(context, MakeResolverAddress(id));

            if (resolverBlob == null)
            {
                throw new InvalidOperationException(string.Format("package {0} not found", id));
            }

            JToken latest = ExtractLatestVersion(resolverBlob, includePrerelease);

            if (latest == null)
            {
                throw new InvalidOperationException(string.Format("package {0} not found", id));
            }

            XElement feed = InterceptFormatting.MakeFeed(_passThroughAddress, "Packages", new List<JToken> { latest }, id);
            await context.WriteResponse(feed);
        }

        public async Task GetAllPackageVersions(InterceptCallContext context, string id)
        {
            context.Log(string.Format("GetAllPackageVersions: {0}", id), ConsoleColor.Magenta);

            JObject resolverBlob = await FetchJson(context, MakeResolverAddress(id));

            if (resolverBlob == null)
            {
                throw new InvalidOperationException(string.Format("package {0} not found", id));
            }

            XElement feed = InterceptFormatting.MakeFeed(_passThroughAddress, "Packages", resolverBlob["package"], id);
            await context.WriteResponse(feed);
        }

        public async Task GetListOfPackageVersions(InterceptCallContext context, string id)
        {
            context.Log(string.Format("GetListOfPackageVersions: {0}", id), ConsoleColor.Magenta);

            JObject resolverBlob = await FetchJson(context, MakeResolverAddress(id));

            if (resolverBlob == null)
            {
                throw new InvalidOperationException(string.Format("package {0} not found", id));
            }

            List<NuGetVersion> versions = new List<NuGetVersion>();
            foreach (JToken package in resolverBlob["package"])
            {
                versions.Add(NuGetVersion.Parse(package["version"].ToString()));
            }

            versions.Sort();

            JArray array = new JArray();
            foreach (NuGetVersion version in versions)
            {
                array.Add(version.ToString());
            }

            await context.WriteResponse(array);
        }

        public async Task ListAllVersion(InterceptCallContext context)
        {
            await PassThrough(context);
        }

        public async Task ListLatestVersion(InterceptCallContext context)
        {
            await PassThrough(context);
        }

        public async Task GetUpdates(InterceptCallContext context, string[] packageIds, string[] versions, string[] versionConstraints, string[] targetFrameworks, bool includePrerelease, bool includeAllVersions)
        {
            context.Log(string.Format("GetUpdates: {0}", string.Join("|", packageIds)), ConsoleColor.Magenta);

            List<JToken> packages = new List<JToken>();

            for (int i = 0; i < packageIds.Length; i++)
            {
                VersionRange range = null;

                if (versionConstraints.Length < i && !String.IsNullOrEmpty(versionConstraints[i]))
                {
                    VersionRange.TryParse(versionConstraints[i], out range);
                }

                JObject resolverBlob = await FetchJson(context, MakeResolverAddress(packageIds[i]));

                // TODO: handle this error
                if (resolverBlob != null)
                {
                    JToken latest = ExtractLatestVersion(resolverBlob, includePrerelease, range);
                    if (latest == null)
                    {
                        throw new Exception(string.Format("package {0} not found", packageIds[i]));
                    }
                    packages.Add(latest);
                }
            }

            XElement feed = InterceptFormatting.MakeFeed(_passThroughAddress, "GetUpdates", packages, packageIds);
            await context.WriteResponse(feed);
        }

        public async Task PassThrough(InterceptCallContext context, bool log = false)
        {
            context.Log(_passThroughAddress + context.RequestUri.PathAndQuery, ConsoleColor.Cyan);

            await InterceptChannel.PassThrough(context, _passThroughAddress, log);
        }

        public static async Task PassThrough(InterceptCallContext context, string baseAddress, bool log = false)
        {
            string pathAndQuery = context.RequestUri.PathAndQuery;
            Uri forwardAddress = new Uri(baseAddress + pathAndQuery);

            Tuple<string, byte[]> content = await Forward(forwardAddress, log);

            context.ResponseContentType = content.Item1;
            await context.WriteResponseAsync(content.Item2);
        }

        static JToken ExtractLatestVersion(JObject resolverBlob, bool includePrerelease, VersionRange range = null)
        {
            //  firstly just pick the first one (or the first in range)

            JToken candidateLatest = null;

            if (range == null)
            {
                candidateLatest = resolverBlob["package"].FirstOrDefault();
            }
            else
            {
                foreach (JToken package in resolverBlob["package"])
                {
                    NuGetVersion currentVersion = NuGetVersion.Parse(package["version"].ToString());
                    if (range.Satisfies(currentVersion))
                    {
                        candidateLatest = package;
                        break;
                    }
                }
            }

            if (candidateLatest == null)
            {
                return null;
            }

            //  secondly iterate through package to see if we have a later package

            NuGetVersion candidateLatestVersion = NuGetVersion.Parse(candidateLatest["version"].ToString());

            foreach (JToken package in resolverBlob["package"])
            {
                NuGetVersion currentVersion = NuGetVersion.Parse(package["version"].ToString());

                if (range != null && !range.Satisfies(currentVersion))
                {
                    continue;
                }

                if (includePrerelease)
                {
                    if (currentVersion > candidateLatestVersion)
                    {
                        candidateLatest = package;
                        candidateLatestVersion = currentVersion;
                    }
                }
                else
                {
                    if (!currentVersion.IsPrerelease && currentVersion > candidateLatestVersion)
                    {
                        candidateLatest = package;
                        candidateLatestVersion = currentVersion;
                    }
                }
            }

            if (candidateLatestVersion.IsPrerelease && !includePrerelease)
            {
                return null;
            }

            return candidateLatest;
        }

        Uri MakeResolverAddress(string id)
        {
            id = id.ToLowerInvariant();
            Uri resolverBlobAddress = new Uri(string.Format("{0}/{1}.json", _baseAddress, id));
            return resolverBlobAddress;
        }

        Uri MakeCountAddress(string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, string feedName)
        {
            string feedArg = feedName == null ? string.Empty : string.Format("&feed={0}", feedName);

            Uri searchAddress = new Uri(string.Format("{0}?q={1}&targetFramework={2}&includePrerelease={3}&countOnly=true{4}",
                _searchBaseAddress, searchTerm, targetFramework, includePrerelease, feedArg));

            return searchAddress;
        }

        Uri MakeSearchAddress(string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, int skip, int take, string feedName)
        {
            string feedArg = feedName == null ? string.Empty : string.Format("&feed={0}", feedName);

            Uri searchAddress = new Uri(string.Format("{0}?q={1}&targetFramework={2}&includePrerelease={3}&skip={4}&take={5}{6}",
                _searchBaseAddress, searchTerm, targetFramework, includePrerelease, skip, take, feedArg));
            return searchAddress;
        }

        async Task<JObject> FetchJson(InterceptCallContext context, Uri address)
        {
            context.Log(address.ToString(), ConsoleColor.Yellow);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(address);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(json);
                return obj;
            }
            else
            {
                // expected in some cases
                return null;
            }
        }

        static async Task<Tuple<string, byte[]>> Forward(Uri forwardAddress, bool log)
        {
            NuGet.ShimDebugLogger.Log("Forward: " + forwardAddress.AbsoluteUri);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(forwardAddress);
            string contentType = response.Content.Headers.ContentType.ToString();
            byte[] data = await response.Content.ReadAsByteArrayAsync();

            if (log)
            {
                Dump(contentType, data);
            }

            return new Tuple<string, byte[]>(contentType, data);
        }

        public static Stream GetResourceStream(string resName)
        {
            NuGet.ShimDebugLogger.Log("Resource: " + resName);

            var assem = Assembly.GetExecutingAssembly();

            var resource = assem.GetManifestResourceNames().Where(s => s.IndexOf(resName) > -1).FirstOrDefault();

            var stream = assem.GetManifestResourceStream(resource);
            return stream;
        }

        //  Just for debugging

        static void Dump(string contentType, byte[] data)
        {
            using (TextReader reader = new StreamReader(new MemoryStream(data)))
            {
                string s = reader.ReadToEnd();
                if (contentType.IndexOf("xml") > -1)
                {
                    XElement xml = XElement.Parse(s);
                    using (XmlWriter writer = XmlWriter.Create(Console.Out, new XmlWriterSettings { Indent = true }))
                    {
                        xml.WriteTo(writer);

                        //int count = xml.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom")).Count();
                        //Console.WriteLine(count);
                    }
                }
                else
                {
                    Console.WriteLine(s);
                }
            }
        }
    }
}

using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NuGet.ShimV3
{
    internal class InterceptChannel
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
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] Root: {0}", feedName ?? string.Empty), ConsoleColor.Magenta);

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
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] Metadata: {0}", feed ?? string.Empty), ConsoleColor.Magenta);

            Stream stream = GetResourceStream(feed == null ? "xml.Metadata.xml" : "xml.FeedMetadata.xml");
            XElement xml = XElement.Load(stream);
            await context.WriteResponse(xml);
        }

        public async Task Count(InterceptCallContext context, string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, string feedName)
        {
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] Count: {0}", searchTerm), ConsoleColor.Magenta);

            JObject obj = await FetchJson(context, MakeCountAddress(searchTerm, isLatestVersion, targetFramework, includePrerelease, feedName));

            string count = obj != null ? count = obj["totalHits"].ToString() : "0";

            await context.WriteResponse(count);
        }

        public async Task Search(InterceptCallContext context, string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, int skip, int take, string feedName)
        {
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] Search: {0} ({1},{2})", searchTerm, skip, take), ConsoleColor.Magenta);

            JObject obj = await FetchJson(context, MakeSearchAddress(searchTerm, isLatestVersion, targetFramework, includePrerelease, skip, take, feedName));

            IEnumerable<JToken> data = (obj != null) ? data = obj["data"] : Enumerable.Empty<JToken>();

            XElement feed = InterceptFormatting.MakeFeedFromSearch(_passThroughAddress, "Packages", data, "");
            await context.WriteResponse(feed);
        }

        public async Task GetPackage(InterceptCallContext context, string id, string version, string feedName)
        {
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] GetPackage: {0} {1}", id, version), ConsoleColor.Magenta);

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
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] GetLatestVersionPackage: {0} {1}", id, includePrerelease ? "[include prerelease]" : ""), ConsoleColor.Magenta);

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
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] GetAllPackageVersions: {0}", id), ConsoleColor.Magenta);

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
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] GetListOfPackageVersions: {0}", id), ConsoleColor.Magenta);

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
            var data = await GetListAvailable(context, null, false);

            XElement feed = InterceptFormatting.MakeFeed(_passThroughAddress, "ListAllVersions", data, string.Empty);
            await context.WriteResponse(feed);
        }

        public async Task ListLatestVersion(InterceptCallContext context)
        {
            var data = await GetListAvailable(context, null, true);

            XElement feed = InterceptFormatting.MakeFeed(_passThroughAddress, "ListAllVersions", data, string.Empty);
            await context.WriteResponse(feed);
        }

        public async Task<IEnumerable<JToken>> GetListAvailable(InterceptCallContext context, string startsWith, bool latestVersionOnly)
        {
            Queue<JToken> results = new Queue<JToken>();
            Queue<string> segments = null;
            bool useStartsWith = !String.IsNullOrEmpty(startsWith);

            if (useStartsWith)
            {
                segments = await GetListAvailableSegments(context);
            }
            else
            {
                segments = await GetListAvailableSegmentsNeeded(context, startsWith);
            }

            Queue<Task<JObject>> tasks = new Queue<Task<JObject>>();
            string lastId = string.Empty;

            while (segments.Count > 0)
            {
                // 8 at a time
                for(int i=0; i < 8 && segments.Count > 0; i++)
                {
                    string url = segments.Dequeue();
                    tasks.Enqueue(FetchJson(context, new Uri(url)));
                }

                while(tasks.Count > 0)
                {
                    var seg = await tasks.Dequeue();

                    if (seg == null)
                    {
                        throw new InvalidOperationException();
                    }

                    foreach(var entry in seg["entry"])
                    {
                        string id = entry["id"].ToString();

                        // assume the highest version is first
                        if (latestVersionOnly)
                        {
                            if (StringComparer.OrdinalIgnoreCase.Equals(id, lastId))
                            {
                                continue;
                            }
                        }

                        if (!useStartsWith || id.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Enqueue(entry);
                        }

                        lastId = id;
                    }
                }
            }

            return results;
        }

        public async Task<Queue<string>> GetListAvailableSegments(InterceptCallContext context)
        {
            var indexUrl = MakeListAvailableIndexAddress();
            var index = await FetchJson(context, indexUrl);

            Queue<string> needed = new Queue<string>();

            foreach(var seg in index["segment"])
            {
                needed.Enqueue(seg["url"].ToString());
            }

            return needed;
        }

        public async Task<Queue<string>> GetListAvailableSegmentsNeeded(InterceptCallContext context, string startsWith)
        {
            var indexUrl = MakeListAvailableIndexAddress();
            var index = await FetchJson(context, indexUrl);

            var segs = index["segment"].ToArray();

            Queue<string> needed = new Queue<string>();

            for (int i=0; i < segs.Length; i++)
            {
                var seg = segs[i];

                string lowest = seg["lowest"].ToString();

                // advance until we go too far
                if (needed.Count < 1 && StringComparer.OrdinalIgnoreCase.Compare(startsWith, lowest) >= 0)
                {
                    if (i > 0)
                    {
                        // get the previous one
                        needed.Enqueue(segs[i - 1]["url"].ToString());
                    }

                    // add the current one
                    needed.Enqueue(segs[i]["url"].ToString());
                }
                // continue adding everything that starst with the prefix
                else if (lowest.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                {
                    needed.Enqueue(segs[i]["url"].ToString());
                }
            }

            return needed;
        }

        private void ThrowNotImplemented()
        {
            throw new NotImplementedException();
        }

        public async Task GetUpdates(InterceptCallContext context, string[] packageIds, string[] versions, string[] versionConstraints, string[] targetFrameworks, bool includePrerelease, bool includeAllVersions)
        {
            context.Log(string.Format(CultureInfo.InvariantCulture, "[V3 CALL] GetUpdates: {0}", string.Join("|", packageIds)), ConsoleColor.Magenta);

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

        //public async Task PassThrough(InterceptCallContext context, bool log = false)
        //{
        //    context.Log(_passThroughAddress + context.RequestUri.PathAndQuery, ConsoleColor.Cyan);

        //    await InterceptChannel.PassThrough(context, _passThroughAddress, log);
        //}

        //public static async Task PassThrough(InterceptCallContext context, string baseAddress, bool log = false)
        //{
        //    string pathAndQuery = context.RequestUri.PathAndQuery.Replace("/ver3", "/api/v2");
        //    Uri forwardAddress = new Uri(baseAddress + pathAndQuery);

        //    Tuple<string, byte[]> content = await Forward(forwardAddress, log);

        //    context.ResponseContentType = content.Item1;
        //    await context.WriteResponseAsync(content.Item2);
        //}

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

        Uri MakeListAvailableIndexAddress()
        {
            // TODO: make dynamic
            return new Uri("https://nuget3.blob.core.windows.net/listavailable/segment_index.json");
        }

        Uri MakeResolverAddress(string id)
        {
            id = id.ToLowerInvariant();
            Uri resolverBlobAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}/{1}.json", _baseAddress, id));
            return resolverBlobAddress;
        }

        Uri MakeCountAddress(string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, string feedName)
        {
            string feedArg = feedName == null ? string.Empty : string.Format(CultureInfo.InvariantCulture, "&feed={0}", feedName);

            Uri searchAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}?q={1}&targetFramework={2}&includePrerelease={3}&countOnly=true{4}",
                _searchBaseAddress, searchTerm, targetFramework, includePrerelease, feedArg));

            return searchAddress;
        }

        Uri MakeSearchAddress(string searchTerm, bool isLatestVersion, string targetFramework, bool includePrerelease, int skip, int take, string feedName)
        {
            string feedArg = feedName == null ? string.Empty : string.Format(CultureInfo.InvariantCulture, "&feed={0}", feedName);

            Uri searchAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "{0}?q={1}&targetFramework={2}&includePrerelease={3}&skip={4}&take={5}{6}",
                _searchBaseAddress, searchTerm, targetFramework, includePrerelease, skip, take, feedArg));
            return searchAddress;
        }

        async Task<JObject> FetchJson(InterceptCallContext context, Uri address)
        {
            context.Log(String.Format(CultureInfo.InvariantCulture, "[V3 REQ] {0}" ,address.ToString()), ConsoleColor.Cyan);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            HttpResponseMessage response = await client.GetAsync(address);

            timer.Stop();

            context.Log(String.Format(CultureInfo.InvariantCulture, "[V3 RES] (status:{0}) (time:{1}ms) {2}", response.StatusCode, timer.ElapsedMilliseconds, address.ToString()),
                response.StatusCode == System.Net.HttpStatusCode.OK ? ConsoleColor.Cyan : ConsoleColor.Red);

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

        //static async Task<Tuple<string, byte[]>> Forward(Uri forwardAddress, bool log)
        //{
        //    ShimDebugLogger.Log("Forward: " + forwardAddress.AbsoluteUri);


        //    System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
        //    HttpResponseMessage response = await client.GetAsync(forwardAddress);
        //    string contentType = response.Content.Headers.ContentType.ToString();
        //    byte[] data = await response.Content.ReadAsByteArrayAsync();

        //    if (log)
        //    {
        //        Dump(contentType, data);
        //    }

        //    return new Tuple<string, byte[]>(contentType, data);
        //}

        public static Stream GetResourceStream(string resName)
        {
            var assem = Assembly.GetExecutingAssembly();

            // TODO: replace this hack
            var resource = assem.GetManifestResourceNames().Where(s => s.IndexOf(resName, StringComparison.OrdinalIgnoreCase) > -1).FirstOrDefault();

            var stream = assem.GetManifestResourceStream(resource);
            return stream;
        }

        //  Just for debugging

        static void Dump(string contentType, byte[] data)
        {
            using (TextReader reader = new StreamReader(new MemoryStream(data)))
            {
                string s = reader.ReadToEnd();
                if (contentType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) > -1)
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

using System;
using System.Security;

namespace NuGet
{
    internal class GalleryV2EnvironmentVariableWrapper : IEnvironmentVariableReader
    {
        public string GetEnvironmentVariable(string variable)
        {
            try
            {
                return Environment.GetEnvironmentVariable(variable);
            }
            catch (SecurityException)
            {
                return null;
            }
        }
    }
}
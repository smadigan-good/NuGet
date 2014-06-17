﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet
{
    public interface IPackageDependency
    {
        string Id { get; }

        IVersionSpec VersionSpec { get; }

        string ToString();
    }
}

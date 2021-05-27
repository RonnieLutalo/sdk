// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.NET.Sdk.WorkloadManifestReader;
using Microsoft.DotNet.Workloads.Workload.Install.InstallRecord;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.NuGetPackageDownloader;

namespace Microsoft.DotNet.Workloads.Workload.Install
{
    internal class WorkloadInstallerFactory
    {
        public static IInstaller GetWorkloadInstaller(
            IReporter reporter,
            SdkFeatureBand sdkFeatureBand,
            IWorkloadResolver workloadResolver, 
            VerbosityOptions verbosity,
            INuGetPackageDownloader nugetPackageDownloader = null,
            string dotnetDir = null, 
            PackageSourceLocation packageSourceLocation = null)
        {
            var installType = GetWorkloadInstallType(sdkFeatureBand, string.IsNullOrWhiteSpace(dotnetDir) ?  Environment.ProcessPath : dotnetDir);

            if (installType == InstallType.Msi)
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new InvalidOperationException(LocalizableStrings.OSDoesNotSupportMsi);
                }

                return new NetSdkMsiInstaller();
            }

            return new NetSdkManagedInstaller(reporter, sdkFeatureBand, workloadResolver, nugetPackageDownloader, verbosity: verbosity, dotnetDir: dotnetDir, packageSourceLocation: packageSourceLocation);
        }

        /// <summary>
        /// Determines the <see cref="InstallType"/> associated with a specific SDK version.
        /// </summary>
        /// <param name="sdkFeatureBand">The SDK version to check.</param>
        /// <returns>The <see cref="InstallType"/> associated with the SDK.</returns>
        public static InstallType GetWorkloadInstallType(SdkFeatureBand sdkFeatureBand, string dotnetDir)
        {
            string installerTypePath = Path.Combine(dotnetDir, "metadata",
                "workloads", $"{sdkFeatureBand}", "installertype");

            if (File.Exists(Path.Combine(installerTypePath, "msi")))
            {
                return InstallType.Msi;
            }

            return InstallType.FileBased;
        }
    }
}

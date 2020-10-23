﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Json.Pointer;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class LocationsMustBeRelativeUrisOrFilePaths : SarifValidationSkimmerBase
    {
        /// <summary>
        /// GH1005
        /// </summary>
        public override string Id => RuleId.LocationsMustBeRelativeUrisOrFilePaths;

        // GitHub Advanced Security code scanning only displays results whose locations are specified
        // by file paths, either as relative URIs or as absolute URIs that use the 'file' scheme.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.GH1005_LocationsMustBeRelativeUrisOrFilePaths_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.GH1005_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override bool EnabledByDefault => false;

        private Uri workingDirectoryUri;
        protected override void Analyze(Run run, string runPointer)
        {
            this.workingDirectoryUri = GetWorkingDirectoryUri(run);
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!(result.Locations?.Any() == true)) 
            {
                // Rule GH1001.ProvideRequiredLocationProperties will catch this, so don't
                // report it here.
                return;
            }

            string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);
            for (int i = 0; i < result.Locations.Count; i++)
            {
                ValidateLocation(result.Locations[i], locationsPointer.AtIndex(i));
            }

            string relatedLocationsPointer = resultPointer.AtProperty(SarifPropertyName.RelatedLocations);
            if (result.RelatedLocations?.Any() == true)
            {
                for (int i = 0; i < result.RelatedLocations.Count; i++)
                {
                    ValidateLocation(result.RelatedLocations[i], relatedLocationsPointer.AtIndex(i));
                }
            }
        }

        private void ValidateLocation(Location location, string locationPointer)
        {
            Uri uri = location?.PhysicalLocation?.ArtifactLocation?.Uri;
            if (uri == null)
            {
                // Rule GH1001.ProvideRequiredLocationProperties will catch this, so don't
                // report it here.
                return;
            }

            if (uri.IsAbsoluteUri && uri.Scheme != "file" && this.workingDirectoryUri == null)
            {
                string uriPointer = locationPointer
                    .AtProperty(SarifPropertyName.PhysicalLocation)
                    .AtProperty(SarifPropertyName.ArtifactLocation)
                    .AtProperty(SarifPropertyName.Uri);

                // {0}: '{1}' is not a file path. GitHub Advanced Security code scanning only
                // displays results whose locations are specified by file paths, either as
                // relative URIs or as absolute URIs that use the 'file' scheme.
                LogResult(
                    uriPointer,
                    nameof(RuleResources.GH1005_LocationsMustBeRelativeUrisOrFilePaths_Error_Default_Text),
                    uri.OriginalString);
            }
        }

        private Uri GetWorkingDirectoryUri(Run run)
        {
            // Find the first workingDirectory
            if (run.Invocations?.Any() == true)
            {
                foreach (Invocation invocation in run.Invocations)
                {
                    if (invocation.WorkingDirectory?.Uri != null)
                    {
                        return invocation.WorkingDirectory?.Uri;
                    }
                }
            }
            return null;
        }
    }
}

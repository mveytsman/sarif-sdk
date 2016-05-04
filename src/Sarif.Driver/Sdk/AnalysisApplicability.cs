﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public enum AnalysisApplicability
    {
        Unknown,
        NotApplicableToSpecifiedTarget,
        ApplicableToSpecifiedTarget,
        NotApplicableDueToMissingConfiguration
    }
}
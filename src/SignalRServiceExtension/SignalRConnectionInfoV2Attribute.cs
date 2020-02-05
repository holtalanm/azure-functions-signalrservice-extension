﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.SignalRService
{
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    [Binding]
    public class SignalRConnectionInfoV2Attribute : Attribute
    {
        [AppSetting(Default = Constants.AzureSignalRConnectionStringName)]
        public string ConnectionStringSetting { get; set; }

        [AutoResolve]
        public string HubName { get; set; }

        [AutoResolve]
        public string UserId { get; set; }

        [AutoResolve]
        public string IdToken { get; set; }

        public string[] ClaimTypeList { get; set; }
    }
}
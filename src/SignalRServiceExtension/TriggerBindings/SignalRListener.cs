﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace Microsoft.Azure.WebJobs.Extensions.SignalRService
{
    internal class SignalRListener: IListener
    {
        public ITriggeredFunctionExecutor Executor { get; }

        private readonly SignalRTriggerRouter _router;
        private readonly string _hubName;
        private readonly string _methodName;

        public SignalRListener(ITriggeredFunctionExecutor executor, SignalRTriggerRouter router, string hubName, string methodName)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _hubName = hubName ?? throw new ArgumentNullException(nameof(hubName));
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            Executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public void Dispose()
        {
            // TODO unsubscribe
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _router.AddRoute((_hubName, _methodName), Executor);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Cancel()
        {
            // TODO cancel any outstanding tasks initiated by this listener
        }
    }
}

﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace Microsoft.Azure.WebJobs.Extensions.SignalRService
{
    internal class SignalRHubMethodExecutor
    {
        private const string OnConnectedTarget = "connect";
        private const string OnDisconnectedTarget = "disconnect";

        private readonly Dictionary<string, ITriggeredFunctionExecutor> _executors = new Dictionary<string, ITriggeredFunctionExecutor>(StringComparer.OrdinalIgnoreCase);

        public string Hub { get; set; }

        public SignalRHubMethodExecutor(string hub)
        {
            Hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        public void AddTarget(string target, ITriggeredFunctionExecutor executor)
        {
            _executors.Add(target, executor);
        }

        public async Task<HttpResponseMessage> ExecuteInvocation(IHubProtocol protocol, InvocationContext context, string target, string invocationId)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            HttpResponseMessage response;
            CompletionMessage completionMessage;
            if (_executors.TryGetValue(target, out var executor))
            {
                await ExecuteAsync(executor, context, tcs);
                var result = await tcs.Task;
                completionMessage = CompletionMessage.WithResult(invocationId, result);
                response = new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                completionMessage = CompletionMessage.WithError(invocationId, $"Target: {target} not found.");
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (!string.IsNullOrEmpty(invocationId))
            {
                response.Content = new ByteArrayContent(protocol.GetMessageBytes(completionMessage).ToArray());
            }
            return response;
        }

        public async Task<HttpResponseMessage> ExecuteOpenConnection(OpenConnectionContext context)
        {
            if (_executors.TryGetValue(OnConnectedTarget, out var executor))
            {
                await ExecuteAsync(executor, context, null);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public async Task<HttpResponseMessage> ExecuteCloseConnection(CloseConnectionContext context)
        {
            if (_executors.TryGetValue(OnDisconnectedTarget, out var executor))
            {
                await ExecuteAsync(executor, context, null);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private async Task ExecuteAsync(ITriggeredFunctionExecutor executor, Context context, TaskCompletionSource<object> tcs)
        {
            var signalRTriggerEvent = new SignalRTriggerEvent
            {
                Context = context,
                TaskCompletionSource = tcs,
            };

            await executor.TryExecuteAsync(
                new TriggeredFunctionData
                {
                    TriggerValue = signalRTriggerEvent
                }, CancellationToken.None);
        }
    }
}

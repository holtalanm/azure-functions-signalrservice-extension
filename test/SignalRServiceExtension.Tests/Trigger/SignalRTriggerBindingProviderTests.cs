﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignalRServiceExtension.Tests.Utils;
using Xunit;

namespace SignalRServiceExtension.Tests
{
    public class SignalRTriggerBindingProviderTests
    {
        [Fact]
        public void ResolveAttributeParameterTest()
        {
            var bindingProvider = CreateBindingProvider();
            var attribute = new SignalRTriggerAttribute();
            var parameter = typeof(TestServerlessHub).GetMethod(nameof(TestServerlessHub.TestFunction), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            var resolvedAttribute = bindingProvider.GetParameterResolvedAttribute(attribute, parameter);
            Assert.Equal(nameof(TestServerlessHub), resolvedAttribute.HubName);
            Assert.Equal(Category.Messages, resolvedAttribute.Category);
            Assert.Equal(nameof(TestServerlessHub.TestFunction), resolvedAttribute.Event);
            Assert.Equal(new string[] {"arg0", "arg1"}, resolvedAttribute.ParameterNames);

            // With SignalRIgoreAttribute
            parameter = typeof(TestServerlessHub).GetMethod(nameof(TestServerlessHub.TestFunctionWithIgnore), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            resolvedAttribute = bindingProvider.GetParameterResolvedAttribute(attribute, parameter);
            Assert.Equal(new string[] { "arg0", "arg1" }, resolvedAttribute.ParameterNames);

            // With ILogger and CancellationToken
            parameter = typeof(TestServerlessHub).GetMethod(nameof(TestServerlessHub.TestFunctionWithSpecificType), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            resolvedAttribute = bindingProvider.GetParameterResolvedAttribute(attribute, parameter);
            Assert.Equal(new string[] { "arg0", "arg1" }, resolvedAttribute.ParameterNames);
        }

        [Fact]
        public void ResolveConnectionAttributeParameterTest()
        {
            var bindingProvider = CreateBindingProvider();
            var attribute = new SignalRTriggerAttribute();
            var parameter = typeof(TestConnectedServerlessHub).GetMethod(nameof(TestConnectedServerlessHub.OnConnected), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            var resolvedAttribute = bindingProvider.GetParameterResolvedAttribute(attribute, parameter);
            Assert.Equal(nameof(TestConnectedServerlessHub), resolvedAttribute.HubName);
            Assert.Equal(Category.Connections, resolvedAttribute.Category);
            Assert.Equal(Event.Connected, resolvedAttribute.Event);
            Assert.Equal(new string[] { "arg0", "arg1" }, resolvedAttribute.ParameterNames);

            parameter = typeof(TestConnectedServerlessHub).GetMethod(nameof(TestConnectedServerlessHub.OnDisconnected), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            resolvedAttribute = bindingProvider.GetParameterResolvedAttribute(attribute, parameter);
            Assert.Equal(nameof(TestConnectedServerlessHub), resolvedAttribute.HubName);
            Assert.Equal(Category.Connections, resolvedAttribute.Category);
            Assert.Equal(Event.Disconnected, resolvedAttribute.Event);
            Assert.Equal(new string[] { "arg0", "arg1" }, resolvedAttribute.ParameterNames);
        }

        [Fact]
        public void ResolveNonServerlessHubAttributeParameterTest()
        {
            var bindingProvider = CreateBindingProvider();
            var attribute = new SignalRTriggerAttribute();
            var parameter = typeof(TestNonServerlessHub).GetMethod(nameof(TestNonServerlessHub.TestFunction), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            var resolvedAttribute = bindingProvider.GetParameterResolvedAttribute(attribute, parameter);
            Assert.Null(resolvedAttribute.HubName);
            Assert.Null(resolvedAttribute.Category);
            Assert.Null(resolvedAttribute.Event);
            Assert.Equal(new string[] { "arg0", "arg1" }, resolvedAttribute.ParameterNames);
        }

        [Fact]
        public void ResolveAttributeParameterConflictTest()
        {
            var bindingProvider = CreateBindingProvider();
            var attribute = new SignalRTriggerAttribute(string.Empty, string.Empty, String.Empty, new string[] {"arg0"});
            var parameter = typeof(TestServerlessHub).GetMethod(nameof(TestServerlessHub.TestFunction), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            Assert.ThrowsAny<Exception>(() => bindingProvider.GetParameterResolvedAttribute(attribute, parameter));
        }

        [Fact]
        public void WebhookFailedTest()
        {
            var bindingProvider = CreateBindingProvider(new Exception());
            var parameter = typeof(TestConnectedServerlessHub).GetMethod(nameof(TestConnectedServerlessHub.OnConnected), BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[0];
            var context = new TriggerBindingProviderContext(parameter, default);
            Assert.ThrowsAsync<Exception>(() => bindingProvider.TryCreateAsync(context));
        }

        private SignalRTriggerBindingProvider CreateBindingProvider(Exception exception = null)
        {
            var dispatcher = new TestTriggerDispatcher();
            return new SignalRTriggerBindingProvider(dispatcher, new DefaultNameResolver(new ConfigurationSection(new ConfigurationRoot(new List<IConfigurationProvider>()), String.Empty)), new SignalROptions(), exception);
        }

        public class TestServerlessHub : ServerlessHub
        {
            internal void TestFunction([SignalRTrigger]InvocationContext context, string arg0, int arg1)
            {
            }

            internal void TestFunctionWithIgnore([SignalRTrigger]InvocationContext context, string arg0, int arg1, [SignalRIgnore]int arg2)
            {
            }

            internal void TestFunctionWithSpecificType([SignalRTrigger]InvocationContext context, string arg0, int arg1, ILogger logger, CancellationToken token)
            {
            }
        }

        public class TestNonServerlessHub
        {
            internal void TestFunction([SignalRTrigger]InvocationContext context, 
                [SignalRParameter]string arg0, 
                [SignalRParameter]int arg1)
            {
            }
        }

        public class TestConnectedServerlessHub : ServerlessHub
        {
            internal void OnConnected([SignalRTrigger]InvocationContext context, string arg0, int arg1)
            {
            }

            internal void OnDisconnected([SignalRTrigger]InvocationContext context, string arg0, int arg1)
            {
            }
        }
    }
}

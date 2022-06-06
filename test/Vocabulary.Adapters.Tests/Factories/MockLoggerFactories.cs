using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Vocabulary.BlazorServer.Tests.Factories
{
    public static class MockLoggerFactories
    {
        public static Mock<ILogger<T>> GetMockILogger<T>(Action<string> writeLine)
        {
            Mock<ILogger<T>> mockLogger = new Mock<ILogger<T>>();

            mockLogger
                .Setup(l =>
                   l.Log(It.IsAny<LogLevel>(),
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((v, t) => true),
                       //It.IsAny<object>(),
                       It.IsAny<Exception>(),
                       It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                   //It.IsAny<Func<object, Exception, string>>()
                   )
                )
                .Callback<IInvocation>(invocation => {
                    var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                    _ = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter
                        .GetType()
                        .GetMethod("Invoke");

                    writeLine(
                        logLevel.ToString().ToLowerInvariant() + ": " + typeof(T).FullName + Environment.NewLine
                        + (string)(invokeMethod?.Invoke(formatter, new[] { state, exception }) ?? ""));
                });

            return mockLogger;
        }

        public static Mock<ILogger> GetMockILogger(Action<string> writeLine)
        {
            Mock<ILogger> mockLogger = new Mock<ILogger>();

            mockLogger
                .Setup(l =>
                   l.Log(It.IsAny<LogLevel>(),
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((v, t) => true),
                       It.IsAny<Exception>(),
                       It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Callback<IInvocation>(invocation => {
                    var logLevel = (LogLevel)invocation.Arguments[0];	// The first two will always be whatever is specified in the setup above
                    _ = (EventId)invocation.Arguments[1];				// so I'm not sure you would ever want to actually use them
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter
                        .GetType()
                        .GetMethod("Invoke");

                    writeLine(
                        logLevel.ToString().ToLowerInvariant() + ": " + Environment.NewLine
                        + (string)(invokeMethod?.Invoke(formatter, new[] { state, exception }) ?? ""));
                });

            return mockLogger;
        }

    }
}

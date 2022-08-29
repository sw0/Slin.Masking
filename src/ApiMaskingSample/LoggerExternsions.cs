using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExternsions
    {
        /// <summary>
        /// work with excludeProperties in NLog configuration
        /// </summary>
        const string KeyMessage = "message";

        static readonly Func<IEnumerable<KeyValuePair<string, object>>, Exception, string> _formatter =
            (o, e) => o.FirstOrDefault(k => KeyMessage == k.Key).Value?.ToString();

        public static void LogInformation<TState>(this ILogger logger, TState logData, EventId eventId = default(EventId)) where TState : IEnumerable<KeyValuePair<string, object>>
        {
            logger.Log(LogLevel.Information, eventId, logData, null, _formatter);
        }

        public static void LogError<TState>(this ILogger logger, Exception exception, TState logData, EventId eventId = default(EventId)) where TState : IEnumerable<KeyValuePair<string, object>>
        {
            logger.Log(LogLevel.Error, eventId, logData, exception, _formatter);
        }

        public static void LogWarning<TState>(this ILogger logger, TState logData, Exception exception = null, EventId eventId = default(EventId)) where TState : IEnumerable<KeyValuePair<string, object>>
        {
            logger.Log(LogLevel.Warning, eventId, logData, exception, _formatter);
        }

        public static void LogDebug<TState>(this ILogger logger, TState logData, Exception exception = null, EventId eventId = default(EventId)) where TState : IEnumerable<KeyValuePair<string, object>>
        {
            logger.Log(LogLevel.Debug, eventId, logData, exception, _formatter);
        }
    }
}

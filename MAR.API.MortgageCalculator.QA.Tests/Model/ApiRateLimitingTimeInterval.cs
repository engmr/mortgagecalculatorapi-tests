using EnumsNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;

namespace MAR.API.MortgageCalculator.QA.Tests.Model
{
    public class ApiRateLimitingTimeInterval
    {
        /// <summary>
        /// Maximum number of requests allowed during the <see cref="RequestResetTimeInterval"/> interval
        /// </summary>
        public int RequestsAllowed { get; set;  }
        /// <summary>
        /// Unit of time (e.g. seconds) 
        /// </summary>
        public ApiRateLimitingUnitsOfTime RequestResetUnitOfTime { get; set;  }

        /// <summary>
        /// Time interval from first request to trigger API limiting to when the reset occurs
        /// </summary>
        public TimeSpan RequestResetTimeInterval { get; set; }

        public ApiRateLimitingTimeInterval()
        {
        }

        public ApiRateLimitingTimeInterval(int numberOfRequests, int timeIntervalValue, string timeIntervalType)
        {
            RequestsAllowed = numberOfRequests;
            RequestResetUnitOfTime = GetUnitOfTimeFromIntervalType(timeIntervalType);
            if (RequestResetUnitOfTime == ApiRateLimitingUnitsOfTime.None)
            {
                throw new ArgumentOutOfRangeException(nameof(timeIntervalType));
            }
            RequestResetTimeInterval = GetTimeSpanFromIntervalValue(timeIntervalValue);
        }

        /// <summary>
        /// Gets the suffix for a returned response body from a <see cref="HttpStatusCode.TooManyRequests"/> HTTP response
        /// </summary>
        /// <returns></returns>
        /// <example>2 per 10s.</example>
        public string GetApiRateLimitingResponseBodySuffix()
        {
            if (RequestsAllowed <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(RequestsAllowed));
            }  
            if (RequestResetTimeInterval == TimeSpan.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(RequestResetTimeInterval));
            }

            return $"{RequestsAllowed} per {GetTimeValueFromTimeSpan()}{RequestResetUnitOfTime.AsString(EnumFormat.Description)}.";
        }


        /// <summary>
        /// Get the time value from the TimeSpan based on the interval unit of time
        /// </summary>
        /// <returns></returns>
        /// <example>120</example>
        public int GetTimeValueFromTimeSpan()
        {
            if (RequestResetTimeInterval == TimeSpan.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(RequestResetTimeInterval));
            }

            return RequestResetUnitOfTime switch
            {
                ApiRateLimitingUnitsOfTime.None => throw new ArgumentOutOfRangeException(nameof(RequestResetUnitOfTime)),
                ApiRateLimitingUnitsOfTime.Seconds => RequestResetTimeInterval.Seconds,
                ApiRateLimitingUnitsOfTime.Minutes => RequestResetTimeInterval.Minutes,
                ApiRateLimitingUnitsOfTime.Hours => RequestResetTimeInterval.Hours,
                ApiRateLimitingUnitsOfTime.Days => RequestResetTimeInterval.Days,
                _ => throw new NotImplementedException($"Type '{RequestResetUnitOfTime.AsString(EnumFormat.Name)}' not supported."),
            };
        }

        private ApiRateLimitingUnitsOfTime GetUnitOfTimeFromIntervalType(string intervalType)
        {
            if (string.IsNullOrWhiteSpace(intervalType))
            {
                throw new ArgumentNullException(nameof(intervalType));
            }

            var fullNamesUnitsOfTime = Enum.GetNames(typeof(ApiRateLimitingUnitsOfTime)).ToList();
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var titleCasedIntervalType = textInfo.ToTitleCase(intervalType);
            if (fullNamesUnitsOfTime.Contains(intervalType, StringComparer.OrdinalIgnoreCase))
            {
                return Enum.TryParse<ApiRateLimitingUnitsOfTime>(titleCasedIntervalType, out var result) 
                    ? result 
                    : throw new ArgumentOutOfRangeException(nameof(intervalType));
            }

            var fullSingularNameUnitsOfTime = new List<string>();
            fullNamesUnitsOfTime.ForEach(u => fullSingularNameUnitsOfTime.Add(u[0..^1]));

            if (fullSingularNameUnitsOfTime.Contains(intervalType, StringComparer.OrdinalIgnoreCase))
            {
                return Enum.TryParse<ApiRateLimitingUnitsOfTime>($"{titleCasedIntervalType}s", out var result)
                    ? result
                    : throw new ArgumentOutOfRangeException(nameof(intervalType));
            }

            var unitOfTimeDescriptions = GetRateLimitingUnitsOfTimeEnumDescriptions();
            if (unitOfTimeDescriptions.Contains(intervalType, StringComparer.OrdinalIgnoreCase))
            {
                var lowerCasedIntervalType = textInfo.ToLower(intervalType);
                return GetRateLimitingEnumFromDescription(lowerCasedIntervalType);
            }
            throw new ArgumentOutOfRangeException(nameof(intervalType), "Did not match an enum value by name, value nor description");
        }

        private TimeSpan GetTimeSpanFromIntervalValue(int timeIntervalValue)
        {
            return RequestResetUnitOfTime switch
            {
                ApiRateLimitingUnitsOfTime.None => throw new ArgumentOutOfRangeException(nameof(RequestResetUnitOfTime)),
                ApiRateLimitingUnitsOfTime.Seconds => TimeSpan.FromSeconds(timeIntervalValue),
                ApiRateLimitingUnitsOfTime.Minutes => TimeSpan.FromMinutes(timeIntervalValue),
                ApiRateLimitingUnitsOfTime.Hours => TimeSpan.FromHours(timeIntervalValue),
                ApiRateLimitingUnitsOfTime.Days => TimeSpan.FromDays(timeIntervalValue),
                _ => throw new NotImplementedException($"Type '{RequestResetUnitOfTime.AsString(EnumFormat.Name)}' not supported."),
            };
        }

        private List<string> GetRateLimitingUnitsOfTimeEnumDescriptions()
        {
            var attributes = typeof(ApiRateLimitingUnitsOfTime).GetMembers()
                .SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>())
                .ToList();

            return attributes.Select(x => x.Description).ToList();
        }

        private ApiRateLimitingUnitsOfTime GetRateLimitingEnumFromDescription(string description)
        {
            foreach (var field in typeof(ApiRateLimitingUnitsOfTime).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (ApiRateLimitingUnitsOfTime)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (ApiRateLimitingUnitsOfTime)field.GetValue(null);
                }
            }

            return default;
        }
    }

    /// <summary>
    /// See https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/IpRateLimitMiddleware#defining-rate-limit-rules for reference.
    /// </summary>
    public enum ApiRateLimitingUnitsOfTime
    {
        None = 0,
        [Description("s")]
        Seconds = 1,
        [Description("m")]
        Minutes = 2,
        [Description("h")]
        Hours = 3,
        [Description("d")]
        Days = 4
    }
}

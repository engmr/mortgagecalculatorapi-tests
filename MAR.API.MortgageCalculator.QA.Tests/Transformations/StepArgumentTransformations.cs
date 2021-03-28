using MAR.API.MortgageCalculator.QA.Tests.Model;
using System;
using TechTalk.SpecFlow;

namespace MAR.API.MortgageCalculator.QA.Tests.Transformations
{
    [Binding]
    public class StepArgumentTransformations
    {
        [StepArgumentTransformation(@"(\d*) per (\d*) (.*)")]
        public ApiRateLimitingTimeInterval TransformToRateLimitingTimeInterval(int numberOfRequests, int timeIntervalValue, string timeIntervalType)
        {
            if (numberOfRequests <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfRequests));
            }
            if (timeIntervalValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeIntervalValue));
            }
            if (string.IsNullOrWhiteSpace(timeIntervalType))
            {
                throw new ArgumentNullException(nameof(timeIntervalType));
            }

            return new ApiRateLimitingTimeInterval(numberOfRequests, timeIntervalValue, timeIntervalType);
        }
    }
}

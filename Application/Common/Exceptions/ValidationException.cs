using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Microservice.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Failures { get; }
        public ValidationException()
        : base("One or more validation failures have occurred.")
        {
            Failures = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
        {
            var failuresGroups = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage);
            foreach (var item in failuresGroups)
            {
                Failures.Add(item.Key, item.ToArray());
            }
        }
    }
}
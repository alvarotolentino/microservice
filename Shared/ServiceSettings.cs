using System;

namespace Microservice.Shared
{
    public class ServiceSettings
    {
        public MetricSettings Metric { get; set; }
        public DatabaseSettings DbSettings { get; set; }

        public ContextSettings Context { get; set; }
    }

    public class DatabaseSettings
    {
    }

    public class MetricSettings
    {

    }

    public class ContextSettings
    {
        public int ResponseTimeThreshold { get; set; }
    }
}

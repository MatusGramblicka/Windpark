namespace WindparkAPIAggregation.Contracts
{
    public class RabbitMqConfiguration
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string Queue { get; set; }
        public string RoutingKey { get; set; }
    }
}
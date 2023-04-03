namespace WindParkAPIAggregation.Interface;

public interface IMessageProducer
{
    void SendMessage<T>(T message) where T : class;
}
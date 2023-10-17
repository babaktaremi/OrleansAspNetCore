using Orleans;

namespace Grains.Interfaces;

public interface IMessagingService:IGrainWithGuidKey
{
    Task<string> InvokeMessage(string message);
    Task<GrainInfoModel> GetGrainInfo();
}
using Grains.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains
{
    public class MessagingService: Grain,IMessagingService
    {
        private readonly ILogger<MessagingService> _logger;
        readonly MessagingDbContext _dbContext;
        private DateTime _creationDate;
        private Guid _grainId;
        private readonly IGrainFactory _grainFactory;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Grain Activated. Grain Id => {grainId}",this.GetPrimaryKey());
            _creationDate=DateTime.Now;
            _grainId= this.GetPrimaryKey();
            return base.OnActivateAsync(cancellationToken);
        }

        public MessagingService(ILogger<MessagingService> logger, MessagingDbContext dbContext, IGrainFactory grainFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _grainFactory = grainFactory;
        }

        public async Task<string> InvokeMessage(string message)
        {

            var messageModel = new MessagingModel
            {
                Id = Guid.NewGuid(),
                MessageContent = message
            };

            await _dbContext.Messages.AddAsync(messageModel);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("Message Received. Message Content => {message}",message);

            var notification =
                $"Message invocation complete. Stored Message Id: {messageModel.Id} With Grain Id : {this.GetPrimaryKey()}";



            return notification;
        }

        public Task<GrainInfoModel> GetGrainInfo()=>Task.FromResult(new GrainInfoModel(_creationDate,_grainId));
    }

    [GenerateSerializer]
    public record GrainInfoModel([property: Id(1)]DateTime ActivationDate, [property:Id(2)] Guid GrainId);
}
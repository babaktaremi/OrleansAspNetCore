using Grains.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains
{
    public class MessagingService : Grain<GrainInfoModel>, IMessagingService
    {
        private readonly ILogger<MessagingService> _logger;
        readonly MessagingDbContext _dbContext;
        private DateTime _creationDate;
        private Guid _grainId;
        private readonly IGrainFactory _grainFactory;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Grain Activated. Grain Id => {grainId}", this.GetPrimaryKey());
            _creationDate = DateTime.Now;
            _grainId = this.GetPrimaryKey();

            await ReadStateAsync();


            if (this.State.ActivationDate == DateTime.MinValue && this.State.GrainId == Guid.Empty)
            {
                this.State.ActivationDate = DateTime.Now;
                this.State.GrainId = this._grainId;
                await WriteStateAsync();
            }



            await base.OnActivateAsync(cancellationToken);
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

            _logger.LogWarning("Message Received. Message Content => {message}", message);

            var notification =
                $"Message invocation complete. Stored Message Id: {messageModel.Id} With Grain Id : {this.GetPrimaryKey()}";

            await this.WriteStateAsync();

            return notification;
        }

        public Task<GrainInfoModel> GetGrainInfo() => Task.FromResult(this.State);
    }

    [GenerateSerializer]
    public class GrainInfoModel
    {
        [Id(1)]
        public DateTime ActivationDate { get; set; }

        [Id(2)]
        public Guid GrainId { get; set; }
    }
}
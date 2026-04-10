using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Database;

namespace TimeForge.Api.Features.Conversations.GetConversation;

public class GetConversationRequest
{
    public string Id { get; set; }
}

public class GetConversationValidator : Validator<GetConversationRequest>
{
    public GetConversationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull()
            .WithMessage("Conversation id cannot be empty");
    }
}

public class GetConversationEndpoint : Endpoint<GetConversationRequest, GetConversationResponse>
{
    private readonly TimeForgeDbContext _context;
    private readonly ILogger<GetConversationEndpoint> _logger;

    public GetConversationEndpoint(TimeForgeDbContext context, ILogger<GetConversationEndpoint> logger)
    {
        this._context = context;
        this._logger = logger;
    }

    public override void Configure()
    {
        Get("/conversations/{id}");
        Description(d => d
            .WithTags("Conversation")
            .WithSummary("Gets a conversation")
            .Produces<GetConversationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetConversationRequest req, CancellationToken ct)
    {
        var conversation = await this._context
            .Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.Id == req.Id, ct);

        if (conversation == null)
        {
            this._logger.LogError($"Conversation with id {req.Id} not found");
            ThrowError($"Conversation with id {req.Id} not found");
        }

        await Send.OkAsync(new GetConversationResponse()
        {
            Id = conversation.Id,
            Title = conversation.Title,
            IsTeamChat = conversation.IsTeamChat,
            TeamId = conversation.TeamId,
        }, ct);
    }
}
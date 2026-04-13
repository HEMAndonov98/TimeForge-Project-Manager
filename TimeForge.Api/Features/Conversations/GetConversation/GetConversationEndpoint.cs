using System.Net;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
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
            .WithMessage("Conversation id cannot be empty")
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString());
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
        Get("/conversation/{id}");
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
            .Include(x => x.Participants)
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.Id == req.Id, ct);

        if (conversation == null)
        {
            this._logger.LogError($"Conversation with id {req.Id} not found");
            ThrowError($"Conversation with id {req.Id} not found", StatusCodes.Status404NotFound);
        }
        
        //Check if caller is member of conversation
        var userId = User.GetUserId();

        if (!conversation.Participants.Any(p => p.UserId == userId))
        {
            this._logger.LogError($"User with id {userId} not found");
            ThrowError($"User with id {userId} is not a member of this conversation", StatusCodes.Status403Forbidden);
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
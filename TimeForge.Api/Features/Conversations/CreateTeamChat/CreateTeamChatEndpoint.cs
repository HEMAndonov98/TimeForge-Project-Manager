using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeForge.Api.Common.Extensions;
using TimeForge.Api.Features.Conversations.GetConversation;
using TimeForge.Database;
using TimeForge.Models;

namespace TimeForge.Api.Features.Conversations.CreateTeamChat;

public class CreateTeamChatRequest
{
    public string TeamId { get; set; } = string.Empty;
}

public class CreateTeamChatValidator : Validator<CreateTeamChatRequest>
{
    public CreateTeamChatValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty()
            .WithMessage("TeamId cannot be empty");
    }
}

public class CreateTeamChatEndpoint(
    TimeForgeDbContext context,
    ILogger<CreateTeamChatEndpoint> logger) : Endpoint<CreateTeamChatRequest, CreateTeamChatResponse>
{
    public override void Configure()
    {
        Post("/conversations/team");
        Description(d => d
            .WithTags("conversation")
            .WithSummary("Creates a team conversation for the specified team")
            .Produces<CreateTeamChatResponse>(StatusCodes.Status201Created)
            .ProducesProblemDetails(StatusCodes.Status404NotFound)
            .ProducesProblemDetails(StatusCodes.Status403Forbidden)
        );
    }

    public override async Task HandleAsync(CreateTeamChatRequest req, CancellationToken ct)
    {
        var currentUserId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var team = await context.Set<Team>()
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == req.TeamId, ct);

        if (team == null)
        {
            logger.LogError("Team with id {TeamId} does not exist", req.TeamId);
            ThrowError("Team does not exist", StatusCodes.Status404NotFound);
        }

        // Check if user is a member of the team
        var isMember = team.Members.Any(m => m.UserId == currentUserId);
        if (!isMember)
        {
            logger.LogWarning("User {UserId} attempted to create a chat for team {TeamId} without being a member", currentUserId, req.TeamId);
            ThrowError("You must be a member of the team to create a team chat", StatusCodes.Status403Forbidden);
        }

        // Check if team chat already exists
        var existingChat = await context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsTeamChat && c.TeamId == req.TeamId, ct);

        if (existingChat != null)
        {
            logger.LogInformation("Team chat already exists for team {TeamId}", req.TeamId);
            await Send.OkAsync(new CreateTeamChatResponse
            {
                ConversationId = existingChat.Id,
                Title = existingChat.Title ?? team.Name
            }, ct);
            return;
        }

        var newConversation = Conversation.CreateTeamChat(team);

        foreach (var member in team.Members)
        {
            newConversation.AddParticipant(ConversationParticipant.Create(newConversation.Id, member.UserId));
        }

        try
        {
            await context.Conversations.AddAsync(newConversation, ct);
            await context.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while creating team conversation");
            ThrowError("An error occurred while saving the conversation to the database", StatusCodes.Status500InternalServerError);
        }

        logger.LogInformation("Created team conversation with id {ConversationId} for team {TeamId}", newConversation.Id, req.TeamId);

        await Send.CreatedAtAsync<GetConversationEndpoint>(
            routeValues: new { id = newConversation.Id },
            responseBody: new CreateTeamChatResponse
            {
                ConversationId = newConversation.Id,
                Title = newConversation.Title ?? team.Name,
            },
            generateAbsoluteUrl: true,
            cancellation: ct);
    }
}
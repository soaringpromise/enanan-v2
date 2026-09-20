using System.Collections.Concurrent;
using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;

namespace EnananV2.Services;

public sealed class TierProfileDraftService
{
    private static TimeSpan Lifetime => TimeSpan.FromMinutes(15);

    private readonly ConcurrentDictionary<ulong, TierProfileDraft> _drafts = [];

    public void Start(ulong userId)
    {
        CleanupExpired();

        var draft = new TierProfileDraft
        {
            Step = 1,
            ExpiresAt = DateTimeOffset.UtcNow + Lifetime
        };

        _drafts[userId] = draft;
    }

    public TierProfileDraft Get(ulong userId, int expectedStep)
    {
        CleanupExpired();

        if (!_drafts.TryGetValue(userId, out var draft))
            throw new InvalidRequestException(
                "Your profile creation session has expired. Run `/tier-profile create` again.");

        lock (draft)
        {
            if (draft.Step != expectedStep)
                throw new InvalidRequestException("This profile creation step is no longer active.");
            draft.ExpiresAt = DateTimeOffset.UtcNow + Lifetime;
        }
        return draft;
    }

    public void Advance(TierProfileDraft draft, int expectedStep, int nextStep)
    {
        lock (draft)
        {
            if (draft.Step != expectedStep)
                throw new InvalidRequestException("This profile creation step is no longer active.");

            draft.Step = nextStep;
            draft.ExpiresAt = DateTimeOffset.UtcNow + Lifetime;
        }
    }

    public void Remove(ulong userId)
    {
        _drafts.TryRemove(userId, out _);
    }

    private void CleanupExpired()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var (userId, draft) in _drafts)
        {
            lock (draft)
            {
                if (draft.ExpiresAt <= now) _drafts.TryRemove(userId, out _);
            }
        }
    }
}
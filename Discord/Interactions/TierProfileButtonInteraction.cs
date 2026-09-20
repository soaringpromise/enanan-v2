using EnananV2.Definitions.Exceptions;
using EnananV2.Services;
using EnananV2.Tools.Factories;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EnananV2.Discord.Interactions;

public sealed class TierProfileButtonInteraction(TierProfileDraftService drafts) 
    : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("tier-profile-step-1")]
    public Task OpenStep1() => OpenStep(1, ModalFactory.CreateProfileCardStep1Modal());

    [ComponentInteraction("tier-profile-step-2")]
    public Task OpenStep2() => OpenStep(2, ModalFactory.CreateProfileCardStep2Modal());

    [ComponentInteraction("tier-profile-step-3")]
    public Task OpenStep3() => OpenStep(3, ModalFactory.CreateProfileCardStep3Modal());

    [ComponentInteraction("tier-profile-step-4")]
    public Task OpenStep4() => OpenStep(4, ModalFactory.CreateProfileCardStep4Modal());

    private async Task OpenStep(int step, ModalProperties modal)
    {
        try
        {
            drafts.Get(Context.User.Id, step);
            await RespondAsync(InteractionCallback.Modal(modal));
        }
        catch (InvalidRequestException e)
        {
            await RespondAsync(
                InteractionCallback.ModifyMessage(options =>
                {
                    options.Content = e.Message;
                    options.Components = [];
                }));
        }
    }
}
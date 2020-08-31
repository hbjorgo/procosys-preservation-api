﻿using System.Threading;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.Validators;
using Equinor.Procosys.Preservation.Command.Validators.JourneyValidators;
using Equinor.Procosys.Preservation.Command.Validators.ModeValidators;
using Equinor.Procosys.Preservation.Command.Validators.ResponsibleValidators;
using Equinor.Procosys.Preservation.Command.Validators.StepValidators;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using FluentValidation;

namespace Equinor.Procosys.Preservation.Command.JourneyCommands.UpdateStep
{
    public class UpdateStepCommandValidator : AbstractValidator<UpdateStepCommand>
    {
        public UpdateStepCommandValidator(
            IJourneyValidator journeyValidator,
            IStepValidator stepValidator,
            IModeValidator modeValidator,
            IResponsibleValidator responsibleValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                .MustAsync((command, token) => BeAnExistingJourneyAsync(command.JourneyId, token))
                .WithMessage(command => $"Journey doesn't exist! Journey={command.JourneyId}")
                .MustAsync((command, token) => BeAnExistingStepAsync(command.StepId, token))
                .WithMessage(command => $"Step doesn't exist! Step={command.StepId}")
                .MustAsync((command, token) => StepHasJourneyAsParentAsync(command.JourneyId, command.StepId, token))
                // this check must come after checking if both journey and step exists.
                // If both exists, but step has another journey as parent, this is a change of parent, i.e a "move"
                .WithMessage(command => $"Can not move a step to another journey! Step={command.StepId}")
                .MustAsync((command, token) => HaveUniqueStepTitleInJourneyAsync(command.JourneyId, command.StepId, command.Title, token))
                .WithMessage(command => $"Another step with title already exists in a journey! Step={command.Title}")
                .MustAsync((command, token) => NotBeAVoidedStepAsync(command.StepId, token))
                .WithMessage(command => $"Step is voided! Step={command.StepId}")
                .MustAsync((command, token) => BeAnExistingModeAsync(command.ModeId, token))
                .WithMessage(command => $"Mode doesn't exist! Mode={command.ModeId}")
                .MustAsync((command, token) => NotChangedToAVoidedModeAsync(command.ModeId, command.StepId, token))
                .WithMessage(command => $"Mode is voided! Mode={command.ModeId}")
                .MustAsync((command, token) => NotBeAnExistingAndVoidedResponsibleAsync(command.ResponsibleCode, token))
                .WithMessage(command => $"Responsible is voided! ResponsibleCode={command.ResponsibleCode}")
                .MustAsync((command, token) => BeFirstStepIfUpdatingToSupplierStep(command.JourneyId, command.ModeId, command.StepId, token))
                .WithMessage(command => $"Only the first step can be supplier step! Mode={command.ModeId}")
                .MustAsync((command, token) => NotHaveOtherStepsWithSameAutoTransferMethodInJourneyAsync(command.JourneyId, command.StepId, command.AutoTransferMethod, token))
                .WithMessage(command => $"Same auto transfer method can not be set on multiple steps in a journey! Method={command.AutoTransferMethod}")
                .Must(command => HaveAValidRowVersion(command.RowVersion))
                .WithMessage(command => $"Not a valid row version! Row version={command.RowVersion}");

            async Task<bool> BeAnExistingJourneyAsync(int journeyId, CancellationToken token)
                => await journeyValidator.ExistsAsync(journeyId, token);
            
            async Task<bool> BeAnExistingStepAsync(int stepId, CancellationToken token)
                => await stepValidator.ExistsAsync(stepId, token);
            
            async Task<bool> StepHasJourneyAsParentAsync(int journeyId, int stepId, CancellationToken token)
                => await stepValidator.StepHasJourneyAsParentAsync(journeyId, stepId, token);
            
            async Task<bool> HaveUniqueStepTitleInJourneyAsync(int journeyId, int stepId, string stepTitle, CancellationToken token) =>
                !await journeyValidator.OtherStepExistsWithSameTitleAsync(journeyId, stepId, stepTitle, token);
            
            async Task<bool> NotBeAVoidedStepAsync(int stepId, CancellationToken token)
                => !await stepValidator.IsVoidedAsync(stepId, token);
            
            async Task<bool> BeAnExistingModeAsync(int modeId, CancellationToken token)
                => await modeValidator.ExistsAsync(modeId, token);

            async Task<bool> NotChangedToAVoidedModeAsync(int modeId, int stepId, CancellationToken token)
                => await stepValidator.HasModeAsync(modeId, stepId, token) ||
                   !await modeValidator.IsVoidedAsync(modeId, token);
            
            async Task<bool> NotBeAnExistingAndVoidedResponsibleAsync(string responsibleCode, CancellationToken token)
                => !await responsibleValidator.ExistsAndIsVoidedAsync(responsibleCode, token);
            
            async Task<bool> BeFirstStepIfUpdatingToSupplierStep(int journeyId, int modeId, int stepId, CancellationToken token)
                => await stepValidator.IsFirstStepOrModeIsNotForSupplierAsync(journeyId, modeId, stepId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
            
            async Task<bool> NotHaveOtherStepsWithSameAutoTransferMethodInJourneyAsync(int journeyId, int stepId, AutoTransferMethod autoTransferMethod, CancellationToken token)
                => autoTransferMethod == AutoTransferMethod.None || !await journeyValidator.HasOtherStepWithAutoTransferMethodAsync(journeyId, stepId, autoTransferMethod, token);
        }
    }
}

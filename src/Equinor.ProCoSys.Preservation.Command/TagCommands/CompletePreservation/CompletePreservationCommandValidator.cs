﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Preservation.Command.Validators;
using Equinor.ProCoSys.Preservation.Command.Validators.TagValidators;
using Equinor.ProCoSys.Preservation.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Preservation.Command.TagCommands.CompletePreservation
{
    public class CompletePreservationCommandValidator : AbstractValidator<CompletePreservationCommand>
    {
        public CompletePreservationCommandValidator(
            IProjectValidator projectValidator,
            ITagValidator tagValidator,
            IRowVersionValidator rowVersionValidator)
        {
            CascadeMode = CascadeMode.Stop;
                        
            RuleFor(command => command.Tags)
                .Must(ids => ids != null && ids.Any())
                .WithMessage("At least 1 tag must be given!")
                .Must(BeUniqueTags)
                .WithMessage("Tags must be unique!");

            When(command => command.Tags.Any() && BeUniqueTags(command.Tags), () =>
            {
                RuleForEach(command => command.Tags)
                    .MustAsync((_, tag, _, token) => BeAnExistingTagAsync(tag.Id, token))
                    .WithMessage((_, tag) => $"Tag doesn't exist! TagId={tag.Id}")
                    .MustAsync((_, tag, _, token) => NotBeAVoidedTagAsync(tag.Id, token))
                    .WithMessage((_, tag) => $"Tag is voided! Tag='{GetTagDetails(tag.Id)}'")
                    .MustAsync((_, tag, _, token) => IsReadyToBeCompletedAsync(tag.Id, token))
                    .WithMessage((_, tag) => $"Preservation on tag can not be completed! Tag='{GetTagDetails(tag.Id)}'")
                    .Must(tag => HaveAValidRowVersion(tag.RowVersion))
                    .WithMessage((_, tag) => $"Not a valid row version! Row version={tag.RowVersion}");
            });

            RuleFor(command => command.Tags)
                .MustAsync(BeInSameProjectAsync)
                .WithMessage("Tags must be in same project!")
                .MustAsync(NotBeAClosedProjectForTagAsync)
                .WithMessage("Project is closed!");

            string GetTagDetails(int tagId) => tagValidator.GetTagDetailsAsync(tagId, default).Result;

            bool BeUniqueTags(IEnumerable<IdAndRowVersion> tags)
            {
                var ids = tags.Select(x => x.Id).ToList();
                return ids.Distinct().Count() == ids.Count;
            }
            
            async Task<bool> BeInSameProjectAsync(IEnumerable<IdAndRowVersion> tags, CancellationToken token)
                => await projectValidator.AllTagsInSameProjectAsync(tags.Select(x => x.Id), token);
            
            async Task<bool> NotBeAClosedProjectForTagAsync(IEnumerable<IdAndRowVersion> tags, CancellationToken token)
                => !await projectValidator.IsClosedForTagAsync(tags.First().Id, token);

            async Task<bool> BeAnExistingTagAsync(int tagId, CancellationToken token)
                => await tagValidator.ExistsAsync(tagId, token);

            async Task<bool> NotBeAVoidedTagAsync(int tagId, CancellationToken token)
                => !await tagValidator.IsVoidedAsync(tagId, token);

            async Task<bool> IsReadyToBeCompletedAsync(int tagId, CancellationToken token)
                => await tagValidator.IsReadyToBeCompletedAsync(tagId, token);

            bool HaveAValidRowVersion(string rowVersion)
                => rowVersionValidator.IsValid(rowVersion);
        }
    }
}

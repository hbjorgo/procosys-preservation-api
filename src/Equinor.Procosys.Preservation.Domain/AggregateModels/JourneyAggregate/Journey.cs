﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.Preservation.Domain.Audit;

namespace Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate
{
    public class Journey : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
    {
        public const int TitleLengthMin = 3;
        public const int TitleLengthMax = 255;

        private readonly List<Step> _steps = new List<Step>();

        protected Journey()
            : base(null)
        {
        }

        public Journey(string schema, string title)
            : base(schema)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            Title = title;
        }

        public IReadOnlyCollection<Step> Steps => _steps.AsReadOnly();
        public string Title { get; private set; }
        public bool IsVoided { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }

        public void Void() => IsVoided = true;
        public void UnVoid() => IsVoided = false;

        public void AddStep(Step step)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }
            
            if (step.Plant != Plant)
            {
                throw new ArgumentException($"Can't relate item in {step.Plant} to item in {Plant}");
            }

            _steps.Add(step);
        }

        public Step GetNextStep(int stepId)
        {
            var orderedSteps = _steps.OrderBy(r => r.SortKey).Where(r => !r.IsVoided).ToList();
            var stepIndex = orderedSteps.FindIndex(s => s.Id == stepId);
            if (stepIndex < orderedSteps.Count - 1)
            {
                return orderedSteps[stepIndex+1];
            }

            return null;
        }

        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }

        public void SetModified(Person modifiedBy)
        {
            ModifiedAtUtc = TimeService.UtcNow;
            if (modifiedBy == null)
            {
                throw new ArgumentNullException(nameof(modifiedBy));
            }
            ModifiedById = modifiedBy.Id;
        }
    }
}

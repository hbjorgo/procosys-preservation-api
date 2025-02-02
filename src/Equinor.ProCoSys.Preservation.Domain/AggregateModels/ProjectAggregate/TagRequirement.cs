﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Equinor.ProCoSys.Preservation.Domain.Audit;
using Equinor.ProCoSys.Preservation.Domain.Events;
using Equinor.ProCoSys.Preservation.Domain.Time;

namespace Equinor.ProCoSys.Preservation.Domain.AggregateModels.ProjectAggregate
{
    public class TagRequirement : PlantEntityBase, ICreationAuditable, IModificationAuditable, IVoidable
    {
        public const int InitialPreservationPeriodStatusMax = 64;

        // _initialPreservationPeriodStatus is made as DB property. Can't be readonly
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private PreservationPeriodStatus _initialPreservationPeriodStatus;
        private readonly List<PreservationPeriod> _preservationPeriods = new List<PreservationPeriod>();

        protected TagRequirement()
            : base(null)
        {
        }

        public TagRequirement(string plant, int intervalWeeks, RequirementDefinition requirementDefinition)
            : base(plant)
        {
            if (requirementDefinition == null)
            {
                throw new ArgumentNullException(nameof(requirementDefinition));
            }
            
            if (requirementDefinition.Plant != plant)
            {
                throw new ArgumentException($"Can't relate item in {requirementDefinition.Plant} to item in {plant}");
            }

            IntervalWeeks = intervalWeeks;
            Usage = requirementDefinition.Usage;
            RequirementDefinitionId = requirementDefinition.Id;
            
            _initialPreservationPeriodStatus = requirementDefinition.NeedsUserInput
                ? PreservationPeriodStatus.NeedsUserInput
                : PreservationPeriodStatus.ReadyToBePreserved;
        }

        public int IntervalWeeks { get; private set; }
        public RequirementUsage Usage { get; private set; }
        public DateTime? NextDueTimeUtc { get; private set; }
        public bool IsVoided { get; set; }
        public bool IsInUse => _preservationPeriods.Any();
        public int RequirementDefinitionId { get; private set; }
        public IReadOnlyCollection<PreservationPeriod> PreservationPeriods => _preservationPeriods.AsReadOnly();
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedById { get; private set; }

        public bool ReadyToBePreserved => PeriodReadyToBePreserved != null;

        public override string ToString() => $"Interval {IntervalWeeks}, NextDue {NextDueTimeUtc}, ReqDefId {RequirementDefinitionId}, Usage {Usage}";

        public int? GetNextDueInWeeks()
        {
            if (!NextDueTimeUtc.HasValue)
            {
                return null;
            }

            return TimeService.UtcNow.GetWeeksUntil(NextDueTimeUtc.Value);
        }
        
        public bool IsReadyAndDueToBePreserved()
        {
            if (!ReadyToBePreserved)
            {
                return false;
            }

            var nextDueInWeeks = GetNextDueInWeeks();

            return nextDueInWeeks.HasValue && nextDueInWeeks.Value <= 0;
        }

        public bool HasActivePeriod => ActivePeriod != null;

        public void StartPreservation() => PrepareNewPreservation();

        public void UndoStartPreservation() => NextDueTimeUtc = null;

        public void CompletePreservation() => NextDueTimeUtc = null;

        public void Preserve(Person preservedBy, bool bulkPreserved)
        {
            if (!ReadyToBePreserved)
            {
                throw new Exception($"{nameof(TagRequirement)} {Id} is not ready to be preserved");
            }

            PeriodReadyToBePreserved.Preserve(preservedBy, bulkPreserved);
            PrepareNewPreservation();
        }

        public PreservationPeriod ActivePeriod
            => PreservationPeriods.SingleOrDefault(pp => 
                pp.Status == PreservationPeriodStatus.ReadyToBePreserved ||
                pp.Status == PreservationPeriodStatus.NeedsUserInput);

        public FieldValue GetCurrentFieldValue(Field field)
            => ActivePeriod?.GetFieldValue(field.Id);

        public string GetCurrentComment() => ActivePeriod?.Comment;

        public FieldValue GetPreviousFieldValue(Field field)
        {
            if (!field.ShowPrevious.HasValue || !field.ShowPrevious.Value)
            {
                return null;
            }
            
            var lastPreservedPeriod = PreservationPeriods
                .Where(pp => pp.Status == PreservationPeriodStatus.Preserved)
                .OrderByDescending(pp => pp.PreservationRecord.PreservedAtUtc)
                .FirstOrDefault();
        
            return lastPreservedPeriod?.GetFieldValue(field.Id);
        }

        public void SetComment(string comment)
        {
            if (!HasActivePeriod)
            {
                throw new Exception($"{nameof(TagRequirement)} {Id} don't have an active {nameof(PreservationPeriod)}. Can't set comment");
            }
            ActivePeriod.SetComment(comment);
        }

        public void RecordNumberValues(Dictionary<int, double?> numberValues, RequirementDefinition requirementDefinition)
        {
            if (numberValues == null)
            {
                throw new ArgumentNullException(nameof(numberValues));
            }

            VerifyReadyForRecording(requirementDefinition);

            var period = ActivePeriod;

            foreach (var numberValue in numberValues)
            {
                var field = requirementDefinition.Fields.Single(f => f.Id == numberValue.Key);

                period.RecordNumberValueForField(field, numberValue.Value);
            }

            period.UpdateStatus(requirementDefinition);
        }

        public void RecordNumberIsNaValues(IList<int> fieldIds, RequirementDefinition requirementDefinition)
        {
            if (fieldIds == null)
            {
                throw new ArgumentNullException(nameof(fieldIds));
            }

            VerifyReadyForRecording(requirementDefinition);

            var period = ActivePeriod;

            foreach (var fieldId in fieldIds)
            {
                var field = requirementDefinition.Fields.Single(f => f.Id == fieldId);

                period.RecordNumberIsNaValueForField(field);
            }

            period.UpdateStatus(requirementDefinition);
        }

        public void RecordCheckBoxValues(Dictionary<int, bool> checkBoxValues, RequirementDefinition requirementDefinition)
        {
            if (checkBoxValues == null)
            {
                throw new ArgumentNullException(nameof(checkBoxValues));
            }

            VerifyReadyForRecording(requirementDefinition);

            var period = ActivePeriod;

            foreach (var checkBoxValue in checkBoxValues)
            {
                var field = requirementDefinition.Fields.Single(f => f.Id == checkBoxValue.Key);

                period.RecordCheckBoxValueForField(field, checkBoxValue.Value);
            }

            period.UpdateStatus(requirementDefinition);
        }
        
        public FieldValueAttachment GetAlreadyRecordedAttachment(int fieldId, RequirementDefinition requirementDefinition)
        {
            VerifyReadyForRecording(requirementDefinition);

            var period = ActivePeriod;

            var field = requirementDefinition.Fields.Single(f => f.Id == fieldId);

            return period.GetAlreadyRecordedAttachmentValueForField(field);
        }

        public void RecordAttachment(FieldValueAttachment attachment, int fieldId, RequirementDefinition requirementDefinition)
        {
            VerifyReadyForRecording(requirementDefinition);

            var period = ActivePeriod;

            var field = requirementDefinition.Fields.Single(f => f.Id == fieldId);

            period.RecordAttachmentValueForField(field, attachment);

            period.UpdateStatus(requirementDefinition);
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

        private PreservationPeriod PeriodReadyToBePreserved
            => PreservationPeriods.SingleOrDefault(pp => pp.Status == PreservationPeriodStatus.ReadyToBePreserved);

        private void PrepareNewPreservation()
        {
            // can have active period since it is possible to Undo Start
            if (HasActivePeriod)
            {
                ActivePeriod.SetDueTimeUtc(IntervalWeeks);
            }
            else
            {
                var preservationPeriod = new PreservationPeriod(Plant, IntervalWeeks, _initialPreservationPeriodStatus);
                _preservationPeriods.Add(preservationPeriod);
            }

            NextDueTimeUtc = ActivePeriod.DueTimeUtc;
        }

        private void VerifyReadyForRecording(RequirementDefinition requirementDefinition)
        {
            if (requirementDefinition == null)
            {
                throw new ArgumentNullException(nameof(requirementDefinition));
            }

            if (requirementDefinition.Id != RequirementDefinitionId)
            {
                throw new Exception(
                    $"{nameof(TagRequirement)} {Id} belong to RequirementDefinition {RequirementDefinitionId}. Can't record values for RequirementDefinition {requirementDefinition.Id}");
            }

            if (!HasActivePeriod)
            {
                throw new Exception($"{nameof(TagRequirement)} {Id} don't have an active {nameof(PreservationPeriod)}. Can't record values");
            }
        }

        public void UpdateInterval(int intervalWeeks)
        {
            IntervalWeeks = intervalWeeks;
            if (ActivePeriod != null)
            {
                NextDueTimeUtc = ActivePeriod.SetDueTimeUtc(intervalWeeks);
            }
        }

        public void Reschedule(int weeks, RescheduledDirection direction)
        {
            if (!HasActivePeriod)
            {
                throw new Exception($"{nameof(TagRequirement)} {Id} don't have an active {nameof(PreservationPeriod)}. Can't reschedule");
            }

            NextDueTimeUtc = ActivePeriod.Reschedule(weeks, direction);
        }
    }
}

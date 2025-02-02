﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.ProjectAggregate;

namespace Equinor.ProCoSys.Preservation.Query.GetTagsQueries.GetTags
{
    public class TagDto
    {
        public TagDto(
            int id,
            ActionStatus? actionStatus,
            string areaCode,
            string calloffNo,
            string commPkgNo,
            string disciplineCode,
            bool isNew,
            bool isVoided,
            string mcPkgNo, 
            string mode,
            string nextMode,
            string nextResponsibleCode,
            bool readyToBeEdited,
            bool readyToBePreserved,
            bool readyToBeStarted,
            bool readyToBeTransferred,
            bool readyToBeCompleted,
            bool readyToBeRescheduled,
            bool readyToBeDuplicated,
            bool readyToUndoStarted,
            string purchaseOrderNo,
            IEnumerable<RequirementDto> requirements,
            string responsibleCode,
            string responsibleDescription,
            string status,
            string storageArea,
            string tagFunctionCode,
            string tagDescription,
            string tagNo,
            TagType tagType,
            string rowVersion)
        {
            Id = id;
            ActionStatus = actionStatus;
            AreaCode = areaCode;
            CalloffNo = calloffNo;
            CommPkgNo = commPkgNo;
            Description = tagDescription;
            DisciplineCode = disciplineCode;
            IsNew = isNew;
            IsVoided = isVoided;
            McPkgNo = mcPkgNo;
            Mode = mode;
            NextMode = nextMode;
            NextResponsibleCode = nextResponsibleCode;
            ReadyToBePreserved = readyToBePreserved;
            ReadyToBeStarted = readyToBeStarted;
            ReadyToBeTransferred = readyToBeTransferred;
            ReadyToBeCompleted = readyToBeCompleted;
            ReadyToBeRescheduled = readyToBeRescheduled;
            ReadyToBeDuplicated = readyToBeDuplicated;
            ReadyToUndoStarted = readyToUndoStarted;
            ReadyToBeEdited = readyToBeEdited;
            PurchaseOrderNo = purchaseOrderNo;
            TagNo = tagNo;
            ResponsibleCode = responsibleCode;
            ResponsibleDescription = responsibleDescription;
            Status = status;
            StorageArea = storageArea;
            TagFunctionCode = tagFunctionCode;
            Requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
            TagType = tagType;
            RowVersion = rowVersion;
        }

        public ActionStatus? ActionStatus { get; }
        public string AreaCode { get; }
        public string CalloffNo { get; }
        public string CommPkgNo { get; }
        public string Description { get; }
        public string DisciplineCode { get; }
        public int Id { get; }
        public bool IsNew { get; }
        public bool IsVoided { get; }
        public string McPkgNo { get; }
        public string Mode { get; }
        public string NextMode { get; }
        public string NextResponsibleCode { get; }
        public string PurchaseOrderNo { get; }
        public bool ReadyToBeEdited { get; }
        public bool ReadyToBePreserved { get; }
        public bool ReadyToBeStarted { get; }
        public bool ReadyToBeTransferred { get; }
        public bool ReadyToBeCompleted { get; }
        public bool ReadyToBeRescheduled { get; }
        public bool ReadyToBeDuplicated { get; }
        public bool ReadyToUndoStarted { get; }
        public IEnumerable<RequirementDto> Requirements { get; }
        public string ResponsibleCode { get; }
        public string ResponsibleDescription { get; }
        public string Status { get; }
        public string StorageArea { get; }
        public string TagFunctionCode { get; }
        public string TagNo { get; }
        public TagType TagType { get; }
        public string RowVersion { get; }
    }
}

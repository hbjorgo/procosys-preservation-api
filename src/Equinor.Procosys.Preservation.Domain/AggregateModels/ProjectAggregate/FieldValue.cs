﻿using System;
using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Equinor.Procosys.Preservation.Domain.Audit;

namespace Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate
{
    public abstract class FieldValue : PlantEntityBase, ICreationAuditable
    {
        protected FieldValue()
            : base(null)
        {
        }

        protected FieldValue(string plant, Field field)
            : base(plant)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            FieldId = field.Id;
        }
        
        public int FieldId { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public int CreatedById { get; private set; }

        public void SetCreated(Person createdBy)
        {
            CreatedAtUtc = TimeService.UtcNow;
            if (createdBy == null)
            {
                throw new ArgumentNullException(nameof(createdBy));
            }
            CreatedById = createdBy.Id;
        }
    }
}

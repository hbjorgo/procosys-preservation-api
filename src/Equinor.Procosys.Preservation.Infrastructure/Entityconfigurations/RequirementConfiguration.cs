﻿using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Equinor.Procosys.Preservation.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.Procosys.Preservation.Infrastructure.EntityConfigurations
{
    internal class RequirementConfiguration : IEntityTypeConfiguration<Requirement>
    {
        private readonly string InitialPreservationPeriodStatusPropertyName = "_initialPreservationPeriodStatus";

        public void Configure(EntityTypeBuilder<Requirement> builder)
        {
            builder.ConfigurePlant();
            builder.ConfigureCreationAudit();
            builder.ConfigureModificationAudit();
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.NextDueTimeUtc)
                .HasConversion(PreservationContext.NullableDateTimeKindConverter);

            builder.HasOne<RequirementDefinition>();

            builder
                .HasMany(x => x.PreservationPeriods)
                .WithOne()
                .IsRequired();

            builder.Property(InitialPreservationPeriodStatusPropertyName)
                .HasMaxLength(Requirement.InitialPreservationPeriodStatusMax)
                .HasConversion<string>()
                .HasDefaultValue(PreservationPeriodStatus.NeedsUserInput)
                .IsRequired();

            builder.HasCheckConstraint(
                "constraint_requirement_check_valid_initial_status",
                $"{InitialPreservationPeriodStatusPropertyName} in ('{PreservationPeriodStatus.NeedsUserInput}','{PreservationPeriodStatus.ReadyToBePreserved}')");
        }
    }
}

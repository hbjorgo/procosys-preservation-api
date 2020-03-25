﻿using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.Preservation.Infrastructure.EntityConfigurations.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Equinor.Procosys.Preservation.Infrastructure.EntityConfigurations
{
    internal class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ConfigureModificationAudit();
            builder.ConfigureConcurrencyToken();

            builder.Property(x => x.Oid)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(Person.FirstNameLengthMax)
                .IsRequired();

            builder.Property(x => x.LastName)
                .HasMaxLength(Person.LastNameLengthMax)
                .IsRequired();
        }
    }
}

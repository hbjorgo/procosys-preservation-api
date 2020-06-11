﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.RequirementCommands.Preserve;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ModeAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ResponsibleAggregate;
using Equinor.Procosys.Preservation.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Command.Tests.RequirementCommands.Preserve
{
    [TestClass]
    public class PreserveCommandHandlerTests : CommandHandlerTestsBase
    {
        private const int SupplierModeId = 1;
        private const int OtherModeId = 2;
        private const int TagInSupplierStepId = 7;
        private const int TagInOtherStepId = 8;
        private const int SupplierStepId = 17;
        private const int OtherStepId = 18;
        private const int RequirementForSupplierId = 71;
        private const int RequirementForOtherId = 72;
        private const int Interval = 2;

        private readonly Guid _currentUserOid = new Guid("12345678-1234-1234-1234-123456789123");
        private Mock<IProjectRepository> _projectRepoMock;
        private Mock<IPersonRepository> _personRepoMock;
        private Mock<ICurrentUserProvider> _currentUserProvider;
        private PreserveCommand _commandForSupplierRequirement;
        private PreserveCommand _commandForOtherRequirement;
        private TagRequirement _requirementForSupplier;
        private TagRequirement _requirementForOther;

        private PreserveCommandHandler _dut;
        private PreservationPeriod _initialPreservationPeriodForSupplierRequirement;
        private PreservationPeriod _initialPreservationPeriodForOtherRequirement;
        private Tag _tagInSupplierStep;
        private Tag _tagInOtherStep;
        private Step _supplierStep;
        private Step _otherStep;

        [TestInitialize]
        public void Setup()
        {
            var responsible = new Responsible(TestPlant, "C", "T");

            var supplierMode = new Mode(TestPlant, "SUP", true);
            supplierMode.SetProtectedIdForTesting(SupplierModeId);
            _supplierStep = new Step(TestPlant, "SUP", supplierMode, responsible);
            _supplierStep.SetProtectedIdForTesting(SupplierStepId);

            var otherMode = new Mode(TestPlant, "OTHER", false);
            otherMode.SetProtectedIdForTesting(OtherModeId);
            _otherStep = new Step(TestPlant, "OTHER", otherMode, responsible);
            _otherStep.SetProtectedIdForTesting(OtherStepId);

            var rdForSupplier = new RequirementDefinition(TestPlant, "ForSupp", Interval, RequirementUsage.ForSuppliersOnly, 1);
            var rdForOther = new RequirementDefinition(TestPlant, "ForOther", Interval, RequirementUsage.ForOtherThanSuppliers, 2);

            _requirementForSupplier = new TagRequirement(TestPlant, Interval, rdForSupplier);
            _requirementForSupplier.SetProtectedIdForTesting(RequirementForSupplierId);

            _requirementForOther = new TagRequirement(TestPlant, Interval, rdForOther);
            _requirementForOther.SetProtectedIdForTesting(RequirementForOtherId);

            _tagInSupplierStep = new Tag(TestPlant, TagType.Standard, "", "", _supplierStep, new List<TagRequirement>
            {
                _requirementForSupplier
            });
            _tagInSupplierStep.SetProtectedIdForTesting(TagInSupplierStepId);
            _tagInOtherStep = new Tag(TestPlant, TagType.Standard, "", "", _otherStep, new List<TagRequirement>
            {
                _requirementForOther
            });
            _tagInOtherStep.SetProtectedIdForTesting(TagInOtherStepId);

            _currentUserProvider = new Mock<ICurrentUserProvider>();
            _currentUserProvider
                .Setup(x => x.GetCurrentUserOid())
                .Returns(_currentUserOid);
            _projectRepoMock = new Mock<IProjectRepository>();
            _projectRepoMock.Setup(r => r.GetTagByTagIdAsync(TagInSupplierStepId)).Returns(Task.FromResult(_tagInSupplierStep));
            _projectRepoMock.Setup(r => r.GetTagByTagIdAsync(TagInOtherStepId)).Returns(Task.FromResult(_tagInOtherStep));
            _personRepoMock = new Mock<IPersonRepository>();
            _personRepoMock
                .Setup(p => p.GetByOidAsync(It.Is<Guid>(x => x == _currentUserOid)))
                .Returns(Task.FromResult(new Person(_currentUserOid, "Test", "User")));

            _commandForSupplierRequirement = new PreserveCommand(TagInSupplierStepId, RequirementForSupplierId);
            _commandForOtherRequirement = new PreserveCommand(TagInOtherStepId, RequirementForOtherId);

            _timeProvider.Elapse(TimeSpan.FromDays(-1));
            _tagInSupplierStep.StartPreservation();
            _tagInOtherStep.StartPreservation();
            
            _timeProvider.SetTime(_utcNow);
            
            _initialPreservationPeriodForSupplierRequirement = _requirementForSupplier.PreservationPeriods.Single();
            _initialPreservationPeriodForOtherRequirement = _requirementForOther.PreservationPeriods.Single();

            _dut = new PreserveCommandHandler(
                _projectRepoMock.Object,
                _personRepoMock.Object,
                UnitOfWorkMock.Object,
                _currentUserProvider.Object);
        }

        [TestMethod]
        public async Task HandlingPreserveCommand_ShouldPreserveSupplierRequirement_WhenTagIsInSupplierStep()
        {
            await _dut.Handle(_commandForSupplierRequirement, default);

            var expectedNextDueTimeUtc = _utcNow.AddWeeks(Interval);
            Assert.AreEqual(expectedNextDueTimeUtc, _requirementForSupplier.NextDueTimeUtc);
            Assert.AreEqual(expectedNextDueTimeUtc, _tagInSupplierStep.NextDueTimeUtc);
            Assert.IsNotNull(_initialPreservationPeriodForSupplierRequirement.PreservationRecord);
        }

        [TestMethod]
        public async Task HandlingPreserveCommand_ShouldPreserveOtherRequirement_WhenTagIsInOtherStep()
        {
            await _dut.Handle(_commandForOtherRequirement, default);

            var expectedNextDueTimeUtc = _utcNow.AddWeeks(Interval);
            Assert.AreEqual(expectedNextDueTimeUtc, _requirementForOther.NextDueTimeUtc);
            Assert.AreEqual(expectedNextDueTimeUtc, _tagInOtherStep.NextDueTimeUtc);
            Assert.IsNotNull(_initialPreservationPeriodForOtherRequirement.PreservationRecord);
        }

        [TestMethod]
        public async Task HandlingPreserveCommand_ShouldThrowException_WhenPreserveSupplierRequirement_WhenTagIsInOtherStep()
        {
            _tagInSupplierStep.SetStep(_otherStep);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _dut.Handle(_commandForSupplierRequirement, default));
        }

        [TestMethod]
        public async Task HandlingPreserveCommand_ShouldThrowException_WhenPreserveOtherRequirement_WhenTagIsInSupplierStep()
        {
            _tagInOtherStep.SetStep(_supplierStep);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _dut.Handle(_commandForOtherRequirement, default));
        }

        [TestMethod]
        public async Task HandlingPreserveCommand_ShouldSave()
        {
            await _dut.Handle(_commandForSupplierRequirement, default);

            UnitOfWorkMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
        }
    }
}

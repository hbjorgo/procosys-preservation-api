﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Preservation.BlobStorage;
using Equinor.ProCoSys.Preservation.Command.RequirementCommands.DeleteAttachment;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Preservation.Command.Tests.RequirementCommands.DeleteAttachment
{
    [TestClass]
    public class DeleteFieldValueAttachmentCommandTests : CommandHandlerTestsBase
    {
        private readonly string _blobContainer = "bc";
        private Mock<RequirementDefinition> _requirementDefinition;
        private TagRequirement _requirement;
        private DeleteFieldValueAttachmentCommand _command;
        private DeleteFieldValueAttachmentCommandHandler _dut;
        private Mock<IBlobStorage> _blobStorageMock;

        [TestInitialize]
        public void Setup()
        {
            var _tagId = 1;
            var _attachmentFieldId = 12;
            var _reqId = 21;

            _command = new DeleteFieldValueAttachmentCommand(
                _tagId, 
                _reqId,
                _attachmentFieldId);

            _requirementDefinition = new Mock<RequirementDefinition>();
            _requirementDefinition.SetupGet(r => r.Id).Returns(_reqId);
            _requirementDefinition.SetupGet(r => r.Plant).Returns(TestPlant);

            var attachmentFieldMock = new Mock<Field>(TestPlant, "", FieldType.Attachment, 0, "", false);
            attachmentFieldMock.SetupGet(f => f.Id).Returns(_attachmentFieldId);
            attachmentFieldMock.SetupGet(f => f.Plant).Returns(TestPlant);
            _requirementDefinition.Object.AddField(attachmentFieldMock.Object);

            var requirementMock = new Mock<TagRequirement>(TestPlant, 2, _requirementDefinition.Object);
            requirementMock.SetupGet(r => r.Id).Returns(_reqId);
            requirementMock.SetupGet(r => r.Plant).Returns(TestPlant);
            _requirement = requirementMock.Object;

            var stepMock = new Mock<Step>();
            stepMock.SetupGet(s => s.Plant).Returns(TestPlant);
            var tag = new Tag(TestPlant, TagType.Standard, "", "", stepMock.Object, new List<TagRequirement>
            {
                _requirement
            });

            tag.StartPreservation();

            Assert.AreEqual(PreservationStatus.Active, tag.Status);
            Assert.IsTrue(_requirement.HasActivePeriod);

            var projectRepositoryMock = new Mock<IProjectRepository>();
            projectRepositoryMock
                .Setup(r => r.GetTagWithPreservationHistoryByTagIdAsync(_tagId))
                .Returns(Task.FromResult(tag));

            var rtRepositoryMock = new Mock<IRequirementTypeRepository>();
            rtRepositoryMock
                .Setup(r => r.GetRequirementDefinitionByIdAsync(_reqId))
                .Returns(Task.FromResult(_requirementDefinition.Object));
            
            _blobStorageMock = new Mock<IBlobStorage>();
            
            var blobStorageOptionsMock = new Mock<IOptionsSnapshot<BlobStorageOptions>>();
            var options = new BlobStorageOptions
            {
                MaxSizeMb = 2,
                BlobContainer = _blobContainer,
                BlockedFileSuffixes = new[] {".exe", ".zip"}
            };
            blobStorageOptionsMock
                .Setup(x => x.Value)
                .Returns(options);

            _dut = new DeleteFieldValueAttachmentCommandHandler(
                projectRepositoryMock.Object,
                rtRepositoryMock.Object,
                UnitOfWorkMock.Object,
                _blobStorageMock.Object,
                blobStorageOptionsMock.Object);
        }

        [TestMethod]
        public async Task HandlingDeleteFieldValueAttachmentCommand_ShouldSetActivePeriodReadyToNeedsUserInput()
        {
            // Arrange
            _requirement.RecordAttachment(new FieldValueAttachment(TestPlant, Guid.Empty, "F"), _command.FieldId, _requirementDefinition.Object);
            Assert.AreEqual(1, _requirement.ActivePeriod.FieldValues.Count);
            Assert.AreEqual(PreservationPeriodStatus.ReadyToBePreserved, _requirement.ActivePeriod.Status);

            // Act
            await _dut.Handle(_command, default);
            
            // Assert
            Assert.AreEqual(0, _requirement.ActivePeriod.FieldValues.Count);
            Assert.AreEqual(PreservationPeriodStatus.NeedsUserInput, _requirement.ActivePeriod.Status);
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldDeleteFromBlobStorage()
        {
            // Arrange
            _requirement.RecordAttachment(new FieldValueAttachment(TestPlant, Guid.Empty, "F"), _command.FieldId, _requirementDefinition.Object);
            var attachmentValue = (AttachmentValue)_requirement.ActivePeriod.FieldValues.Single();
            var path = attachmentValue.FieldValueAttachment.GetFullBlobPath(_blobContainer);

            // Act
            await _dut.Handle(_command, default);

            // Assert
            _blobStorageMock.Verify(b => b.DeleteAsync(path, default), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldNotTryToDeleteFromBlobStorage_WhenAttachmentNotExistsInAdvance()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            _blobStorageMock.Verify(b => b.DeleteAsync(It.IsAny<string>(), default), Times.Never);
        }

        [TestMethod]
        public async Task HandlingDeleteFieldValueAttachmentCommand_ShouldSave()
        {
            // Act
            await _dut.Handle(_command, default);
            
            // Assert
            UnitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
    }
}

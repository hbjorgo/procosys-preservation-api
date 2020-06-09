﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.BlobStorage;
using Equinor.Procosys.Preservation.Command.TagAttachmentCommands.Upload;
using Equinor.Procosys.Preservation.Domain;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Command.Tests.TagAttachmentCommands.Upload
{
    [TestClass]
    public class UploadTagAttachmentCommandHandlerTests : CommandHandlerTestsBase
    {
        private readonly string _fileName = "AttachmentFileName";
        private readonly string _blobContainer = "bc";

        private UploadTagAttachmentCommand _commandWithoutOverwrite;
        private UploadTagAttachmentCommandHandler _dut;

        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<IBlobStorage> _blobStorageMock;
        private Tag _tag;
        private readonly int _tagId = 2;

        [TestInitialize]
        public void Setup()
        {
            _commandWithoutOverwrite = new UploadTagAttachmentCommand(_tagId, _fileName, false, new MemoryStream(), TestUserOid);

            _projectRepositoryMock = new Mock<IProjectRepository>();
            _blobStorageMock = new Mock<IBlobStorage>();

            var attachmentOptionsMock = new Mock<IOptionsMonitor<AttachmentOptions>>();
            var options = new AttachmentOptions
            {
                MaxSizeKb = 2,
                BlobContainer = _blobContainer,
                ValidFileSuffixes = new[] {".gif", ".jpg"}
            };
            attachmentOptionsMock
                .Setup(x => x.CurrentValue)
                .Returns(options);

            var stepMock = new Mock<Step>();
            stepMock.SetupGet(s => s.Plant).Returns(TestPlant);

            var reqMock = new Mock<TagRequirement>();
            reqMock.SetupGet(s => s.Plant).Returns(TestPlant);

            _tag = new Tag(TestPlant, TagType.Standard, "", "", stepMock.Object, new List<TagRequirement> { reqMock.Object });

            _projectRepositoryMock
                .Setup(r => r.GetTagByTagIdAsync(_commandWithoutOverwrite.TagId))
                .Returns(Task.FromResult(_tag));

            _dut = new UploadTagAttachmentCommandHandler(
                _projectRepositoryMock.Object,
                UnitOfWorkMock.Object,
                PlantProviderMock.Object,
                _blobStorageMock.Object,
                attachmentOptionsMock.Object);
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldAddAttachmentToTag_WhenNotExist()
        {
            // Arrange
            Assert.IsTrue(_tag.Attachments.Count == 0);

            // Act
            var result = await _dut.Handle(_commandWithoutOverwrite, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Data);
            Assert.IsTrue(_tag.Attachments.Count == 1);
            var attachment = _tag.Attachments.Single();
            Assert.AreEqual(_fileName, attachment.FileName);
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldThrowException_WhenExistAndNotOverWrite()
        {
            // Arrange
            await _dut.Handle(_commandWithoutOverwrite, default);
            Assert.IsTrue(_tag.Attachments.Count == 1);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_commandWithoutOverwrite, default));
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldUpdateExistingAttachment_WhenExistAndOverWrite()
        {
            // Arrange
            await _dut.Handle(_commandWithoutOverwrite, default);
            Assert.IsTrue(_tag.Attachments.Count == 1);
            var commandWithOverwrite = new UploadTagAttachmentCommand(_tagId, _fileName, true, new MemoryStream(), TestUserOid);

            // Act
            var result = await _dut.Handle(commandWithOverwrite, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Data);
            Assert.IsTrue(_tag.Attachments.Count == 1);
            var attachment = _tag.Attachments.Single();
            Assert.AreEqual(_fileName, attachment.FileName);
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldSave()
        {
            // Act
            await _dut.Handle(_commandWithoutOverwrite, default);

            // Assert
            UnitOfWorkMock.Verify(u => u.SaveChangesAsync(_commandWithoutOverwrite.CurrentUserOid, default), Times.Once);
        }

        [TestMethod]
        public async Task HandlingUploadTagAttachmentCommand_ShouldUploadToBlobStorage()
        {
            // Act
            await _dut.Handle(_commandWithoutOverwrite, default);

            // Assert
            var attachment = _tag.Attachments.Single();
            var p = attachment.GetFullBlobPath(_blobContainer);
            _blobStorageMock.Verify(b 
                => b.UploadAsync(p, It.IsAny<Stream>(), _commandWithoutOverwrite.OverwriteIfExists, default), Times.Once);
        }
    }
}

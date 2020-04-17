﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Command.TagCommands.CreateTags;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Equinor.Procosys.Preservation.MainApi.Tag;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Command.Tests.TagCommands.CreateTags
{
    [TestClass]
    public class CreateTagsCommandHandlerTests : CommandHandlerTestsBase
    {
        private const string TestTagNo1 = "TagNo1";
        private const string TestTagNo2 = "TagNo2";
        private const string TestProjectName = "TestProjectX";
        private const string TestProjectDescription = "TestProjectXDescription";
        private const int StepId = 11;
        private const int ReqDefId1 = 99;
        private const int ReqDefId2 = 199;
        private const int Interval1 = 2;
        private const int Interval2 = 3;

        private Mock<Step> _stepMock;
        private Mock<IJourneyRepository> _journeyRepositoryMock;
        private Project _projectAddedToRepository;
        private Mock<IProjectRepository> _projectRepositoryMock;
        private Mock<IRequirementTypeRepository> _rtRepositoryMock;
        private Mock<ITagApiService> _tagApiServiceMock;

        private ProcosysTagDetails _mainTagDetails1;
        private ProcosysTagDetails _mainTagDetails2;
        
        private CreateTagsCommand _command;
        private CreateTagsCommandHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            _stepMock = new Mock<Step>();
            _stepMock.SetupGet(s => s.Id).Returns(StepId);
            _stepMock.SetupGet(s => s.Plant).Returns(TestPlant);
            
            _journeyRepositoryMock = new Mock<IJourneyRepository>();
            _journeyRepositoryMock
                .Setup(x => x.GetStepByStepIdAsync(StepId))
                .Returns(Task.FromResult(_stepMock.Object));

            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectRepositoryMock
                .Setup(x => x.Add(It.IsAny<Project>()))
                .Callback<Project>(project =>
                {
                    _projectAddedToRepository = project;
                });

            _rtRepositoryMock = new Mock<IRequirementTypeRepository>();
            var rdMock1 = new Mock<RequirementDefinition>();
            rdMock1.SetupGet(x => x.Id).Returns(ReqDefId1);
            rdMock1.SetupGet(x => x.Plant).Returns(TestPlant);
            var rdMock2 = new Mock<RequirementDefinition>();
            rdMock2.SetupGet(x => x.Id).Returns(ReqDefId2);
            rdMock2.SetupGet(x => x.Plant).Returns(TestPlant);
            _rtRepositoryMock
                .Setup(r => r.GetRequirementDefinitionsByIdsAsync(new List<int> {ReqDefId1, ReqDefId2}))
                .Returns(Task.FromResult(new List<RequirementDefinition> {rdMock1.Object, rdMock2.Object}));

            _mainTagDetails1 = new ProcosysTagDetails
            {
                AreaCode = "AreaCode1",
                AreaDescription = "AreaDescription1",
                CallOffNo = "CalloffNo1",
                CommPkgNo = "CommPkgNo1",
                Description = "Description1",
                DisciplineCode = "DisciplineCode1",
                DisciplineDescription = "DisciplineDescription1",
                McPkgNo = "McPkgNo1",
                PurchaseOrderNo = "PurchaseOrderNo1",
                TagFunctionCode = "TagFunctionCode1",
                TagNo = TestTagNo1,
                ProjectDescription = TestProjectDescription
            };
            _mainTagDetails2 = new ProcosysTagDetails
            {
                AreaCode = "AreaCode2",
                AreaDescription = "AreaDescription2",
                CallOffNo = "CalloffNo2",
                CommPkgNo = "CommPkgNo2",
                Description = "Description2",
                DisciplineCode = "DisciplineCode2",
                DisciplineDescription = "DisciplineDescription1",
                McPkgNo = "McPkgNo2",
                PurchaseOrderNo = "PurchaseOrderNo2",
                TagFunctionCode = "TagFunctionCode2",
                TagNo = TestTagNo2,
                ProjectDescription = TestProjectDescription
            };

            IList<ProcosysTagDetails> mainTagDetailList = new List<ProcosysTagDetails> {_mainTagDetails1, _mainTagDetails2};
            _tagApiServiceMock = new Mock<ITagApiService>();
            _tagApiServiceMock
                .Setup(x => x.GetTagDetailsAsync(TestPlant, TestProjectName, new List<string>{TestTagNo1, TestTagNo2}))
                .Returns(Task.FromResult(mainTagDetailList));

            _command = new CreateTagsCommand(
                new List<string>{TestTagNo1, TestTagNo2}, 
                TestProjectName,
                _stepMock.Object.Id,
                new List<RequirementForCommand>
                {
                    new RequirementForCommand(ReqDefId1, Interval1),
                    new RequirementForCommand(ReqDefId2, Interval2),
                },
                "Remark",
                "SA");
            
            _dut = new CreateTagsCommandHandler(
                _projectRepositoryMock.Object,
                _journeyRepositoryMock.Object,
                _rtRepositoryMock.Object,
                UnitOfWorkMock.Object,
                PlantProviderMock.Object,
                _tagApiServiceMock.Object);
        }

        [TestMethod]
        public async Task HandlingCreateTagsCommand_ShouldAddProjectToRepository_WhenProjectNotExists()
        {
            // Arrange
            _projectRepositoryMock
                .Setup(r => r.GetByNameAsync(TestProjectName)).Returns(Task.FromResult((Project)null));

            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(2, result.Data.Count);
            Assert.AreEqual(TestProjectName, _projectAddedToRepository.Name);
            Assert.AreEqual(TestProjectDescription, _projectAddedToRepository.Description);
        }

        [TestMethod]
        public async Task HandlingCreateTagsCommand_ShouldNotAddAnyProjectToRepository_WhenProjectAlreadyExists()
        {
            // Arrange
            var project = new Project(TestPlant, TestProjectName, "");
            _projectRepositoryMock
                .Setup(r => r.GetByNameAsync(TestProjectName)).Returns(Task.FromResult(project));

            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            Assert.IsNull(_projectAddedToRepository);
        }

        [TestMethod]
        public async Task HandlingCreateTagsCommand_ShouldAdd2TagsToNewProject_WhenProjectNotExists()
        {
            // Arrange
            _projectRepositoryMock
                .Setup(r => r.GetByNameAsync(TestProjectName)).Returns(Task.FromResult((Project)null));

            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(2, result.Data.Count);
            
            var tags = _projectAddedToRepository.Tags;
            Assert.AreEqual(2, tags.Count);
            AssertTagProperties(_command, _mainTagDetails1, tags.First());
            AssertTagProperties(_command, _mainTagDetails2, tags.Last());
        }

        [TestMethod]
        public async Task HandlingCreateTagsCommand_ShouldAdd2TagsToExistingProject_WhenProjectAlreadyExists()
        {
            // Arrange
            var project = new Project(TestPlant, TestProjectName, "");
            _projectRepositoryMock
                .Setup(r => r.GetByNameAsync(TestProjectName)).Returns(Task.FromResult(project));
            Assert.AreEqual(0, project.Tags.Count);
            
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(2, result.Data.Count);
            
            var tags = project.Tags;
            Assert.AreEqual(2, tags.Count);
            AssertTagProperties(_command, _mainTagDetails1, tags.First());
            AssertTagProperties(_command, _mainTagDetails2, tags.Last());
        }

        [TestMethod]
        public async Task HandlingCreateTagsCommand_ShouldSave()
        {
            // Act
            await _dut.Handle(_command, default);
            
            // Assert
            UnitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        private void AssertTagProperties(CreateTagsCommand command, ProcosysTagDetails mainTagDetails, Tag tagAddedToProject)
        {
            Assert.AreEqual(mainTagDetails.AreaCode, tagAddedToProject.AreaCode);
            Assert.AreEqual(mainTagDetails.AreaDescription, tagAddedToProject.AreaDescription);
            Assert.AreEqual(mainTagDetails.CallOffNo, tagAddedToProject.Calloff);
            Assert.AreEqual(mainTagDetails.CommPkgNo, tagAddedToProject.CommPkgNo);
            Assert.AreEqual(mainTagDetails.DisciplineCode, tagAddedToProject.DisciplineCode);
            Assert.AreEqual(mainTagDetails.DisciplineDescription, tagAddedToProject.DisciplineDescription);
            Assert.AreEqual(TagType.Standard, tagAddedToProject.TagType);
            Assert.AreEqual(mainTagDetails.McPkgNo, tagAddedToProject.McPkgNo);
            Assert.AreEqual(mainTagDetails.Description, tagAddedToProject.Description);
            Assert.AreEqual(command.Remark, tagAddedToProject.Remark);
            Assert.AreEqual(command.StorageArea, tagAddedToProject.StorageArea);
            Assert.AreEqual(mainTagDetails.PurchaseOrderNo, tagAddedToProject.PurchaseOrderNo);
            Assert.AreEqual(TestPlant, tagAddedToProject.Plant);
            Assert.AreEqual(StepId, tagAddedToProject.StepId);
            Assert.AreEqual(mainTagDetails.TagFunctionCode, tagAddedToProject.TagFunctionCode);
            Assert.AreEqual(mainTagDetails.TagNo, tagAddedToProject.TagNo);
            Assert.AreEqual(2, tagAddedToProject.Requirements.Count);
            AssertReqProperties(tagAddedToProject.Requirements.First(), ReqDefId1, Interval1);
            AssertReqProperties(tagAddedToProject.Requirements.Last(), ReqDefId2, Interval2);
        }

        private void AssertReqProperties(Domain.AggregateModels.ProjectAggregate.Requirement req, int reqDefId, int interval)
        {
            Assert.AreEqual(reqDefId, req.RequirementDefinitionId);
            Assert.AreEqual(interval, req.IntervalWeeks);
        }
    }
}

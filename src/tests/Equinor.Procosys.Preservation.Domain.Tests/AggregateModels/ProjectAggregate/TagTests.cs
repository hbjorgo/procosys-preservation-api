﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.Procosys.Preservation.Domain.AggregateModels.JourneyAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Domain.Tests.AggregateModels.ProjectAggregate
{
    [TestClass]
    public class TagTests
    {
        private const int TwoWeeksInterval = 2;
        private const int ThreeWeeksInterval = 3;
        private Mock<Step> _stepMock;
        
        private RequirementDefinition _reqDef1NotNeedInput;
        private RequirementDefinition _reqDef2NotNeedInput;
        private RequirementDefinition _reqDef1NeedInput;
        private RequirementDefinition _reqDef2NeedInput;
        private Requirement _reqNotNeedInputTwoWeekInterval;
        private Requirement _reqNotNeedInputThreeWeekInterval;
        private Requirement _reqNeedInputTwoWeekInterval;
        private Requirement _reqNeedInputThreeWeekInterval;
        private List<Requirement> _twoReqs_NoneNeedInput_DifferentIntervals;
        private List<Requirement> _oneReq_NotNeedInputTwoWeekInterval;
        private List<Requirement> _oneReq_NeedInputTwoWeekInterval;
        private List<Requirement> _twoReqs_FirstNeedInputTwoWeekInterval_SecondNotNeedInputThreeWeekInterval;
        private List<Requirement> _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval;
        
        private DateTime _utcNow;
        private DateTime _dueTimeForTwoWeeksInterval;
        private DateTime _dueTimeForThreeWeeksInterval;
        private Tag _dutWithOneReqNotNeedInput;

        [TestInitialize]
        public void Setup()
        {
            _stepMock = new Mock<Step>();
            _stepMock.SetupGet(x => x.Id).Returns(3);

            _reqDef1NotNeedInput = new RequirementDefinition("", "", 2, 0);
            _reqDef1NotNeedInput.AddField(new Field("", "", FieldType.Info, 0));
            _reqDef2NotNeedInput = new RequirementDefinition("", "", 2, 0);
            _reqDef2NotNeedInput.AddField(new Field("", "", FieldType.Info, 0));
            _reqDef1NeedInput = new RequirementDefinition("", "", 1, 0);
            _reqDef1NeedInput.AddField(new Field("", "", FieldType.CheckBox, 0));
            _reqDef2NeedInput = new RequirementDefinition("", "", 1, 0);
            _reqDef2NeedInput.AddField(new Field("", "", FieldType.CheckBox, 0));
            
            _reqNotNeedInputTwoWeekInterval = new Requirement("", TwoWeeksInterval, _reqDef1NotNeedInput);
            _reqNotNeedInputThreeWeekInterval = new Requirement("", ThreeWeeksInterval, _reqDef2NotNeedInput);
            _reqNeedInputTwoWeekInterval = new Requirement("", TwoWeeksInterval, _reqDef1NeedInput);
            _reqNeedInputThreeWeekInterval = new Requirement("", ThreeWeeksInterval, _reqDef2NeedInput);

            _twoReqs_NoneNeedInput_DifferentIntervals = new List<Requirement>
            {
                _reqNotNeedInputTwoWeekInterval, _reqNotNeedInputThreeWeekInterval
            };

            _oneReq_NotNeedInputTwoWeekInterval = new List<Requirement>
            {
                _reqNotNeedInputTwoWeekInterval
            };

            _oneReq_NeedInputTwoWeekInterval = new List<Requirement>
            {
                _reqNeedInputTwoWeekInterval
            };

            _twoReqs_FirstNeedInputTwoWeekInterval_SecondNotNeedInputThreeWeekInterval = new List<Requirement>
            {
                _reqNeedInputTwoWeekInterval, _reqNotNeedInputThreeWeekInterval
            };

            _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval = new List<Requirement>
            {
                _reqNotNeedInputTwoWeekInterval, _reqNeedInputThreeWeekInterval
            };

            _utcNow = new DateTime(2020, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            _dueTimeForTwoWeeksInterval = _utcNow.AddWeeks(TwoWeeksInterval);
            _dueTimeForThreeWeeksInterval = _utcNow.AddWeeks(ThreeWeeksInterval);

            _dutWithOneReqNotNeedInput = new Tag("SchemaA",
                "TagNoA",
                "DescA", 
                "AreaCodeA", 
                "CalloffA", 
                "DisciplineA", 
                "McPkgA", 
                "CommPkgA", 
                "PurchaseOrderA", 
                "RemarkA", 
                "TagFunctionCodeA", 
                _stepMock.Object,
                _oneReq_NotNeedInputTwoWeekInterval);
        }

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual("SchemaA", _dutWithOneReqNotNeedInput.Schema);
            Assert.AreEqual("TagNoA", _dutWithOneReqNotNeedInput.TagNo);
            Assert.AreEqual("DescA", _dutWithOneReqNotNeedInput.Description);
            Assert.AreEqual("AreaCodeA", _dutWithOneReqNotNeedInput.AreaCode);
            Assert.AreEqual("CalloffA", _dutWithOneReqNotNeedInput.Calloff);
            Assert.AreEqual("DisciplineA", _dutWithOneReqNotNeedInput.DisciplineCode);
            Assert.AreEqual("McPkgA", _dutWithOneReqNotNeedInput.McPkgNo);
            Assert.AreEqual("PurchaseOrderA", _dutWithOneReqNotNeedInput.PurchaseOrderNo);
            Assert.AreEqual("RemarkA", _dutWithOneReqNotNeedInput.Remark);
            Assert.AreEqual("TagFunctionCodeA", _dutWithOneReqNotNeedInput.TagFunctionCode);
            Assert.AreEqual(_stepMock.Object.Id, _dutWithOneReqNotNeedInput.StepId);
            var requirements = _dutWithOneReqNotNeedInput.Requirements;
            Assert.AreEqual(1, requirements.Count);
            
            var req = _dutWithOneReqNotNeedInput.Requirements.ElementAt(0);
            Assert.IsNull(req.NextDueTimeUtc);
            Assert.AreEqual(PreservationStatus.NotStarted, _dutWithOneReqNotNeedInput.Status);
        }
        
        [TestMethod]
        public void Constructor_ShouldNotSetReadyToBePreserved_AtAnyTime()
        {
            Assert.IsFalse(_dutWithOneReqNotNeedInput.IsReadyToBePreserved(_utcNow));
            Assert.IsFalse(_dutWithOneReqNotNeedInput.IsReadyToBePreserved(_utcNow.AddWeeks(TwoWeeksInterval)));
            Assert.IsFalse(_dutWithOneReqNotNeedInput.IsReadyToBePreserved(_utcNow.AddWeeks(ThreeWeeksInterval)));
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenStepNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(()
                => new Tag("", "", "", "", "", "", "", "", "", "", "", null, _twoReqs_NoneNeedInput_DifferentIntervals));

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenRequirementsNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(()
                => new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, null));

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenEmptyListOfRequirementsGiven()
            => Assert.ThrowsException<Exception>(()
                => new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, new List<Requirement>()));

        [TestMethod]
        public void SetStep_ShouldSetStepId()
        {
            var newStep = new Mock<Step>();
            newStep.SetupGet(x => x.Id).Returns(4);
            _dutWithOneReqNotNeedInput.SetStep(newStep.Object);

            Assert.AreEqual(newStep.Object.Id, _dutWithOneReqNotNeedInput.StepId);
        }

        [TestMethod]
        public void SetStep_ShouldThrowException_WhenStepNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithOneReqNotNeedInput.SetStep(null));

        [TestMethod]
        public void AddRequirement_ShouldThrowException_WhenRequirementNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithOneReqNotNeedInput.AddRequirement(null));

        [TestMethod]
        public void StartPreservation_ShouldSetStatusActive()
        {
            Assert.AreEqual(PreservationStatus.NotStarted, _dutWithOneReqNotNeedInput.Status);

            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);

            Assert.AreEqual(PreservationStatus.Active, _dutWithOneReqNotNeedInput.Status);
        }

        [TestMethod]
        public void StartPreservation_ShouldSetCorrectNextDueDateOnEachRequirement()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_NoneNeedInput_DifferentIntervals);

            dut.StartPreservation(_utcNow);

            var expectedNextDueTimeFirstUtc = _utcNow.AddWeeks(TwoWeeksInterval);
            var expectedNextDueTimeLaterUtc = _utcNow.AddWeeks(ThreeWeeksInterval);
            Assert.AreEqual(expectedNextDueTimeFirstUtc, dut.Requirements.ElementAt(0).NextDueTimeUtc);
            Assert.AreEqual(expectedNextDueTimeLaterUtc, dut.Requirements.ElementAt(1).NextDueTimeUtc);
        }
        
        [TestMethod]
        public void IsReadyToBePreserved_ShouldBeFalse_BeforePeriod()
        {
            Assert.AreEqual(PreservationStatus.NotStarted, _dutWithOneReqNotNeedInput.Status);
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);

            Assert.IsFalse(_dutWithOneReqNotNeedInput.IsReadyToBePreserved(_utcNow));
        }
        
        [TestMethod]
        public void IsReadyToBePreserved_ShouldBeTrue_InPeriod_WhenNotNeedInput()
        {
            Assert.AreEqual(PreservationStatus.NotStarted, _dutWithOneReqNotNeedInput.Status);
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            
            Assert.IsTrue(_dutWithOneReqNotNeedInput.IsReadyToBePreserved(_dueTimeForTwoWeeksInterval));
        }
        
        [TestMethod]
        public void IsReadyToBePreserved_ShouldBeTrue_OnOverdue_WhenNotNeedInput()
        {
            Assert.AreEqual(PreservationStatus.NotStarted, _dutWithOneReqNotNeedInput.Status);
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);

            var overDue = _utcNow.AddWeeks(TwoWeeksInterval + TwoWeeksInterval);
            Assert.IsTrue(_dutWithOneReqNotNeedInput.IsReadyToBePreserved(overDue));
        }

        [TestMethod]
        public void IsReadyToBePreserved_ShouldBeFalse_InPeriod_WhenNeedInput()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _oneReq_NeedInputTwoWeekInterval);
            Assert.AreEqual(PreservationStatus.NotStarted, dut.Status);

            dut.StartPreservation(_utcNow);

            Assert.IsFalse(dut.IsReadyToBePreserved(_dueTimeForTwoWeeksInterval));
        }

        [TestMethod]
        public void FirstUpcomingRequirement_ShouldGiveRequirement_WhenPreservationStarted()
        {
            Assert.AreEqual(PreservationStatus.NotStarted, _dutWithOneReqNotNeedInput.Status);

            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            var firstUpcomingRequirement = _dutWithOneReqNotNeedInput.FirstUpcomingRequirement;

            Assert.IsNotNull(firstUpcomingRequirement);
        }

        [TestMethod]
        public void Preserve_ShouldThrowException_WhenPreservingBeforeTime()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
                   
            Assert.ThrowsException<Exception>(() 
                => _dutWithOneReqNotNeedInput.Preserve(_utcNow, new Mock<Person>().Object));
     
        }

        [TestMethod]
        public void Preserve_ShouldPreserve_WhenPreservingOnDue()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            Assert.AreEqual(1, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
            
            _dutWithOneReqNotNeedInput.Preserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
        }

        [TestMethod]
        public void Preserve_ShouldPreserve_WhenPreservingOverDue()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            Assert.AreEqual(1, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
            
            _dutWithOneReqNotNeedInput.Preserve(_dueTimeForThreeWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
        }
        
        [TestMethod]
        public void Preserve_ShouldPreserveOnce_WhenPreservingMultipleTimesAtSameTime()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            Assert.AreEqual(1, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
            
            _dutWithOneReqNotNeedInput.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);

            Assert.ThrowsException<Exception>(() 
                => _dutWithOneReqNotNeedInput.Preserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object));
        }

        [TestMethod]
        public void Preserve_ShouldThrowException_WhenRequirementNeedsInput()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _oneReq_NeedInputTwoWeekInterval);
            dut.StartPreservation(_utcNow);

            Assert.ThrowsException<Exception>(() => dut.Preserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object));
        }

        [TestMethod]
        public void Preserve_ShouldThrowException_WhenFirstUpcomingRequirementNeedsInput()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNeedInputTwoWeekInterval_SecondNotNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);

            Assert.ThrowsException<Exception>(() => dut.Preserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object));
        }
        
        [TestMethod]
        public void Preserve_ShouldThrowException_WhenFirstUpcomingRequirementNeedInput_BecauseFirstInListIsVoided()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);
            dut.Requirements.ElementAt(0).Void();

            Assert.ThrowsException<Exception>(() => dut.Preserve(_dueTimeForThreeWeeksInterval, new Mock<Person>().Object));
        }

        [TestMethod]
        public void Preserve_ShouldThrowException_WhenPreservedByNotGiven()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_NoneNeedInput_DifferentIntervals);
            dut.StartPreservation(_utcNow);

            Assert.ThrowsException<ArgumentNullException>(() => dut.Preserve(_dueTimeForTwoWeeksInterval, null));
        }
        
        [TestMethod]
        public void Preserve_ShouldChangeUpComingRequirement()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);
            Assert.AreEqual(_reqNotNeedInputTwoWeekInterval, dut.FirstUpcomingRequirement);

            dut.Preserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);

            Assert.AreEqual(_reqNeedInputThreeWeekInterval, dut.FirstUpcomingRequirement);
        }
        
        [TestMethod]
        public void Preserve_ShouldPreserveDueRequirementsOnly()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);

            var req1 = dut.Requirements.ElementAt(0);
            var req2 = dut.Requirements.ElementAt(1);
            Assert.AreEqual(1, req1.PreservationPeriods.Count);
            Assert.AreEqual(1, req2.PreservationPeriods.Count);

            dut.Preserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, req1.PreservationPeriods.Count);
            Assert.AreEqual(1, req2.PreservationPeriods.Count);
        }

        [TestMethod]
        public void BulkPreserve_ShouldThrowException_WhenRequirementNeedInput()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _oneReq_NeedInputTwoWeekInterval);
            dut.StartPreservation(_utcNow);

            Assert.ThrowsException<Exception>(() => dut.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object));
        }

        [TestMethod]
        public void BulkPreserve_ShouldThrowException_WhenFirstUpcomingRequirementNeedInput()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNeedInputTwoWeekInterval_SecondNotNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);

            Assert.ThrowsException<Exception>(() => dut.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object));
        }

        [TestMethod]
        public void BulkPreserve_ShouldThrowException_WhenFirstUpcomingRequirementNeedInput_BecauseFirstInListIsVoided()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);
            dut.Requirements.ElementAt(0).Void();

            Assert.ThrowsException<Exception>(() => dut.BulkPreserve(_dueTimeForThreeWeeksInterval, new Mock<Person>().Object));
        }

        [TestMethod]
        public void BulkPreserve_ShouldThrowException_WhenPreservedByNotGiven()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_NoneNeedInput_DifferentIntervals);
            dut.StartPreservation(_utcNow);

            Assert.ThrowsException<ArgumentNullException>(() => dut.BulkPreserve(_dueTimeForTwoWeeksInterval, null));
        }

        [TestMethod]
        public void BulkPreserve_ShouldThrowException_WhenPreservingBeforeTime()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            
            Assert.ThrowsException<Exception>(() 
                => _dutWithOneReqNotNeedInput.BulkPreserve(_utcNow, new Mock<Person>().Object));
        }

        [TestMethod]
        public void BulkPreserve_ShouldPreserve_WhenPreservingOnDue()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            Assert.AreEqual(1, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
            
            _dutWithOneReqNotNeedInput.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
        }

        [TestMethod]
        public void BulkPreserve_ShouldPreserve_WhenPreservingOverDue()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            Assert.AreEqual(1, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
            
            _dutWithOneReqNotNeedInput.BulkPreserve(_dueTimeForThreeWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
        }
        
        [TestMethod]
        public void BulkPreserve_ShouldPreserveOnce_WhenPreservingMultipleTimesAtSameTime()
        {
            _dutWithOneReqNotNeedInput.StartPreservation(_utcNow);
            Assert.AreEqual(1, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);
            
            _dutWithOneReqNotNeedInput.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, _dutWithOneReqNotNeedInput.FirstUpcomingRequirement.PreservationPeriods.Count);

            Assert.ThrowsException<Exception>(() 
                => _dutWithOneReqNotNeedInput.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object));
        }
        
        [TestMethod]
        public void BulkPreserve_ShouldPreserveDueRequirementsOnly()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);

            var req1 = dut.Requirements.ElementAt(0);
            var req2 = dut.Requirements.ElementAt(1);
            Assert.AreEqual(1, req1.PreservationPeriods.Count);
            Assert.AreEqual(1, req2.PreservationPeriods.Count);

            dut.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);
            Assert.AreEqual(2, req1.PreservationPeriods.Count);
            Assert.AreEqual(1, req2.PreservationPeriods.Count);
        }
        
        [TestMethod]
        public void BulkPreserve_ShouldChangeUpComingRequirement()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);
            Assert.AreEqual(_reqNotNeedInputTwoWeekInterval, dut.FirstUpcomingRequirement);

            dut.BulkPreserve(_dueTimeForTwoWeeksInterval, new Mock<Person>().Object);

            Assert.AreEqual(_reqNeedInputThreeWeekInterval, dut.FirstUpcomingRequirement);
        }
        
        [TestMethod]
        public void UpComingRequirements_ShouldReturnNoneRequirements_BeforePreservationStarted()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);

            Assert.AreEqual(0, dut.UpComingRequirements().Count());

        }
        
        [TestMethod]
        public void UpComingRequirements_ShouldReturnAllRequirements_AfterPreservationStarted()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);

            Assert.AreEqual(dut.Requirements.Count, dut.UpComingRequirements().Count());
        }
        
        [TestMethod]
        public void UpComingRequirements_ShouldNotReturnVoidedRequirements()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);
            dut.Requirements.ElementAt(0).Void();

            Assert.AreEqual(dut.Requirements.Count-1, dut.UpComingRequirements().Count());
        }
        
        [TestMethod]
        public void OrderedRequirements_ShouldReturnAllRequirements_BeforeAndAfterPreservationStarted()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);

            Assert.AreEqual(dut.Requirements.Count, dut.OrderedRequirements().Count());
        
            dut.StartPreservation(_utcNow);
            
            Assert.AreEqual(dut.Requirements.Count, dut.OrderedRequirements().Count());
        }
        
        [TestMethod]
        public void OrderedRequirements_ShouldNotReturnVoidedRequirements()
        {
            var dut = new Tag("", "", "", "", "", "", "", "", "", "", "", _stepMock.Object, _twoReqs_FirstNotNeedInputTwoWeekInterval_SecondNeedInputThreeWeekInterval);
            dut.StartPreservation(_utcNow);
            dut.Requirements.ElementAt(0).Void();

            Assert.AreEqual(dut.Requirements.Count-1, dut.OrderedRequirements().Count());
        }
    }
}

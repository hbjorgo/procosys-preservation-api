﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Equinor.Procosys.Preservation.Command.RequirementCommands.RecordValues;
using Equinor.Procosys.Preservation.Command.Validators.FieldValidators;
using Equinor.Procosys.Preservation.Command.Validators.ProjectValidators;
using Equinor.Procosys.Preservation.Command.Validators.TagValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Command.Tests.RequirementCommands.RecordValues
{
    [TestClass]
    public class RecordValuesCommandValidatorTests
    {
        private const string Comment = "Comment";
        private const int TagId = 1;
        private const int NumberFieldId = 11;
        private const int CheckBoxFieldId = 14;
        private const int ReqId = 21;

        private RecordValuesCommandValidator _dut;
        private Mock<IProjectValidator> _projectValidatorMock;
        private Mock<ITagValidator> _tagValidatorMock;
        private Mock<IFieldValidator> _fieldValidatorMock;
        private RecordValuesCommand _recordValuesCommandWithCommentOnly;
        private RecordValuesCommand _recordValuesCommand;

        [TestInitialize]
        public void Setup_OkState()
        {
            _projectValidatorMock = new Mock<IProjectValidator>();
            _tagValidatorMock = new Mock<ITagValidator>();
            _tagValidatorMock.Setup(v => v.ExistsAsync(TagId, default)).Returns(Task.FromResult(true));
            _tagValidatorMock.Setup(v => v.HasRequirementWithActivePeriodAsync(TagId, ReqId, default)).Returns(Task.FromResult(true));
            _fieldValidatorMock = new Mock<IFieldValidator>();
            _fieldValidatorMock.Setup(v => v.ExistsAsync(NumberFieldId, default)).Returns(Task.FromResult(true));
            _fieldValidatorMock.Setup(r => r.IsValidForRecordingAsync(NumberFieldId, default)).Returns(Task.FromResult(true));
            _fieldValidatorMock.Setup(v => v.ExistsAsync(CheckBoxFieldId, default)).Returns(Task.FromResult(true));
            _fieldValidatorMock.Setup(r => r.IsValidForRecordingAsync(CheckBoxFieldId, default)).Returns(Task.FromResult(true));

            _recordValuesCommandWithCommentOnly = new RecordValuesCommand(
                TagId,
                ReqId, 
                null, 
                null,
                Comment);

            _recordValuesCommand = new RecordValuesCommand(
                TagId, 
                ReqId, 
                new EditableList<NumberFieldValue>{new NumberFieldValue(NumberFieldId, 1282.91, false)}, 
                new List<CheckBoxFieldValue>{new CheckBoxFieldValue(CheckBoxFieldId, true)}, 
                Comment);

            _dut = new RecordValuesCommandValidator(_projectValidatorMock.Object, _tagValidatorMock.Object, _fieldValidatorMock.Object);
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenCommentOnly()
        {
            var result = _dut.Validate(_recordValuesCommandWithCommentOnly);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenTagNotExists()
        {
            _tagValidatorMock.Setup(v => v.ExistsAsync(TagId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommandWithCommentOnly);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag doesn't exist!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenTagIsVoided()
        {
            _tagValidatorMock.Setup(r => r.IsVoidedAsync(TagId, default)).Returns(Task.FromResult(true));
            
            var result = _dut.Validate(_recordValuesCommandWithCommentOnly);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag is voided!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenProjectForTagIsClosed()
        {
            _projectValidatorMock.Setup(r => r.IsClosedForTagAsync(TagId, default)).Returns(Task.FromResult(true));
            
            var result = _dut.Validate(_recordValuesCommandWithCommentOnly);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project for tag is closed!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenRequirementDontHaveActivePeriod()
        {
            _tagValidatorMock.Setup(v => v.HasRequirementWithActivePeriodAsync(TagId, ReqId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommandWithCommentOnly);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag doesn't have this requirement with active period!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenNumberFieldNotExists()
        {
            _fieldValidatorMock.Setup(r => r.ExistsAsync(NumberFieldId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Field doesn't exists!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenNumberFieldNotForRecording()
        {
            _fieldValidatorMock.Setup(r => r.IsValidForRecordingAsync(NumberFieldId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Field values can not be recorded for field type!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCheckBoxFieldNotExists()
        {
            _fieldValidatorMock.Setup(r => r.ExistsAsync(CheckBoxFieldId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Field doesn't exists!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCheckBoxFieldNotForRecording()
        {
            _fieldValidatorMock.Setup(r => r.IsValidForRecordingAsync(CheckBoxFieldId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Field values can not be recorded for field type!"));
        }

        [TestMethod]
        public void Validate_ShouldFailWith1Error_WhenMultipleErrorsInSameRule()
        {
            _projectValidatorMock.Setup(r => r.IsClosedForTagAsync(TagId, default)).Returns(Task.FromResult(true));
            _tagValidatorMock.Setup(v => v.HasRequirementWithActivePeriodAsync(TagId, ReqId, default)).Returns(Task.FromResult(false));
            
            var result = _dut.Validate(_recordValuesCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project for tag is closed!"));
        }
 
        [TestMethod]
        public void Validate_ShouldFailWith3Errors_WhenErrorsInDifferentRules()
        {
            _projectValidatorMock.Setup(r => r.IsClosedForTagAsync(TagId, default)).Returns(Task.FromResult(true));
            _fieldValidatorMock.Setup(v => v.ExistsAsync(NumberFieldId, default)).Returns(Task.FromResult(false));
            _fieldValidatorMock.Setup(v => v.ExistsAsync(CheckBoxFieldId, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_recordValuesCommand);
            
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(3, result.Errors.Count);
        }
    }
}

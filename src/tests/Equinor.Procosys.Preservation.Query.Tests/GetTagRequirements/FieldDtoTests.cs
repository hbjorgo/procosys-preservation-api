﻿using Equinor.Procosys.Preservation.Domain.AggregateModels.ProjectAggregate;
using Equinor.Procosys.Preservation.Domain.AggregateModels.RequirementTypeAggregate;
using Equinor.Procosys.Preservation.Query.GetTagRequirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.Procosys.Preservation.Query.Tests.GetTagRequirements
{
    [TestClass]
    public class FieldDtoTests
    {
        static double _number = 1282.91;

        [TestMethod]
        public void Constructor_ShouldSetProperties_ForNumberWithoutCurrent()
        {
            var fieldMock = new Mock<Field>("", "Label", FieldType.Number, 0, "mm", true);
            fieldMock.SetupGet(f => f.Id).Returns(12);

            var dut = new FieldDto(fieldMock.Object, null, null);

            Assert.AreEqual(12, dut.Id);
            Assert.AreEqual(FieldType.Number, dut.FieldType);
            Assert.AreEqual("Label", dut.Label);
            Assert.AreEqual("mm", dut.Unit);
            Assert.IsTrue(dut.ShowPrevious);
            Assert.IsNull(dut.CurrentValue);
            Assert.IsNull(dut.PreviousValue);
        }

        [TestMethod]
        public void Constructor_ShouldSetProperties_ForNumberWithCurrentNotShowPrevious()
        {
            var field = new Field("", "", FieldType.Number, 0, "mm", false);

            var dut = new FieldDto(
                field, 
                new NumberValue("", field, _number), 
                new NumberValue("", field, _number));

            Assert.IsFalse(dut.ShowPrevious);
            Assert.IsNotNull(dut.CurrentValue);
            Assert.IsNull(dut.PreviousValue);
            AssertNumberDto(dut.CurrentValue);
        }

        [TestMethod]
        public void Constructor_ShouldSetProperties_ForNumberWithCurrentShowPrevious()
        {
            var field = new Field("", "", FieldType.Number, 0, "mm", true);

            var dut = new FieldDto(
                field, 
                new NumberValue("", field, _number), 
                new NumberValue("", field, _number));

            Assert.IsTrue(dut.ShowPrevious);
            Assert.IsNotNull(dut.CurrentValue);
            Assert.IsNotNull(dut.PreviousValue);
            AssertNumberDto(dut.CurrentValue);
            AssertNumberDto(dut.PreviousValue);
        }

        private void AssertNumberDto(object dto)
        {
            Assert.IsInstanceOfType(dto, typeof(NumberDto));

            var numberDto = dto as NumberDto;
            Assert.IsNotNull(numberDto);
            Assert.IsNotNull(numberDto);
            Assert.IsFalse(numberDto.IsNA);
            Assert.IsFalse(numberDto.IsNA);
            Assert.IsTrue(numberDto.Value.HasValue);
            Assert.AreEqual(_number, numberDto.Value.Value);

        }
    }
}

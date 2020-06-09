﻿using System;
using Equinor.Procosys.Preservation.Command.JourneyCommands.CreateStep;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.Procosys.Preservation.Command.Tests.JourneyCommands.CreateStep
{
    [TestClass]
    public class CreateStepCommandTests
    {
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            var dut = new CreateStepCommand(1, "S", 2, "B", Guid.Empty);

            Assert.AreEqual(1, dut.JourneyId);
            Assert.AreEqual(2, dut.ModeId);
            Assert.AreEqual("S", dut.Title);
            Assert.AreEqual("B", dut.ResponsibleCode);
            Assert.AreEqual(Guid.Empty, dut.CurrentUserOid);
        }
    }
}

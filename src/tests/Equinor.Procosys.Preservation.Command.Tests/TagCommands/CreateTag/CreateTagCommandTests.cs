﻿using System.Collections.Generic;
using Equinor.Procosys.Preservation.Command.TagCommands.CreateTag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.Procosys.Preservation.Command.Tests.TagCommands.CreateTag
{
    [TestClass]
    public class CreateTagCommandTests
    {
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            var dut = new CreateTagCommand("TagNoA", "ProjectNameA", 2, new List<Requirement>());

            Assert.AreEqual("TagNoA", dut.TagNo);
            Assert.AreEqual("ProjectNameA", dut.ProjectName);
            Assert.AreEqual(2, dut.StepId);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.Procosys.Preservation.Command.TagCommands.BulkPreserve;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.Procosys.Preservation.Command.Tests.TagCommands.BulkPreserve
{
    [TestClass]
    public class BulkPreserveCommandTests
    {
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            var dut = new BulkPreserveCommand(new List<int>{17}, Guid.Empty);

            Assert.AreEqual(1, dut.TagIds.Count());
            Assert.AreEqual(17, dut.TagIds.First());
            Assert.AreEqual(Guid.Empty, dut.CurrentUserOid);
        }
    }
}

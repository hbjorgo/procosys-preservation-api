﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.Procosys.Preservation.Command.TagCommands.CompletePreservation;
using Equinor.Procosys.Preservation.Command.TagCommands.Transfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.Procosys.Preservation.Command.Tests.TagCommands.CompletePreservation
{
    [TestClass]
    public class CompletePreservationCommandTests
    {
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            var idAndRowVersion = new IdAndRowVersion(17, "AAAAAAAAABA=");
            var dut = new CompletePreservationCommand(new List<IdAndRowVersion>{idAndRowVersion}, Guid.Empty);

            Assert.AreEqual(1, dut.Tags.Count());
            Assert.AreEqual(idAndRowVersion, dut.Tags.First());
            Assert.AreEqual(Guid.Empty, dut.CurrentUserOid);
        }
    }
}

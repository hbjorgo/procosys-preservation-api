﻿using Equinor.Procosys.Preservation.Query.GetUniqueTagFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.Procosys.Preservation.Query.Tests.GetUniqueTagFunctions
{
    [TestClass]
    public class GetUniqueTagFunctionsQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new GetUniqueTagFunctionsQuery("PX");

            Assert.AreEqual("PX", dut.ProjectName);
        }
    }
}

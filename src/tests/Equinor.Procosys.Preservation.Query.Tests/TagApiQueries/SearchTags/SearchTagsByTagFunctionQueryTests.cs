﻿using Equinor.Procosys.Preservation.Query.TagApiQueries.SearchTags;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.Procosys.Preservation.Query.Tests.TagApiQueries.SearchTags
{
    [TestClass]
    public class SearchTagsByTagFunctionQueryTests
    {
        [TestMethod]
        public void Constructor_SetsProperties()
        {
            var dut = new SearchTagsByTagFunctionQuery("ProjectName");

            Assert.AreEqual("ProjectName", dut.ProjectName);
        }
    }
}

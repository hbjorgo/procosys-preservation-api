﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.Procosys.Preservation.Infrastructure;
using Equinor.Procosys.Preservation.MainApi.Tag;
using Equinor.Procosys.Preservation.Query.TagApiQueries.SearchTags;
using Equinor.Procosys.Preservation.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceResult;

namespace Equinor.Procosys.Preservation.Query.Tests.TagApiQueries.SearchTags
{
    [TestClass]
    public class SearchTagsByTagNoQueryHandlerTests : ReadOnlyTestsBase
    {
        private Mock<ITagApiService> _tagApiServiceMock;
        private IList<ProcosysTagOverview> _apiTags;
        private SearchTagsByTagNoQuery _query;
        private TestDataSet _testDataSet;

        protected override void SetupNewDatabase(DbContextOptions<PreservationContext> dbContextOptions)
        {
            using (var context = new PreservationContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _testDataSet = AddTestDataSet(context);

                _tagApiServiceMock = new Mock<ITagApiService>();
                _apiTags = new List<ProcosysTagOverview>
                {
                    new ProcosysTagOverview
                    {
                        TagNo = _testDataSet.Project1.Tags.First().TagNo,
                        CommPkgNo = "CommPkgNo1",
                        Description = "Desc1",
                        Id = 1,
                        McPkgNo = "McPkgNo1",
                        PurchaseOrderNo = "PoNo1",
                        TagFunctionCode = "TFC1",
                        RegisterCode = "RC1",
                        MccrResponsibleCodes = "R1"
                    },
                    new ProcosysTagOverview
                    {
                        TagNo = "TagNo2",
                        CommPkgNo = "CommPkgNo2",
                        Description = "Desc2",
                        Id = 2,
                        McPkgNo = "McPkgNo2",
                        PurchaseOrderNo = "PoNo2",
                        TagFunctionCode = "TFC2",
                        RegisterCode = "RC2",
                        MccrResponsibleCodes = "R2"
                    },
                    new ProcosysTagOverview
                    {
                        TagNo = "TagNo3",
                        CommPkgNo = "CommPkgNo3",
                        Description = "Desc3",
                        Id = 3,
                        McPkgNo = "McPkgNo3",
                        PurchaseOrderNo = "PoNo3",
                        TagFunctionCode = "TFC3",
                        RegisterCode = "RC3",
                        MccrResponsibleCodes = "R3"
                    }
                };
                _tagApiServiceMock
                    .Setup(x => x.SearchTagsByTagNoAsync(TestPlant, _testDataSet.Project1.Name, _testDataSet.SiteTagPrefix))
                    .Returns(Task.FromResult(_apiTags));
                _query = new SearchTagsByTagNoQuery(_testDataSet.Project1.Name, _testDataSet.SiteTagPrefix);
            }
        }

        [TestMethod]
        public async Task Handle_ReturnsOkResult()
        {
            using (var context = new PreservationContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new SearchTagsByTagNoQueryHandler(context, _tagApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
            }
        }

        [TestMethod]
        public async Task Handle_ReturnsCorrectItems()
        {
            using (var context = new PreservationContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new SearchTagsByTagNoQueryHandler(context, _tagApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.AreEqual(3, result.Data.Count);
                var item1 = result.Data.ElementAt(0);
                var item2 = result.Data.ElementAt(1);
                var item3 = result.Data.ElementAt(2);
                AssertTagData(_apiTags.Single(t => t.TagNo == item1.TagNo), item1);
                AssertTagData(_apiTags.Single(t => t.TagNo == item2.TagNo), item2);
                AssertTagData(_apiTags.Single(t => t.TagNo == item3.TagNo), item3);
            }
        }

        [TestMethod]
        public async Task Handle_SetsCorrectIsPreservedStatus()
        {
            using (var context = new PreservationContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new SearchTagsByTagNoQueryHandler(context, _tagApiServiceMock.Object, _plantProvider);
                var result = await dut.Handle(_query, default);

                Assert.IsTrue(result.Data[0].IsPreserved);
                Assert.IsFalse(result.Data[1].IsPreserved);
                Assert.IsFalse(result.Data[2].IsPreserved);
            }
        }

        [TestMethod]
        public async Task Handle_ReturnsEmptyList_WhenTagApiReturnsNull()
        {
            using (var context = new PreservationContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new SearchTagsByTagNoQueryHandler(context, _tagApiServiceMock.Object, _plantProvider);
                _tagApiServiceMock
                    .Setup(x => x.SearchTagsByTagNoAsync(TestPlant, _testDataSet.Project1.Name, _testDataSet.SiteTagPrefix))
                    .Returns(Task.FromResult<IList<ProcosysTagOverview>>(null));

                var result = await dut.Handle(_query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(0, result.Data.Count);
            }
        }

        [TestMethod]
        public async Task Handle_ReturnsApiTags_WhenProjectRepositoryReturnsNull()
        {
            using (var context = new PreservationContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _tagApiServiceMock
                    .Setup(x => x.SearchTagsByTagNoAsync(TestPlant, "Project XYZ", "TagNo"))
                    .Returns(Task.FromResult(_apiTags));

                var dut = new SearchTagsByTagNoQueryHandler(context, _tagApiServiceMock.Object, _plantProvider);

                var query = new SearchTagsByTagNoQuery("Project XYZ", "TagNo");
                var result = await dut.Handle(query, default);

                Assert.AreEqual(ResultType.Ok, result.ResultType);
                Assert.AreEqual(3, result.Data.Count);
            }
        }

        private void AssertTagData(ProcosysTagOverview tagOverview, ProcosysTagDto tagDto)
        {
            Assert.AreEqual(tagOverview.TagNo, tagDto.TagNo);
            Assert.AreEqual(tagOverview.RegisterCode, tagDto.RegisterCode);
            Assert.AreEqual(tagOverview.TagFunctionCode, tagDto.TagFunctionCode);
            Assert.AreEqual(tagOverview.CommPkgNo, tagDto.CommPkgNo);
            Assert.AreEqual(tagOverview.Description, tagDto.Description);
            Assert.AreEqual(tagOverview.McPkgNo, tagDto.McPkgNo);
            Assert.AreEqual(tagOverview.PurchaseOrderNo, tagDto.PurchaseOrderNumber);
            Assert.AreEqual(tagOverview.MccrResponsibleCodes, tagDto.MccrResponsibleCodes);
        }
    }
}

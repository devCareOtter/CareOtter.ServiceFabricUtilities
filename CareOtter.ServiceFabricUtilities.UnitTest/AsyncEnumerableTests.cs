using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CareOtter.ServiceFabricUtilities.AsyncEnumerable.Extensions;
using CareOtter.ServiceFabricUtilities.UnitTest.Mocks;
using Microsoft.ServiceFabric.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CareOtter.ServiceFabricUtilities.UnitTest
{
    [TestClass]
    public class AsyncEnumerableTests
    {
        Mock<ITransaction> _txMock = new Mock<ITransaction>();

        [TestMethod]
        public async Task AsyncEnumerable_ToList_SequenceEqual()
        {
            //arrange
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumerable = new MockAsyncEnumerable<int>(originList);

            //act
            var cpy = await enumerable.ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(originList.SequenceEqual(cpy));
        }

        [TestMethod]
        public async Task AsyncSelect_SelectEnumerator_ValidOutput()
        {
            //arrange
            var originList = new List<int>() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var enumerable = new MockAsyncEnumerable<int>(originList);

            //act
            var res = enumerable.GetAsyncEnumerator().Select(x => x%2);

            //assert
            Assert.IsTrue(await res.AllAsync(x=>x >= 0 && x<2));
        }

        [TestMethod]
        public async Task AsyncAll_AllTrue_TrueReturned()
        {
            //arrange
            var originList = new List<bool>() {true, true, true, true, true};
            var enumerable = new MockAsyncEnumerable<bool>(originList);

            //act
            var res = await enumerable.AllAsync(x => x);

            //assert
            Assert.IsTrue(res);
        }

        [TestMethod]
        public async Task AsyncAll_FirstFalse_FalseReturned_OnlyOneCallToMoveNext()
        {

            //arrange
            var accessCounter = 0;
            var originList = new List<bool>() { false, true, true, true, true };
            var enumerable = new MockAsyncEnumerable<bool>(originList,()=>++accessCounter);

            //act
            var res = await enumerable.AllAsync(x => x);

            //assert
            Assert.IsFalse(res);
            Assert.AreEqual(1,accessCounter);
        }

        [TestMethod]
        public async Task AsyncWhere_OnlyMod2_GoodOutput()
        {
            //arrange
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumerable = new MockAsyncEnumerable<int>(originList);
            
            //act
            var res = enumerable.Where(x => x%2 == 0);

            //assert
            Assert.IsTrue(await res.AllAsync(x=>x%2==0));
        }

        [TestMethod]
        public async Task AsyncSelectMany_EmptyLists_EmptyResult()
        {
            //arrange
            var originList = new List<List<int>>();
            for(int i = 0; i < 100;++i)
                originList.Add(new List<int>());
            var enumerable = new MockAsyncEnumerable<List<int>>(originList);

            //act
            var res = enumerable.SelectMany<List<int>, List<int>, int>(x => x);

            //assert
            var resList = await res.ToListAsync(_txMock.Object);
            Assert.AreEqual(0,resList.Count);
        }

        [TestMethod]
        public async Task AsyncSelectMany_HundredElementsEach_GoodResults()
        {
            //arrange
            var originList = new List<List<int>>();
            for (int i = 0; i < 100; ++i)
            {
                var newList = new List<int>();
                for (int z = 0; z < 100; ++z)
                {
                    newList.Add(z);
                }
                originList.Add(newList);
            }
            var enumerable = new MockAsyncEnumerable<List<int>>(originList);

            //act
            var res = enumerable.SelectMany<List<int>, List<int>, int>(x => x);

            //assert
            var resList = await res.ToListAsync(_txMock.Object);
            Assert.AreEqual(100*100,resList.Count);
        }

        [TestMethod]
        public async Task AsyncAny_NoElements_ReturnFalse()
        {
            //arrange
            var originList = new List<int>();
            var enumer = new MockAsyncEnumerable<int>(originList);

            //act
            var res = await enumer.AnyAsync();

            //assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        public async Task AsyncAny_NoMatchingElements_ReturnFalse()
        {
            //arrange
            var originList = new List<int>() {3,5,7};
            var enumer = new MockAsyncEnumerable<int>(originList);

            //act
            var res = await enumer.AnyAsync(x=>x%2==0); //no even elements

            //assert
            Assert.IsFalse(res);
        }

        [TestMethod]
        public async Task AsyncAny_Elements_ReturnTrue()
        {
            //arrange
            var originList = new List<int>() {1,2,3};
            var enumer = new MockAsyncEnumerable<int>(originList);

            //act
            var res = await enumer.AnyAsync();

            //assert
            Assert.IsTrue(res);
        }


        [TestMethod]
        public async Task AsyncAny_ElementsMatching_ReturnTrue_RequireFewAccesses()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 1, 2, 3 };
            var enumer = new MockAsyncEnumerable<int>(originList,()=>++ct);

            //act
            var res = await enumer.AnyAsync(x=>x%2==0);

            //assert
            Assert.IsTrue(res);
            Assert.AreEqual(2,ct);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AsyncFirst_NoElements_Exception()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() ;
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.FirstAsync();

            //assert
            
        }

        [TestMethod]
        public async Task AsyncFirst_TenElements_ValidOuput()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() {0,1,2,3,4,5,6,7,8,9};
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.FirstAsync();

            //assert
            Assert.AreEqual(0,res);
            Assert.AreEqual(1,ct);
        }


        [TestMethod]
        public async Task AsyncFirst_TenElements_Lambda_ValidOuput()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.FirstAsync(x=>x>0 && x%2==0);

            //assert
            Assert.AreEqual(2, res);
            Assert.AreEqual(3, ct);
        }

        [TestMethod]
        public async Task AsyncFirstOrDefault_TenElements_ValidOuput()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.FirstOrDefaultAsync();

            //assert
            Assert.AreEqual(0, res);
            Assert.AreEqual(1, ct);
        }

        [TestMethod]
        public async Task AsyncFirstOrDefault_TenElements_Lambda_ValidOuput()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.FirstOrDefaultAsync(x => x > 0 && x % 2 == 0);

            //assert
            Assert.AreEqual(2, res);
            Assert.AreEqual(3, ct);
        }


        [TestMethod]
        public async Task AsyncFirstOrDefault_NoElements_Default()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>();
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.FirstOrDefaultAsync();

            //assert
            Assert.AreEqual(default(int),res);
        }

        [TestMethod]
        public async Task AsyncSkip_SkipOne_SequenceEqual()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.Skip(1).ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(res.SequenceEqual(originList.Skip(1)));
        }


        [TestMethod]
        public async Task AsyncSkip_SkipFive_SequenceEqual()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.Skip(5).ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(res.SequenceEqual(originList.Skip(5)));
        }


        [TestMethod]
        public async Task AsyncSkip_SkipMany_EmptyCollection()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.Skip(100).ToListAsync(_txMock.Object);

            //assert
            Assert.AreEqual(0,res.Count);
        }


        [TestMethod]
        public async Task AsyncSkipWhile_SkipLessThanFive_SequenceEqual()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.SkipWhile(x=>x<5).ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(res.SequenceEqual(originList.SkipWhile(x=>x<5)));
        }


        [TestMethod]
        public async Task AsyncSkipWhile_SkipAll_EmptyCollection()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.SkipWhile(x=>true).ToListAsync(_txMock.Object);

            //assert
            Assert.AreEqual(0,res.Count);
        }


        [TestMethod]
        public async Task AsyncTakeWhile_TakeNone_EmptyCollection()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.TakeWhile(x => false).ToListAsync(_txMock.Object);

            //assert
            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public async Task AsyncTakeWhile_TakeAll_EmptyCollection()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.TakeWhile(x => true).ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(res.SequenceEqual(originList));
        }


        [TestMethod]
        public async Task AsyncTakeWhile_TakeLessThanFive_SequenceEqual()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.TakeWhile(x => x < 5).ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(res.SequenceEqual(originList.TakeWhile(x=>x<5)));
        }


        [TestMethod]
        public async Task AsyncTake_TakeFive_SequenceEqual()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.Take(5).ToListAsync(_txMock.Object);

            //assert
            Assert.IsTrue(res.SequenceEqual(originList.Take(5)));
        }

        [TestMethod]
        public async Task AsyncTake_None_EmptyCollection()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.Take(0).ToListAsync(_txMock.Object);

            //assert
            Assert.AreEqual(0,res.Count);
        }

        [TestMethod]
        public async Task AsyncTake_TakeMany_CountOfTen()
        {
            //arrange
            var ct = 0;
            var originList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var enumer = new MockAsyncEnumerable<int>(originList, () => ++ct);

            //act
            var res = await enumer.Take(100).ToListAsync(_txMock.Object);

            //assert
            Assert.AreEqual(10,res.Count);
        }
    }
}
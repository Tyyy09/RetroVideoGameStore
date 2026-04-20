using Microsoft.AspNetCore.Mvc;
using RetroVideoGameStore.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroVideoGameStoreTest
{
    [TestClass]
    public class ExampleControllerTest
    {
        [TestMethod]
        public void IndexReturnSomething()
        {
            var controller = new ExampleController();

            var result = controller.Index();

            Assert.IsNotNull(result);

        }
        [TestMethod]
        public void IndexViewIsNotNull()
        {
            var controller = new ExampleController();

            var result = (ViewResult)controller.Index();

            Assert.AreEqual("Index", result.ViewName);
        }
    }
}

using ElGato_API.Controllers;
using ElGato_API.Interfaces;
using ElGato_API.VM;
using ElGato_API.VMO.UserAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElGato_API_Tests.Account
{
    public class LoginTests
    {
        private Mock<IAccountService> _accountServiceMock;
        private Mock<IDietService> _dietServiceMock;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _dietServiceMock = new Mock<IDietService>();
            _controller = new AccountController(_accountServiceMock.Object, _dietServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Login_ReturnsOk_WhenLoginSucceeds()
        {
            var loginVM = new LoginVM { Email = "test", Password = "123!" };
            var loginResponse = new LoginVMO
            {
                IdentityResult = IdentityResult.Success,
                JwtToken = "valid-jwt-token"
            };

            _accountServiceMock.Setup(s => s.LoginUser(loginVM)).ReturnsAsync(loginResponse);

            var res = await _controller.Login(loginVM) as OkObjectResult;

            Assert.That(res, Is.Not.Null);
            Assert.That(res.StatusCode, Is.EqualTo(200));

            var tokenProp = res.Value.GetType().GetProperty("token");
            var actualToken = tokenProp?.GetValue(res.Value, null);

            Assert.That(actualToken, Is.EqualTo(loginResponse.JwtToken));
        }



        [Test]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var loginVM = new LoginVM { Email = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Required");

            var res = await _controller.Login(loginVM) as ObjectResult;

            Assert.That(res, Is.Not.Null);
            Assert.That(res.StatusCode, Is.EqualTo(400));
            Assert.That(res.Value, Is.EqualTo("Invalid form send"));
        }

        [Test]
        public async Task Login_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            var loginVM = new LoginVM { Email = "test", Password = "123!" };

            _accountServiceMock.Setup(s => s.LoginUser(loginVM)).ThrowsAsync(new Exception("An error occurred"));

            var res = await _controller.Login(loginVM) as ObjectResult;

            Assert.That(res, Is.Not.Null);
            Assert.That(res.StatusCode, Is.EqualTo(500));
            Assert.That(res.Value, Is.EqualTo("An error occurred"));
        }


    }

}


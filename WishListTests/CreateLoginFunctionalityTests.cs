﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WishList.Models;
using Xunit;

namespace WishListTests
{
    public class CreateLoginFunctionalityTests
    {
        [Fact(DisplayName = "Create Login Model @create-login-model")]
        public void CreateLoginModel()
        {
            var filePath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "WishList" + Path.DirectorySeparatorChar + "Models" + Path.DirectorySeparatorChar + "AccountViewModels" + Path.DirectorySeparatorChar + "LoginViewModel.cs";
            Assert.True(File.Exists(filePath), @"`LoginViewModel.cs` was not found in the `Models\AccountViewModels` folder.");

            var loginViewModel = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                  from type in assembly.GetTypes()
                                  where type.FullName == "WishList.Models.AccountViewModels.LoginViewModel"
                                  select type).FirstOrDefault();
            Assert.True(loginViewModel != null, "A `public` class `LoginViewModel` was not found in the `WishList.Models.AccountViewModels` namespace.");

            var emailProperty = loginViewModel.GetProperty("Email");
            Assert.True(emailProperty != null, "`LoginViewModel` does not appear to contain a `public` `string` property `Email`.");
            Assert.True(emailProperty.PropertyType == typeof(string), "`LoginViewModel` has a property `Email` but it is not of type `string`.");
            Assert.True(Attribute.IsDefined(emailProperty, typeof(RequiredAttribute)), "`LoginViewModel` has a property `Email` but it doesn't appear to have a `Required` attribute.");
            Assert.True(emailProperty.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(EmailAddressAttribute)) != null, "`LoginViewModel` has a property `Email` but it doesn't appear to have an `EmailAddress` attribute.");

            var passwordProperty = loginViewModel.GetProperty("Password");
            Assert.True(passwordProperty != null, "`LoginViewModel` does not appear to contain a `public` `string` property `Password`.");
            Assert.True(passwordProperty.PropertyType == typeof(string), "`LoginViewModel` has a property `Password` but it is not of type `string`.");
            Assert.True(passwordProperty.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(RequiredAttribute)) != null, "`LoginViewModel` has a property `Password` but it doesn't appear to have a `Required` attribute.");
            Assert.True(passwordProperty.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(StringLengthAttribute)) != null, "`LoginViewModel` has a property `Password` but it doesn't appear to have a `StringLength` attribute of 100, with a minimum length of 8");
            // need to verify string length's max and minlength
            Assert.True(passwordProperty.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(DataTypeAttribute)) != null, "`LoginViewModel` has a property `Password` but it doesn't appear to have a `DataType` attribute set to `Password`.");
            // need to verify datatype is set to password
        }

        [Fact(DisplayName = "Create Login View @create-login-view")]
        public void CreateLoginView()
        {
            var filePath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "WishList" + Path.DirectorySeparatorChar + "Views" + Path.DirectorySeparatorChar + "Account" + Path.DirectorySeparatorChar + "Login.cshtml";
            Assert.True(File.Exists(filePath), @"`Login.cshtml` was not found in the `Views\Account` folder.");
            // need to verify contents were copied correctly
        }

        [Fact(DisplayName = "Create Login Action @create-login-action")]
        public async void CreateHttpPostLoginActionTest()
        {
            var filePath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "WishList" + Path.DirectorySeparatorChar + "Controllers" + Path.DirectorySeparatorChar + "AccountController.cs";
            Assert.True(File.Exists(filePath), @"`AccountController.cs` was not found in the `Controllers` folder.");

            var accountController = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     from type in assembly.GetTypes()
                                     where type.FullName == "WishList.Controllers.AccountController"
                                     select type).FirstOrDefault();
            Assert.True(accountController != null, "A `public` class `AccountController` was not found in the `WishList.Controllers` namespace.");

            var loginViewModel = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     from type in assembly.GetTypes()
                                     where type.FullName == "WishList.Models.AccountViewModels.LoginViewModel"
                                     select type).FirstOrDefault();
            Assert.True(loginViewModel != null, "A `public` class `LoginViewModel` was not found in the `WishList.Models.AccountViewModels` namespace.");

            var method = accountController.GetMethod("Login", new Type[] { loginViewModel });
            Assert.True(method != null, "`AccountController` did not contain a `Login` method with a parameter of type `LoginViewModel`.");
            Assert.True(method.ReturnType == typeof(IActionResult), "`AccountController`'s Post `Login` method did not have a return type of `IActionResult`");
            Assert.True(method.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(HttpPostAttribute)) != null, "`AccountController``s `Login` method did not have the `HttpPost` attribute.");
            Assert.True(method.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(AllowAnonymousAttribute)) != null, "`AccountController`'s `Login` method did not have the `AllowAnonymous` attribute.");
            Assert.True(method.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(ValidateAntiForgeryTokenAttribute)) != null, "`AccountController`'s `Login` method did not have the `ValidateAntiForgeryToken` attribute.");
            // Note: Attribute AsyncStateMachine can be used to test for the presence of the `async` keyword as it should only exist on methods with the `async` keyword
            Assert.True(method.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(AsyncStateMachineAttribute)) != null, "`AccountController`'s `Login` method did not have the keyword `async` in it's signature.");
            Assert.True(method.ReturnType == typeof(Task<IActionResult>), "`AccountController`'s `Login` method did not have a return type of `Task<IActionResult>`.");

            var userManeger = new UserManager<ApplicationUser>(null, null, null, null, null, null, null, null, null);
            var signInManager = new SignInManager<ApplicationUser>(null, null, null, null, null, null);
            var controller = Activator.CreateInstance(accountController, new object[] { userManeger, signInManager });
            var model = Activator.CreateInstance(loginViewModel, null);
            loginViewModel.GetProperty("Email").SetValue(model, "Valid@Email.com");
            loginViewModel.GetProperty("Password").SetValue(model, "bad_password");

            var badResults = (ViewResult)method.Invoke(controller, new object[] { model });
            Assert.True(badResults.ViewName == "Login", "`AccountController`'s `Login` method did not return the `Login` view when the login failed.");
            Assert.True(badResults.Model == model, "`AccountController`'s `Login` method did not provide the invalid model when returning the `Login` view when login failed.");

            loginViewModel.GetProperty("Password").SetValue(model, "!4oOauidT_5");
            var goodResults = await (dynamic)method.Invoke(controller, new object[] { model });
            Assert.True(goodResults.ControllerName == "Home" && goodResults.ActionName == "Index", "`AccountController`'s `Login` method did not return a `RedirectToAction` to the `Home.Index` action when login was successful.");
        }
    }
}
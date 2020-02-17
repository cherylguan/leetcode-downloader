using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace LeetCode
{
    public static class DriverExtensions
    {
        public static IWebElement GetElement(
            this ISearchContext driver,
            string tagName,
            string attribute,
            string value)
        {
            return driver.GetElement(tagName, attribute, value, TimeSpan.FromSeconds(15));
        }

        public static IWebElement GetElement(
            this ISearchContext driver,
            string tagName,
            string attribute,
            string value,
            TimeSpan timeSpan)
        {
            return driver.GetElement($"//{tagName}[contains(@{attribute}, '{value}')]", timeSpan);
        }

        public static T Retry<T>(
            Func<T> func,
            string context,
            TimeSpan timeSpan)
        {
            var start = DateTimeOffset.UtcNow;
            while (DateTimeOffset.UtcNow - start < timeSpan)
            {
                try
                {
                    return func.Invoke();
                }
                catch
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            throw new TimeoutException($"Timeout: {context}");
        }

        public static T Retry<T>(
            Func<T> func,
            string context)
        {
            return Retry(func, context, TimeSpan.FromSeconds(15));
        }

        public static ReadOnlyCollection<IWebElement> GetElements(
            this ISearchContext driver,
            string xpath)
        {
            return driver.GetElements(xpath, TimeSpan.FromSeconds(15));
        }

        public static ReadOnlyCollection<IWebElement> GetElements(
            this ISearchContext driver,
            string xpath,
            TimeSpan timeSpan)
        {
            ReadOnlyCollection<IWebElement> func()
            {
                var element = driver.FindElements(By.XPath(xpath));
                return element;
            }

            return Retry(func, $"Element not found: {xpath}", timeSpan);
        }

        public static void ClickClear(this IWebElement element, ChromeDriver driver)
        {
            driver.ExecuteScript(
                "var elems = document.getElementsByClassName('wrapper__WKLZ loading-wrapper__1pmE');" +
                "if(elems && elems[0]){elems[0].remove();}");

            driver.ExecuteScript(
                "var elem = document.getElementById('initial-loading');" +
                "if(elem){elem.remove();}");

            element.Click();
        }

        public static IWebElement GetElement(
            this ISearchContext driver,
            string xpath)
        {
            return driver.GetElement(xpath, TimeSpan.FromSeconds(15));
        }

        public static IWebElement GetElement(
            this ISearchContext driver,
            string xpath,
            TimeSpan timeSpan)
        {
            IWebElement func()
            {
                var element = driver.FindElement(By.XPath(xpath));
                return element;
            }

            return Retry(func, $"Element not found: {xpath}", timeSpan);
        }
    }
}

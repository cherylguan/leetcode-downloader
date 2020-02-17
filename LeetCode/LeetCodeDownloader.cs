using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;

namespace LeetCode
{
    class LeetCodeDownloader
    {
        private readonly ChromeDriver driver;

        public LeetCodeDownloader()
        {
            driver = new ChromeDriver(@"C:\chromedriver");
            driver.Manage().Window.Maximize();
        }

        void Retry(Action action, int retryCount = 3)
        {
            while (retryCount > 0)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch (Exception e)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw e;
                    }

                    Log($"Retrying ({retryCount} left)... {e.Message}");
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        public void ShowAllProblems()
        {
            // Show all problems.
            var element = driver.GetElement("select", "class", "form-control", TimeSpan.FromSeconds(20));
            var select = new SelectElement(element);
            select.SelectByText("all");
        }

        public void DownloadProblem(int index)
        {
            GoToAllProblems();
            var linkContainers = GetAllProblemLinkContainers();
            var linkContainer = linkContainers[index];
            ProcessProblemLinkContainer(linkContainer);
        }

        private string batchName = "batch.txt";
        public void Log(string message)
        {
            Console.WriteLine(message);
            try
            {
                File.AppendAllLines(batchName, new[] { message });
            }
            catch(Exception e)
            {
                Console.WriteLine("Can't write log to file." + e);
            }
        }

        public void DownloadProblems(int start, int end)
        {
            this.batchName = $"batch-{start}-{end}.txt";
            GoToAllProblems();

            Retry(() => Signin());

            Thread.Sleep(TimeSpan.FromSeconds(3));

            GoToAllProblems();

            Retry(() => ShowAllProblems());

            Thread.Sleep(TimeSpan.FromSeconds(3));

            ReadOnlyCollection<IWebElement> linkContainers;
            var total = 0;
            while (total < 100)
            {
                GoToAllProblems();

                // Get all problem link containers.
                linkContainers = GetAllProblemLinkContainers();
                total = linkContainers.Count;
            }

            var processed = start;
            var failed = new List<int>();
            while (processed < end && processed < total)
            {
                try
                {
                    Retry(() => DownloadProblem(processed));
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                    failed.Add(processed);
                }

                processed++;
            }

            Log($"Failed: {failed.Count}");
            Log(JsonConvert.SerializeObject(failed));
        }

        void GoToAllProblems()
        {
            const string home = "https://leetcode.com/problemset/all/";
            driver.Url = home;
        }

        ReadOnlyCollection<IWebElement> GetAllProblemLinkContainers()
        {
            var element = driver.GetElement("tbody", "class", "reactable-data", TimeSpan.FromSeconds(20));
            var linkContainers = element.FindElements(By.TagName("tr"));
            return linkContainers;
        }

        void Signin()
        {
            // Click on signing button
            var element = driver.GetElement("a", "class", "btn sign-in-btn");
            element.ClickClear(driver);

            // Signin.
            element = driver.GetElement("input", "data-cy", "username");
            element.SendKeys("");
            element = driver.GetElement("input", "data-cy", "password");
            element.SendKeys("");
            element = driver.GetElement("button", "data-cy", "sign-in-btn");
            element.ClickClear(driver);
        }

        void ProcessProblemLinkContainer(IWebElement linkContainer)
        {
            // Click problem link.
            var testLink = linkContainer.FindElement(By.TagName("a"));
            testLink.ClickClear(driver);

            // Wait for question content to show up.
            var element = driver.GetElement("div", "class", "question-content");

            // Get problem description.
            element = driver.GetElement("div", "data-cy", "description-content");
            var question = element.GetAttribute("innerHTML");

            // Wait for loading bar.
            Thread.Sleep(TimeSpan.FromSeconds(3));
            var questionShot = driver.GetScreenshot();

            // Get problem title.
            element = driver.GetElement("div", "data-cy", "question-title");
            var title = element.GetAttribute("innerHTML");
            Log(title);

            const string dir = "questions";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var fileName = title + ".html";
            var filePath = Path.Join(dir, fileName);
            if (File.Exists(filePath))
            {
                Log("File exists, skipping...");
                return;
            }

            var questionUrl = driver.Url;

            // Go to solution page.
            var solutionUrl = questionUrl + "solution";
            driver.Url = solutionUrl;

            // Get solution text.
            var solution = "NO_SOLUTION";
            try
            {
                element = driver.GetElement("div", "id", "solution");
                solution = element.GetAttribute("innerHTML");
                if (!string.IsNullOrWhiteSpace(solution))
                {
                    solution = solution.Split("Comments:")[0];
                }
            }
            catch (TimeoutException)
            {
                Log("NO_SOLUTION");
            }

            var solutionShot = driver.GetScreenshot();

            // Go to discuss page.
            var discussUrl = questionUrl + "discuss";
            driver.Url = discussUrl;

            // Click on most voted.
            element = driver.GetElement("//label[contains(text(), 'Most Votes')]");
            element.ClickClear(driver);

            // Get discuss title.
            element = driver.GetElement("div", "class", "topic-title_");
            string discussTitle = element.GetAttribute("innerHTML");

            // Click on first title.
            element = driver.GetElement("a", "class", "title-link");
            element.ClickClear(driver);

            // Get discuss solution.
            element = driver.GetElement("div", "class", "discuss-markdown-container");
            string discuss = element.GetAttribute("innerHTML");

            var discussShot = driver.GetScreenshot();

            var fileContent = new StringBuilder();
            fileContent.AppendLine(question);
            fileContent.AppendLine(solution);
            fileContent.AppendLine(discussTitle);
            fileContent.AppendLine(discuss);

            File.WriteAllText(filePath, fileContent.ToString());

            questionShot.SaveAsFile(Path.Join(dir, title + ".png"));
            solutionShot.SaveAsFile(Path.Join(dir, title + "-sln.png"));
            discussShot.SaveAsFile(Path.Join(dir, title + "-dis.png"));
        }
    }
}

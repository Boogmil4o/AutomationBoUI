using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AutomationBoUI;


namespace AcademyVideosWEB;

[TestFixture]
class AcademyVideosWEB
{
    IWebDriver _driver = new ChromeDriver("C:\\Automation\\chrome");
    private Task<string?> token;
    GetData dataUtil = new GetData();


    [SetUp]
    public void DriverInit()
    {
        token = dataUtil.FetchToken(new GetData.User("dm.sumskoy87+test50@gmail.com", "qwerty12345", 41)).WaitAsync(TimeSpan.FromSeconds(5));
    }


    [Test]
    public async Task VerifyAcademyVideosVisibleLangEN()
    {
        await AssertAcademyItemVisibility("en");
    }

    [Test]
    public async Task VerifyAcademyVideosVisibleLangIT()
    {
        await AssertAcademyItemVisibility("it");
    }

    [Test]
    public async Task VerifyAcademyVideosVisibleLangPL()
    {
        await AssertAcademyItemVisibility("pl");
    }

    private async Task AssertAcademyItemVisibility(string lang)
    {
        var legalDataConfig = await dataUtil.GetLegalData("https://serving.plexop.net/assets/Shared/pl/Platform_full.json");
        var itemsTuple = dataUtil.GetItems(legalDataConfig);
        bool hasData = itemsTuple?.Item1?.Length > 0 || itemsTuple?.Item2?.Length > 0;
        string pageUrl = "https://fortissio.stplatform.naxex-tech.com/?lang=" + lang + "&token=" + await token;

        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        _driver.Navigate().GoToUrl(pageUrl);
        _driver.Manage().Window.Maximize();

        IWebElement AcademyVideosSection = _driver.FindElement(By.Id("_wt_mmnu_lessons"));
        Assert.IsTrue(
            AcademyVideosSection.Displayed == hasData,
            hasData ? "Expected academy item to be displayed but it's NOT" : "Expected academy item to NOT be displayed, but it IS."
        );
    }


    [OneTimeTearDown]
    public void DriverClose()
    {
        _driver.Close();
    }
}
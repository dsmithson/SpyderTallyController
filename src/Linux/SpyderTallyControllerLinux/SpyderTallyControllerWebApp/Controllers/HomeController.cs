using Microsoft.AspNetCore.Mvc;
using SpyderTallyControllerWebApp.Models;
using SpyderTallyControllerWebApp.ViewModels;
using System.Diagnostics;

namespace SpyderTallyControllerWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRelayRepository relayRepository;
        private readonly ISystemHealthRepository healthRepository;

        public HomeController(ILogger<HomeController> logger, 
            IRelayRepository relayRepository, 
            ISystemHealthRepository healthRepository)
        {
            _logger = logger;
            this.relayRepository = relayRepository;
            this.healthRepository = healthRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel()
            {
                CurrentTallyStatus = relayRepository.GetRelayStatus(),
                CpuUsage = await healthRepository.GetCPUUsageInfo(),
                DiskUsage = await healthRepository.GetRootFileSystemUsageInfo(),
                MemoryUsage = await healthRepository.GetMemoryUsageInfo(),
                Version = await healthRepository.GetApplicationVersion(),
                Uptime = await healthRepository.GetUptime(),
                Model = await healthRepository.GetSystemModel()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
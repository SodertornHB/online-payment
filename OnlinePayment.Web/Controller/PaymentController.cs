
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//--------------------------------------------------------------------------------------------------------------------

using AutoMapper;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlinePayment.Web.Controllers
{
    public partial class PaymentController : Controller
    {
        private readonly ILogger<PaymentController> logger;
        private readonly IPaymentService service;
        private readonly IMapper mapper;

        public PaymentController(ILogger<PaymentController> logger, 
        IPaymentService service, 
        IMapper mapper)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }

        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<PaymentViewModel>>(list);
            return View(viewModels.OrderByDescending(x => x.Id));
        }
        
        public ActionResult Create()
        {
            return View(new PaymentViewModel());
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm]PaymentViewModel viewModel)
        {
            var model = mapper.Map<Payment>(viewModel);
            await service.Insert(model);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<PaymentViewModel>(entity));
        }


        [HttpPost]
        public virtual async Task<ActionResult> Edit([FromForm]PaymentViewModel viewModel)
        {
            var model = mapper.Map<Payment>(viewModel);
            await service.Update(model);
            return RedirectToAction(nameof(Index));         
        }

        public virtual async Task<ActionResult> Remove(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<PaymentViewModel>(entity));        
        }

        [HttpPost]
        public virtual async Task<ActionResult> Remove([FromForm]PaymentViewModel viewModel)
        {
            var model = mapper.Map<Payment>(viewModel);
            await service.Delete(viewModel.Id);
            return RedirectToAction(nameof(Index));         
        }
    }
}
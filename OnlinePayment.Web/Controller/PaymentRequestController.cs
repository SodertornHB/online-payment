
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
    public partial class PaymentRequestController : Controller
    {
        private readonly ILogger<PaymentRequestController> logger;
        private readonly IPaymentRequestService service;
        private readonly IMapper mapper;

        public PaymentRequestController(ILogger<PaymentRequestController> logger, 
        IPaymentRequestService service, 
        IMapper mapper)
        {
            this.logger = logger;
            this.service = service;
            this.mapper = mapper;
        }

        public virtual async Task<IActionResult> Index()
        {
            var list = await service.GetAll();
            var viewModels = mapper.Map<IEnumerable<PaymentRequestViewModel>>(list);
            return View(viewModels.OrderByDescending(x => x.Id));
        }
        
        public ActionResult Create()
        {
            return View(new PaymentRequestViewModel());
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromForm]PaymentRequestViewModel viewModel)
        {
            var model = mapper.Map<PaymentRequest>(viewModel);
            await service.Insert(model);
            return RedirectToAction(nameof(Index));
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<PaymentRequestViewModel>(entity));
        }


        [HttpPost]
        public virtual async Task<ActionResult> Edit([FromForm]PaymentRequestViewModel viewModel)
        {
            var model = mapper.Map<PaymentRequest>(viewModel);
            await service.Update(model);
            return RedirectToAction(nameof(Index));         
        }

        public virtual async Task<ActionResult> Remove(int id)
        {
            var entity = await service.Get(id);
            return View(mapper.Map<PaymentRequestViewModel>(entity));        
        }

        [HttpPost]
        public virtual async Task<ActionResult> Remove([FromForm]PaymentRequestViewModel viewModel)
        {
            var model = mapper.Map<PaymentRequest>(viewModel);
            await service.Delete(viewModel.Id);
            return RedirectToAction(nameof(Index));         
        }
    }
}
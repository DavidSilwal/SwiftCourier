﻿using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using SwiftCourier.Models;
using SwiftCourier.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwiftCourier.Controllers
{
    public class PaymentsController : BaseController
    {
        private ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .SingleAsync(m => m.BookingId == id);

            if (invoice == null)
            {
                return HttpNotFound();
            }

            if(invoice.AmountDue <= 0)
            {
                return RedirectToAction("Invoice", "Bookings", new {
                    id = id
                });
            }

            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name");

            return View(new PaymentViewModel() {
                AmountDue = invoice.AmountDue,
                Amount = invoice.AmountDue
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int id, PaymentViewModel model)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .SingleAsync(m => m.BookingId == id);

            if(invoice == null)
            {
                return HttpNotFound();
            }

            if (invoice.AmountDue <= 0)
            {
                return RedirectToAction("Invoice", "Bookings", new
                {
                    id = id
                });
            }

            if (ModelState.IsValid)
            {
                var payment = model.ToEntity();
                payment.ProcessedAt = DateTime.Now;
                int userId;

                if(!int.TryParse(HttpContext.User.GetUserId(), out userId))
                {
                    //XXX:TODO Gracefully handle this
                    throw new Exception("Unable to get logged in User Id.");
                }

                payment.UserId = userId;

                invoice.Payments.Add(payment);
                invoice.AmountPaid += payment.Amount;
                invoice.AmountDue -= payment.Amount;

                if(invoice.AmountPaid >= invoice.Total)
                {
                    invoice.Status = InvoiceStatus.Paid;
                }

                _context.Update(invoice);

                await _context.SaveChangesAsync();

                return RedirectToAction("Invoice", "Bookings", new
                {
                    id = id
                });
            }

            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name");

            return View(model);
        }
    }
}
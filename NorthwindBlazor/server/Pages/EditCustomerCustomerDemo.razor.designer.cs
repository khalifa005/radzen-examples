﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using NorthwindBlazor.Models.Northwind;
using Microsoft.EntityFrameworkCore;

namespace NorthwindBlazor.Pages
{
    public partial class EditCustomerCustomerDemoComponent : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, dynamic> Attributes { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager UriHelper { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected NorthwindService Northwind { get; set; }

        [Parameter]
        public dynamic CustomerID { get; set; }

        [Parameter]
        public dynamic CustomerTypeID { get; set; }

        NorthwindBlazor.Models.Northwind.CustomerCustomerDemo _customercustomerdemo;
        protected NorthwindBlazor.Models.Northwind.CustomerCustomerDemo customercustomerdemo
        {
            get
            {
                return _customercustomerdemo;
            }
            set
            {
                if(!object.Equals(_customercustomerdemo, value))
                {
                    _customercustomerdemo = value;
                    InvokeAsync(() => { StateHasChanged(); });
                }
            }
        }

        IEnumerable<NorthwindBlazor.Models.Northwind.Customer> _getCustomersResult;
        protected IEnumerable<NorthwindBlazor.Models.Northwind.Customer> getCustomersResult
        {
            get
            {
                return _getCustomersResult;
            }
            set
            {
                if(!object.Equals(_getCustomersResult, value))
                {
                    _getCustomersResult = value;
                    InvokeAsync(() => { StateHasChanged(); });
                }
            }
        }

        IEnumerable<NorthwindBlazor.Models.Northwind.CustomerDemographic> _getCustomerDemographicsResult;
        protected IEnumerable<NorthwindBlazor.Models.Northwind.CustomerDemographic> getCustomerDemographicsResult
        {
            get
            {
                return _getCustomerDemographicsResult;
            }
            set
            {
                if(!object.Equals(_getCustomerDemographicsResult, value))
                {
                    _getCustomerDemographicsResult = value;
                    InvokeAsync(() => { StateHasChanged(); });
                }
            }
        }

        protected override async System.Threading.Tasks.Task OnInitializedAsync()
        {
            await Load();
        }
        protected async System.Threading.Tasks.Task Load()
        {
            var northwindGetCustomerCustomerDemoByCustomerIdAndCustomerTypeIdResult = await Northwind.GetCustomerCustomerDemoByCustomerIdAndCustomerTypeId($"{CustomerID}", $"{CustomerTypeID}");
            customercustomerdemo = northwindGetCustomerCustomerDemoByCustomerIdAndCustomerTypeIdResult;

            var northwindGetCustomersResult = await Northwind.GetCustomers();
            getCustomersResult = northwindGetCustomersResult;

            var northwindGetCustomerDemographicsResult = await Northwind.GetCustomerDemographics();
            getCustomerDemographicsResult = northwindGetCustomerDemographicsResult;
        }

        protected async System.Threading.Tasks.Task Form0Submit(NorthwindBlazor.Models.Northwind.CustomerCustomerDemo args)
        {
            try
            {
                var northwindUpdateCustomerCustomerDemoResult = await Northwind.UpdateCustomerCustomerDemo($"{CustomerID}", $"{CustomerTypeID}", customercustomerdemo);
                DialogService.Close(customercustomerdemo);
            }
            catch (Exception northwindUpdateCustomerCustomerDemoException)
            {
                    NotificationService.Notify(NotificationSeverity.Error, $"Error", $"Unable to update CustomerCustomerDemo");
            }
        }

        protected async System.Threading.Tasks.Task Button2Click(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
    }
}

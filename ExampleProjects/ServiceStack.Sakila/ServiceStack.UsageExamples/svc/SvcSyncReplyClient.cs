﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServiceStack.UsageExamples.svc
{
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ArrayOfIntId", Namespace="http://schemas.servicestack.net/types/", ItemName="Id")]
    [System.SerializableAttribute()]
    public class ArrayOfIntId : System.Collections.Generic.List<int>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="Properties", Namespace="http://schemas.servicestack.net/types/", ItemName="Property")]
    [System.SerializableAttribute()]
    public class Properties : System.Collections.Generic.List<ServiceStack.UsageExamples.svc.Property>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Property", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class Property : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ValueField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Value
        {
            get
            {
                return this.ValueField;
            }
            set
            {
                this.ValueField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Customer", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class Customer : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ServiceStack.UsageExamples.svc.Address AddressField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FirstNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LastNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int StoreIdField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ServiceStack.UsageExamples.svc.Address Address
        {
            get
            {
                return this.AddressField;
            }
            set
            {
                this.AddressField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email
        {
            get
            {
                return this.EmailField;
            }
            set
            {
                this.EmailField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FirstName
        {
            get
            {
                return this.FirstNameField;
            }
            set
            {
                this.FirstNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastName
        {
            get
            {
                return this.LastNameField;
            }
            set
            {
                this.LastNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int StoreId
        {
            get
            {
                return this.StoreIdField;
            }
            set
            {
                this.StoreIdField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Address", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class Address : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ServiceStack.UsageExamples.svc.City CityField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string Line1Field;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string Line2Field;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PostCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string TownField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ServiceStack.UsageExamples.svc.City City
        {
            get
            {
                return this.CityField;
            }
            set
            {
                this.CityField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Line1
        {
            get
            {
                return this.Line1Field;
            }
            set
            {
                this.Line1Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Line2
        {
            get
            {
                return this.Line2Field;
            }
            set
            {
                this.Line2Field = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PostCode
        {
            get
            {
                return this.PostCodeField;
            }
            set
            {
                this.PostCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Town
        {
            get
            {
                return this.TownField;
            }
            set
            {
                this.TownField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="City", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class City : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ServiceStack.UsageExamples.svc.Country CountryField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ServiceStack.UsageExamples.svc.Country Country
        {
            get
            {
                return this.CountryField;
            }
            set
            {
                this.CountryField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Country", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class Country : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResponseStatus", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class ResponseStatus : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ServiceStack.UsageExamples.svc.ResponseError[] ErrorsField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MessageField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ErrorCode
        {
            get
            {
                return this.ErrorCodeField;
            }
            set
            {
                this.ErrorCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ServiceStack.UsageExamples.svc.ResponseError[] Errors
        {
            get
            {
                return this.ErrorsField;
            }
            set
            {
                this.ErrorsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Message
        {
            get
            {
                return this.MessageField;
            }
            set
            {
                this.MessageField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResponseError", Namespace="http://schemas.servicestack.net/types/")]
    [System.SerializableAttribute()]
    public partial class ResponseError : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FieldNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MessageField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ErrorCode
        {
            get
            {
                return this.ErrorCodeField;
            }
            set
            {
                this.ErrorCodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FieldName
        {
            get
            {
                return this.FieldNameField;
            }
            set
            {
                this.FieldNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Message
        {
            get
            {
                return this.MessageField;
            }
            set
            {
                this.MessageField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://services.servicestack.net/", ConfigurationName="ServiceStack.UsageExamples.svc.ISyncReply")]
    public interface ISyncReply
    {
        
        // CODEGEN: Generating message contract since the wrapper namespace (http://schemas.servicestack.net/types/) of message GetCustomersRequest does not match the default value (http://services.servicestack.net/)
        [System.ServiceModel.OperationContractAttribute(Action="http://services.servicestack.net/GetCustomers", ReplyAction="*")]
        ServiceStack.UsageExamples.svc.GetCustomersResponse GetCustomers(ServiceStack.UsageExamples.svc.GetCustomersRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetCustomers", WrapperNamespace="http://schemas.servicestack.net/types/", IsWrapped=true)]
    public partial class GetCustomersRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=0)]
        public ServiceStack.UsageExamples.svc.ArrayOfIntId CustomerIds;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=1)]
        public ServiceStack.UsageExamples.svc.Properties Properties;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=2)]
        public int Version;
        
        public GetCustomersRequest()
        {
        }
        
        public GetCustomersRequest(ServiceStack.UsageExamples.svc.ArrayOfIntId CustomerIds, ServiceStack.UsageExamples.svc.Properties Properties, int Version)
        {
            this.CustomerIds = CustomerIds;
            this.Properties = Properties;
            this.Version = Version;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetCustomersResponse", WrapperNamespace="http://schemas.servicestack.net/types/", IsWrapped=true)]
    public partial class GetCustomersResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=0)]
        public ServiceStack.UsageExamples.svc.Customer[] Customers;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=1)]
        public ServiceStack.UsageExamples.svc.Properties Properties;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=2)]
        public ServiceStack.UsageExamples.svc.ResponseStatus ResponseStatus;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=3)]
        public int Version;
        
        public GetCustomersResponse()
        {
        }
        
        public GetCustomersResponse(ServiceStack.UsageExamples.svc.Customer[] Customers, ServiceStack.UsageExamples.svc.Properties Properties, ServiceStack.UsageExamples.svc.ResponseStatus ResponseStatus, int Version)
        {
            this.Customers = Customers;
            this.Properties = Properties;
            this.ResponseStatus = ResponseStatus;
            this.Version = Version;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface ISyncReplyChannel : ServiceStack.UsageExamples.svc.ISyncReply, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class SyncReplyClient : System.ServiceModel.ClientBase<ServiceStack.UsageExamples.svc.ISyncReply>, ServiceStack.UsageExamples.svc.ISyncReply
    {
        
        public SyncReplyClient()
        {
        }
        
        public SyncReplyClient(string endpointConfigurationName) : 
                base(endpointConfigurationName)
        {
        }
        
        public SyncReplyClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public SyncReplyClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public SyncReplyClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        ServiceStack.UsageExamples.svc.GetCustomersResponse ServiceStack.UsageExamples.svc.ISyncReply.GetCustomers(ServiceStack.UsageExamples.svc.GetCustomersRequest request)
        {
            return base.Channel.GetCustomers(request);
        }
        
        public ServiceStack.UsageExamples.svc.Customer[] GetCustomers(ServiceStack.UsageExamples.svc.ArrayOfIntId CustomerIds, ref ServiceStack.UsageExamples.svc.Properties Properties, ref int Version, out ServiceStack.UsageExamples.svc.ResponseStatus ResponseStatus)
        {
            ServiceStack.UsageExamples.svc.GetCustomersRequest inValue = new ServiceStack.UsageExamples.svc.GetCustomersRequest();
            inValue.CustomerIds = CustomerIds;
            inValue.Properties = Properties;
            inValue.Version = Version;
            ServiceStack.UsageExamples.svc.GetCustomersResponse retVal = ((ServiceStack.UsageExamples.svc.ISyncReply)(this)).GetCustomers(inValue);
            Properties = retVal.Properties;
            ResponseStatus = retVal.ResponseStatus;
            Version = retVal.Version;
            return retVal.Customers;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://services.servicestack.net/", ConfigurationName="ServiceStack.UsageExamples.svc.IOneWay")]
    public interface IOneWay
    {
        
        // CODEGEN: Generating message contract since the wrapper namespace (http://schemas.servicestack.net/types/) of message StoreCustomer does not match the default value (http://services.servicestack.net/)
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://services.servicestack.net/StoreCustomer")]
        void StoreCustomer(ServiceStack.UsageExamples.svc.StoreCustomer request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="StoreCustomer", WrapperNamespace="http://schemas.servicestack.net/types/", IsWrapped=true)]
    public partial class StoreCustomer
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=0)]
        public ServiceStack.UsageExamples.svc.Customer Customer;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=1)]
        public ServiceStack.UsageExamples.svc.Properties Properties;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.servicestack.net/types/", Order=2)]
        public int Version;
        
        public StoreCustomer()
        {
        }
        
        public StoreCustomer(ServiceStack.UsageExamples.svc.Customer Customer, ServiceStack.UsageExamples.svc.Properties Properties, int Version)
        {
            this.Customer = Customer;
            this.Properties = Properties;
            this.Version = Version;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IOneWayChannel : ServiceStack.UsageExamples.svc.IOneWay, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class OneWayClient : System.ServiceModel.ClientBase<ServiceStack.UsageExamples.svc.IOneWay>, ServiceStack.UsageExamples.svc.IOneWay
    {
        
        public OneWayClient()
        {
        }
        
        public OneWayClient(string endpointConfigurationName) : 
                base(endpointConfigurationName)
        {
        }
        
        public OneWayClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public OneWayClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public OneWayClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        void ServiceStack.UsageExamples.svc.IOneWay.StoreCustomer(ServiceStack.UsageExamples.svc.StoreCustomer request)
        {
            base.Channel.StoreCustomer(request);
        }
        
        public void StoreCustomer(ServiceStack.UsageExamples.svc.Customer Customer, ServiceStack.UsageExamples.svc.Properties Properties, int Version)
        {
            ServiceStack.UsageExamples.svc.StoreCustomer inValue = new ServiceStack.UsageExamples.svc.StoreCustomer();
            inValue.Customer = Customer;
            inValue.Properties = Properties;
            inValue.Version = Version;
            ((ServiceStack.UsageExamples.svc.IOneWay)(this)).StoreCustomer(inValue);
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace api.fefarm.mx.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class cat_Requests
    {
        public int Request_Id { get; set; }
        public string Request_Name { get; set; }
        public string Request_JSON_Body { get; set; }
        public System.DateTime Request_Creation_Date { get; set; }
        public System.DateTime Request_Start_Date { get; set; }
        public System.DateTime Request_Finish_Date { get; set; }
        public Nullable<int> Request_Max_Applications { get; set; }
        public Nullable<int> Request_Max_Beneficiaries { get; set; }
    }
}

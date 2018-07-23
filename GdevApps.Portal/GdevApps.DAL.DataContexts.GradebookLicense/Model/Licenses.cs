using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataContexts.GradebookLicense.Model
{
    public partial class Licenses
    {
        public int Id { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int UserId { get; set; }
        public int AccountType { get; set; }
        public int DaysRemaining { get; set; }
        public string Price { get; set; }
        public string BuyerEmail { get; set; }
        public int Product { get; set; }
        public DateTime PurchaseDate { get; set; }

        public Account AccountTypeNavigation { get; set; }
        public Products ProductNavigation { get; set; }
        public Users User { get; set; }
    }
}

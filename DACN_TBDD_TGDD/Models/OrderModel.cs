    using System.ComponentModel.DataAnnotations.Schema;

    namespace DACN_TBDD_TGDD.Models
    {
        public class OrderModel
        {
            public int Id { get; set; }
            public string OrderCode { get; set; }

            public string UserName { get; set; }
            public DateTime CreatedDate { get; set; }
            public int Status { get; set; }
		    public decimal? TotalAmount { get; set; }
           public string Address {  get; set; }
         public string PhoneNumber {  get; set; }



    }
    }

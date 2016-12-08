using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace RiskTracker.Entities {
  public class AddressData {
    [Key]
    public Guid Id { get; set; }
    public string Details { get; set; }
    public string PostCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public bool IsInDeprivedArea { get { return AreaOfDeprivation.InAOD(PostCode); } }

    public void CopyFrom(AddressData other) {
      Details = other.Details;
      PostCode = other.PostCode;
      PhoneNumber = other.PhoneNumber;
      Email = other.Email;
      Website = other.Website;
    } // CopyFrom
  } // class Address
} // namespace ...
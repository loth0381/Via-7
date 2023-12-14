using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ecommer.ViewModels
{
    public class CheckoutViewModel
    {
        public Order Order { get; set; }
        public ShoppingCart Cart { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        public string City { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Formato de código postal no válido.")]
        public string ZipCode { get; set; }

        public string StripePublishableKey { get; set; }
        public string StripeToken { get; set; }

        public List<CartDetail> CartDetails { get; set; }

        // Nueva propiedad para la confirmación
        public bool OrderPlaced { get; set; }
    }
}
